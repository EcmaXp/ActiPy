using System.Windows.Forms;
using Advanced_Combat_Tracker;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ActiPy
{
    public class ActiPyPlugin : IActPluginV1
    {
        public TabPage pluginScreenSpace;
        public Label pluginStatusText;
        internal List<PythonPlugin> plugins;

        public ActiPyPlugin()
        {

        }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            this.pluginScreenSpace = pluginScreenSpace;
            this.pluginStatusText = pluginStatusText;

            var directory = Path.Combine(GetPluginPath(), "addons");
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            plugins = new List<PythonPlugin>();
            foreach (var pluginFile in Directory.GetFiles(directory, "*.py"))
            {
                pluginStatusText.Text = "Processing " + pluginFile;
                PythonPlugin plugin = new PythonPlugin(this, pluginFile);
                plugins.Add(plugin);
            }

            foreach (var plugin in plugins.Reverse<PythonPlugin>())
            {
                pluginStatusText.Text = "Loading " + plugin.scriptPath;
                plugin.pdata.cbEnabled.Checked = true;

                try
                {
                    try
                    {
                        plugin.InitPlugin();
                    }
                    catch (Exception)
                    {
                        plugin.DeInitPython();
                        plugin.DeInitPlugin();
                        plugins.Remove(plugin);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("While loading script by ActiPy, exception are raised.\r\n" + ex.ToString());
                }
            }

            switch (plugins.Count)
            {
                case 0:
                    pluginStatusText.Text = "ActiPy Loaded but there is no script.";
                    break;
                case 1:
                    pluginStatusText.Text = "ActiPy Loaded with 1 script.";
                    break;
                default:
                    pluginStatusText.Text = String.Format("ActiPy Loaded with {0} scripts.", plugins.Count);
                    break;
            }

            // TODO: Support OverlayPlugin?
        }

        public void DeInitPlugin()
        {
            foreach (var plugin in plugins.Reverse<PythonPlugin>())
            {
                pluginStatusText.Text = "Unloading " + plugin.scriptPath;
                try
                {
                    plugin.DeInitPlugin();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                plugins.Remove(plugin);
            }

            pluginStatusText.Text = "ActiPy Unloaded.";
        }

        public string GetPluginPath() {
            var thisPlugin = ActGlobals.oFormActMain.ActPlugins.Where(x => x.pluginObj == this).FirstOrDefault();
            return Path.GetDirectoryName(thisPlugin.pluginFile.FullName);
        }
    }
}
using System.Windows.Forms;
using Advanced_Combat_Tracker;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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
            
            var plugin = ActGlobals.oFormActMain.ActPlugins.Where(x => x.pluginObj == this).FirstOrDefault();
            var directory = Path.Combine(System.IO.Path.GetDirectoryName(plugin.pluginFile.FullName), "addons");

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            plugins = new List<PythonPlugin>();
            foreach (var pluginFile in Directory.GetFiles(directory, "*.py"))
            {
                plugins.Add(new PythonPlugin(pluginFile));
            }
            
            // this.plugin.enabled = true;
            // pluginScreenSpace.Controls.Add(this);
            // TODO: Support scripting by file (by config panel; required GUI)
            // TODO: Multi-scripting
            // TODO: Support OverlayPlugin
        }

        public void DeInitPlugin()
        {
            foreach (var plugin in plugins)
            {
                plugin.DeInitPlugin();
            }
        }

        // TODO: fix Add/Enable plugins.
    }
}
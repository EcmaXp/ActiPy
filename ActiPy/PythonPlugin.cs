using System;
using IronPython.Hosting;
using Advanced_Combat_Tracker;
using System.Windows.Forms;
using Microsoft.Scripting.Hosting;
using System.ComponentModel;
using System.Reflection;
using System.Drawing;
using System.Runtime.Remoting;

namespace ActiPy
{
    public class PythonPlugin : IActPluginV1
    {
        protected ScriptEngine engine;
        protected ScriptScope scope;
        public ActPluginData pdata;
        public String path;
        
        public PythonPlugin(String path) : this(ActGlobals.oFormActMain.AddPluginPanel(path, true))
        {
            this.path = path;
        }

        public PythonPlugin(ActPluginData data)
        {
            pdata = data;
            pdata.pluginObj = this;

            pluginPanelFixSettingPage();
            pluginPanelFixEnableCheckbox();
        }

        private void pluginPanelFixSettingPage()
        {
            pdata.tpPluginSpace = new TabPage();
            pdata.tpPluginSpace.Dispose(); // place-holder.
        }

        private void pluginPanelFixEnableCheckbox()
        {
            CheckBox oldCheckBox = pdata.cbEnabled;
            CheckBox newCheckBox = new CheckBox();

            newCheckBox.Size = oldCheckBox.Size;
            newCheckBox.Location = oldCheckBox.Location;
            newCheckBox.Text = oldCheckBox.Text;
            newCheckBox.Anchor = oldCheckBox.Anchor;

            newCheckBox.GotFocus += new System.EventHandler(this.pluginPanelChildGotFocus);
            newCheckBox.CheckedChanged += new System.EventHandler(this.pluginPanelEnabledChecked);

            pdata.pPluginInfo.Controls.Remove(oldCheckBox);
            pdata.pPluginInfo.Controls.Add(newCheckBox);
        }

        private void pluginPanelChildGotFocus(object sender, System.EventArgs e)
        {
            Control control = (Control)sender;
            control.Parent.Focus();
        }

        public void pluginPanelEnabledChecked(object sender, System.EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            if (checkBox.Checked)
                InitPlugin();
            else
                DeInitPlugin(false);
        }
        
        public void InitPlugin()
        {
            InitPlugin(pdata.tpPluginSpace, pdata.lblPluginStatus);
        }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            TabControl tcPlugins = ReflectHelper.Get_FormActMain_TcPlugins();
            pdata.tpPluginSpace = new TabPage(pdata.lblPluginTitle.Text);
            tcPlugins.TabPages.Add(pdata.tpPluginSpace);
            
            InitPython();
            InitScript();
            LoadScript();

            // TODO: preload script for assambly infomation?
        }

        public void DeInitPlugin(Boolean isFullDeinit)
        {
            if (IsPythonLoaded())
            {
                UnloadScript(); // TODO: isScriptLoaded require?
                DeInitPython();
            }

            TabControl tcPlugins = ReflectHelper.Get_FormActMain_TcPlugins();
            tcPlugins.TabPages.Remove(pdata.tpPluginSpace);
            pdata.tpPluginSpace.Dispose();

            if (!isFullDeinit)
            {
                pdata.lblPluginStatus.Text = "Plugin Unloaded";
            }
            else
            {
                ReflectHelper.Invoke_FormActMain_PluginRemovePanel(pdata);
                pdata.lblPluginStatus.Text = "You can't see this message :P";
            }
        }

        public void DeInitPlugin()
        {
            DeInitPlugin(true);
        }

        internal void InitPython()
        {
            if (IsPythonLoaded())
                DeInitPython();

            engine = Python.CreateEngine();
            engine.Runtime.LoadAssembly(System.Reflection.Assembly.GetEntryAssembly());
            scope = engine.CreateScope();
        }

        internal void DeInitPython()
        {
            engine = null;
            scope = null;
        }

        internal void InitScript()
        {
            if (!IsPythonLoaded())
                throw new Exception("Python does not loaded.");

            scope.SetVariable("plugin", this);

            engine.Execute("import Advanced_Combat_Tracker as ACT", scope);
            engine.Execute("from Advanced_Combat_Tracker import ActGlobals", scope);
        }

        public Boolean IsPythonLoaded()
        {
            return (engine != null && scope != null);
        }

        internal void LoadScript()
        {
            string statusText = "Loading script...";
            pdata.lblPluginStatus.Text = statusText;

            try
            {
                engine.ExecuteFile(path, scope);
                ExecuteScript("load_hook()");
            }
            catch (Exception ex)
            {
                // TODO: detail ex
                pdata.lblPluginStatus.Text = "Failed to load script :(";
                MessageBox.Show(ex.ToString());
                return;
            }
            
            if (pdata.lblPluginStatus.Text == statusText)
                pdata.lblPluginStatus.Text = "Script Loaded.";
        }

        public ObjectHandle ExecuteScript(String code)
        {
            if (!IsPythonLoaded())
                throw new Exception("Python does not loaded.");

            return engine.ExecuteAndWrap(code, scope);
        }

        internal void UnloadScript()
        {
            ExecuteScript("unload_hook()");
        }
    }
}

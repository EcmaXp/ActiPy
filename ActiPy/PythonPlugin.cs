using System;
using IronPython.Hosting;
using Advanced_Combat_Tracker;
using System.Windows.Forms;
using Microsoft.Scripting.Hosting;
using System.ComponentModel;
using System.Reflection;
using System.Drawing;
using System.Runtime.Remoting;
using System.IO;
using System.Collections.Generic;

namespace ActiPy
{
    public class PythonPlugin : IActPluginV1
    {
        protected ScriptEngine engine;
        protected ScriptScope scope;
        protected ActiPyPlugin parent;
        public ActPluginData pdata;
        public String scriptPath;
        public Boolean controlEnabled;
        
        public PythonPlugin(ActiPyPlugin parent, String scriptPath) : this(parent, ActGlobals.oFormActMain.AddPluginPanel(scriptPath, true))
        {
            this.parent = parent;
            this.scriptPath = scriptPath;
        }

        public PythonPlugin(ActiPyPlugin parent, ActPluginData data)
        {
            this.parent = parent;
            this.pdata = data;

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

            pdata.cbEnabled = newCheckBox;
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
                InitPlugin(false);
            else
                DeInitPlugin(false);
        }

        public void InitPlugin()
        {
            InitPlugin(true);
        }

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            pdata.tpPluginSpace = pluginScreenSpace;
            pdata.lblPluginStatus = pluginStatusText;
            InitPlugin(true);
        }

        public void InitPlugin(Boolean isFirst)
        {
            if (isFirst)
            {
                ReflectHelper.Get_FormActMain_Plugins().Remove(pdata);
                pdata.cbEnabled.CheckedChanged += new System.EventHandler(this.pluginPanelEnabledChecked);
            }
            
            pdata.tpPluginSpace = new TabPage(pdata.lblPluginTitle.Text);

            // TODO: preload script for assambly infomation?
            InitPython();
            InitScript();
            LoadScript();
        }

        public void EnableTabPage()
        {
            TabControl tcPlugins = ReflectHelper.Get_FormActMain_TcPlugins();

            if (!tcPlugins.TabPages.Contains(pdata.tpPluginSpace))
                tcPlugins.TabPages.Add(pdata.tpPluginSpace);
        }

        public void DeInitPlugin()
        {
            DeInitPlugin(true);
        }

        public void DeInitPlugin(Boolean isLast)
        {
            string statusText = "Script Stopping";
            pdata.lblPluginStatus.Text = statusText;

            if (IsPythonLoaded())
            {
                UnloadScript(); // TODO: isScriptLoaded require?
                DeInitPython();
            }

            TabControl tcPlugins = ReflectHelper.Get_FormActMain_TcPlugins();
            if (tcPlugins.TabPages.Contains(pdata.tpPluginSpace))
                tcPlugins.TabPages.Remove(pdata.tpPluginSpace);
            pdata.tpPluginSpace.Dispose();

            if (pdata.lblPluginStatus.Text == statusText)
                pdata.lblPluginStatus.Text = "Script Stopped";

            if (isLast)
            {
                ReflectHelper.Get_FormActMain_Plugins().Add(pdata);
                ReflectHelper.Invoke_FormActMain_PluginRemovePanel(pdata);
            }
        }

        internal void InitPython()
        {
            if (IsPythonLoaded())
                DeInitPython();

            engine = Python.CreateEngine();
            engine.Runtime.LoadAssembly(System.Reflection.Assembly.GetEntryAssembly());
            engine.Execute("import Advanced_Combat_Tracker");

            var pluginPath = parent.GetPluginPath();
            var paths = new List<string>();
            paths.Add(Path.Combine(pluginPath, "addons"));
            paths.Add(Path.Combine(pluginPath, "PyLib"));
            paths.Add(Path.Combine(pluginPath, "PyDLLs"));
            engine.SetSearchPaths(paths);

            scope = engine.CreateScope();
        }

        internal void DeInitPython()
        {
            engine = null;
            scope = null;
        }

        internal void InitScript()
        {
            // XXX: unknown behavior make __name__ are forced as <module>
            // scope.SetVariable("__name__", "__main__");

            scope.SetVariable("__path__", this.scriptPath);
            scope.SetVariable("__plugin__", this);
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
                ScriptSource source = engine.CreateScriptSourceFromFile(scriptPath);
                source.Execute(scope);
            }
            catch (Exception)
            {
                pdata.lblPluginStatus.Text = "Script has Exception";
                throw;
            }

            if (pdata.lblPluginStatus.Text == statusText)
                pdata.lblPluginStatus.Text = "Script Started";
        }

        public ObjectHandle ExecuteScript(String code)
        {
            return engine.ExecuteAndWrap(code, scope);
        }

        internal void UnloadScript()
        {
            ExecuteScript(@"
def __exit():
    import sys
    globals().pop('__exit')
    if hasattr(sys, 'exitfunc'):
        sys.exitfunc()
__exit()
");
        }
    }
}

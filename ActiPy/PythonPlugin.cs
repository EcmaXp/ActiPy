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
        public Boolean controlEnabled;
        
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
                pdata.cbEnabled.CheckedChanged += new System.EventHandler(this.pluginPanelEnabledChecked);

            TabControl tcPlugins = ReflectHelper.Get_FormActMain_TcPlugins();
            pdata.tpPluginSpace = new TabPage(pdata.lblPluginTitle.Text);
            tcPlugins.TabPages.Add(pdata.tpPluginSpace);

            // TODO: preload script for assambly infomation?

            InitPython();
            InitScript();
            LoadScript();
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
            tcPlugins.TabPages.Remove(pdata.tpPluginSpace);
            pdata.tpPluginSpace.Dispose();

            if (pdata.lblPluginStatus.Text == statusText)
                pdata.lblPluginStatus.Text = "Script Stopped";

            if (isLast)
                ReflectHelper.Invoke_FormActMain_PluginRemovePanel(pdata);            
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

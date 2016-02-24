using Advanced_Combat_Tracker;
using IronPython.Hosting;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Hosting;
using System.Windows.Forms;

namespace ActiPy
{
    public class ActiPyPlugin : IActPluginV1
    {
        private ScriptEngine engine;
        private ScriptScope scope;
        public TabPage pluginScreenSpace;
        public Label pluginStatusText;

        public void InitPlugin(TabPage pluginScreenSpace, Label pluginStatusText)
        {
            this.pluginScreenSpace = pluginScreenSpace;
            this.pluginStatusText = pluginStatusText;

            engine = Python.CreateEngine();
            scope = engine.CreateScope();
            scope.SetVariable("plugin", this);

            // TODO: Support scripting by file (but how?)
            // TODO: Multi-scripting

            // pre-loader stage A (without scope)
            engine.Execute(@"
import clr
import System.Reflection.Assembly
clr.AddReferenceToFileAndPath(System.Reflection.Assembly.GetEntryAssembly().Location)
");

            // pre-loader stage B (with scope ~)
            engine.Execute(@"
import Advanced_Combat_Tracker as ACT
ActGlobals = ACT.ActGlobals
", scope);

            // loader
            var result = engine.ExecuteAndWrap(@"
repr(ActGlobals.charName)

# last execute code will write as label text
dir()
", scope);

            // write status
            scope.SetVariable("_result", result);
            pluginStatusText.Text = engine.Execute<string>(@"_result if isinstance(_result, str) else repr(_result)", scope); ;
        }

        public void DeInitPlugin()
        {
            // unloader
            engine.Execute(@"
# unload code here
", scope);

            pluginStatusText.Text = "Plugin Unloaded";
        }
    }
}

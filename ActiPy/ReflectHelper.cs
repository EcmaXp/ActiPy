using Advanced_Combat_Tracker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ActiPy
{
    static class ReflectHelper
    {
        public static TabControl Get_FormActMain_TcPlugins()
        {
            FieldInfo field = ActGlobals.oFormActMain.GetType().GetField("tcPlugins",
                BindingFlags.NonPublic | BindingFlags.Instance);
            TabControl tcPlugins = (TabControl)field.GetValue(ActGlobals.oFormActMain);
            return tcPlugins;
        }

        public static void Invoke_FormActMain_PluginRemovePanel(ActPluginData pluginData)
        {
            MethodInfo method = ActGlobals.oFormActMain.GetType().GetMethod("PluginRemovePanel",
                BindingFlags.NonPublic | BindingFlags.Instance);

            Object[] args = { pluginData };
            method.Invoke(ActGlobals.oFormActMain, args);
        }
    }
}

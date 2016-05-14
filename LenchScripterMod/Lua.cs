using LenchScripter.Internal;

namespace LenchScripter
{
    /// <summary>
    /// Lua API of the scripting mod.
    /// </summary>
    public static class Lua
    {

        /// <summary>
        /// Lua state.
        /// Is null if Lua environment is not initialised.
        /// </summary>
        public static NLua.Lua State
        {
            get
            {
                return Scripter.Instance.lua;
            }
        }

        /// <summary>
        /// Is Lua enabled to start on simulation.
        /// </summary>
        public static bool Enabled
        {
            get
            {
                return Scripter.Instance.enableLua;
            }
            set
            {
                Scripter.Instance.RunScriptSettingToggle(value);
            }
        }

        /// <summary>
        /// Is Lua environment currently initialised.
        /// </summary>
        public static bool IsActive
        {
            get
            {
                return Scripter.Instance.lua != null;
            }
        }

        /// <summary>
        /// Is Lua currently executing code.
        /// </summary>
        public static bool IsExecuting
        {
            get
            {
                return
                    Scripter.Instance.luaOnUpdate != null ||
                    Scripter.Instance.luaOnFixedUpdate != null ||
                    Scripter.Instance.luaOnKey != null ||
                    Scripter.Instance.luaOnKeyDown != null ||
                    Scripter.Instance.luaOnKeyUp != null;
            }
        }

        /// <summary>
        /// Evaluate Lua expression.
        /// </summary>
        /// <param name="LuaExpression">Lua expression string.</param>
        /// <returns>Returns an array of objects.</returns>
        public static System.Object[] Evaluate(string LuaExpression)
        {
            return Scripter.Instance.lua.DoString(LuaExpression);
        }

        /// <summary>
        /// Queues the script to load at the start
        /// or at the next frame of the simulation.
        /// </summary>
        /// <param name="path">Path to the *.lua file.
        /// Can be absolute or relative from the Scripts folder.
        /// Extension can be omitted.</param>
        public static void LoadScript(string path)
        {
            ScripterMod.ScriptOptions.FindScript(path);
            Scripter.Instance.scriptFile = ScripterMod.ScriptOptions.ScriptPath;
        }
    }
}

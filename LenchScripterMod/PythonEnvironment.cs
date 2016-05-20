
using System;
using UnityEngine;
using IronPython.Hosting;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Hosting;

namespace LenchScripter
{
    /// <summary>
    /// Class handling Python environment.
    /// </summary>
    public class PythonEnvironment
    {
        private static ScriptEngine _engine;

        private ScriptScope scope;
        private Action update;
        private Action fixedupdate;

        /// <summary>
        /// Python Engine.
        /// </summary>
        public ScriptEngine Engine { get { return _engine; } }

        /// <summary>
        /// Python Scope.
        /// </summary>
        public ScriptScope Scope { get { return scope; } }

        /// <summary>
        /// Update function from compiled script.
        /// </summary>
        public Action Update { get { return update; } }

        /// <summary>
        /// FixedUpdate function from compiled script.
        /// </summary>
        public Action FixedUpdate { get { return fixedupdate; } }

        /// <summary>
        /// Scripter Mod's current environment instance.
        /// </summary>
        public static PythonEnvironment Instance {
            get
            {
                return Internal.Scripter.Instance.python;
            }
            set
            {
                Internal.Scripter.Instance.python = value;
            }
        }

        static PythonEnvironment()
        {
            _engine = Python.CreateEngine();
            _engine.GetSearchPaths().Add(Application.dataPath + "/Scripts/");
        }

        /// <summary>
        /// Creates a new Python Environment and sets up the scope.
        /// </summary>
        public PythonEnvironment()
        {
            // Initialize scope
            scope = _engine.CreateScope();

            // Create environment
            _engine.Execute("import clr", scope);
            _engine.Execute("clr.AddReference(\"UnityEngine\")", scope);
            _engine.Execute("from UnityEngine import Vector2, Vector3, Vector4, Mathf, Time, Input", scope);
            scope.SetVariable("Besiege", DynamicHelpers.GetPythonTypeFromType(typeof(Functions)));
        }

        /// <summary>
        /// Compiles and executes code from string.
        /// </summary>
        /// <param name="code"></param>
        public void LoadCode(string code)
        {
            var source = _engine.CreateScriptSourceFromString(code);
            var compiled = source.Compile();
            compiled.Execute(scope);
            GetFunctions();
        }

        /// <summary>
        /// Compiles and executes code from file.
        /// </summary>
        /// <param name="path"></param>
        public void LoadScript(string path)
        {
            path = Internal.ScripterMod.ScriptOptions.FindScript(path);
            var source = _engine.CreateScriptSourceFromFile(path);
            var compiled = source.Compile();
            compiled.Execute(scope);
            GetFunctions();
        }

        /// <summary>
        /// Evaluates expression and returns the result as type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">Python expression.</param>
        /// <returns></returns>
        public object Evaluate<T>(string expression)
        {
            return _engine.Execute<T>(expression, scope);
        }

        private void GetFunctions()
        {
            scope.TryGetVariable("Update", out update);
            scope.TryGetVariable("FixedUpdate", out fixedupdate);
        }

    }
}

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
        private Exception exception;

        /// <summary>
        /// Python Engine.
        /// </summary>
        public static ScriptEngine Engine { get { return _engine; } }

        /// <summary>
        /// Python Scope.
        /// </summary>
        public ScriptScope Scope { get { return scope; } }

        /// <summary>
        /// Returns last occured formatted exception.
        /// </summary>
        public string LastException
        {
            get
            {
                if (exception == null) return null;
                ExceptionOperations eo = _engine.GetService<ExceptionOperations>();
                return eo.FormatException(exception);
            }
        }

        /// <summary>
        /// Scripter Mod's current environment instance.
        /// </summary>
        public static PythonEnvironment ScripterInstance {
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
            _engine.Execute("clr.AddReference(\"System\")", scope);
            _engine.Execute("clr.AddReference(\"UnityEngine\")", scope);
            _engine.Execute("from UnityEngine import Vector2, Vector3, Vector4, Mathf, Time, Input, KeyCode", scope);
            scope.SetVariable("Besiege", DynamicHelpers.GetPythonTypeFromType(typeof(Functions)));
        }

        /// <summary>
        /// Compiles and executes code from string.
        /// </summary>
        /// <param name="code"></param>
        public bool LoadCode(string code)
        {
            try
            {
                var source = _engine.CreateScriptSourceFromString(code);
                var compiled = source.Compile();
                compiled.Execute(scope);
                GetFunctions();
                return true;
            }
            catch (Exception e)
            {
                exception = e;
                update = null;
                fixedupdate = null;
                return false;
            }
        }

        /// <summary>
        /// Compiles and executes code from file.
        /// </summary>
        /// <param name="path"></param>
        public bool LoadScript(string path)
        {
            try
            {
                path = Internal.Scripter.Instance.ScriptOptions.FindScript(path);
                var source = _engine.CreateScriptSourceFromFile(path);
                var compiled = source.Compile();
                compiled.Execute(scope);
                GetFunctions();
                return true;
            }
            catch (Exception e)
            {
                exception = e;
                update = null;
                fixedupdate = null;
                return false;
            }
        }

        /// <summary>
        /// Calls python Update function.
        /// In case of exception, stops execution and returns false.
        /// </summary>
        /// <returns></returns>
        public bool CallUpdate()
        {
            try
            {
                update?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                exception = e;
                update = null;
                return false;
            }
        }

        /// <summary>
        /// Calls python Update function.
        /// In case of exception, stops execution and returns false.
        /// </summary>
        /// <returns></returns>
        public bool CallFixedUpdate()
        {
            try
            {
                update?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                exception = e;
                fixedupdate = null;
                return false;
            }
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

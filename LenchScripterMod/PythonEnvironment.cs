using System;
using System.Collections.Generic;
using UnityEngine;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using IronPython.Runtime;

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
        /// Returns search paths of the engine.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSearchPaths()
        {
            return (List<string>)_engine.GetSearchPaths();
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

        /// <summary>
        /// Initializes IronPython engine.
        /// </summary>
        public static void InitializeEngine()
        {
            _engine = Python.CreateEngine();

            // Add search paths
            ICollection<string> paths = _engine.GetSearchPaths();
            paths.Add(Application.dataPath + "/Scripts/");
            _engine.SetSearchPaths(paths);
        }

        /// <summary>
        /// Destroys IronPython engine.
        /// </summary>
        public static void DestroyEngine()
        {
            _engine?.Runtime.Shutdown();
            _engine = null;
        }

        /// <summary>
        /// Creates a new Python Environment and sets up the scope.
        /// </summary>
        public PythonEnvironment()
        {
            // Initialize engine
            if (_engine == null)
                InitializeEngine();

            // Initialize scope
            scope = _engine.CreateScope();

            // Set up environment
            _engine.Execute("import clr", scope);
            _engine.Execute("clr.AddReference(\"System\")", scope);
            _engine.Execute("clr.AddReference(\"UnityEngine\")", scope);
            _engine.Execute("from UnityEngine import Vector2, Vector3, Vector4, Mathf, Time, Input, KeyCode", scope);
            _engine.Execute("clr.AddReference(\"LenchScripterMod\")", scope);
            _engine.Execute("from LenchScripter import Functions as Besiege", scope);
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
        /// Evaluates Python expression and saves the result in an output parameter.
        /// Returns true if expression was executed with no errors.
        /// </summary>
        /// <param name="expression">Python expression.</param>
        /// <param name="result">Output variable.</param>
        /// <returns>Successfull execution.</returns>
        public bool Evaluate(string expression, out object result)
        {
            try
            {
                result = _engine.Execute(expression, scope);
                return true;
            }
            catch (Exception e)
            {
                exception = e;
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Evaluates Python expression.
        /// Returns true if expression wa executed with no errors.
        /// </summary>
        /// <param name="expression">Python expression.</param>
        /// <returns>Successfull execution.</returns>
        public bool Evaluate(string expression)
        {
            try
            {
                _engine.Execute(expression, scope);
                return true;
            }
            catch (Exception e)
            {
                exception = e;
                return false;
            }
        }

        private void GetFunctions()
        {
            scope.TryGetVariable("Update", out update);
            scope.TryGetVariable("FixedUpdate", out fixedupdate);
        }
    }
}

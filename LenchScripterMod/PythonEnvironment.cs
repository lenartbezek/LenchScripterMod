using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

// ReSharper disable UseMethodAny.2
// ReSharper disable PossibleNullReferenceException

namespace Lench.Scripter
{
    /// <summary>
    ///     Class handling Python environment.
    /// </summary>
    public class PythonEnvironment
    {
        internal static Assembly IronPythonAssembly;
        internal static Assembly MicrosoftScriptingAssembly;

        private static string _version = "ironpython2.7";

        private static Type _python;
        private static Type _scriptEngine;
        private static Type _scriptScope;
        private static Type _scriptRuntime;
        private static Type _scriptSource;
        private static Type _compiledCode;
        private static Type _exceptionOperations;
        private static MethodInfo _executeMethod;
        private static MethodInfo _getVariableMethod;
        private static MethodInfo _setVariableMethod;
        private static MethodInfo _containsVariableMethod;

        private static object _eo;
        private static object _engine;

        private static readonly List<string> InitStatements = new List<string>();
        private readonly object _scope;
        private Action _fixedupdate;

        private Action _update;

        /// <summary>
        ///     Creates a new Python Environment and sets up the scope.
        /// </summary>
        public PythonEnvironment()
        {
            // Initialize engine
            if (_engine == null)
                InitializeEngine();

            // Initialize scope
            _scope = _scriptEngine.GetMethods()
                .Single(method =>
                    method.Name == "CreateScope" &&
                    method.GetParameters().Count() == 0)
                .Invoke(_engine, null);

            // Set up environment
            Execute("import clr");
            Execute("clr.AddReference(\"System\")");
            Execute("clr.AddReference(\"UnityEngine\")");
            Execute("from UnityEngine import Vector2, Vector3, Vector4, Mathf, Time, Input, KeyCode, Color");

            Execute("clr.AddReference(\"LenchScripterMod\")");
            Execute("from Lench.Scripter import Functions as Besiege");

            // Execute additional init statements
            for (var i = 0; i < InitStatements.Count;)
                try
                {
                    Execute(InitStatements[i]);
                    i++;
                }
                catch
                {
                    InitStatements.RemoveAt(i);
                }

            // Redirect standard output
            Execute("import sys");
            SetVariable("pythonenv", this);
            Execute("sys.stdout = pythonenv");
            Execute("del pythonenv");
        }

        internal static string Version
        {
            get { return _version; }
            set
            {
                if (value == "ironpython2.7" || value == "ironpython3.0")
                    _version = value;
                else
                    throw new Exception(
                        "Invalid Python version. Supported values are 'ironpython2.7' and 'ironpython3.0'.");
            }
        }

        internal static string LibPath => Application.dataPath + "/Mods/Resources/LenchScripter/lib/" + Version + "/";

        /// <summary>
        ///     Are Python assemblies loaded and engine ready to be initialised.
        /// </summary>
        public static bool Loaded => !(IronPythonAssembly == null || MicrosoftScriptingAssembly == null);

        /// <summary>
        ///     Is script enabled to be ran on simulation start.
        /// </summary>
        public static bool Enabled { get; set; }

        /// <summary>
        ///     Returns PythonEnvironment instance currently used by the scripting mod.
        ///     Only instantiated during simulation.
        /// </summary>
        public static PythonEnvironment ScripterEnvironment { get; internal set; }

        /// <summary>
        ///     Loads Python assemblies.
        /// </summary>
        /// <returns>Returns true if successfull.</returns>
        public static bool LoadPythonAssembly()
        {
            try
            {
                IronPythonAssembly = Assembly.LoadFrom(LibPath + "IronPython.dll");
                MicrosoftScriptingAssembly = Assembly.LoadFrom(LibPath + "Microsoft.Scripting.dll");
                Assembly.LoadFrom(LibPath + "IronPython.Modules.dll");
                Assembly.LoadFrom(LibPath + "Microsoft.Scripting.Core.dll");
                Assembly.LoadFrom(LibPath + "Microsoft.Dynamic.dll");

                return true;
            }
            catch (FileNotFoundException e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        /// <summary>
        ///     Returns last occured exception in Python format.
        /// </summary>
        /// <returns></returns>
        public static string FormatException(Exception e)
        {
            if (_engine == null)
                throw new InvalidOperationException("Python engine not initialised.");
            if (_eo == null)
                _eo = _scriptEngine.GetMethods()
                    .Single(method =>
                        method.Name == "GetService" &&
                        method.IsGenericMethodDefinition)
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(_exceptionOperations)
                    .Invoke(_engine, new object[] {null});
            if (e.InnerException != null)
                e = e.InnerException;
            return
                (string)
                _exceptionOperations.GetMethod("FormatException", new[] {typeof(Exception)})
                    .Invoke(_eo, new object[] {e});
        }

        /// <summary>
        ///     Adds a statement to the list of initialisation statements
        ///     to be executed every time when creating an environment.
        /// </summary>
        /// <param name="s">Python expression.</param>
        public static void AddInitStatement(string s)
        {
            InitStatements.Add(s);
        }

        /// <summary>
        ///     Initializes IronPython engine.
        /// </summary>
        public static void InitializeEngine()
        {
            if (IronPythonAssembly == null || MicrosoftScriptingAssembly == null)
                throw new InvalidOperationException("IronPython assemblies not loaded. Script engine not available.");

            _python = IronPythonAssembly.GetType("IronPython.Hosting.Python");
            _scriptEngine = MicrosoftScriptingAssembly.GetType("Microsoft.Scripting.Hosting.ScriptEngine");
            _scriptScope = MicrosoftScriptingAssembly.GetType("Microsoft.Scripting.Hosting.ScriptScope");
            _scriptRuntime = MicrosoftScriptingAssembly.GetType("Microsoft.Scripting.Hosting.ScriptRuntime");
            _scriptSource = MicrosoftScriptingAssembly.GetType("Microsoft.Scripting.Hosting.ScriptSource");
            _compiledCode = MicrosoftScriptingAssembly.GetType("Microsoft.Scripting.Hosting.CompiledCode");
            _exceptionOperations = MicrosoftScriptingAssembly.GetType("Microsoft.Scripting.Hosting.ExceptionOperations");

            _executeMethod = _scriptEngine.GetMethods()
                .Single(method =>
                    method.Name == "Execute" && !method.IsGenericMethod &&
                    method.GetParameters().Count() == 2 &&
                    method.GetParameters()[0].ParameterType == typeof(string) &&
                    method.GetParameters()[1].ParameterType == _scriptScope);
            _getVariableMethod = _scriptScope.GetMethod("GetVariable", new[] {typeof(string)});
            _setVariableMethod = _scriptScope.GetMethod("SetVariable", new[] {typeof(string), typeof(object)});
            _containsVariableMethod = _scriptScope.GetMethod("ContainsVariable", new[] {typeof(string)});

            _engine = _python.GetMethods()
                .Single(method =>
                    method.Name == "CreateEngine" &&
                    method.GetParameters().Count() == 0)
                .Invoke(null, null);

            // Add search path
            var paths = _scriptEngine.GetMethod("GetSearchPaths").Invoke(_engine, null) as ICollection<string>;
            paths.Add(Application.dataPath + "/Scripts/");
            _scriptEngine.GetMethod("SetSearchPaths").Invoke(_engine, new object[] {paths});
        }

        /// <summary>
        ///     Destroys IronPython engine.
        /// </summary>
        public static void DestroyEngine()
        {
            if (_engine != null)
            {
                var runtime = _scriptEngine.GetProperty("Runtime").GetValue(_engine, null);
                _scriptRuntime.GetMethod("Shutdown").Invoke(runtime, null);
            }
            _engine = null;
            _eo = null;
        }

        /// <summary>
        ///     Wrapper for ScriptScope.GetVariableNames()
        /// </summary>
        /// <returns>Returns a list of all variable names in scope.</returns>
        public IEnumerable<string> GetVariableNames()
        {
            return (IEnumerable<string>) _scriptScope.GetMethods()
                .Single(m => m.Name == "GetVariableNames")
                .Invoke(_scope, null);
        }

        /// <summary>
        ///     Returns true if the scope contains variable with name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsVariable(string name)
        {
            return (bool) _containsVariableMethod.Invoke(_scope, new object[] {name});
        }

        /// <summary>
        ///     Returns variable with given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetVariable(string name)
        {
            return _getVariableMethod.Invoke(_scope, new object[] {name});
        }

        /// <summary>
        ///     Returns variable with given name of type T.
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        /// <returns>Object of type T.</returns>
        public T GetVariable<T>(string name)
        {
            var method = _scriptScope.GetMethods()
                .Single(m =>
                    m.Name == "GetVariable" &&
                    m.IsGenericMethod &&
                    m.GetParameters().Count() == 1 &&
                    m.GetParameters()[0].ParameterType == typeof(string))
                .MakeGenericMethod(typeof(T));

            return (T) method.Invoke(_scope, new object[] {name});
        }

        /// <summary>
        ///     Sets the value of the variable with given name in current scope.
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        /// <param name="value">Value of the variable.</param>
        public void SetVariable(string name, object value)
        {
            _setVariableMethod.Invoke(_scope, new[] {name, value});
        }

        /// <summary>
        ///     Compiles and executes code from string.
        /// </summary>
        /// <param name="code">Complete Python script.</param>
        public void LoadCode(string code)
        {
            var source = _scriptEngine.GetMethod("CreateScriptSourceFromString", new[] {typeof(string)})
                .Invoke(_engine, new object[] {code});
            var compiled = _scriptSource.GetMethods().
                FirstOrDefault(method =>
                    method.Name == "Compile" &&
                    method.GetParameters().Count() == 0)
                .Invoke(source, null);
            _compiledCode.GetMethods()
                .FirstOrDefault(method =>
                    method.Name == "Execute" &&
                    method.GetParameters().Count() == 1 &&
                    method.GetParameters()[0].ParameterType == _scriptScope)
                .Invoke(compiled, new[] {_scope});
            GetFunctions();
        }

        /// <summary>
        ///     Compiles and executes code from file.
        /// </summary>
        /// <param name="path">Path to a Python script.</param>
        public void LoadScript(string path)
        {
            var source = _scriptEngine.GetMethod("CreateScriptSourceFromFile", new[] {typeof(string)})
                .Invoke(_engine, new object[] {path});
            var compiled = _scriptSource.GetMethods()
                .FirstOrDefault(method =>
                    method.Name == "Compile" &&
                    method.GetParameters().Count() == 0)
                .Invoke(source, null);
            _compiledCode.GetMethods()
                .FirstOrDefault(method =>
                    method.Name == "Execute" &&
                    method.GetParameters().Count() == 1 &&
                    method.GetParameters()[0].ParameterType == _scriptScope)
                .Invoke(compiled, new[] {_scope});
            GetFunctions();
        }

        /// <summary>
        ///     Compiles code into a function.
        /// </summary>
        /// <param name="code">Python code.</param>
        /// <returns>Function that returns</returns>
        public Func<object> Compile(string code)
        {
            var source = _scriptEngine.GetMethod("CreateScriptSourceFromString", new[] {typeof(string)})
                .Invoke(_engine, new object[] {code});
            var compiled = _scriptSource.GetMethods().
                FirstOrDefault(method =>
                    method.Name == "Compile" &&
                    method.GetParameters().Count() == 0)
                .Invoke(source, null);
            var execute = _compiledCode.GetMethods()
                .FirstOrDefault(method =>
                    method.Name == "Execute" &&
                    method.GetParameters().Count() == 1 &&
                    method.GetParameters()[0].ParameterType == _scriptScope);
            return () => execute.Invoke(compiled, new[] {_scope});
        }

        /// <summary>
        ///     Calls Python Update function.
        ///     Does nothing if currently loaded script has no Update function.
        /// </summary>
        public void CallUpdate()
        {
            _update?.Invoke();
        }

        /// <summary>
        ///     Calls Python FixedUpdate function.
        ///     Does nothing if currently loaded script has no FixedUpdate function.
        ///     In case of exception stops execution and returns false.
        /// </summary>
        /// <returns></returns>
        public void CallFixedUpdate()
        {
            _fixedupdate?.Invoke();
        }

        /// <summary>
        ///     Evaluates Python expression and saves the result in an output parameter.
        ///     Returns true if expression was executed with no errors.
        /// </summary>
        /// <param name="expression">Python expression.</param>
        /// <returns>Successfull execution.</returns>
        public object Execute(string expression)
        {
            return _executeMethod.Invoke(_engine, new[] {expression, _scope});
        }

        private void GetFunctions()
        {
            if (ContainsVariable("Update"))
                _update = GetVariable<Action>("Update");

            if (ContainsVariable("FixedUpdate"))
                _fixedupdate = GetVariable<Action>("FixedUpdate");
        }

        /// <summary>
        ///     Used by the Python engine as standard output;
        /// </summary>
        /// <param name="s">Message to be sent.</param>
        // ReSharper disable once InconsistentNaming
        public void write(object s)
        {
            if (s.ToString().Trim().Length != 0)
                Debug.Log(s.ToString().TrimEnd());
        }
    }

    internal class Proxy : MarshalByRefObject
    {
        public Assembly Load(string assemblyPath)
        {
            return Assembly.LoadFile(assemblyPath);
        }
    }
}
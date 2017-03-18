using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
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
        internal static Assembly MicrosoftDynamicAssembly;

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
        private readonly object _scope;

        internal static object Engine;

        /// <summary>
        ///     Creates a new Python Environment and sets up the scope.
        /// </summary>
        public PythonEnvironment(object scope = null, bool redirectOutput = true)
        {
            // Initialize engine
            if (Engine == null)
                InitializeEngine();

            // Initialize scope
            if (scope == null)
            {
                _scope = _scriptEngine.GetMethods()
                    .Single(method =>
                        method.Name == "CreateScope" &&
                        method.GetParameters().Count() == 0)
                    .Invoke(Engine, null);
            }
            else
            {
                _scope = scope;
            }

            // Set up environment
            Execute("import sys");
            Execute("sys.modules.clear()");
            Execute("import clr");
            Execute("clr.AddReference(\"System\")");
            Execute("clr.AddReference(\"UnityEngine\")");
            Execute("from UnityEngine import Vector2, Vector3, Vector4, Mathf, Time, Input, KeyCode, Color");

            Execute("clr.AddReference(\"LenchScripterMod\")");
            Execute("from Lench.Scripter import Functions as Besiege");

            // Redirect standard output
            if (redirectOutput)
            {
                this["pythonenv"] = this;
                Execute("sys.stdout = pythonenv");
                Execute("del pythonenv");
            }

            OnInitialization?.Invoke(this);
        }

        /// <summary>
        ///     Invoked on scope creation. Use this to import modules into default script scope.
        ///     Called for every PythonEnvironment instance. PythonEnvironment is passed to the delegate as an argument.
        /// </summary>
        public static event Action<PythonEnvironment> OnInitialization;

        /// <summary>
        ///     Wrapper for `OnInitialization += python => python.Execute(expression);` to simplify reflection calls.
        /// </summary>
        /// <param name="expression">Python expression.</param>
        [Obsolete("If you see this, you should probably be using the OnInitialization event instead.")]
        public static void AddInitStatement([NotNull] string expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            OnInitialization += python =>
            {
                try
                {
                    python.Execute(expression);
                }
                catch (Exception e)
                {
                    Debug.Log($"<b><color=#FF0000>Python error during initialization statement:\n{e.Message}</color></b>\n{FormatException(e)}");
                }
            };
        }

        /// <summary>
        ///     Currently selected ironpython version. Can only be "ironpython2.7" or "ironpython3.0".
        ///     Engine needs to be reloaded after changing this.
        /// </summary>
        public static string Version
        {
            get { return _version; }
            set
            {
                if (value != "ironpython2.7" && value != "ironpython3.0")
                    throw new Exception("Invalid Python version. Supported values are 'ironpython2.7' and 'ironpython3.0'.");

                _version = value;
            }
        }

        /// <summary>
        ///     Points to the directory where IronPython assemblies are located.
        /// </summary>
        public static string LibPath => $"{Application.dataPath}/Mods/Resources/LenchScripter/lib/{Version}/";

        /// <summary>
        ///     Are Python assemblies loaded and engine ready to be initialised.
        /// </summary>
        public static bool Loaded =>
            IronPythonAssembly != null &&
            MicrosoftScriptingAssembly != null &&
            MicrosoftDynamicAssembly != null;

        /// <summary>
        ///     Is script enabled to be ran on simulation start.
        /// </summary>
        public static bool Enabled { get; set; }

        /// <summary>
        ///     Loads Python assemblies.
        /// </summary>
        /// <returns>Returns true if successfull.</returns>
        public static bool LoadPythonAssembly()
        {
            try
            {
                IronPythonAssembly = Assembly.LoadFile(LibPath + "IronPython.dll");
                MicrosoftScriptingAssembly = Assembly.LoadFile(LibPath + "Microsoft.Scripting.dll");
                MicrosoftDynamicAssembly = Assembly.LoadFile(LibPath + "Microsoft.Dynamic.dll");
                Assembly.LoadFile(LibPath + "IronPython.Modules.dll");
                Assembly.LoadFile(LibPath + "Microsoft.Scripting.Core.dll");

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Returns last occured exception in Python format.
        /// </summary>
        /// <returns></returns>
        public static string FormatException(Exception e)
        {
            if (Engine == null)
                throw new InvalidOperationException("Python engine not initialised.");
            if (_eo == null)
                _eo = _scriptEngine.GetMethods()
                    .Single(method =>
                        method.Name == "GetService" &&
                        method.IsGenericMethodDefinition)
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(_exceptionOperations)
                    .Invoke(Engine, new object[] {null});
            if (e.InnerException != null)
                e = e.InnerException;
            return
                (string)
                _exceptionOperations.GetMethod("FormatException", new[] {typeof(Exception)})
                    .Invoke(_eo, new object[] {e});
        }

        /// <summary>
        ///     Initializes IronPython engine.
        /// </summary>
        public static void InitializeEngine()
        {
            if (!Loaded)
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

            Engine = _python.GetMethods()
                .Single(method =>
                    method.Name == "CreateEngine" &&
                    method.GetParameters().Count() == 0)
                .Invoke(null, null);

            // Add search path
            var paths = _scriptEngine.GetMethod("GetSearchPaths").Invoke(Engine, null) as ICollection<string>;
            paths.Add(Application.dataPath + "/Scripts/");
            _scriptEngine.GetMethod("SetSearchPaths").Invoke(Engine, new object[] {paths});
        }

        /// <summary>
        ///     Destroys IronPython engine.
        /// </summary>
        public static void DestroyEngine()
        {
            if (Engine != null)
            {
                var runtime = _scriptEngine.GetProperty("Runtime").GetValue(Engine, null);
                _scriptRuntime.GetMethod("Shutdown").Invoke(runtime, null);
            }
            Engine = null;
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
        public bool Contains(string name)
        {
            return (bool) _containsVariableMethod.Invoke(_scope, new object[] {name});
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

            return (T)method.Invoke(_scope, new object[] { name });
        }

        /// <summary>
        ///     Access global variables by their name.
        /// </summary>
        /// <param name="key">Variable name</param>
        public object this[string key]
        {
            get
            {
                return _getVariableMethod.Invoke(_scope, new object[] { key });
            }
            set
            {
                _setVariableMethod.Invoke(_scope, new[] { key, value });
            }
        }

        /// <summary>
        ///     Compiles and executes code from string.
        /// </summary>
        /// <param name="code">Complete Python script.</param>
        public void LoadCode(string code)
        {
            var source = _scriptEngine.GetMethod("CreateScriptSourceFromString", new[] {typeof(string)})
                .Invoke(Engine, new object[] {code});
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
        }

        /// <summary>
        ///     Compiles and executes code from file.
        /// </summary>
        /// <param name="path">Path to a Python script.</param>
        public void LoadScript(string path)
        {
            var source = _scriptEngine.GetMethod("CreateScriptSourceFromFile", new[] {typeof(string)})
                .Invoke(Engine, new object[] {path});
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
        }

        /// <summary>
        ///     Compiles code into a function.
        /// </summary>
        /// <param name="code">Python code.</param>
        /// <returns>Function that returns</returns>
        public Func<object> Compile(string code)
        {
            var source = _scriptEngine.GetMethod("CreateScriptSourceFromString", new[] {typeof(string)})
                .Invoke(Engine, new object[] {code});
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
        ///     Evaluates Python expression and returns result.
        /// </summary>
        /// <param name="expression">Python expression.</param>
        /// <returns>Value of the expression.</returns>
        public object Execute(string expression)
        {
            return _executeMethod.Invoke(Engine, new[] {expression, _scope});
        }

        /// <summary>
        ///     Used by the Python engine as standard output.
        ///     Not intended to be called.
        /// </summary>
        /// <param name="s">Message to be sent.</param>
        // ReSharper disable once InconsistentNaming
        public void write(object s)
        {
            if (s.ToString().Trim().Length != 0)
                Debug.Log(s.ToString().TrimEnd());
        }
    }
}
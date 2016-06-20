using LenchScripter.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LenchScripter
{
    /// <summary>
    /// Class handling Python environment.
    /// </summary>
    public class PythonEnvironment
    {
        internal static Assembly ironPythonAssembly;
        internal static Assembly microsoftScriptingAssembly;

        private static Type python;
        private static Type scriptEngine;
        private static Type scriptScope;
        private static Type scriptRuntime;
        private static Type scriptSource;
        private static Type compiledCode;
        private static Type exceptionOperations;
        private static MethodInfo executeMethod;
        private static MethodInfo getVariableMethod;
        private static MethodInfo setVariableMethod;
        private static MethodInfo containsVariableMethod;

        private static object _eo;
        private static object _engine;
        private object scope;

        private Action update;
        private Action fixedupdate;

        private static List<string> init_statements = new List<string>();

        /// <summary>
        /// Are Python assemblies loaded and engine ready to be initialised.
        /// </summary>
        public static bool Loaded { get { return !(ironPythonAssembly == null || microsoftScriptingAssembly == null); } }

        /// <summary>
        /// Is script enabled to be ran on simulation start.
        /// </summary>
        public static bool Enabled { get { return Scripter.Instance.enableScript; } }

        /// <summary>
        /// Returns PythonEnvironment instance currently used by the scripting mod.
        /// Only instantiated during simulation.
        /// </summary>
        public static PythonEnvironment ScripterEnvironment
        {
            get { return Scripter.Instance.python; }
        }

        /// <summary>
        /// Returns last occured exception in Python format.
        /// </summary>
        /// <returns></returns>
        public static string FormatException(Exception e)
        {
            if (_engine == null)
                throw new InvalidOperationException("Python engine not initialised.");
            if (_eo == null)
                _eo = scriptEngine.GetMethods()
                    .Single(method =>
                        method.Name == "GetService" &&
                        method.IsGenericMethodDefinition)
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(exceptionOperations)
                    .Invoke(_engine, new object[] { null });
            if (e.InnerException != null)
                e = e.InnerException;
            return (string)exceptionOperations.GetMethod("FormatException", new[] { typeof(Exception) }).Invoke(_eo, new[] { e });
        }

        /// <summary>
        /// Adds a statement to the list of initialisation statements
        /// to be executed every time when creating an environment.
        /// </summary>
        /// <param name="s">Python expression.</param>
        public static void AddInitStatement(string s)
        {
            init_statements.Add(s);
        }

        /// <summary>
        /// Initializes IronPython engine.
        /// </summary>
        public static void InitializeEngine()
        {
            if (ironPythonAssembly == null || microsoftScriptingAssembly == null)
                throw new InvalidOperationException("IronPython assemblies not loaded. Script engine not available.");

            python = ironPythonAssembly.GetType("IronPython.Hosting.Python");
            scriptEngine = microsoftScriptingAssembly.GetType("Microsoft.Scripting.Hosting.ScriptEngine");
            scriptScope = microsoftScriptingAssembly.GetType("Microsoft.Scripting.Hosting.ScriptScope");
            scriptRuntime = microsoftScriptingAssembly.GetType("Microsoft.Scripting.Hosting.ScriptRuntime");
            scriptSource = microsoftScriptingAssembly.GetType("Microsoft.Scripting.Hosting.ScriptSource");
            compiledCode = microsoftScriptingAssembly.GetType("Microsoft.Scripting.Hosting.CompiledCode");
            exceptionOperations = microsoftScriptingAssembly.GetType("Microsoft.Scripting.Hosting.ExceptionOperations");

            executeMethod = scriptEngine.GetMethods()
                .Single(method => 
                    method.Name == "Execute" && !method.IsGenericMethod &&
                    method.GetParameters().Count() == 2 &&
                    method.GetParameters()[0].ParameterType == typeof(string) &&
                    method.GetParameters()[1].ParameterType == scriptScope);
            getVariableMethod = scriptScope.GetMethod("GetVariable", new[] { typeof(string) });
            setVariableMethod = scriptScope.GetMethod("SetVariable", new[] { typeof(string), typeof(object) });
            containsVariableMethod = scriptScope.GetMethod("ContainsVariable", new[] { typeof(string) });
            
            _engine = python.GetMethods()
                .Single(method => 
                        method.Name == "CreateEngine" &&
                        method.GetParameters().Count() == 0)
                .Invoke(null, null);

            // Add search path
            var paths = scriptEngine.GetMethod("GetSearchPaths").Invoke(_engine, null) as ICollection<string>;
            paths.Add(Application.dataPath + "/Scripts/");
            scriptEngine.GetMethod("SetSearchPaths").Invoke(_engine, new [] {paths});
        }

        /// <summary>
        /// Destroys IronPython engine.
        /// </summary>
        public static void DestroyEngine()
        {
            if (_engine != null)
            {
                var runtime = scriptEngine.GetProperty("Runtime").GetValue(_engine, null);
                scriptRuntime.GetMethod("Shutdown").Invoke(runtime, null);
            }
            _engine = null;
            _eo = null;
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
            scope = scriptEngine.GetMethods()
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
            Execute("from LenchScripter import Functions as Besiege");

            // Redirect standard output
            Execute("import sys");
            SetVariable("pythonenv", this);
            Execute("sys.stdout = pythonenv");
            SetVariable("pythonenv", null);

            // Run additional init statements
            foreach (string s in init_statements)
                Execute(s);
        }

        /// <summary>
        /// Returns true if the scope contains variable with name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool ContainsVariable(string name)
        {
            return (bool)containsVariableMethod.Invoke(scope, new [] { name });
        }

        /// <summary>
        /// Returns variable with given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object GetVariable(string name)
        {
            return getVariableMethod.Invoke(scope, new [] { name });
        }

        /// <summary>
        /// Returns variable with given name of type T.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetVariable<T>(string name)
        {
            var method = scriptScope.GetMethods()
                .Single(m =>
                    m.Name == "GetVariable" &&
                    m.IsGenericMethod &&
                    m.GetParameters().Count() == 1 &&
                    m.GetParameters()[0].ParameterType == typeof(string))
                .MakeGenericMethod(typeof(T));

            return (T)method.Invoke(scope, new[] { name });
        }

        /// <summary>
        /// Sets the value of the variable with given name in current scope.
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        /// <param name="value">Value of the variable.</param>
        public void SetVariable(string name, object value)
        {
            setVariableMethod.Invoke(scope, new[] { name, value });
        }

        /// <summary>
        /// Compiles and executes code from string.
        /// </summary>
        /// <param name="code">Complete Python script.</param>
        public void LoadCode(string code)
        {
            var source = scriptEngine.GetMethod("CreateScriptSourceFromString", new[] { typeof(string) }).Invoke(_engine, new [] { code });
            var compiled = scriptSource.GetMethods().
                FirstOrDefault(method =>
                    method.Name == "Compile" &&
                    method.GetParameters().Count() == 0)
                .Invoke(source, null);
            compiledCode.GetMethods()
                .FirstOrDefault(method =>
                    method.Name == "Execute" &&
                    method.GetParameters().Count() == 1 &&
                    method.GetParameters()[0].ParameterType == scriptScope)
                .Invoke(compiled, new[] { scope });
            GetFunctions();
        }

        /// <summary>
        /// Compiles and executes code from file.
        /// </summary>
        /// <param name="path">Path to a Python script.</param>
        public void LoadScript(string path)
        {
            var source = scriptEngine.GetMethod("CreateScriptSourceFromFile", new[] { typeof(string) }).Invoke(_engine, new[] { path });
            var compiled = scriptSource.GetMethods()
                .FirstOrDefault(method => 
                    method.Name == "Compile" &&
                    method.GetParameters().Count() == 0)
                .Invoke(source, null);
            compiledCode.GetMethods()
                .FirstOrDefault(method => 
                    method.Name == "Execute" &&
                    method.GetParameters().Count() == 1 &&
                    method.GetParameters()[0].ParameterType == scriptScope)
                .Invoke(compiled, new[] { scope });
            GetFunctions();
        }

        /// <summary>
        /// Compiles code into a function.
        /// </summary>
        /// <param name="code">Python code.</param>
        /// <returns>Function that returns</returns>
        public Func<object> Compile(string code)
        {
            var source = scriptEngine.GetMethod("CreateScriptSourceFromString", new[] { typeof(string) }).Invoke(_engine, new[] { code });
            var compiled = scriptSource.GetMethods().
                FirstOrDefault(method =>
                    method.Name == "Compile" &&
                    method.GetParameters().Count() == 0)
                .Invoke(source, null);
            var execute = compiledCode.GetMethods()
                .FirstOrDefault(method =>
                    method.Name == "Execute" &&
                    method.GetParameters().Count() == 1 &&
                    method.GetParameters()[0].ParameterType == scriptScope);
            return () => execute.Invoke(compiled, new[] { scope });
        }

        /// <summary>
        /// Calls Python Update function.
        /// Does nothing if currently loaded script has no Update function.
        /// </summary>
        public void CallUpdate()
        {
            update?.Invoke();
        }

        /// <summary>
        /// Calls Python FixedUpdate function.
        /// Does nothing if currently loaded script has no FixedUpdate function.
        /// In case of exception stops execution and returns false.
        /// </summary>
        /// <returns></returns>
        public void CallFixedUpdate()
        {
            fixedupdate?.Invoke();
        }

        /// <summary>
        /// Evaluates Python expression and saves the result in an output parameter.
        /// Returns true if expression was executed with no errors.
        /// </summary>
        /// <param name="expression">Python expression.</param>
        /// <returns>Successfull execution.</returns>
        public object Execute(string expression)
        {
            return executeMethod.Invoke(_engine, new [] { expression, scope });
        }

        private void GetFunctions()
        {
            if (ContainsVariable("Update"))
                update = GetVariable<Action>("Update");

            if (ContainsVariable("FixedUpdate"))
                fixedupdate = GetVariable<Action>("FixedUpdate");
        }

        /// <summary>
        /// Used by the Python engine as standard output;
        /// </summary>
        /// <param name="s">Message to be sent.</param>
        public void write(object s)
        {
            if (s.ToString().Trim().Length != 0)
                Debug.Log(s.ToString().TrimEnd());
        }
    }
}

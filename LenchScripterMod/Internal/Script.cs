using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using spaar.ModLoader;
using UnityEngine;
#pragma warning disable 649

// ReSharper disable UnusedMember.Local

namespace Lench.Scripter.Internal
{
    /// <summary>
    ///     Script controller.
    /// </summary>
    internal static class Script
    {
        public enum SourceType
        {
            None = 0,
            Bsg = 1,
            Py = 2
        }

        // Python environment
        public static PythonEnvironment Python;
        private static Action _update;
        private static Action _fixedUpdate;

        public static event Action OnStart;
        public static event Action OnStop;
        public static event Action OnError;

        private static ScriptComponent _component;

        private static FileSystemWatcher[] _watchers;
        private static string[] _paths;

        /// <summary>
        ///     Is script execution enabled.
        /// </summary>
        public static bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (StatMaster.isSimulating && value && PythonEnvironment.Loaded)
                    Start();
                else
                    Stop();
                _enabled = value;
            }
        }
        private static bool _enabled;

        // Script options
        public static string FileName
        {
            get { return _name; }
            set
            {
                if (_name == value) return;
                _name = value;
                StartWatchers(_name);
                SetSource();
            }
        }
        private static string _name = "";

        public static string FilePath => _paths?.FirstOrDefault(s => !string.IsNullOrEmpty(s));
        public static string EmbeddedCode { get; set; }
        public static bool SaveToBsg { get; set; }
        public static SourceType Source { get; set; } = SourceType.None;

        /// <summary>
        ///     Looks for script file and sets source. Called on machine load.
        /// </summary>
        public static void SetSource()
        {
            if (FilePath != null)
                Source = SourceType.Py;
            else if (EmbeddedCode != null)
                Source = SourceType.Bsg;
            else
                Source = SourceType.None;
        }

        /// <summary>
        ///     Loads mod's scripting features.
        ///     Returns true if successful.
        /// </summary>
        public static bool LoadEngine(bool verbose = false)
        {
            if (PythonEnvironment.LoadPythonAssembly())
            {
                PythonEnvironment.InitializeEngine();
                CreateScriptingEnvironment();
                _component = Mod.Controller.AddComponent<ScriptComponent>();

                if (verbose)
                    ModConsole.AddMessage(LogType.Log,
                        $"[LenchScripterMod]: {Python.Execute("sys.version")}");

                Mod.LoadedScripter = true;
                return true;
            }
            Mod.LoadedScripter = false;
            return false;
        }

        private static void CreateScriptingEnvironment()
        {
            if (!Mod.LoadedScripter) return;

            Python = new PythonEnvironment();
        }

        private static void DestroyScriptingEnvironment()
        {
            Python = null;
        }

        /// <summary>
        ///     Saves machine data code to .py file.
        /// </summary>
        public static void Export()
        {
            if (EmbeddedCode == null)
                throw new Exception("This machine contains no code to be exported.");

            var path = FileName.EndsWith(".py") ? FileName : FileName + ".py";
            path = string.Concat(Application.dataPath, "/Scripts/", path);
            File.WriteAllText(path, EmbeddedCode);
        }

        /// <summary>
        ///     Start watchers that attempt to find the script file.
        /// </summary>
        /// <param name="name">Script search pattern.</param>
        public static void StartWatchers([NotNull] string name)
        {
            if (_watchers != null)
                foreach (var w in _watchers)
                    w?.Dispose();

            var paths = new[]
            {
                name,
                string.Concat(Application.dataPath, "/Scripts/", name, ".py"),
                string.Concat(Application.dataPath, "/Scripts/", name),
                string.Concat(name, ".py")
            };

            _watchers = new FileSystemWatcher[paths.Length];
            _paths = new string[paths.Length];

            for (var i = 0; i < _watchers.Length; i++)
            {
                try
                {
                    var dir = Path.GetDirectoryName(paths[i]);
                    var file = Path.GetFileName(paths[i]);
                    _watchers[i] = new FileSystemWatcher(dir, file);
                }
                catch
                {
                    continue;
                }
                finally
                {
                    _watchers[i]?.Dispose();
                }
                
                var index = i; // copy to local var

                Action handleCreate = delegate
                {
#if DEBUG
                    Debug.Log($"File {paths[index]} found.");
#endif
                    _paths[index] = paths[index];
                    SetSource();
                };
                Action handleDelete = delegate
                {
#if DEBUG
                    Debug.Log($"File {paths[index]} lost.");
#endif
                    _paths[index] = null;
                    SetSource();
                };

                if (File.Exists(paths[index])) handleCreate.Invoke();

                _watchers[index].EnableRaisingEvents = true;
                _watchers[index].Created += delegate { handleCreate(); };
                _watchers[index].Renamed += delegate { handleDelete(); };
                _watchers[index].Deleted += delegate { handleDelete(); };

                _watchers[index].BeginInit();
            }
        }

        public static void Start()
        {
            try
            {
                switch (Source)
                {
                    case SourceType.Py:
                        Python.LoadScript(FilePath);
                        break;
                    case SourceType.Bsg:
                        Python.LoadCode(EmbeddedCode);
                        break;
                    default:
                        return;
                }
                _update = Python.GetVariable<Action>("Update");
                _fixedUpdate = Python.GetVariable<Action>("FixedUpdate");
                _component.enabled = true;
                OnStart?.Invoke();
            }
            catch (Exception e)
            {
                Error(e);
            }
        }

        public static void Stop()
        {
            _component.enabled = false;
            OnStop?.Invoke();
        }

        public static void Error(Exception e)
        {
            _component.enabled = false;
            Debug.Log($"<b><color=#FF0000>Python error: {e.Message}</color></b>\n{PythonEnvironment.FormatException(e)}");
            OnError?.Invoke();
        }

        internal static void OnSimulationToggle(bool isSimulating)
        {
            if (!Enabled || !PythonEnvironment.Loaded) return;

            DestroyScriptingEnvironment();
            CreateScriptingEnvironment();

            if (isSimulating) Start();
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class ScriptComponent : MonoBehaviour
        {
            private void Update()
            {
                if (!StatMaster.isSimulating) return;

                // Call script update.
                try
                {
                    _update?.Invoke();
                }
                catch (Exception e)
                {
                    Error(e);
                }
            }

            private void FixedUpdate()
            {
                if (!StatMaster.isSimulating) return;

                // Call script update.
                try
                {
                    _fixedUpdate?.Invoke();
                }
                catch (Exception e)
                {
                    Error(e);
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using spaar.ModLoader;
using spaar.ModLoader.UI;

namespace Lench.Scripter.Internal
{
    internal class ScriptOptions : SingleInstance<ScriptOptions>
    {
        public override string Name { get { return "ScriptOptions"; } }

        internal Vector2 ConfigurationPosition;

        internal bool Visible { get; set; } = false;
        internal string ScriptName { get; set; } = "";
        internal string ScriptPath { get; set; } = "";
        internal string ScriptSource { get; set; } = "none";
        internal bool ScriptFound { get; set; } = false;
        internal bool SaveToBsg { get; set; } = false;
        internal bool BsgHasCode { get; set; } = false;
        internal string Code { get; set; }

        internal string SuccessMessage { get; set; }
        internal string NoteMessage { get; set; }
        internal string ErrorMessage { get; set; }

        private float timer = 0;
        private bool init = false;

        private int windowID = Util.GetWindowID();
        private Rect windowRect;

        /// <summary>
        /// Render window.
        /// </summary>
        private void OnGUI()
        {
            if (Visible)
            {
                InitialiseWindowRect();

                GUI.skin = ModGUI.Skin;
                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                GUI.skin.window.padding.left = 8;
                GUI.skin.window.padding.right = 8;
                GUI.skin.window.padding.bottom = 8;
                windowRect = GUILayout.Window(windowID, windowRect, DoWindow, "Script Options",
                    GUILayout.Height(200),
                    GUILayout.Width(320));

                ConfigurationPosition.x = windowRect.x < Screen.width / 2 ? windowRect.x : windowRect.x - Screen.width;
                ConfigurationPosition.y = windowRect.y < Screen.height / 2 ? windowRect.y : windowRect.y - Screen.height;
            }
        }

        /// <summary>
        /// Checks for script file every second.
        /// </summary>
        private void Update()
        {
            timer += Time.deltaTime;
            if (timer > 1)
            {
                timer -= 1;
                CheckForScript();
            }
        }

        /// <summary>
        /// Checks for script and sets properties.
        /// </summary>
        internal void CheckForScript(string path = null)
        {
            if (path != null) ScriptName = path;
            try
            {
                ScriptPath = FindScript(ScriptName);
                ScriptFound = true;
            }
            catch (FileNotFoundException)
            {
                ScriptFound = false;
            }
            if (ScriptFound && ScriptSource == "none")
                ScriptSource = "py";
            if (!ScriptFound)
                ScriptSource = BsgHasCode ? "bsg" : "none";
        }

        /// <summary>
        /// Saves machine data code to lua script.
        /// </summary>
        internal void SaveToScript()
        {
            if (!BsgHasCode)
            {
                ErrorMessage = ".bsg file contains no code to be exported.";
                return;
            }
            try
            {
                var path = ScriptName.EndsWith(".py") ? ScriptName : ScriptName + ".py";
                path = string.Concat(Application.dataPath, "/Scripts/", path);
                File.WriteAllText(path, Code);
                SuccessMessage = "Successfully wrote code to\n" + path;
            }
            catch (Exception e)
            {
                ErrorMessage = "Error writing code to script.\nSee console (Ctrl+K) for more info.";
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Attempts to find the script file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal string FindScript(string path)
        {
            List<string> possibleFiles = new List<string>()
            {
                path,
                string.Concat(Application.dataPath, "/Scripts/", path, ".py"),
                string.Concat(Application.dataPath, "/Scripts/", path),
                string.Concat(path, ".py")
            };

            foreach (string p in possibleFiles)
            {
                if (File.Exists(p))
                    return p;
            }
            throw new FileNotFoundException("Script file not found: " + path);
        }

        /// <summary>
        /// Initialises main window Rect on first call.
        /// Intended to set the position from the configuration.
        /// </summary>
        private void InitialiseWindowRect()
        {
            if (init) return;

            windowRect = new Rect();
            windowRect.width = 320;
            windowRect.height = 200;
            if (ConfigurationPosition != null)
            {
                windowRect.x = ConfigurationPosition.x >= 0 ? ConfigurationPosition.x : Screen.width + ConfigurationPosition.x;
                windowRect.y = ConfigurationPosition.y >= 0 ? ConfigurationPosition.y : Screen.height + ConfigurationPosition.y;
            }
            else
            {
                windowRect.x = Screen.width - windowRect.width - 60;
                windowRect.y = Screen.height - windowRect.height - 120;
            }

            init = true;
        }

        private void DrawEnabledBadge(bool enabled)
        {
            if (enabled)
            {
                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0f, 1f, 0f, 1f);
                GUILayout.Label("✓", Elements.InputFields.Default, GUILayout.Width(30));
                GUI.backgroundColor = oldColor;
            }
            else
            {
                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1f, 0f, 0f, 1f);
                GUILayout.Label("✘", Elements.InputFields.Default, GUILayout.Width(30));
                GUI.backgroundColor = oldColor;
            }
        }

        private void DoWindow(int id)
        {
            // Draw script file text field
            GUILayout.Label("Script file", Elements.Labels.Title);
            GUILayout.BeginHorizontal();
            ScriptName = GUILayout.TextField(ScriptName);
            DrawEnabledBadge(ScriptFound);
            GUILayout.EndHorizontal();

            // Draw open folder button
            if (GUILayout.Button("Open Scripts folder", Elements.Buttons.ComponentField))
            {
                string dir = Application.dataPath + "/Scripts/";
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                Application.OpenURL(dir);
            }

            // Draw script source
            GUILayout.Label(" ", Elements.Labels.Title);
            GUILayout.Label("Script source", Elements.Labels.Title);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(".bsg", ScriptSource == "bsg" ? Elements.Buttons.Default : Elements.Buttons.Disabled) && BsgHasCode)
            {
                ScriptSource = "bsg";
            }
            if (GUILayout.Button(".py", ScriptSource == "py" ? Elements.Buttons.Default : Elements.Buttons.Disabled) && ScriptFound)
            {
                ScriptSource = "py";
            }
            GUILayout.EndHorizontal();

            // Draw import script to bsg toggle
            GUILayout.Label(" ", Elements.Labels.Title);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Save code to .bsg", Elements.InputFields.Default);
            var b = GUILayout.Toggle(SaveToBsg, "Import", ScriptFound ? Elements.Buttons.Default : Elements.Buttons.Disabled, GUILayout.Width(100)) && ScriptFound;
            if (b != SaveToBsg)
                NoteMessage = b ? "Code will be saved to .bsg when you\n save the machine." : "Code will not be saved to .bsg when you\n save the machine.";
            SaveToBsg = b;
            DrawEnabledBadge(SaveToBsg);
            GUILayout.EndHorizontal();

            // Draw export script to lua
            GUILayout.Label(" ", Elements.Labels.Title);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Save code to .py", Elements.InputFields.Default);
            if(GUILayout.Button("Export", BsgHasCode ? Elements.Buttons.Default : Elements.Buttons.Disabled, GUILayout.Width(100)))
            {
                SaveToScript();
            }
            DrawEnabledBadge(BsgHasCode);
            GUILayout.EndHorizontal();

            // Draw message
            GUILayout.Label(" ", Elements.Labels.Title);
            if (SuccessMessage != null)
            {
                GUILayout.Label("\n<color=#00FF00>Success</color>", new GUIStyle(Elements.Labels.Title) { richText = true });
                GUILayout.Label(SuccessMessage);
            }
            if (NoteMessage != null)
            {
                GUILayout.Label("\n<color=#FFFF00>Note</color>", new GUIStyle(Elements.Labels.Title) { richText = true });
                GUILayout.Label(NoteMessage);
            }
            if (ErrorMessage != null)
            {
                GUILayout.Label("\n<color=#FF0000>Error</color>", new GUIStyle(Elements.Labels.Title) { richText = true });
                GUILayout.Label(ErrorMessage);
            }

            // Draw close button
            if (GUI.Button(new Rect(windowRect.width - 38, 8, 30, 30),
                "×", Elements.Buttons.Red))
                Visible = false;

            // Drag window
            GUI.DragWindow(new Rect(0, 0, windowRect.width, GUI.skin.window.padding.top));
        }
    }
}

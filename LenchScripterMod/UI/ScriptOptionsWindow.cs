using System;
using System.IO;
using Lench.Scripter.Internal;
using spaar.ModLoader;
using spaar.ModLoader.UI;
using UnityEngine;
// ReSharper disable UnusedMember.Local
// ReSharper disable PossibleLossOfFraction

namespace Lench.Scripter.UI
{
    internal class ScriptOptionsWindow
    {
        public Vector2 Position;
        public bool Visible { get; set; }

        private string _successMessage;
        private string _noteMessage;
        private string _errorMessage;

        public ScriptOptionsWindow()
        {
            var component = Mod.Controller.AddComponent<ScriptOptionsWindowComponent>();
            component.Handler = this;

            Internal.MachineData.OnLoadSuccess += message => _successMessage = message;
            Internal.MachineData.OnSaveSuccess += message => _successMessage = message;
            Internal.MachineData.OnLoadWarning += message => _noteMessage = message;
            Internal.MachineData.OnSaveWarning += message => _noteMessage = message;
            Script.OnStart += () =>
            {
                _successMessage = "Script is running.";
                _noteMessage = null;
                _errorMessage = null;
            };
            Script.OnStop += () =>
            {
                _successMessage = "Script has stopped.";
                _noteMessage = null;
                _errorMessage = null;
            };
            Script.OnError += () =>
            {
                _successMessage = null;
                _noteMessage = null;
                _errorMessage = "Script runtime error.";
            };
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class ScriptOptionsWindowComponent : MonoBehaviour
        {
            public ScriptOptionsWindow Handler;

            private bool _init;
            private readonly int _windowID = Util.GetWindowID();
            private Rect _windowRect;

            private void OnGUI()
            {
                if (Handler == null || !Handler.Visible) return;

                InitialiseWindowRect();

                GUI.skin = ModGUI.Skin;
                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                GUI.skin.window.padding.left = 8;
                GUI.skin.window.padding.right = 8;
                GUI.skin.window.padding.bottom = 8;
                _windowRect = GUILayout.Window(_windowID, _windowRect, DoWindow, "Script Options",
                    GUILayout.Height(200),
                    GUILayout.Width(320));

                Handler.Position.x = _windowRect.x < Screen.width / 2
                    ? _windowRect.x
                    : _windowRect.x - Screen.width;
                Handler.Position.y = _windowRect.y < Screen.height / 2
                    ? _windowRect.y
                    : _windowRect.y - Screen.height;

            }

            private void InitialiseWindowRect()
            {
                if (_init) return;

                _windowRect = new Rect
                {
                    width = 320,
                    height = 200,
                    x = Handler.Position.x >= 0
                        ? Handler.Position.x
                        : Screen.width + Handler.Position.x,
                    y = Handler.Position.y >= 0
                        ? Handler.Position.y
                        : Screen.height + Handler.Position.y
                };

                _init = true;
            }

            private static void DrawEnabledBadge(bool e)
            {
                if (e)
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
                Script.FileName = GUILayout.TextField(Script.FileName);
                DrawEnabledBadge(Script.FilePath != null);
                GUILayout.EndHorizontal();

                // Draw open folder button
                if (GUILayout.Button("Open Scripts folder", Elements.Buttons.ComponentField))
                {
                    var dir = Application.dataPath + "/Scripts/";
                    Directory.CreateDirectory(dir);
                    Application.OpenURL(dir);
                }

                // Draw script source
                GUILayout.Label(" ", Elements.Labels.Title);
                GUILayout.Label("Script source", Elements.Labels.Title);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(".bsg",
                        Script.Source == Script.SourceType.Bsg
                            ? Elements.Buttons.Default
                            : Elements.Buttons.Disabled) &&
                    Script.EmbeddedCode != null)
                    Script.Source = Script.SourceType.Bsg;
                if (GUILayout.Button(".py",
                        Script.Source == Script.SourceType.Py
                            ? Elements.Buttons.Default
                            : Elements.Buttons.Disabled) &&
                    Script.FilePath != null)
                    Script.Source = Script.SourceType.Py;
                GUILayout.EndHorizontal();

                // Draw import script to bsg toggle
                GUILayout.Label(" ", Elements.Labels.Title);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Save code to .bsg", Elements.InputFields.Default);
                var b = GUILayout.Toggle(Script.SaveToBsg, "Import",
                        Script.FilePath != null ? Elements.Buttons.Default : Elements.Buttons.Disabled,
                        GUILayout.Width(100)) && Script.FilePath != null;
                if (b != Script.SaveToBsg)
                    Handler._noteMessage = b
                        ? "Code will be saved to .bsg when you\n save the machine."
                        : "Code will not be saved to .bsg when you\n save the machine.";
                Script.SaveToBsg = b;
                DrawEnabledBadge(Script.SaveToBsg);
                GUILayout.EndHorizontal();

                // Draw export script to Python
                GUILayout.Label(" ", Elements.Labels.Title);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Save code to .py", Elements.InputFields.Default);
                if (GUILayout.Button("Export",
                    Script.EmbeddedCode != null ? Elements.Buttons.Default : Elements.Buttons.Disabled,
                    GUILayout.Width(100)))
                {
                    try
                    {
                        var path = Script.Export();
                        if (path.StartsWith(Application.dataPath))
                            path = "... " + path.Substring(Application.dataPath.Length);
                        Handler._successMessage = "Successfully exported embedded code to\n" + path;
                    }
                    catch (Exception e)
                    {
                        Handler._errorMessage = e.Message;
                    }
                }
                    
                DrawEnabledBadge(Script.EmbeddedCode != null);
                GUILayout.EndHorizontal();

                // Draw message
                GUILayout.Label(" ", Elements.Labels.Title);
                if (Handler._successMessage != null)
                {
                    GUILayout.Label("\n<color=#00FF00>Success</color>",
                        new GUIStyle(Elements.Labels.Title) {richText = true});
                    GUILayout.Label(Handler._successMessage);
                }
                if (Handler._noteMessage != null)
                {
                    GUILayout.Label("\n<color=#FFFF00>Note</color>",
                        new GUIStyle(Elements.Labels.Title) {richText = true});
                    GUILayout.Label(Handler._noteMessage);
                }
                if (Handler._errorMessage != null)
                {
                    GUILayout.Label("\n<color=#FF0000>Error</color>",
                        new GUIStyle(Elements.Labels.Title) {richText = true});
                    GUILayout.Label(Handler._errorMessage);
                }

                // Draw close button
                if (GUI.Button(new Rect(_windowRect.width - 38, 8, 30, 30),
                    "×", Elements.Buttons.Red))
                    Handler.Visible = false;

                // Drag window
                GUI.DragWindow(new Rect(0, 0, _windowRect.width, GUI.skin.window.padding.top));
            }
        }
    }
}
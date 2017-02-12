using System.Collections.Generic;
using System.Text.RegularExpressions;
using Lench.Scripter.Internal;
using spaar.ModLoader;
using spaar.ModLoader.UI;
using UnityEngine;

// ReSharper disable UnusedMember.Local
// ReSharper disable PossibleLossOfFraction
// ReSharper disable ParameterHidesMember

namespace Lench.Scripter.UI
{
    /// <summary>
    ///     Displays global Python variables in a GUI.
    /// </summary>
    internal class WatchlistWindow
    {
        private const float EditWindowWidth = 240;
        private const float EditWindowHeight = 124;

        public Vector2 Position;

        public WatchlistWindow()
        {
            var component = Mod.Controller.AddComponent<WatchlistComponent>();
            component.Handler = this;
        }

        public bool Visible { get; set; }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class WatchlistComponent : MonoBehaviour
        {
            private readonly int _editWindowID = Util.GetWindowID();
            private readonly int _mainWindowID = Util.GetWindowID();

            private bool _editing;
            private VariableWatch _editingVariable;
            private Rect _editWindowRect;

            private bool _init;
            private Rect _mainWindowRect;

            private string _newVariableName = "";
            private string _newVariableValue;

            private readonly Texture2D _tex = Resources.Images.IconClear;

            private Vector2 _scrollPosition = Vector2.zero;
            public WatchlistWindow Handler;

            private void OnGUI()
            {
                if (!Elements.IsInitialized || Handler == null || !Handler.Visible) return;

                InitialiseWindowRect();

                GUI.skin = ModGUI.Skin;
                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                GUI.skin.window.padding.left = 8;
                GUI.skin.window.padding.right = 8;
                GUI.skin.window.padding.bottom = 8;
                _mainWindowRect = GUI.Window(_mainWindowID, _mainWindowRect, DoMainWindow, "Watchlist");
                if (_editing)
                    _editWindowRect = GUI.Window(_editWindowID, _editWindowRect, DoEditWindow,
                        "Edit " + _editingVariable.GetName());

                Handler.Position.x = _mainWindowRect.x < Screen.width / 2
                    ? _mainWindowRect.x
                    : _mainWindowRect.x - Screen.width;
                Handler.Position.y = _mainWindowRect.y < Screen.height / 2
                    ? _mainWindowRect.y
                    : _mainWindowRect.y - Screen.height;
            }

            private void InitialiseWindowRect()
            {
                if (_init) return;

                _mainWindowRect = new Rect
                {
                    width = 320,
                    height = 500,
                    x = Handler.Position.x >= 0
                        ? Handler.Position.x
                        : Screen.width + Handler.Position.x,
                    y = Handler.Position.y >= 0
                        ? Handler.Position.y
                        : Screen.height + Handler.Position.y
                };

                _init = true;
            }

            private void DoMainWindow(int id)
            {
                var oldColor = GUI.backgroundColor;

                // Draw close button
                if (GUI.Button(new Rect(_mainWindowRect.width - 38, 8, 30, 30),
                    "×", Elements.Buttons.Red))
                    Handler.Visible = false;

                // Draw clear button
                if (GUI.Button(new Rect(_mainWindowRect.width - 76, 8, 30, 30),
                    _tex, Elements.Buttons.Red))
                    Watchlist.Clear();

                var toBeRemoved = new List<VariableWatch>();

                _newVariableName = GUI.TextField(new Rect(68, 48, 248, 20), _newVariableName,
                    Elements.InputFields.ComponentField);
                if (GUI.Button(new Rect(4, 48, 60, 20), "Add", Elements.Buttons.Default) &&
                    Regex.Replace(_newVariableName, @"\s+", "") != "")
                {
                    _newVariableName = Regex.Replace(_newVariableName, @"\s+", "");
                    Watchlist.Add(_newVariableName, null, true);
                    _newVariableName = "";
                }

                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                _scrollPosition = GUI.BeginScrollView(
                    new Rect(4, 72, 312, 424),
                    _scrollPosition,
                    new Rect(0, 0, 296, 4 + Watchlist.Watched.Count * 24));
                GUI.backgroundColor = oldColor;

                var i = 0;
                foreach (var v in Watchlist.Watched)
                {
                    // Button for removing line
                    if (GUI.Button(new Rect(4, 4 + i * 24, 20, 20), "×", Elements.Buttons.Red))
                        toBeRemoved.Add(v);

                    // Color of labels
                    GUI.backgroundColor = Color.black;

                    // Variable name: button for global, label for local
                    if (v.Global)
                    {
                        if (GUI.Button(new Rect(28, 4 + i * 24, 130, 20), v.GetName(), Elements.Buttons.Default))
                        {
                            _editing = true;
                            _editingVariable = v;
                            _newVariableValue = v.GetEditString();
                            _editWindowRect = new Rect(
                                _mainWindowRect.x + 24,
                                _mainWindowRect.y + 60 + i * 24,
                                EditWindowWidth,
                                EditWindowHeight);
                        }
                    }
                    else
                    {
                        GUI.Label(new Rect(28, 4 + i * 24, 130, 20), v.GetName(), Elements.InputFields.ComponentField);
                    }

                    // Label for variable value
                    GUI.Label(new Rect(162, 4 + i * 24, 136, 20), v.GetValue(), Elements.InputFields.ComponentField);

                    GUI.backgroundColor = oldColor;

                    i++;
                }

                // Remove variables
                foreach (var v in toBeRemoved) Watchlist.Watched.Remove(v);

                GUI.EndScrollView();

                GUI.DragWindow(new Rect(0, 0, _mainWindowRect.width, GUI.skin.window.padding.top));
            }

            private void DoEditWindow(int id)
            {
                if (GUI.Button(new Rect(4, 96, 114, 20), "Set value", Elements.Buttons.Default))
                {
                    _editing = false;
                    _editingVariable.SetValue(_newVariableValue);
                }
                if (GUI.Button(new Rect(122, 96, 114, 20), "Cancel", Elements.Buttons.Red))
                    _editing = false;

                GUI.Label(new Rect(8, 52, 224, 20), "Enter a Python expression:", Elements.Labels.Default);

                var oldColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.black;

                _newVariableValue = GUI.TextField(new Rect(4, 72, 232, 20), _newVariableValue,
                    Elements.InputFields.ComponentField);

                GUI.backgroundColor = oldColor;

                GUI.DragWindow(new Rect(0, 0, _editWindowRect.width, GUI.skin.window.padding.top));
            }
        }
    }
}
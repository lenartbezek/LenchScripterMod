using System;
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
    internal class Watchlist
    {
        private const float EditWindowWidth = 240;
        private const float EditWindowHeight = 124;

        private readonly List<VariableWatch> _watched = new List<VariableWatch>();
        public Vector2 Position;

        public Watchlist()
        {
            var component = Mod.GameObject.AddComponent<WatchlistComponent>();
            component.Handler = this;
        }

        public bool Visible { get; set; }

        /// <summary>
        ///     Adds a variable to the watchlist.
        ///     If it's already added, it only updates the value.
        /// </summary>
        /// <param name="name">variable name</param>
        /// <param name="value">reported value</param>
        /// <param name="global">is global</param>
        public void Add(string name, object value, bool global = false)
        {
            var newVar = new VariableWatch(name) {Global = global};
            newVar.ReportValue(value);
            foreach (var v in _watched)
                if (v.Equals(newVar))
                {
                    if (value != null)
                        v.ReportValue(value);
                    return;
                }
            _watched.Add(newVar);
        }

        /// <summary>
        ///     Returns a variable from the watchlist.
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        /// <returns>VariableWatch class</returns>
        public VariableWatch Get(string name)
        {
            foreach (var v in _watched)
                if (v.GetName() == name) return v;
            return new VariableWatch(name);
        }

        /// <summary>
        ///     Removes all variables from the watchlist.
        /// </summary>
        public void ClearWatchlist()
        {
            _watched.Clear();
        }

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

            private Vector2 _scrollPosition = Vector2.zero;
            public Watchlist Handler;

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

                var toBeRemoved = new List<VariableWatch>();

                _newVariableName = GUI.TextField(new Rect(68, 48, 248, 20), _newVariableName,
                    Elements.InputFields.ComponentField);
                if (GUI.Button(new Rect(4, 48, 60, 20), "Add", Elements.Buttons.Default) &&
                    Regex.Replace(_newVariableName, @"\s+", "") != "")
                {
                    _newVariableName = Regex.Replace(_newVariableName, @"\s+", "");
                    Handler.Add(_newVariableName, null, true);
                    _newVariableName = "";
                }

                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                _scrollPosition = GUI.BeginScrollView(
                    new Rect(4, 72, 312, 400),
                    _scrollPosition,
                    new Rect(0, 0, 296, 4 + Handler._watched.Count * 24));
                GUI.backgroundColor = oldColor;

                var i = 0;
                foreach (var v in Handler._watched)
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
                foreach (var v in toBeRemoved) Handler._watched.Remove(v);

                GUI.EndScrollView();

                if (GUI.Button(new Rect(4, 476, 312, 20), "Clear Watchlist", Elements.Buttons.Red))
                    Handler.ClearWatchlist();

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

    /// <summary>
    ///     Represents a single variable.
    /// </summary>
    internal class VariableWatch : IEquatable<VariableWatch>
    {
        private readonly string _name;
        private object _value;
        internal bool Global;

        internal VariableWatch(string name)
        {
            _name = name;
        }

        /// <summary>
        ///     Returns true if the entries represent the same variable.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(VariableWatch other)
        {
            return other != null && _name == other._name && Global == other.Global;
        }

        /// <summary>
        ///     Used to report the value.
        ///     Checks if the variable is global.
        /// </summary>
        /// <param name="value"></param>
        public void ReportValue(object value)
        {
            if (value == null) return;
            _value = value;

            if (Global) return;
            if (!Script.Python.Contains(_name)) return;
            var globalValue = Script.Python[_name];
            Global = value.Equals(globalValue);
        }

        /// <summary>
        ///     Returns the variable's display name.
        /// </summary>
        public string GetName()
        {
            return _name;
        }

        /// <summary>
        ///     Looks for the global variable. If not found,
        ///     returns the last reported value.
        /// </summary>
        /// <returns></returns>
        public string GetValue()
        {
            if (Global && Game.IsSimulating)
                if (Script.Python.Contains(_name))
                    _value = Script.Python[_name];
            try
            {
                return _value.ToString();
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        ///     Returns an edit string used in edit variable window.
        ///     Supposed to be a Lua expression to initialize the edited variable.
        /// </summary>
        /// <returns></returns>
        public string GetEditString()
        {
            if (_value == null) return "";
            var type = _value.GetType();
            if (type == typeof(Vector4))
                return "Vector3" + _value;
            if (type == typeof(Vector3))
                return "Vector3" + _value;
            if (type == typeof(Vector2))
                return "Vector2" + _value;
            if (type == typeof(string))
                return '"' + _value.ToString() + '"';
            return _value.ToString();
        }

        /// <summary>
        ///     Executes Lua statement to set the value.
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(string value)
        {
            if (Global && Script.Python != null)
                Script.Python.Execute(_name + " = " + value);
        }
    }
}
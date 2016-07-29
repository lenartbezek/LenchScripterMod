using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using spaar.ModLoader;
using spaar.ModLoader.UI;

namespace Lench.Scripter.Internal
{

    /// <summary>
    /// Displays global Lua variables in a GUI.
    /// </summary>
    public class Watchlist : MonoBehaviour
    {
        /// <summary>
        /// Name in the Unity Hierarchy.
        /// </summary>
        public new string name { get { return "Watchlist"; } }

        internal Vector2 ConfigurationPosition;

        private static float editWindowWidth = 240;
        private static float editWindowHeight = 124;

        private int mainWindowID = Util.GetWindowID();
        private Rect mainWindowRect;
        private int editWindowID = Util.GetWindowID();
        private Rect editWindowRect;

        private string newVariableName = "";
        private string newVariableValue;

        private Vector2 scrollPosition = Vector2.zero;

        internal List<VariableWatch> watched;

        internal bool Visible { get; set; } = false;

        private bool init = false;
        private bool editing = false;
        private VariableWatch editingVariable;

        private void Awake()
        {
            watched = new List<VariableWatch>();
        }

        /// <summary>
        /// Render windows.
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
                mainWindowRect = GUI.Window(mainWindowID, mainWindowRect, DoMainWindow, "Watchlist");
                if (editing)
                {
                    editWindowRect = GUI.Window(editWindowID, editWindowRect, DoEditWindow, "Edit " + editingVariable.GetName());
                }

                ConfigurationPosition.x = mainWindowRect.x < Screen.width/2 ? mainWindowRect.x : mainWindowRect.x - Screen.width;
                ConfigurationPosition.y = mainWindowRect.y < Screen.height/2 ? mainWindowRect.y : mainWindowRect.y - Screen.height;
            }
        }

        /// <summary>
        /// Initialises main window Rect on first call.
        /// Intended to set the position from the configuration.
        /// </summary>
        private void InitialiseWindowRect()
        {
            if (init) return;

            mainWindowRect = new Rect();
            mainWindowRect.width = 320;
            mainWindowRect.height = 500;
            if (ConfigurationPosition != null)
            {
                mainWindowRect.x = ConfigurationPosition.x >= 0 ? ConfigurationPosition.x : Screen.width + ConfigurationPosition.x; 
                mainWindowRect.y = ConfigurationPosition.y >= 0 ? ConfigurationPosition.y : Screen.height + ConfigurationPosition.y;
            }
            else
            {
                mainWindowRect.x = Screen.width - mainWindowRect.width - 60;
                mainWindowRect.y = 200;
            }

            init = true;
        }

        /// <summary>
        /// Adds a variable to the watchlist.
        /// If it's already added, it only updates the value.
        /// </summary>
        /// <param name="name">variable name</param>
        /// <param name="value">reported value</param>
        /// <param name="global">is global</param>
        public void AddToWatchlist(string name, System.Object value, bool global = false)
        {
            var newVar = new VariableWatch(name);
            newVar.global = global;
            newVar.ReportValue(value);
            foreach (VariableWatch v in watched)
            {
                if (v.Equals(newVar))
                {
                    if(value != null)
                        v.ReportValue(value);
                    return;
                }
            }
            watched.Add(newVar);
        }

        /// <summary>
        /// Returns a variable from the watchlist.
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        /// <returns>VariableWatch class</returns>
        public VariableWatch GetVariable(string name)
        {
            foreach (VariableWatch v in watched)
            {
                if (v.GetName() == name) return v;
            }
            return new VariableWatch(name);
        }

        /// <summary>
        /// Removes all variables from the watchlist.
        /// </summary>
        public void ClearWatchlist()
        {
            watched.Clear();
        }

        /// <summary>
        /// Draws the main window.
        /// </summary>
        /// <param name="id"></param>
        private void DoMainWindow(int id)
        {
            var oldColor = GUI.backgroundColor;

            // Draw close button
            if (GUI.Button(new Rect(mainWindowRect.width - 38, 8, 30, 30),
                "×", Elements.Buttons.Red))
                Visible = false;

            List<VariableWatch> toBeRemoved = new List<VariableWatch>();

            newVariableName = GUI.TextField(new Rect(68, 48, 248, 20), newVariableName, Elements.InputFields.ComponentField);
            if (GUI.Button(new Rect(4, 48, 60, 20), "Add", Elements.Buttons.Default) && Regex.Replace(newVariableName, @"\s+", "") != "")
            {
                newVariableName = Regex.Replace(newVariableName, @"\s+", "");
                AddToWatchlist(newVariableName, null, true);
                newVariableName = "";
            }

            GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
            scrollPosition = GUI.BeginScrollView(
                new Rect(4, 72, 312, 400),
                scrollPosition,
                new Rect(0, 0, 296, 4 + (watched.Count * 24)));
            GUI.backgroundColor = oldColor;

            int i = 0;
            foreach (VariableWatch v in watched)
            {
                // Button for removing line
                if (GUI.Button(new Rect(4, 4 + i * 24, 20, 20), "×", Elements.Buttons.Red))
                    toBeRemoved.Add(v);

                // Color of labels
                GUI.backgroundColor = Color.black;

                // Variable name: button for global, label for local
                if (v.global)
                {
                    if (GUI.Button(new Rect(28, 4 + i * 24, 130, 20), v.GetName(), Elements.Buttons.Default))
                    {
                        editing = true;
                        editingVariable = v;
                        newVariableValue = v.GetEditString();
                        editWindowRect = new Rect(
                            mainWindowRect.x + 24,
                            mainWindowRect.y + 60 + i * 24,
                            editWindowWidth,
                            editWindowHeight);
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
            foreach (VariableWatch v in toBeRemoved) watched.Remove(v);

            GUI.EndScrollView();

            if (GUI.Button(new Rect(4, 476, 312, 20), "Clear Watchlist", Elements.Buttons.Red))
                ClearWatchlist();

            GUI.DragWindow(new Rect(0, 0, mainWindowRect.width, GUI.skin.window.padding.top));
        }

        /// <summary>
        /// Draws the edit window on top of existing window.
        /// </summary>
        /// <param name="id"></param>
        private void DoEditWindow(int id)
        {
            
            if (GUI.Button(new Rect(4, 96, 114, 20), "Set value", Elements.Buttons.Default))
            {
                editing = false;
                editingVariable.SetValue(newVariableValue);
            }
            if (GUI.Button(new Rect(122, 96, 114, 20), "Cancel", Elements.Buttons.Red))
            {
                editing = false;
            }

            GUI.Label(new Rect(8, 52, 224, 20), "Enter a Python expression:", Elements.Labels.Default);

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.black;

            newVariableValue = GUI.TextField(new Rect(4, 72, 232, 20), newVariableValue, Elements.InputFields.ComponentField);

            GUI.backgroundColor = oldColor;

            GUI.DragWindow(new Rect(0, 0, editWindowRect.width, GUI.skin.window.padding.top));
        }
    }

    /// <summary>
    /// Represents a single variable.
    /// </summary>
    public class VariableWatch : IEquatable<VariableWatch>
    {
        private string name;
        private object value;
        internal bool global = false;

        internal VariableWatch(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Used to report the value.
        /// Checks if the variable is global.
        /// </summary>
        /// <param name="value"></param>
        public void ReportValue(object value)
        {
            if (value != null)
            {
                this.value = value;
                if (!global)
                { 
                    // Check if global
                    if (PythonEnvironment.MainInstance.ContainsVariable(name))
                    {
                        object globalValue = PythonEnvironment.MainInstance.GetVariable(name);
                        global = value.Equals(globalValue);
                    }
                }  
            }
        }

        /// <summary>
        /// Returns the variable's display name.
        /// </summary>
        public string GetName()
        {
            return name;
        }

        /// <summary>
        /// Looks for the global variable. If not found,
        /// returns the last reported value.
        /// </summary>
        /// <returns></returns>
        public string GetValue()
        {
            if (global && Game.IsSimulating)
            {
                if(PythonEnvironment.MainInstance.ContainsVariable(name))
                    value = PythonEnvironment.MainInstance.GetVariable(name);
            }    
            try
            {
                return value.ToString();
            }
            catch
            {
                return "";
            }
            
        }

        /// <summary>
        /// Returns an edit string used in edit variable window.
        /// Supposed to be a Lua expression to initialize the edited variable.
        /// </summary>
        /// <returns></returns>
        public string GetEditString()
        {
            if (value != null)
            {
                var type = value.GetType();
                if (type == typeof(Vector4))
                    return "Vector3" + value.ToString();
                if (type == typeof(Vector3))
                    return "Vector3" + value.ToString();
                if (type == typeof(Vector2))
                    return "Vector2" + value.ToString();
                if (type == typeof(string))
                    return '"' + value.ToString() + '"';
                return value.ToString();
            }
            return "";
        }

        /// <summary>
        /// Executes Lua statement to set the value.
        /// </summary>
        /// <param name="value"></param>
        public void SetValue(string value)
        {
            if (global && PythonEnvironment.MainInstance != null)
            {
                PythonEnvironment.MainInstance.Execute(name + " = " + value);
            }
        }

        /// <summary>
        /// Returns true if the entries represent the same variable.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(VariableWatch other)
        {
            return this.name == other.name && this.global == other.global;
        }
    }
}

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using spaar.ModLoader;
using spaar.ModLoader.UI;

namespace LenchScripterMod
{

    /// <summary>
    /// Displays global Lua variables in a GUI.
    /// </summary>
    internal class LuaWatchlist
    {

        private static float mainWindowWidth = 320;
        private static float mainWindowHeight = 500;

        private static float editWindowWidth = 240;
        private static float editWindowHeight = 124;

        private int mainWindowID = Util.GetWindowID();
        private Rect mainWindowRect = new Rect(Screen.width - mainWindowWidth - 50, 50, mainWindowWidth, mainWindowHeight);
        private int editWindowID = Util.GetWindowID();
        private Rect editWindowRect;

        private string newVariableName = "";
        private string newVariableValue;

        private Vector2 scrollPosition = Vector2.zero;

        internal List<VariableWatch> watched;

        internal bool visible { get; set; } = false;
        internal bool autoadd { get; set; } = false;

        private bool editing = false;
        private VariableWatch editingVariable;

        internal LuaWatchlist()
        {
            watched = new List<VariableWatch>();
        }

        /// <summary>
        /// Adds a variable to the watchlist.
        /// If it's already added, it only updates the value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="global"></param>
        internal void AddToWatchlist(string name, System.Object value, bool global = false)
        {
            var newVar = new VariableWatch(name);
            newVar.global = global;
            newVar.Reportvalue(value);
            foreach (VariableWatch v in watched)
            {
                if (v.Equals(newVar))
                {
                    if(value != null)
                        v.Reportvalue(value);
                    return;
                }
            }
            watched.Add(newVar);
        }

        internal void ClearWatchlist()
        {
            watched.Clear();
        }

        /// <summary>
        /// Called by OnGUI() for rendering.
        /// </summary>
        internal void Render()
        {
            if (visible)
            {
                GUI.skin = ModGUI.Skin;
                mainWindowRect = GUI.Window(mainWindowID, mainWindowRect, DoMainWindow, "Lua Watchlist");
                if (editing)
                {
                    editWindowRect = GUI.Window(editWindowID, editWindowRect, DoEditWindow, "Edit "+editingVariable.GetName());
                }
            }
        }

        /// <summary>
        /// Draws the main window.
        /// </summary>
        /// <param name="id"></param>
        private void DoMainWindow(int id)
        {
            List<VariableWatch> toBeRemoved = new List<VariableWatch>();

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1);
            newVariableName = GUI.TextField(new Rect(68, 48, 248, 20), newVariableName, Elements.InputFields.ComponentField);
            GUI.backgroundColor = oldColor;
            if (GUI.Button(new Rect(4, 48, 60, 20), "Add", Elements.Buttons.Default) && Regex.Replace(newVariableName, @"\s+", "") != "")
            {
                newVariableName = Regex.Replace(newVariableName, @"\s+", "");
                AddToWatchlist(newVariableName, null, true);
                newVariableName = "";
            }

            scrollPosition = GUI.BeginScrollView(
                new Rect(4, 72, 312, 400),
                scrollPosition,
                new Rect(0, 0, 296, 4 + (watched.Count * 24)));

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

            GUI.Label(new Rect(8, 52, 224, 20), "Enter a Lua expression:", Elements.Labels.Default);

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
    class VariableWatch : IEquatable<VariableWatch>
    {
        private string name;
        private System.Object value;
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
        internal void Reportvalue(System.Object value)
        {
            if (value != null)
            {
                this.value = value;
                if (!global)
                { // Check if global
                    if (ScripterMod.scripter.lua[name] != null)
                    {
                        System.Object globalValue = ScripterMod.scripter.lua[name];
                        global = value.Equals(globalValue);
                    }
                }  
            }
        }

        internal string GetName()
        {
            return name;
        }

        /// <summary>
        /// Looks for the global variable. If not found,
        /// returns the last reported value.
        /// </summary>
        /// <returns></returns>
        internal string GetValue()
        {
            if (global && ScripterMod.scripter.isSimulating)
            {
                if(ScripterMod.scripter.lua[name] != null)
                    value = ScripterMod.scripter.lua[name];
            }    
            if (value == null || name == "")
                return "";
            return value.ToString();
        }

        /// <summary>
        /// Returns an edit string used in edit variable window.
        /// Supposed to be a Lua expression to initialize the edited variable.
        /// </summary>
        /// <returns></returns>
        internal string GetEditString()
        {
            if (value != null)
            {
                var type = value.GetType();
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
        internal void SetValue(string value)
        {
            if (global && ScripterMod.scripter.lua != null)
            {
                ScripterMod.scripter.lua.DoString(name+" = "+value);
            }
        }

        public bool Equals(VariableWatch other)
        {
            return this.name == other.name && this.global == other.global;
        }
    }
}

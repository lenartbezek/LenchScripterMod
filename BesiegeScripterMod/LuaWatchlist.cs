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
        private static float editWindowHeight = 80;

        private int mainWindowID = Util.GetWindowID();
        private Rect mainWindowRect = new Rect(Screen.width - mainWindowWidth - 50, 50, mainWindowWidth, mainWindowHeight);
        private int editWindowID = Util.GetWindowID();
        private Rect editWindowRect;

        private string newVariableName = "";
        private string newVariableValue;

        private Vector2 scrollPosition = Vector2.zero;

        internal List<VariableWatch> watched;

        internal bool visible { get; set; } = false;

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

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(296), GUILayout.Height(50 + (watched.Count * 24)), GUILayout.MaxWidth(296), GUILayout.MaxHeight(412));

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.gray;
            newVariableName = GUI.TextField(new Rect(68, 4, 224, 20), newVariableName, Elements.InputFields.ComponentField);
            GUI.backgroundColor = oldColor;
            if (GUI.Button(new Rect(4, 4, 60, 20), "Add", Elements.Buttons.Default) && newVariableName != "")
            {
                newVariableName = Regex.Replace(newVariableName, @"\s+", "");
                AddToWatchlist(newVariableName, null, true);
                newVariableName = "";
            }

            int i = 0;
            foreach (VariableWatch v in watched)
            {

                // Button for removing line
                if (GUI.Button(new Rect(4, 30 + i * 24, 20, 20), "×", Elements.Buttons.Red))
                    toBeRemoved.Add(v);

                // Color of labels
                GUI.backgroundColor = Color.black;

                // Variable name: button for global, label for local
                if (v.global)
                {
                    if (GUI.Button(new Rect(28, 30 + i * 24, 130, 20), v.GetName(), Elements.Buttons.Default))
                    {
                        editing = true;
                        editingVariable = v;
                        newVariableValue = v.GetValue();
                        editWindowRect = new Rect(
                            mainWindowRect.x + 24,
                            mainWindowRect.y + 50 + i * 24,
                            editWindowWidth,
                            editWindowHeight);
                    }
                }
                else
                {
                    GUI.Label(new Rect(28, 30 + i * 24, 130, 20), v.GetName(), Elements.InputFields.ComponentField);
                }

                // Label for variable value
                GUI.Label(new Rect(162, 30 + i * 24, 130, 20), v.GetValue(), Elements.InputFields.ComponentField);

                GUI.backgroundColor = oldColor;

                i++;
            }

            // Remove variables
            foreach (VariableWatch v in toBeRemoved) watched.Remove(v);

            GUILayout.EndScrollView();

            if (GUI.Button(new Rect(12, mainWindowWidth - 26, 296, 20), "Clear Watchlist", Elements.Buttons.Red))
                ClearWatchlist();

            GUI.DragWindow(new Rect(0, 0, mainWindowRect.width, GUI.skin.window.padding.top));
        }

        /// <summary>
        /// Draws the edit window on top of existing window.
        /// </summary>
        /// <param name="id"></param>
        private void DoEditWindow(int id)
        {
            
            if (GUI.Button(new Rect(4, GUI.skin.window.padding.top, 60, 20), "Set", Elements.Buttons.Default))
            {
                editing = false;
                editingVariable.SetValue(newVariableValue);
            }

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.black;

            newVariableValue = GUI.TextField(new Rect(68, GUI.skin.window.padding.top, 168, 20), newVariableValue, Elements.InputFields.ComponentField);

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
        private string value;
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
                this.value = value.ToString();
                if (!global)
                {
                    if (ScripterMod.scripter.lua[name] != null)
                        global = this.value == ScripterMod.scripter.lua[name].ToString();
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
                    value = ScripterMod.scripter.lua[name].ToString();
            }    
            if (value == null || name == "")
                return "";
            return value;
        }

        internal void SetValue(string value)
        {
            if (global)
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

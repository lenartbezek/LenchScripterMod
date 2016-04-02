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

        private static float defaultWidth = 320;
        private static float defaultHeight = 500;

        private int windowID = Util.GetWindowID();
        private Rect windowRect = new Rect(Screen.width - defaultWidth - 50, 50, defaultWidth, defaultHeight);
        private string newName = "";
        private Vector2 scrollPosition = Vector2.zero;

        internal List<VariableWatch> watched;

        internal bool visible { get; set; } = false;

        internal LuaWatchlist()
        {
            watched = new List<VariableWatch>();
        }

        /// <summary>
        /// Called by OnGUI() for rendering.
        /// </summary>
        internal void Render()
        {
            if (visible)
            {
                GUI.skin = ModGUI.Skin;
                windowRect = GUI.Window(windowID, windowRect, DoWindow, "Lua Watchlist");
            }
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
        /// Draws the window.
        /// </summary>
        /// <param name="id"></param>
        private void DoWindow(int id)
        {
            List<VariableWatch> toBeRemoved = new List<VariableWatch>();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(296), GUILayout.Height(50 + (watched.Count * 24)), GUILayout.MaxWidth(296), GUILayout.MaxHeight(412));

            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.gray;
            newName = GUI.TextField(new Rect(68, 4, 224, 20), newName, Elements.InputFields.ComponentField);
            GUI.backgroundColor = oldColor;
            if (GUI.Button(new Rect(4, 4, 60, 20), "Add", Elements.Buttons.Default) && newName != "")
            {
                newName = Regex.Replace(newName, @"\s+", "");
                AddToWatchlist(newName, null, true);
                newName = "";
            }

            int i = 0;
            foreach (VariableWatch v in watched)
            {

                // Button for removing line
                if (GUI.Button(new Rect(4, 30 + i * 24, 20, 20), "×", Elements.Buttons.Red))
                    toBeRemoved.Add(v);

                // Color of labels
                GUI.backgroundColor = Color.black;

                // Label for variable name
                GUI.Label(new Rect(28, 30 + i * 24, 130, 20), v.GetName(), Elements.InputFields.ComponentField);

                // Label for variable value
                GUI.Label(new Rect(162, 30 + i * 24, 130, 20), v.GetValue(), Elements.InputFields.ComponentField);

                GUI.backgroundColor = oldColor;

                i++;
            }

            // Remove variables
            foreach (VariableWatch v in toBeRemoved) watched.Remove(v);

            GUILayout.EndScrollView();

            if (GUI.Button(new Rect(12, defaultHeight - 26, 296, 20), "Clear Watchlist", Elements.Buttons.Red))
                ClearWatchlist();

            GUI.DragWindow(new Rect(0, 0, windowRect.width, GUI.skin.window.padding.top));
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
                try
                {
                    var number = float.Parse(value);
                    ScripterMod.scripter.lua[name] = number;
                }
                catch (System.FormatException)
                {
                    if(value == "true")
                    {
                        ScripterMod.scripter.lua[name] = true;
                        return;
                    }   
                    if(value == "false")
                    {
                        ScripterMod.scripter.lua[name] = false;
                        return;
                    }
                    ScripterMod.scripter.lua[name] = value;
                }
            }
        }

        public bool Equals(VariableWatch other)
        {
            return this.name == other.name && this.global == other.global;
        }
    }
}

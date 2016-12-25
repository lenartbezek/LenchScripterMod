using System;
using System.Collections.Generic;
using spaar.ModLoader;
using UnityEngine;

namespace Lench.Scripter.Internal
{
    internal static class Watchlist
    {
        public static readonly List<VariableWatch> Watched = new List<VariableWatch>();

        /// <summary>
        ///     Adds a variable to the watchlist.
        ///     If it's already added, it only updates the value.
        /// </summary>
        /// <param name="name">variable name</param>
        /// <param name="value">reported value</param>
        /// <param name="global">is global</param>
        public static void Add(string name, object value, bool global = false)
        {
            var newVar = new VariableWatch(name) { Global = global };
            newVar.ReportValue(value);
            foreach (var v in Watched)
                if (v.Equals(newVar))
                {
                    if (value != null)
                        v.ReportValue(value);
                    return;
                }
            Watched.Add(newVar);
        }

        /// <summary>
        ///     Returns a variable from the watchlist.
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        /// <returns>VariableWatch class</returns>
        public static VariableWatch Get(string name)
        {
            foreach (var v in Watched)
                if (v.GetName() == name) return v;
            return new VariableWatch(name);
        }

        /// <summary>
        ///     Removes all variables from the watchlist.
        /// </summary>
        public static void Clear()
        {
            Watched.Clear();
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

using System;
using System.Reflection;
using System.Collections.Generic;
using spaar.ModLoader;
using spaar.ModLoader.UI;
using UnityEngine;

namespace Lench.Scripter.Internal
{
    internal class IdentifierDisplay : MonoBehaviour
    {
        internal Vector2 ConfigurationPosition;

        private GenericBlock block;
        internal bool Visible { get; set; } = false;

        private bool init = false;

        private int windowID = Util.GetWindowID();
        private Rect windowRect;

        internal void ShowBlock(GenericBlock block)
        {
            this.block = block;
            Visible = true;
        }

        /// <summary>
        /// Render window.
        /// </summary>
        private void OnGUI()
        {
            if (Visible && !Game.IsSimulating)
            {
                InitialiseWindowRect();

                GUI.skin = ModGUI.Skin;
                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                GUI.skin.window.padding.left = 8;
                GUI.skin.window.padding.right = 8;
                GUI.skin.window.padding.bottom = 8;
                windowRect = GUILayout.Window(windowID, windowRect, DoWindow, "Block Info", GUILayout.Height(100));

                ConfigurationPosition.x = windowRect.x < Screen.width/2 ? windowRect.x : windowRect.x - Screen.width;
                ConfigurationPosition.y = windowRect.y < Screen.height/2 ? windowRect.y : windowRect.y - Screen.height;
            }
        }

        /// <summary>
        /// Initialises main window Rect on first call.
        /// Intended to set the position from the configuration.
        /// </summary>
        private void InitialiseWindowRect()
        {
            if (init) return;

            windowRect = new Rect();
            windowRect.width = 350;
            windowRect.height = 140;
            if (ConfigurationPosition != null)
            {
                windowRect.x = ConfigurationPosition.x >= 0 ? ConfigurationPosition.x : Screen.width + ConfigurationPosition.x;
                windowRect.y = ConfigurationPosition.y >= 0 ? ConfigurationPosition.y : Screen.height + ConfigurationPosition.y;
            }
            else
            {
                windowRect.x = Screen.width - windowRect.width - 60;
                windowRect.y = 200;
            }

            init = true;
        }

        private void DoWindow(int id)
        {
            // Draw close button
            if (GUI.Button(new Rect(windowRect.width - 38, 8, 30, 30),
                "×", Elements.Buttons.Red))
                Visible = false;

            string sequential_id;

            try
            {
                sequential_id = BlockHandlers.GetID(block.Guid);
            }
            catch (KeyNotFoundException)
            {
                Visible = false;
                return;
            }
            // Sequential identifier field
            GUILayout.BeginHorizontal();

            GUILayout.TextField(sequential_id);
            if (GUILayout.Button("✂", Elements.Buttons.Red, GUILayout.Width(30)))
                ClipboardHelper.clipBoard = sequential_id;

            GUILayout.EndHorizontal();

            // GUID field
            GUILayout.BeginHorizontal();

            GUILayout.TextField(block.Guid.ToString());
            if (GUILayout.Button("✂", Elements.Buttons.Red, GUILayout.Width(30)))
                ClipboardHelper.clipBoard = block.Guid.ToString();

            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, windowRect.width, GUI.skin.window.padding.top));
        }
    }

    internal class ClipboardHelper
    {
        private static PropertyInfo m_systemCopyBufferProperty = null;
        private static PropertyInfo GetSystemCopyBufferProperty()
        {
            if (m_systemCopyBufferProperty == null)
            {
                Type T = typeof(GUIUtility);
                m_systemCopyBufferProperty = T.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
                if (m_systemCopyBufferProperty == null)
                    throw new Exception("Can't access 'GUIUtility.systemCopyBuffer'.");
            }
            return m_systemCopyBufferProperty;
        }
        public static string clipBoard
        {
            get
            {
                PropertyInfo P = GetSystemCopyBufferProperty();
                return (string)P.GetValue(null, null);
            }
            set
            {
                PropertyInfo P = GetSystemCopyBufferProperty();
                P.SetValue(null, value, null);
            }
        }
    }
}

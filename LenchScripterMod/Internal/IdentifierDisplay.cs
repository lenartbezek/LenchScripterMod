using System;
using System.Reflection;
using spaar.ModLoader;
using spaar.ModLoader.UI;
using UnityEngine;

namespace LenchScripter.Internal
{
    internal class IdentifierDisplay : MonoBehaviour
    {

        private GenericBlock block;
        internal bool Visible { get; set; }

        private int windowID = Util.GetWindowID();
        private Rect windowRect = new Rect(Screen.width / 2 - 350 / 2, Screen.height - 300, 350, 140);

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
            if (Visible)
            {
                GUI.skin = ModGUI.Skin;
                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                GUI.skin.window.padding.left = 8;
                GUI.skin.window.padding.right = 8;
                GUI.skin.window.padding.bottom = 8;
                windowRect = GUI.Window(windowID, windowRect, DoWindow, "Block Info");
            }
        }

        private void DoWindow(int id)
        {
            // Draw close button
            if (GUI.Button(new Rect(windowRect.width - 28, 8, 20, 20),
                "×", Elements.Buttons.Red))
                Visible = false;

            // Sequential identifier field
            GUILayout.BeginHorizontal();

            GUILayout.TextField(Scripter.Instance.buildingBlocks[block]);
            if (GUILayout.Button("✄", Elements.Buttons.Red, GUILayout.Width(30)))
                ClipboardHelper.clipBoard = Scripter.Instance.buildingBlocks[block];

            GUILayout.EndHorizontal();

            // GUID field
            GUILayout.BeginHorizontal();

            GUILayout.TextField(block.Guid.ToString());
            if (GUILayout.Button("✄", Elements.Buttons.Red, GUILayout.Width(30)))
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

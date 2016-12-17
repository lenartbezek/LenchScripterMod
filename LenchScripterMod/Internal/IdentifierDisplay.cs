using System.Collections.Generic;
using spaar.ModLoader;
using spaar.ModLoader.UI;
using UnityEngine;
// ReSharper disable PossibleLossOfFraction
// ReSharper disable UnusedMember.Local
// ReSharper disable ArrangeThisQualifier

namespace Lench.Scripter.Internal
{
    internal class IdentifierDisplay : SingleInstance<IdentifierDisplay>
    {
        private GenericBlock _block;

        internal Vector2 ConfigurationPosition;

        private bool _init;

        private readonly int _windowID = Util.GetWindowID();
        private Rect _windowRect;

        public override string Name => "IdentifierDisplay";

        internal bool Visible { get; set; }

        private static string Clipboard
        {
            get { return GUIUtility.systemCopyBuffer; }
            set { GUIUtility.systemCopyBuffer = value; }
        }

        internal void ShowBlock(GenericBlock block)
        {
            _block = block;
            Visible = true;
        }

        /// <summary>
        ///     Render window.
        /// </summary>
        private void OnGUI()
        {
            if (!Visible || Game.IsSimulating) return;

            InitialiseWindowRect();

            GUI.skin = ModGUI.Skin;
            GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
            GUI.skin.window.padding.left = 8;
            GUI.skin.window.padding.right = 8;
            GUI.skin.window.padding.bottom = 8;
            _windowRect = GUILayout.Window(_windowID, _windowRect, DoWindow, "Block Info", GUILayout.Height(100));

            ConfigurationPosition.x = _windowRect.x < Screen.width / 2 ? _windowRect.x : _windowRect.x - Screen.width;
            ConfigurationPosition.y = _windowRect.y < Screen.height / 2 ? _windowRect.y : _windowRect.y - Screen.height;
        }

        /// <summary>
        ///     Initialises main window Rect on first call.
        ///     Intended to set the position from the configuration.
        /// </summary>
        private void InitialiseWindowRect()
        {
            if (_init) return;

            _windowRect = new Rect
            {
                width = 350,
                height = 140,
                x = ConfigurationPosition.x >= 0
                    ? ConfigurationPosition.x
                    : Screen.width + ConfigurationPosition.x,
                y = ConfigurationPosition.y >= 0
                    ? ConfigurationPosition.y
                    : Screen.height + ConfigurationPosition.y
            };

            _init = true;
        }

        private void DoWindow(int id)
        {
            // Draw close button
            if (GUI.Button(new Rect(_windowRect.width - 38, 8, 30, 30),
                "×", Elements.Buttons.Red))
                Visible = false;

            string sequentialID;

            try
            {
                sequentialID = BlockHandlerController.GetID(_block.Guid);
            }
            catch (KeyNotFoundException)
            {
                Visible = false;
                return;
            }
            // Sequential identifier field
            GUILayout.BeginHorizontal();

            GUILayout.TextField(sequentialID);
            if (GUILayout.Button("✂", Elements.Buttons.Red, GUILayout.Width(30)))
                Clipboard = sequentialID;

            GUILayout.EndHorizontal();

            // GUID field
            GUILayout.BeginHorizontal();

            GUILayout.TextField(_block.Guid.ToString());
            if (GUILayout.Button("✂", Elements.Buttons.Red, GUILayout.Width(30)))
                Clipboard = _block.Guid.ToString();

            GUILayout.EndHorizontal();

            GUI.DragWindow(new Rect(0, 0, _windowRect.width, GUI.skin.window.padding.top));
        }
    }
}
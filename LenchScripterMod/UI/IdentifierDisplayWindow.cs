using System.Collections.Generic;
using spaar.ModLoader;
using spaar.ModLoader.UI;
using UnityEngine;
// ReSharper disable PossibleLossOfFraction
// ReSharper disable UnusedMember.Local
// ReSharper disable ArrangeThisQualifier

namespace Lench.Scripter.UI
{
    internal class IdentifierDisplayWindow
    {
        private GenericBlock _block;

        public bool Visible { get; set; }

        public Vector2 Position;

        public static string Clipboard
        {
            get { return GUIUtility.systemCopyBuffer; }
            set { GUIUtility.systemCopyBuffer = value; }
        }

        public void ShowBlock(GenericBlock block)
        {
            _block = block;
            Visible = true;
        }

        public IdentifierDisplayWindow()
        {
            var component = Mod.GameObject.AddComponent<IdentifierDisplayComponent>();
            component.Handler = this;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class IdentifierDisplayComponent : MonoBehaviour
        {
            public IdentifierDisplayWindow Handler;

            private bool _init;

            private readonly int _windowID = Util.GetWindowID();
            private Rect _windowRect;

            private void OnGUI()
            {
                if (!Elements.IsInitialized || StatMaster.isSimulating || Handler == null) return;

                InitialiseWindowRect();

                GUI.skin = ModGUI.Skin;
                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                _windowRect = GUILayout.Window(_windowID, _windowRect, DoWindow, "Block ID", GUILayout.Height(100));

                Handler.Position.x = _windowRect.x < Screen.width / 2 ? _windowRect.x : _windowRect.x - Screen.width;
                Handler.Position.y = _windowRect.y < Screen.height / 2 ? _windowRect.y : _windowRect.y - Screen.height;
            }

            private void InitialiseWindowRect()
            {
                if (_init) return;

                _windowRect = new Rect
                {
                    width = 350,
                    height = 140,
                    x = Handler.Position.x >= 0
                        ? Handler.Position.x
                        : Screen.width + Handler.Position.x,
                    y = Handler.Position.y >= 0
                        ? Handler.Position.y
                        : Screen.height + Handler.Position.y
                };

                _init = true;
            }

            private void DoWindow(int id)
            {
                // Draw close button
                if (GUI.Button(new Rect(_windowRect.width - 38, 8, 30, 30),
                    "×", Elements.Buttons.Red))
                    Handler.Visible = false;

                string sequentialID;

                try
                {
                    sequentialID = Block.GetID(Handler._block.Guid);
                }
                catch (KeyNotFoundException)
                {
                    Handler.Visible = false;
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

                GUILayout.TextField(Handler._block.Guid.ToString());
                if (GUILayout.Button("✂", Elements.Buttons.Red, GUILayout.Width(30)))
                    Clipboard = Handler._block.Guid.ToString();

                GUILayout.EndHorizontal();

                GUI.DragWindow(new Rect(0, 0, _windowRect.width, GUI.skin.window.padding.top));
            }
        }
    }
}
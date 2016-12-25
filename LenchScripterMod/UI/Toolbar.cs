using System;
using System.Collections;
using System.Collections.Generic;
using spaar.ModLoader;
using spaar.ModLoader.UI;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable UnusedMember.Local

namespace Lench.Scripter.UI
{
    /// <summary>
    ///     Extending toolbar along the left screen edge.
    /// </summary>
    internal class Toolbar
    {
        /// <summary>
        ///     Component that the toolbar object is added to.
        /// </summary>
        public static GameObject DefaultParentComponent => Mod.Controller;

        /// <summary>
        ///     Transform that is assigned as toolbars parent.
        /// </summary>
        public static Transform DefaultParentTransform => Mod.Controller.transform;

        /// <summary>
        ///     List containing all buttons in the toolbar.
        /// </summary>
        public List<Button> Buttons { get; set; } = new List<Button>();

        /// <summary>
        ///     Is toolbar visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        ///     Toolbar badge text that is visible when collapsed.
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        ///     Style of the toolbar label.
        /// </summary>
        public GUIStyle Style { get; set; } = new GUIStyle
        {
            normal = new GUIStyleState
            {
                textColor = Color.white
            },
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };

        /// <summary>
        ///     Texture applied on the toolbar header.
        /// </summary>
        public Texture2D Texture { get; set; } = null;

        /// <summary>
        ///     Vertical position along the left edge.
        /// </summary>
        public float Position { get; set; } = 400;

        private readonly ToolbarObject _toolbarObject;

        /// <summary>
        ///     Creates toolbar.
        /// </summary>
        public Toolbar()
        {
            _toolbarObject = DefaultParentComponent.AddComponent<ToolbarObject>();
            _toolbarObject.transform.parent = DefaultParentTransform;
            _toolbarObject.Handler = this;
        }

        /// <summary>
        ///     Creates toolbar with specified parents.
        /// </summary>
        public Toolbar(GameObject parentComponent, Transform parentTransform)
        {
            _toolbarObject = parentComponent.AddComponent<ToolbarObject>();
            _toolbarObject.transform.parent = parentTransform;
            _toolbarObject.Handler = this;
        }

        private void Destroy()
        {
            Object.Destroy(_toolbarObject);
        }

        /// <summary>
        ///     Button to be added to the toolbar.
        /// </summary>
        public class Button
        {
            /// <summary>
            ///     Called on button click.
            /// </summary>
            public Action OnClick;

            /// <summary>
            ///     Button text, displayed if there is no button texture.
            /// </summary>
            public string Text = "B";

            /// <summary>
            ///     Button style.
            /// </summary>
            public GUIStyle Style;

            /// <summary>
            ///     Texture that overrides text.
            /// </summary>
            public Texture2D Texture = null;

            /// <summary>
            /// Draws the button and returns if it was clicked or not.
            /// </summary>
            public bool Draw(Rect rect)
            {
                return Texture != null 
                    ? GUI.Button(rect, Texture, Style ?? Elements.Buttons.Disabled) 
                    : GUI.Button(rect, Text, Style ?? Elements.Buttons.Disabled);
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class ToolbarObject : MonoBehaviour
        {
            public Toolbar Handler;

            private const float XPos = -8;

            private readonly int _windowID = Util.GetWindowID();
            private Rect _windowRect;
            private bool _expanded;
            private bool _animating;
            private bool _init;

            private static Vector2 CollapsedSize => new Vector2(50, 46);
            private Vector2 ExpandedSize => new Vector2(50 + Handler.Buttons.Count * 38, 46);

            private bool ContainsMouse
            {
                get
                {
                    var mousePos = Input.mousePosition;
                    mousePos.y = Screen.height - mousePos.y;
                    return _windowRect.Contains(mousePos);
                }
            }

            private void OnGUI()
            {
                if (Handler == null) return;
                if (!Handler.Visible) return;

                try
                {
                    // Init skin
                    GUI.skin = ModGUI.Skin;
                    GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);

                    // Init rect
                    if (!_init)
                    {
                        _windowRect = new Rect(new Vector2(XPos, Handler.Position), _windowRect.size);
                        if (!_animating) _windowRect.size = _expanded ? ExpandedSize : CollapsedSize;
                        _init = true;
                    }
                }
                catch
                {
                    return;
                }

                // Expand and collapse on hover
                if (!_expanded && ContainsMouse) StartCoroutine(Expand());
                if (_expanded && !ContainsMouse) StartCoroutine(Collapse());

                // Draw window
                _windowRect = GUI.Window(_windowID, _windowRect, DoWindow, "", Elements.Windows.ClearDark);

                // Only make window draggable along the edge
                Handler.Position = _windowRect.position.y;
                Handler.Position = Mathf.Clamp(Handler.Position, 0, Screen.height - _windowRect.height);
                _windowRect.position = new Vector2(XPos, Handler.Position);
            }

            private void DoWindow(int id)
            {
                // Draw badge
                if (Handler.Texture == null)
                    GUI.Label(new Rect(10, 6, 32, 32), Handler.Text, Handler.Style);
                else
                    GUI.Label(new Rect(10, 6, 32, 32), Handler.Texture, Handler.Style);

                // Draw buttons
                if (_expanded || _animating)
                {
                    var xPos = 50;
                    foreach (var b in Handler.Buttons)
                    {
                        if (b.Draw(new Rect(xPos, 6, 32, 32))) b.OnClick.Invoke();
                        xPos += 38;
                    }
                }

                // Drag window
                GUI.DragWindow(new Rect(0, 0, _windowRect.width, _windowRect.height));
            }

            private IEnumerator Expand()
            {
#if DEBUG
            Debug.Log($"Expanding toolbar to {ExpandedSize}");
#endif
                _animating = true;
                _expanded = true;
                var size = _windowRect.size;
                var t = 0f;
                while (t <= 1f)
                {
                    if (!_expanded) yield break;
                    t += Time.deltaTime * 2;
                    size = Vector2.Lerp(size, ExpandedSize, t);
                    yield return _windowRect.size = size;
                }
                _animating = false;
            }

            private IEnumerator Collapse()
            {
#if DEBUG
            Debug.Log($"Collapsing toolbar to {CollapsedSize}");
#endif
                _animating = true;
                _expanded = false;
                var size = _windowRect.size;
                var t = 0f;
                while (t <= 1f)
                {
                    if (_expanded) yield break;
                    t += Time.deltaTime * 2;
                    size = Vector2.Lerp(size, CollapsedSize, t);
                    yield return _windowRect.size = size;
                }
                _animating = false;
            }
        }
    }
}
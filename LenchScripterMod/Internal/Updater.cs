using System;
using System.Collections;
using System.Collections.Generic;
using spaar.ModLoader;
using spaar.ModLoader.UI;
using SimpleJSON;
using UnityEngine;

// ReSharper disable UnusedMember.Local

namespace Lench.Scripter.Internal
{
    /// <summary>
    ///     Update checker.
    /// </summary>
    internal static class Updater
    {
        /// <summary>
        ///     Is update available. Checked on start.
        /// </summary>
        public static bool UpdateAvailable { get; private set; }

        /// <summary>
        ///     Current installed version.
        /// </summary>
        public static Version CurrentVersion { get; private set; }

        /// <summary>
        ///     Latest version available.
        /// </summary>
        public static Version LatestVersion { get; private set; }

        /// <summary>
        ///     Latest GitHub release name.
        /// </summary>
        public static string LatestReleaseName { get; private set; }

        /// <summary>
        ///     Latest GitHub release description body.
        /// </summary>
        public static string LatestReleaseBody { get; private set; }

        /// <summary>
        ///     Window visibility.
        /// </summary>
        public static bool Visible { get; set; }

        /// <summary>
        ///     GitHub API URL for checking the latest release.
        /// </summary>
        public static string API { get; private set; }

        /// <summary>
        ///     Update checker window name.
        /// </summary>
        public static string WindowName { get; set; }

        /// <summary>
        ///     Links to be displayed below the notification.
        /// </summary>
        public static List<Link> Links { get; set; }

        /// <summary>
        ///     Check for update.
        /// </summary>
        /// <param name="windowName">Title of the updater window.</param>
        /// <param name="api">GitHub API release url.</param>
        /// <param name="current">Current version.</param>
        /// <param name="links">Links to be displayed.</param>
        /// <param name="verbose">Verbose mode.</param>
        public static void Check(string windowName, string api, Version current, List<Link> links, bool verbose = false)
        {
            WindowName = windowName;
            API = api;
            CurrentVersion = current;
            Links = links;
            _component = Mod.Controller.AddComponent<UpdaterComponent>();
            _component.StartCoroutine(_component.Check(verbose));
        }

        private static UpdaterComponent _component;

        // ReSharper disable once ClassNeverInstantiated.Local
        private class UpdaterComponent : MonoBehaviour
        {
            private readonly int _windowID = Util.GetWindowID();
            private Rect _windowRect = new Rect(300, 300, 320, 100);

            public IEnumerator Check(bool verbose)
            {
                var www = new WWW(API);

                yield return www;

                if (!www.isDone || !string.IsNullOrEmpty(www.error))
                {
                    if (verbose) Debug.Log("=> Unable to connect.");
                    Destroy(this);
                    yield break;
                }

                var response = www.text;

                var release = JSON.Parse(response);
                LatestVersion = new Version(release["tag_name"].Value.Trim('v'));
                LatestReleaseName = release["name"].Value;
                LatestReleaseBody = release["body"].Value.Replace(@"\r\n", "\n");

                if (LatestVersion > CurrentVersion)
                {
                    if (verbose) Debug.Log("=> Update available: v" + LatestVersion + ": " + LatestReleaseName);
                    UpdateAvailable = true;
                    Visible = true;
                }
                else if (verbose)
                {
                    Debug.Log("=> Mod is up to date.");
                }
            }

            private void OnGUI()
            {
                if (!Visible) return;

                GUI.skin = ModGUI.Skin;
                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                _windowRect = GUILayout.Window(_windowID, _windowRect, DoWindow, WindowName);
            }

            private void DoWindow(int id)
            {
                // Draw release info
                GUILayout.Label("New update available",
                    new GUIStyle(Elements.Labels.Default) { alignment = TextAnchor.MiddleCenter });
                GUILayout.Label($"<b>v{LatestVersion}: {LatestReleaseName}</b>",
                    new GUIStyle(Elements.Labels.Default) { alignment = TextAnchor.MiddleCenter, fontSize = 16 });
                GUILayout.Label(LatestReleaseBody,
                    new GUIStyle(Elements.Labels.Default) { fontSize = 12, margin = new RectOffset(8, 8, 16, 16) });

                // Draw updater links
                foreach (var link in Links)
                    if (GUILayout.Button(link.DisplayName, Elements.Buttons.ComponentField))
                        Application.OpenURL(link.URL);

                // Draw close button
                if (GUI.Button(new Rect(_windowRect.width - 38, 8, 30, 30),
                    "×", Elements.Buttons.Red))
                    Destroy(this);

                // Drag window
                GUI.DragWindow(new Rect(0, 0, _windowRect.width, GUI.skin.window.padding.top));
            }
        }

        /// <summary>
        ///     Struct representing a link with a display name and URL.
        /// </summary>
        public struct Link
        {
            /// <summary>
            ///     Display name of the link.
            /// </summary>
            public string DisplayName;

            /// <summary>
            ///     Link URL.
            /// </summary>
            public string URL;
        }
    }
}
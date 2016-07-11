using System;
using System.Collections;
using System.Reflection;
using SimpleJSON;
using UnityEngine;
using spaar.ModLoader.UI;
using System.Collections.Generic;

namespace Lench.Scripter
{
    /// <summary>
    /// Overridable update checker.
    /// </summary>
    public class Updater : SingleInstance<Updater>
    {
        /// <summary>
        /// Struct representing a link with a display name and URL.
        /// </summary>
        public struct Link
        {
            /// <summary>
            /// Display name of the link.
            /// </summary>
            public string DisplayName;

            /// <summary>
            /// Link URL.
            /// </summary>
            public string URL;
        }

        /// <summary>
        /// Name of the object in the Unity hierarchy.
        /// </summary>
        public override string Name { get { return "LenchScripter Updater"; } }

        /// <summary>
        /// Is update available. Checked on start.
        /// </summary>
        public bool UpdateAvailable { get; private set; } = false;

        /// <summary>
        /// Current installed version.
        /// </summary>
        public virtual Version CurrentVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } } 

        /// <summary>
        /// Latest version available.
        /// </summary>
        public Version LatestVersion { get; private set; }

        /// <summary>
        /// Latest GitHub release name.
        /// </summary>
        public string LatestReleaseName { get; private set; }

        /// <summary>
        /// Latest GitHub release description body.
        /// </summary>
        public string LatestReleaseBody { get; private set; }

        /// <summary>
        /// Window visibility.
        /// </summary>
        public bool Visible { get; private set; } = false;

        /// <summary>
        /// GitHub API URL for checking the latest release.
        /// </summary>
        public virtual string APIURL { get; set; } = "https://api.github.com/repos/lench4991/LenchScripterMod/releases";

        /// <summary>
        /// Update checker window name.
        /// </summary>
        public virtual string WindowName { get; set; } = "Lench Scripter Mod";

        /// <summary>
        /// Links to be displayed below the notification.
        /// </summary>
        public virtual List<Link> Links { get; set; } = new List<Link>()
            {
                new Link() { DisplayName = "Spiderling forum page", URL = "http://forum.spiderlinggames.co.uk/index.php?threads/3003/" },
                new Link() { DisplayName = "GitHub release page", URL = "https://github.com/lench4991/LenchScripterMod/releases/latest"}
            };

        private int windowID = spaar.ModLoader.Util.GetWindowID();
        private Rect windowRect = new Rect(300, 300, 320, 100);

        private IEnumerator Start()
        {
            var www = new WWW(APIURL);

            yield return www;

            if (!www.isDone || !string.IsNullOrEmpty(www.error))
                yield break;

            string response = www.text;

            var releases = JSON.Parse(response);
            LatestVersion = new Version(releases[0]["tag_name"].Value.Trim('v'));
            LatestReleaseName = releases[0]["name"].Value;
            LatestReleaseBody = releases[0]["body"].Value.Replace(@"\r\n", "\n");

            if (LatestVersion > CurrentVersion)
            {
                UpdateAvailable = true;
                Visible = true;
            }
        }

        private void OnGUI()
        {
            if (Visible)
            {
                GUI.skin = ModGUI.Skin;
                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                GUI.skin.window.padding.left = 8;
                GUI.skin.window.padding.right = 8;
                GUI.skin.window.padding.bottom = 8;
                windowRect = GUILayout.Window(windowID, windowRect, DoWindow, WindowName);
            }
        }

        private void DoWindow(int id)
        {
            // Draw release info
            GUILayout.Label("New update available",
                new GUIStyle(Elements.Labels.Default) { alignment = TextAnchor.MiddleCenter });
            GUILayout.Label("<b>v" + LatestVersion + ": " + LatestReleaseName + "</b>",
                new GUIStyle(Elements.Labels.Default) { alignment = TextAnchor.MiddleCenter, fontSize = 16 });
            GUILayout.Label(LatestReleaseBody, new GUIStyle(Elements.Labels.Default) { fontSize = 12, margin = new RectOffset(8, 8, 16, 16) });

            // Draw updater links
            foreach (Link link in Links)
            {
                if (GUILayout.Button(link.DisplayName, Elements.Buttons.ComponentField))
                    System.Diagnostics.Process.Start(link.URL);
            }

            // Draw close button
            if (GUI.Button(new Rect(windowRect.width - 38, 8, 30, 30),
                "×", Elements.Buttons.Red))
            {
                Visible = false;
            }

            // Drag window
            GUI.DragWindow(new Rect(0, 0, windowRect.width, GUI.skin.window.padding.top));
        }
    }
}
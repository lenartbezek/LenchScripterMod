using System;
using System.IO;
using System.Linq;
using System.Net;
using spaar.ModLoader;
using spaar.ModLoader.UI;
using UnityEngine;

namespace Lench.Scripter.Internal
{
    internal class DependencyInstaller : SingleInstance<DependencyInstaller>
    {
        private static bool _downloadingInProgress;
        private static string _downloadButtonText = "Download";

        private static string _infoText = "<b>Lench Scripter Mod</b> needs to download additional assets.\n" +
                                          "Files will be placed in Mods/Resources/LenchScripter.";

        private static int _filesDownloaded;
        private const int FilesRequired = 5;

        private static readonly long[] ReceivedSize = new long[FilesRequired];
        private static readonly long[] TotalSize = new long[FilesRequired];

        private const string BaseUri = "http://lenartbezek.github.io/LenchScripterMod/files/";

        private static readonly string[] FileNames =
        {
            "IronPython.dll",
            "IronPython.Modules.dll",
            "Microsoft.Dynamic.dll",
            "Microsoft.Scripting.dll",
            "Microsoft.Scripting.Core.dll"
        };

        private readonly int _windowId = Util.GetWindowID();
        private Rect _windowRect = new Rect(0, 0, 200, 360);
        public override string Name => "Dependency Installer";

        public static bool Visible { get; set; }

        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Local
        private void OnGUI()
        {
            if (!Visible || !Elements.IsInitialized || Time.time < 1) return;

            GUI.skin = ModGUI.Skin;
            GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
            _windowRect.x = (Screen.width - _windowRect.width) / 2;
            _windowRect.y = Screen.height - 400;
            _windowRect = GUILayout.Window(_windowId, _windowRect, DoWindow, "Additional assets required",
                GUILayout.Height(200),
                GUILayout.Width(360));
        }

        private void DoWindow(int id)
        {
            // Draw info text
            GUILayout.Label(_infoText, new GUIStyle(Elements.Labels.Default) {alignment = TextAnchor.MiddleCenter},
                GUILayout.MinHeight(120));

            // Draw dowload button
            if (GUILayout.Button(_downloadButtonText) && !_downloadingInProgress)
                InstallIronPython();

            // Draw close button
            if (GUI.Button(new Rect(_windowRect.width - 38, 8, 30, 30),
                "×", Elements.Buttons.Red))
                Visible = false;
        }

        public static void InstallIronPython()
        {
            _filesDownloaded = 0;
            _downloadingInProgress = true;
            _downloadButtonText = "0.0 %";
            _infoText = "<b>Please wait</b>\n";
            if (!Directory.Exists(PythonEnvironment.LibPath))
                Directory.CreateDirectory(PythonEnvironment.LibPath);
            try
            {
                for (var fileIndex = 0; fileIndex < FilesRequired; fileIndex++)
                    using (var client = new WebClient())
                    {
                        var i = fileIndex;

                        // delete existing file
                        if (File.Exists(PythonEnvironment.LibPath + FileNames[i]))
                            File.Delete(PythonEnvironment.LibPath + FileNames[i]);

                        // progress handler
                        client.DownloadProgressChanged += (sender, e) =>
                        {
                            ReceivedSize[i] = e.BytesReceived;
                            TotalSize[i] = e.TotalBytesToReceive;
                            var progress = Convert.ToSingle(ReceivedSize.Sum()) / Convert.ToSingle(TotalSize.Sum()) *
                                           100f;
                            _downloadButtonText = progress.ToString("0.0") + " %";
                        };

                        // completion handler
                        client.DownloadFileCompleted += (sender, e) =>
                        {
                            if (e.Error != null)
                            {
                                // set error messages
                                ModConsole.AddMessage(LogType.Log,
                                    "[LenchScripterMod]: Error downloading file:" + FileNames[i]);
                                ModConsole.AddMessage(LogType.Error, "\t" + e.Error.Message);
                                _infoText = FileNames[i] + " <color=red>✘</color>" +
                                            "\n\n<b><color=red>Download failed</color></b>\n" + e.Error.Message;

                                _downloadingInProgress = false;
                                _downloadButtonText = "Retry";

                                // delete failed file
                                if (File.Exists(PythonEnvironment.LibPath + FileNames[i]))
                                    File.Delete(PythonEnvironment.LibPath + FileNames[i]);
                            }
                            else
                            {
                                ModConsole.AddMessage(LogType.Log,
                                    "[LenchScripterMod]: File downloaded: " + FileNames[i]);
                                _infoText += "\n" + FileNames[i] + " <color=green>✓</color>";

                                _filesDownloaded++;
                                if (_filesDownloaded != FilesRequired) return;

                                // finish download and load assemblies
                                _downloadButtonText = "Loading";
                                if (Script.LoadEngine(true))
                                {
                                    Visible = false;
                                }
                                else
                                {
                                    _downloadButtonText = "Retry";
                                    _infoText =
                                        "<b><color=red>Download failed</color></b>\nFailed to initialize Python engine.";
                                }
                                _downloadingInProgress = false;
                            }
                        };

                        // start download
                        client.DownloadFileAsync(
                            new Uri(BaseUri + PythonEnvironment.Version + "/" + FileNames[i]),
                            PythonEnvironment.LibPath + FileNames[i]);
                    }
            }
            catch (Exception e)
            {
                Debug.Log("[LenchScripterMod]: Error while downloading:");
                Debug.LogException(e);
                _downloadingInProgress = false;
                _downloadButtonText = "Retry";
                _infoText = "<b><color=red>Download failed</color></b>\n" + e.Message;
            }
        }
    }
}

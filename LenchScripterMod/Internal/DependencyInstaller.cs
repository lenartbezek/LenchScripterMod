using spaar.ModLoader;
using spaar.ModLoader.UI;
using System;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

namespace Lench.Scripter.Internal
{
    internal class DependencyInstaller : SingleInstance<DependencyInstaller>
    {
        public override string Name { get { return "Dependency Installer"; } }

        private static bool _downloadingInProgress = false;
        private static string _downloadButtonText = "Download";
        private static string _infoText = "<b>Lench Scripter Mod</b> needs to download additional assets.\n" +
                                          "Files will be placed in Mods/Resources/LenchScripter.";

        public bool Visible { get; set; } = false;
        public static string PythonVersion { get; set; } = "ironpython2.7/";
        private int _windowId = Util.GetWindowID();
        private Rect _windowRect = new Rect(0, 0, 200, 360);

        private void OnGUI()
        {
            if (Visible && Elements.IsInitialized && Time.time > 1)
            {
                GUI.skin = ModGUI.Skin;
                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                GUI.skin.window.padding.left = 8;
                GUI.skin.window.padding.right = 8;
                GUI.skin.window.padding.bottom = 8;
                _windowRect.x = (Screen.width - _windowRect.width) / 2;
                _windowRect.y = (Screen.height - 400);
                _windowRect = GUILayout.Window(_windowId, _windowRect, DoWindow, "Additional assets required",
                    GUILayout.Height(200),
                    GUILayout.Width(360));
            }
        }

        private void DoWindow(int id)
        {
            // Draw info text
            GUILayout.Label(_infoText, new GUIStyle(Elements.Labels.Default) { alignment = TextAnchor.MiddleCenter }, GUILayout.MinHeight(120));

            // Draw dowload button
            if (GUILayout.Button(_downloadButtonText) && !_downloadingInProgress)
            {
                InstallIronPython();
            }

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
            if (!Directory.Exists(Application.dataPath + "/Mods/Resources/LenchScripter/lib/" + PythonVersion))
                Directory.CreateDirectory(Application.dataPath + "/Mods/Resources/LenchScripter/lib/" + PythonVersion);
            try
            {
                for (int file_index = 0; file_index < _filesRequired; file_index++)
                {
                    using (var client = new WebClient())
                    {
                        var i = file_index;

                        // delete existing file
                        if (File.Exists(Application.dataPath + _libPath + PythonVersion + _fileNames[i]))
                            File.Delete(Application.dataPath + _libPath + PythonVersion + _fileNames[i]);

                        // progress handler
                        client.DownloadProgressChanged += (sender, e) =>
                        {
                            _receivedSize[i] = e.BytesReceived;
                            _totalSize[i] = e.TotalBytesToReceive;
                            var progress = Convert.ToSingle(_receivedSize.Sum()) / Convert.ToSingle(_totalSize.Sum()) * 100f;
                            _downloadButtonText = progress.ToString("0.0") + " %";
                        };

                        // completion handler
                        client.DownloadFileCompleted += (sender, e) =>
                        {
                            if (e.Error != null)
                            {
                                // set error messages
                                ModConsole.AddMessage(LogType.Log, "[LenchScripterMod]: Error downloading file:" + _fileNames[i]);
                                ModConsole.AddMessage(LogType.Error, "\t" + e.Error.Message);
                                _infoText = _fileNames[i] + " <color=red>✘</color>" +
                                            "\n\n<b><color=red>Download failed</color></b>\n" + e.Error.Message;

                                _downloadingInProgress = false;
                                _downloadButtonText = "Retry";

                                // delete failed file
                                if (File.Exists(Application.dataPath + _libPath + PythonVersion + _fileNames[i]))
                                    File.Delete(Application.dataPath + _libPath + PythonVersion + _fileNames[i]);
                            }
                            else
                            {
                                ModConsole.AddMessage(LogType.Log, "[LenchScripterMod]: File downloaded: " + _fileNames[i]);
                                _infoText += "\n" + _fileNames[i] + " <color=green>✓</color>";

                                _filesDownloaded++;
                                if (_filesDownloaded == _filesRequired)
                                {
                                    // finish download and load assemblies
                                    _downloadButtonText = "Loading";
                                    if (ScripterMod.LoadScripter())
                                    {
                                        Instance.Visible = false;
                                    }
                                    else
                                    {
                                        _downloadButtonText = "Retry";
                                        _infoText = "<b><color=red>Download failed</color></b>\nFailed to initialize Python engine.";
                                    }
                                    _downloadingInProgress = false;
                                }
                            }
                        };

                        // start download
                        client.DownloadFileAsync(
                            new Uri(_baseUri + PythonVersion + _fileNames[i]),
                            Application.dataPath + _libPath + PythonVersion + _fileNames[i]);
                    }
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

        private static int _filesDownloaded;
        private static readonly int _filesRequired = 5;

        private static long[] _receivedSize = new long[_filesRequired];
        private static long[] _totalSize = new long[_filesRequired];

        private static readonly string _baseUri = "http://lench4991.github.io/LenchScripterMod/files/";
        private static readonly string _libPath = "/Mods/Resources/LenchScripter/lib/";
        private static readonly string[] _fileNames =
        {
            "IronPython.dll",
            "IronPython.Modules.dll",
            "Microsoft.Dynamic.dll",
            "Microsoft.Scripting.dll",
            "Microsoft.Scripting.Core.dll"
        };
    }
}

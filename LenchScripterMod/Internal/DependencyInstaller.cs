using spaar.ModLoader;
using spaar.ModLoader.UI;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

namespace Lench.Scripter.Internal
{
    internal class DependencyInstaller : SingleInstance<DependencyInstaller>
    {
        public override string Name { get { return "Dependency Installer"; } }

        private static bool downloading_in_progress = false;
        private static string download_button_text = "Download";
        private static string info_text = "<b>Lench Scripter Mod</b> needs to download additional assets.\n" +
                                          "Files will be placed in Mods/Resources/LenchScripter.";

        internal bool Visible { get; set; } = false;

        private int windowID = Util.GetWindowID();
        private Rect windowRect;

        private void OnGUI()
        {
            if (Visible && ModGUI.Skin != null)
            {
                GUI.skin = ModGUI.Skin;
                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                GUI.skin.window.padding.left = 8;
                GUI.skin.window.padding.right = 8;
                GUI.skin.window.padding.bottom = 8;
                windowRect = GUILayout.Window(windowID, windowRect, DoWindow, "Additional assets required",
                    GUILayout.Height(200),
                    GUILayout.Width(360));
                windowRect.x = (Screen.width - windowRect.width) / 2;
                windowRect.y = (Screen.height - 400);
            }
        }

        private void DoWindow(int id)
        {
            GUILayout.Label(info_text, new GUIStyle(Elements.Labels.Default) { alignment = TextAnchor.MiddleCenter }, GUILayout.MinHeight(120));

            if (GUILayout.Button(download_button_text) && !downloading_in_progress)
            {
                InstallIronPython();
            }

            // Draw close button
            if (GUI.Button(new Rect(windowRect.width - 38, 8, 30, 30),
                "×", Elements.Buttons.Red))
                Visible = false;
        }

        private static void InstallIronPython()
        {
            downloading_in_progress = true;
            download_button_text = "0.0 %";
            info_text = "<b>Please wait</b>\n";
            if (!Directory.Exists(Application.dataPath + @"\Mods\Resources\LenchScripter\lib\"))
                Directory.CreateDirectory(Application.dataPath + @"\Mods\Resources\LenchScripter\lib\");
            try
            {
                for (int file_index = 0; file_index < files_required; file_index++)
                {
                    using (var client = new WebClient())
                    {
                        var i = file_index;

                        // delete existing file
                        if (File.Exists(Application.dataPath + file_paths[i]))
                            File.Delete(Application.dataPath + file_paths[i]);

                        // progress handler
                        client.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) =>
                        {
                            received_size[i] = e.BytesReceived;
                            float progress = (Convert.ToSingle(received_size.Sum()) / Convert.ToSingle(total_size.Sum()) * 100f);
                            download_button_text = progress.ToString("0.0") + " %";
                        };

                        // completion handler
                        client.DownloadFileCompleted += (object sender, AsyncCompletedEventArgs e) =>
                        {
                            if (e.Error != null)
                            {
                                // set error messages
                                ModConsole.AddMessage(LogType.Log, "[LenchScripterMod]: Error downloading file:" + file_paths[i].Split('\\').Last());
                                ModConsole.AddMessage(LogType.Error, "\t" + e.Error.Message);
                                info_text = file_paths[i].Split('\\').Last() + " <color=red>✘</color>" +
                                            "\n\n<b><color=red>Download failed</color></b>\n" + e.Error.Message;

                                downloading_in_progress = false;
                                download_button_text = "Retry";

                                // delete failed file
                                if (File.Exists(Application.dataPath + file_paths[i]))
                                    File.Delete(Application.dataPath + file_paths[i]);
                            }
                            else
                            {
                                ModConsole.AddMessage(LogType.Log, "[LenchScripterMod]: File downloaded: " + file_paths[i].Split('\\').Last());
                                info_text += "\n" + file_paths[i].Split('\\').Last() + " <color=green>✓</color>";

                                files_downloaded++;
                                if (files_downloaded == files_required)
                                {
                                    // finish download and load assemblies
                                    if (PythonEnvironment.LoadPythonAssembly())
                                    {
                                        download_button_text = "Complete";
                                        ScripterMod.LoadScripter();
                                        PythonEnvironment.ScripterEnvironment = new PythonEnvironment();
                                        Instance.Visible = false;
                                        Destroy(Instance);
                                    }
                                    else
                                    {
                                        download_button_text = "Error";
                                    }
                                }
                            }
                        };

                        // start download
                        client.DownloadFileAsync(
                            file_uris[i],
                            Application.dataPath + file_paths[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log("[LenchScripterMod]: Error while downloading:");
                Debug.LogException(e);
                downloading_in_progress = false;
                download_button_text = "Retry";
                info_text = "<b><color=red>Download failed</color></b>\n" + e.Message;
            }
        }

        private static int files_downloaded = 0;
        private static int files_required = 5;

        private static long[] received_size = new long[files_required];
        private static long[] total_size = new long[]
        {
            1805824,
            727040,
            1033728,
            142848,
            383488
        };

        private static Uri[] file_uris = new Uri[]
        {
            new Uri("http://lench4991.github.io/LenchScripterMod/files/IronPython.dll"),
            new Uri("http://lench4991.github.io/LenchScripterMod/files/IronPython.Modules.dll"),
            new Uri("http://lench4991.github.io/LenchScripterMod/files/Microsoft.Dynamic.dll"),
            new Uri("http://lench4991.github.io/LenchScripterMod/files/Microsoft.Scripting.dll"),
            new Uri("http://lench4991.github.io/LenchScripterMod/files/Microsoft.Scripting.Core.dll")
        };
        private static string[] file_paths = new string[]
        {
            @"\Mods\Resources\LenchScripter\lib\IronPython.dll",
            @"\Mods\Resources\LenchScripter\lib\IronPython.Modules.dll",
            @"\Mods\Resources\LenchScripter\lib\Microsoft.Dynamic.dll",
            @"\Mods\Resources\LenchScripter\lib\Microsoft.Scripting.dll",
            @"\Mods\Resources\LenchScripter\lib\Microsoft.Scripting.Core.dll"
        };
    }
}

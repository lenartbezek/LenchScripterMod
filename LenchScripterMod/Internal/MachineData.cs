using System;
using System.IO;
using System.Reflection;

namespace Lench.Scripter.Internal
{
    internal static class MachineData
    {
        public static event Action<string> OnLoadSuccess;
        public static event Action<string> OnLoadWarning;
        public static event Action<string> OnSaveSuccess;
        public static event Action<string> OnSaveWarning;

        public static void Load(MachineInfo machineInfo)
        {
            Script.FileName = machineInfo.Name;
            if (!machineInfo.MachineData.HasKey("LenchScripterMod-Version"))
            {
                Script.EmbeddedCode = null;
                OnLoadWarning?.Invoke("No embedded code found.");
            }
            else
            {
                var version = new Version(machineInfo.MachineData.ReadString("LenchScripterMod-Version").TrimStart('v'));
                if (version > Assembly.GetExecutingAssembly().GetName().Version)
                    OnLoadWarning?.Invoke($"Loaded code is from a newer version v{version}.\nSome features might be incompatible.");
                if (new Version(2, 0, 0) > version)
                    OnLoadWarning?.Invoke($"Loaded code is from version v{version}.\nLua code is no longer supported.");
                var code = machineInfo.MachineData.ReadString("LenchScripterMod-Code");
                Script.EmbeddedCode = code;
                OnLoadSuccess?.Invoke("Successfully loaded embedded code.");
            }

            Script.SetSource();
        }

        public static void Save(MachineInfo machineInfo)
        {
            if (Script.SaveToBsg)
            {
                var code = File.ReadAllText(Script.FilePath);
                machineInfo.MachineData.Write("LenchScripterMod-Version",
                    Assembly.GetExecutingAssembly().GetName().Version.ToString());
                machineInfo.MachineData.Write("LenchScripterMod-Code", code);
                Script.EmbeddedCode = code;
                OnSaveSuccess?.Invoke("Successfully embedded code.");
            }
            else
            {
                if (Script.EmbeddedCode != null)
                    OnSaveWarning?.Invoke("Embedded code has not been updated.");
            }
        }
    }
}
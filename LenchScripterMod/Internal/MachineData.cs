using System;
using System.IO;
using System.Reflection;

namespace Lench.Scripter.Internal
{
    internal static class MachineData
    { 

        internal static void Load(MachineInfo machineInfo)
        {
            Scripter.Instance.ScriptOptions.ScriptName = machineInfo.Name;
            if (!machineInfo.MachineData.HasKey("LenchScripterMod-Version"))
            {
                Scripter.Instance.ScriptOptions.BsgHasCode = false;
                Scripter.Instance.ScriptOptions.Code = null;
            }
            else
            {
                var version = new Version(machineInfo.MachineData.ReadString("LenchScripterMod-Version").TrimStart('v'));
                if (version > Assembly.GetExecutingAssembly().GetName().Version)
                    Scripter.Instance.ScriptOptions.NoteMessage = "Loaded code is from a newer (v" + version + ") version.\nSome features might be incompatible.";
                if (new Version(2, 0, 0) > version)
                    Scripter.Instance.ScriptOptions.ErrorMessage = "Loaded code is from version v" + version + ".\nLua code is no longer supported.";
                var code = machineInfo.MachineData.ReadString("LenchScripterMod-Code");
                Scripter.Instance.ScriptOptions.Code = code;
                Scripter.Instance.ScriptOptions.BsgHasCode = true;
                Scripter.Instance.ScriptOptions.SuccessMessage = "Successfully loaded code from .bsg.";
            }

            Scripter.Instance.ScriptOptions.CheckForScript();
        }

        internal static void Save(MachineInfo machineInfo)
        {
            Scripter.Instance.ScriptOptions.CheckForScript();
            if (Scripter.Instance.ScriptOptions.SaveToBsg)
            {
                Scripter.Instance.ScriptOptions.CheckForScript();
                var code = File.ReadAllText(Scripter.Instance.ScriptOptions.ScriptPath);
                machineInfo.MachineData.Write("LenchScripterMod-Version", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                machineInfo.MachineData.Write("LenchScripterMod-Code", code);
                Scripter.Instance.ScriptOptions.Code = code;
                Scripter.Instance.ScriptOptions.BsgHasCode = true;
                Scripter.Instance.ScriptOptions.SuccessMessage = "Successfully saved code to .bsg.";
                Scripter.Instance.ScriptOptions.NoteMessage = null;
            }
            else
            {
                Scripter.Instance.ScriptOptions.SuccessMessage = null;
                if (Scripter.Instance.ScriptOptions.Code != null)
                    Scripter.Instance.ScriptOptions.NoteMessage = "Code has not been saved to .bsg file.";
            }
        }
    }
}

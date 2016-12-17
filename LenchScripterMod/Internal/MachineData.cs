using System;
using System.IO;
using System.Reflection;

namespace Lench.Scripter.Internal
{
    internal static class MachineData
    {
        internal static void Load(MachineInfo machineInfo)
        {
            ScriptOptions.Instance.ScriptName = machineInfo.Name;
            if (!machineInfo.MachineData.HasKey("LenchScripterMod-Version"))
            {
                ScriptOptions.Instance.BsgHasCode = false;
                ScriptOptions.Instance.Code = null;
            }
            else
            {
                var version = new Version(machineInfo.MachineData.ReadString("LenchScripterMod-Version").TrimStart('v'));
                if (version > Assembly.GetExecutingAssembly().GetName().Version)
                    ScriptOptions.Instance.NoteMessage = $"Loaded code is from a newer version v{version}.\nSome features might be incompatible.";
                if (new Version(2, 0, 0) > version)
                    ScriptOptions.Instance.ErrorMessage = $"Loaded code is from version v{version}.\nLua code is no longer supported.";
                var code = machineInfo.MachineData.ReadString("LenchScripterMod-Code");
                ScriptOptions.Instance.Code = code;
                ScriptOptions.Instance.BsgHasCode = true;
                ScriptOptions.Instance.SuccessMessage = "Successfully loaded code from .bsg.";
            }

            ScriptOptions.Instance.CheckForScript();
        }

        internal static void Save(MachineInfo machineInfo)
        {
            ScriptOptions.Instance.CheckForScript();
            if (ScriptOptions.Instance.SaveToBsg)
            {
                ScriptOptions.Instance.CheckForScript();
                var code = File.ReadAllText(ScriptOptions.Instance.ScriptPath);
                machineInfo.MachineData.Write("LenchScripterMod-Version",
                    Assembly.GetExecutingAssembly().GetName().Version.ToString());
                machineInfo.MachineData.Write("LenchScripterMod-Code", code);
                ScriptOptions.Instance.Code = code;
                ScriptOptions.Instance.BsgHasCode = true;
                ScriptOptions.Instance.SuccessMessage = "Successfully saved code to .bsg.";
                ScriptOptions.Instance.NoteMessage = null;
            }
            else
            {
                ScriptOptions.Instance.SuccessMessage = null;
                if (ScriptOptions.Instance.Code != null)
                    ScriptOptions.Instance.NoteMessage = "Code has not been saved to .bsg file.";
            }
        }
    }
}
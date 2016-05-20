using System.IO;

namespace LenchScripter.Internal
{
    internal static class MachineData
    { 

        internal static void Load(MachineInfo machineInfo)
        {
            Scripter.Instance.ScriptOptions.ScriptName = machineInfo.Name;
            if (!machineInfo.MachineData.HasKey("LenchScripterMod-Version")) return;
            var version = machineInfo.MachineData.ReadString("LenchScripterMod-Version");
            var code = machineInfo.MachineData.ReadString("LenchScripterMod-Code");
            Scripter.Instance.ScriptOptions.Code = code;
            Scripter.Instance.ScriptOptions.BsgHasCode = true;
            Scripter.Instance.ScriptOptions.SuccessMessage = "Successfully loaded code from .bsg.";
            Scripter.Instance.ScriptOptions.CheckForScript();
        }

        internal static void Save(MachineInfo machineInfo)
        {
            Scripter.Instance.ScriptOptions.ScriptName = machineInfo.Name;
            Scripter.Instance.ScriptOptions.CheckForScript();
            if (Scripter.Instance.ScriptOptions.SaveToBsg)
            {
                Scripter.Instance.ScriptOptions.CheckForScript();
                var code = File.ReadAllText(Scripter.Instance.ScriptOptions.ScriptPath);
                machineInfo.MachineData.Write("LenchScripterMod-Version", "v2.0.0");
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

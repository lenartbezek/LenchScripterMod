using System.IO;

namespace LenchScripter.Internal
{
    internal static class MachineData
    { 

        internal static void Load(MachineInfo machineInfo)
        {
            ScripterMod.ScriptOptions.ScriptName = machineInfo.Name;
            if (!machineInfo.MachineData.HasKey("LenchScripterMod-Version")) return;
            var version = machineInfo.MachineData.ReadString("LenchScripterMod-Version");
            var code = machineInfo.MachineData.ReadString("LenchScripterMod-Code");
            ScripterMod.ScriptOptions.Code = code;
            ScripterMod.ScriptOptions.BsgHasCode = true;
            ScripterMod.ScriptOptions.SuccessMessage = "Successfully loaded code from .bsg.";
            ScripterMod.ScriptOptions.CheckForScript();
        }

        internal static void Save(MachineInfo machineInfo)
        {
            ScripterMod.ScriptOptions.ScriptName = machineInfo.Name;
            ScripterMod.ScriptOptions.CheckForScript();
            if (ScripterMod.ScriptOptions.SaveToBsg)
            {
                ScripterMod.ScriptOptions.CheckForScript();
                var code = File.ReadAllText(ScripterMod.ScriptOptions.ScriptPath);
                machineInfo.MachineData.Write("LenchScripterMod-Version", "v2.0.0");
                machineInfo.MachineData.Write("LenchScripterMod-Code", code);
                ScripterMod.ScriptOptions.Code = code;
                ScripterMod.ScriptOptions.BsgHasCode = true;
                ScripterMod.ScriptOptions.SuccessMessage = "Successfully saved code to .bsg.";
                ScripterMod.ScriptOptions.NoteMessage = null;
            }
            else
            {
                ScripterMod.ScriptOptions.SuccessMessage = null;
                if (ScripterMod.ScriptOptions.Code != null)
                    ScripterMod.ScriptOptions.NoteMessage = "Code has not been included in .bsg file.";
            }
        }
    }
}

using System.IO;

namespace LenchScripter.Internal
{
    internal static class MachineData
    { 

        internal static void Load(MachineInfo machineInfo)
        {
            ScripterMod.ScriptOptions.ScriptName = machineInfo.Name;
            ScripterMod.ScriptOptions.CheckForScript();
            if (!machineInfo.MachineData.HasKey("LenchScripterMod-Version")) return;
            var version = machineInfo.MachineData.ReadString("LenchScripterMod-Version");
            var code = machineInfo.MachineData.ReadString("LenchScripterMod-Code");
            ScripterMod.ScriptOptions.Code = code;
            ScripterMod.ScriptOptions.BsgHasCode = true;
            ScripterMod.ScriptOptions.SuccessMessage = "Successfully loaded code from .bsg.";
        }

        internal static void Save(MachineInfo machineInfo)
        {
            if (ScripterMod.ScriptOptions.SaveToBsg)
            {
                ScripterMod.ScriptOptions.CheckForScript();
                var code = File.ReadAllText(ScripterMod.ScriptOptions.ScriptPath);
                machineInfo.MachineData.Write("LenchScripterMod-Version", "v1.1.0");
                machineInfo.MachineData.Write("LenchScripterMod-Code", code);
                ScripterMod.ScriptOptions.Code = code;
                ScripterMod.ScriptOptions.BsgHasCode = true;
                ScripterMod.ScriptOptions.SuccessMessage = "Successfully saved code to .bsg.";
                ScripterMod.ScriptOptions.NoteMessage = null;
            }
            else
            {
                machineInfo.MachineData.EraseCustomData();
            }
        }
    }
}

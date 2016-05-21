using System;
using System.IO;
using System.Reflection;
using spaar.ModLoader;
using UnityEngine;
using LenchScripter.Internal;
using System.Security.Policy;

namespace LenchScripter
{
    /// <summary>
    /// Mod class loaded by the Mod Loader.
    /// </summary>
    public class ScripterMod : Mod
    {
        public override string Name { get; } = "Lench Scripter Mod";
        public override string DisplayName { get; } = "Lench Scripter Mod";
        public override string Author { get; } = "Lench";
        public override Version Version { get; } = new Version(2, 0, 0);
        public override string VersionExtra { get; } = "";
        public override string BesiegeVersion { get; } = "v0.27";
        public override bool CanBeUnloaded { get; } = true;
        public override bool Preload { get; } = false;

        internal static Type blockScriptType;

        /// <summary>
        /// Is LenchScripterMod loaded.
        /// </summary>
        public static bool Loaded { get { return Scripter.Instance != null; } }

        /// <summary>
        /// Instantiates the mod and it's components.
        /// Looks for and loads assemblies.
        /// </summary>
        public override void OnLoad()
        {
            UnityEngine.Object.DontDestroyOnLoad(Scripter.Instance);
            Game.OnSimulationToggle += Scripter.Instance.OnSimulationToggle;
            Game.OnBlockPlaced += (Transform block) => BlockHandlers.rebuildDict = true;
            Game.OnBlockRemoved += () => BlockHandlers.rebuildDict = true;
            XmlSaver.OnSave += Internal.MachineData.Save;
            XmlLoader.OnLoad += Internal.MachineData.Load;

            LoadBlockLoaderAssembly();

            Internal.Configuration.Load();

            Keybindings.AddKeybinding("Show Block ID", new Key(KeyCode.None, KeyCode.LeftShift));
            Keybindings.AddKeybinding("Watchlist", new Key(KeyCode.LeftControl, KeyCode.I));
            Keybindings.AddKeybinding("Script Options", new Key(KeyCode.LeftControl, KeyCode.U));

            Commands.RegisterCommand("python", Scripter.Instance.InteractiveCommand, "Executes Python expression.");

            SettingsMenu.RegisterSettingsButton("SCRIPT", Scripter.Instance.RunScriptSettingToggle, true, 12);
        }

        /// <summary>
        /// Disables the mod from executing scripts.
        /// Destroys GameObjects.
        /// </summary>
        public override void OnUnload()
        {
            Game.OnSimulationToggle -= Scripter.Instance.OnSimulationToggle;
            Game.OnBlockPlaced -= (Transform block) => BlockHandlers.rebuildDict = true;
            Game.OnBlockRemoved -= () => BlockHandlers.rebuildDict = true;
            XmlSaver.OnSave -= Internal.MachineData.Save;
            XmlLoader.OnLoad -= Internal.MachineData.Load;

            Scripter.Instance.OnSimulationToggle(false);

            Internal.Configuration.Save();

            UnityEngine.Object.Destroy(Scripter.Instance);
        }

        /// <summary>
        /// Attempts to load TGYD's BlockLoader assembly.
        /// </summary>
        /// <returns>Returns true if successfull.</returns>
        private bool LoadBlockLoaderAssembly()
        {
            Assembly blockLoaderAssembly;
            try
            {
                blockLoaderAssembly = Assembly.LoadFrom(Application.dataPath + "/Mods/BlockLoader.dll");
            }
            catch (FileNotFoundException)
            {
                return false;
            }

            foreach (Type type in blockLoaderAssembly.GetExportedTypes())
            {
                if (type.FullName == "BlockScript")
                    blockScriptType = type;
            }

            if (blockScriptType == null)
                return false;

            return true;
        }
    }
}

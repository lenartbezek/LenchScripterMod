using UnityEngine;
using static spaar.ModLoader.Configuration;

namespace Lench.Scripter.Internal
{
    internal static class Configuration
    {
        internal static void Load()
        {
            Mod.UpdateCheckerEnabled = GetBool("mod-updater-enabled", true);
            Script.Enabled = GetBool("script-enabled", true);

            Mod.WatchlistWindow.Position = new Vector2
            {
                x = GetFloat("WatchlistXPos", -380),
                y = GetFloat("WatchlistYPos", 200)
            };

            Mod.IdentifierDisplayWindow.Position = new Vector2
            {
                x = GetFloat("IdentifierDisplayXPos", 900),
                y = GetFloat("IdentifierDisplayYPos", -240)
            };

            Mod.ScriptOptionsWindow.Position = new Vector2
            {
                x = GetFloat("ScriptOptionsXPos", -380),
                y = GetFloat("ScriptOptionsYPos", -400)
            };

            Mod.Toolbar.Position = GetFloat("ToolbarPos", 400);

            PythonEnvironment.Version = GetString("PythonVersion", "ironpython2.7");
        }

        internal static void Save()
        {
            SetBool("mod-updater-enabled", Mod.UpdateCheckerEnabled);
            SetBool("script-enabled", Script.Enabled);

            SetFloat("WatchlistXPos", Mod.WatchlistWindow.Position.x);
            SetFloat("WatchlistYPos", Mod.WatchlistWindow.Position.y);

            SetFloat("IdentifierDisplayXPos", Mod.IdentifierDisplayWindow.Position.x);
            SetFloat("IdentifierDisplayYPos", Mod.IdentifierDisplayWindow.Position.y);

            SetFloat("ScriptOptionsXPos", Mod.ScriptOptionsWindow.Position.x);
            SetFloat("ScriptOptionsYPos", Mod.ScriptOptionsWindow.Position.y);

            SetFloat("ToolbarPos", Mod.Toolbar.Position);

            SetString("PythonVersion", PythonEnvironment.Version);

            spaar.ModLoader.Configuration.Save();
        }
    }
}
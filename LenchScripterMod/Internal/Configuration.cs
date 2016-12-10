using UnityEngine;

namespace Lench.Scripter.Internal
{
    internal static class Configuration
    {
        internal static void Load()
        {
            Scripter.Instance.ModUpdaterEnabled = spaar.ModLoader.Configuration.GetBool("mod-updater-enabled", true);

            Watchlist.Instance.ConfigurationPosition = new Vector2();
            Watchlist.Instance.ConfigurationPosition.x = spaar.ModLoader.Configuration.GetFloat("WatchlistXPos", -380);
            Watchlist.Instance.ConfigurationPosition.y = spaar.ModLoader.Configuration.GetFloat("WatchlistYPos", 200);

            IdentifierDisplay.Instance.ConfigurationPosition = new Vector2();
            IdentifierDisplay.Instance.ConfigurationPosition.x = spaar.ModLoader.Configuration.GetFloat("IdentifierDisplayXPos", 900);
            IdentifierDisplay.Instance.ConfigurationPosition.y = spaar.ModLoader.Configuration.GetFloat("IdentifierDisplayYPos", -240);

            ScriptOptions.Instance.ConfigurationPosition = new Vector2();
            ScriptOptions.Instance.ConfigurationPosition.x = spaar.ModLoader.Configuration.GetFloat("ScriptOptionsXPos", -380);
            ScriptOptions.Instance.ConfigurationPosition.y = spaar.ModLoader.Configuration.GetFloat("ScriptOptionsYPos", -400);

            PythonEnvironment.Version = spaar.ModLoader.Configuration.GetString("PythonVersion", "ironpython2.7");
        }

        internal static void Save()
        {
            spaar.ModLoader.Configuration.SetBool("mod-updater-enabled", Scripter.Instance.ModUpdaterEnabled);

            spaar.ModLoader.Configuration.SetFloat("WatchlistXPos", Watchlist.Instance.ConfigurationPosition.x);
            spaar.ModLoader.Configuration.SetFloat("WatchlistYPos", Watchlist.Instance.ConfigurationPosition.y);

            spaar.ModLoader.Configuration.SetFloat("IdentifierDisplayXPos", IdentifierDisplay.Instance.ConfigurationPosition.x);
            spaar.ModLoader.Configuration.SetFloat("IdentifierDisplayYPos", IdentifierDisplay.Instance.ConfigurationPosition.y);

            spaar.ModLoader.Configuration.SetFloat("ScriptOptionsXPos", ScriptOptions.Instance.ConfigurationPosition.x);
            spaar.ModLoader.Configuration.SetFloat("ScriptOptionsYPos", ScriptOptions.Instance.ConfigurationPosition.y);

            spaar.ModLoader.Configuration.SetString("PythonVersion", PythonEnvironment.Version);

            spaar.ModLoader.Configuration.Save();
        }
    }
}

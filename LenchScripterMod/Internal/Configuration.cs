using UnityEngine;

namespace LenchScripter.Internal
{
    internal static class Configuration
    {
        internal static void Load()
        {
            Scripter.Instance.Watchlist.ConfigurationPosition = new Vector2();
            Scripter.Instance.Watchlist.ConfigurationPosition.x = spaar.ModLoader.Configuration.GetFloat("WatchlistXPos", -380);
            Scripter.Instance.Watchlist.ConfigurationPosition.y = spaar.ModLoader.Configuration.GetFloat("WatchlistYPos", 200);

            Scripter.Instance.IdentifierDisplay.ConfigurationPosition = new Vector2();
            Scripter.Instance.IdentifierDisplay.ConfigurationPosition.x = spaar.ModLoader.Configuration.GetFloat("IdentifierDisplayXPos", 900);
            Scripter.Instance.IdentifierDisplay.ConfigurationPosition.y = spaar.ModLoader.Configuration.GetFloat("IdentifierDisplayYPos", -240);

            Scripter.Instance.ScriptOptions.ConfigurationPosition = new Vector2();
            Scripter.Instance.ScriptOptions.ConfigurationPosition.x = spaar.ModLoader.Configuration.GetFloat("ScriptOptionsXPos", -380);
            Scripter.Instance.ScriptOptions.ConfigurationPosition.y = spaar.ModLoader.Configuration.GetFloat("ScriptOptionsYPos", -400);
        }

        internal static void Save()
        {
            spaar.ModLoader.Configuration.SetFloat("WatchlistXPos", Scripter.Instance.Watchlist.ConfigurationPosition.x);
            spaar.ModLoader.Configuration.SetFloat("WatchlistYPos", Scripter.Instance.Watchlist.ConfigurationPosition.y);

            spaar.ModLoader.Configuration.SetFloat("IdentifierDisplayXPos", Scripter.Instance.IdentifierDisplay.ConfigurationPosition.x);
            spaar.ModLoader.Configuration.SetFloat("IdentifierDisplayYPos", Scripter.Instance.IdentifierDisplay.ConfigurationPosition.y);

            spaar.ModLoader.Configuration.SetFloat("ScriptOptionsXPos", Scripter.Instance.ScriptOptions.ConfigurationPosition.x);
            spaar.ModLoader.Configuration.SetFloat("ScriptOptionsYPos", Scripter.Instance.ScriptOptions.ConfigurationPosition.y);

            spaar.ModLoader.Configuration.Save();
        }
    }
}

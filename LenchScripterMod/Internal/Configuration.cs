using System;
using UnityEngine;

namespace LenchScripter.Internal
{
    internal static class Configuration
    {
        internal static void Load()
        {
            ScripterMod.Watchlist.ConfigurationPosition = new Vector2();
            ScripterMod.Watchlist.ConfigurationPosition.x = spaar.ModLoader.Configuration.GetFloat("WatchlistXPos", -380);
            ScripterMod.Watchlist.ConfigurationPosition.y = spaar.ModLoader.Configuration.GetFloat("WatchlistYPos", 200);

            ScripterMod.IdentifierDisplay.ConfigurationPosition = new Vector2();
            ScripterMod.IdentifierDisplay.ConfigurationPosition.x = spaar.ModLoader.Configuration.GetFloat("IdentifierDisplayXPos", 900);
            ScripterMod.IdentifierDisplay.ConfigurationPosition.y = spaar.ModLoader.Configuration.GetFloat("IdentifierDisplayYPos", -240);

            ScripterMod.ScriptOptions.ConfigurationPosition = new Vector2();
            ScripterMod.ScriptOptions.ConfigurationPosition.x = spaar.ModLoader.Configuration.GetFloat("ScriptOptionsXPos", -380);
            ScripterMod.ScriptOptions.ConfigurationPosition.y = spaar.ModLoader.Configuration.GetFloat("ScriptOptionsYPos", -400);
        }

        internal static void Save()
        {
            spaar.ModLoader.Configuration.SetFloat("WatchlistXPos", ScripterMod.Watchlist.ConfigurationPosition.x);
            spaar.ModLoader.Configuration.SetFloat("WatchlistYPos", ScripterMod.Watchlist.ConfigurationPosition.y);

            spaar.ModLoader.Configuration.SetFloat("IdentifierDisplayXPos", ScripterMod.IdentifierDisplay.ConfigurationPosition.x);
            spaar.ModLoader.Configuration.SetFloat("IdentifierDisplayYPos", ScripterMod.IdentifierDisplay.ConfigurationPosition.y);

            spaar.ModLoader.Configuration.SetFloat("ScriptOptionsXPos", ScripterMod.ScriptOptions.ConfigurationPosition.x);
            spaar.ModLoader.Configuration.SetFloat("ScriptOptionsYPos", ScripterMod.ScriptOptions.ConfigurationPosition.y);

            spaar.ModLoader.Configuration.Save();
        }
    }
}

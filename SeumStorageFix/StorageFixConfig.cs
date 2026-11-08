using BepInEx.Configuration;

namespace SeumStorageFix
{
    internal static class StorageFixConfig
    {
        internal static ConfigEntry<bool> Enabled;
        internal static ConfigEntry<bool> FixRechargeRace;

        internal static void Bind(ConfigFile cfg)
        {
            Enabled = cfg.Bind("01 - General", "Enabled", true,
                "Master switch. When false no Harmony patch is applied at all.");

            FixRechargeRace = cfg.Bind("02 - Restart", "FixRechargeRace", true,
                "Do not let a landing refund the teleport/roar charge that an activation spent on "
                + "the same instant (the teleport-storage window). A normal landing recharges as "
                + "usual - only the 0.2 s window right after an activation is protected.");
        }
    }
}

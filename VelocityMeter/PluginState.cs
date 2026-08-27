using System.Collections.Generic;

namespace VelocityMeter
{
    internal static class PluginState
    {
        internal static bool OnParticles;
        internal static bool OnEffect;
        internal static bool DlcSky;
        internal static bool DlcNoTheme;
        internal static bool NoFireballs;
        internal static bool NoBlockBreak;
        internal static float YAxis;
        internal static bool RestartBlockEnabled;
        internal static float RestartBlockTimer;
        internal static bool ShowTrail;
        internal static bool ShowInputOverlay = true;
        internal static float CalculatedHSpd;
        internal static float CalculatedVSpd;
        internal static readonly List<float> SpeedPeaks = new List<float>();
        internal static int NumberStartS;
        internal static float CurrentBurstMax;
        internal static bool IsSpeeding;
        internal static bool BufferedFire1;
        internal static bool BufferedFire2;
    }
}

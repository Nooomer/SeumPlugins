using UnityEngine;

namespace SeumPerf
{
    /// <summary>
    /// A private GUIStyle for the overlay.
    ///
    /// The overlay cannot use GUI.skin.label: SeumUI mutates the shared skin from inside the game's
    /// own OnGUI - it assigns GUI.skin.font once and rewrites GUI.skin.label.alignment before every
    /// single label it draws. Whatever the HUD happened to set last leaks into anything else that
    /// draws, which is why the panel came out centred, in the game's all-caps display font, and
    /// clipped at both ends.
    /// </summary>
    internal static class OverlayStyle
    {
        private static GUIStyle style;
        private static Font font;

        internal static GUIStyle Get()
        {
            if (style != null && style.font != null)
            {
                return style;
            }

            style = new GUIStyle
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 13,
                richText = false,
                wordWrap = false,
                font = Resolve(),
            };
            style.normal.textColor = Color.white;
            return style;
        }

        /// <summary>
        /// A monospaced face if the OS has one - the profiler table is columns of numbers, and they
        /// only line up in a fixed-width font. Falls back to the built-in Arial.
        /// </summary>
        private static Font Resolve()
        {
            if (font != null)
            {
                return font;
            }

            try
            {
                font = Font.CreateDynamicFontFromOSFont(
                    new[] { "Consolas", "Courier New", "Lucida Console", "DejaVu Sans Mono" }, 13);
            }
            catch (System.Exception)
            {
                font = null;
            }

            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font;
        }
    }
}

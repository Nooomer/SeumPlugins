using UnityEngine;

namespace SeumFreeCam
{
    /// <summary>
    /// A stand-in body for the runner. SEUM is first person and ships no character model — the
    /// hands are drawn by a separate camera at a fixed screen position — so from the outside the
    /// replay would otherwise be an empty level with projectiles coming out of nowhere.
    ///
    /// A translucent capsule is what the classic VelocityMeter ghost uses, and it needs no asset
    /// from the game: the shader it wants (<c>Transparent/Diffuse</c>) is one of the handful the
    /// game itself resolves through <see cref="AEShaders"/>, so it is guaranteed to be in the build.
    /// </summary>
    internal static class PlayerMarker
    {
        private static GameObject marker;
        private static MeshRenderer markerRenderer;
        private static string appliedColor;

        internal static void Follow(FPSInputController controller)
        {
            if (controller == null || !FreeCamConfig.ShowMarker.Value)
            {
                Hide();
                return;
            }

            if (marker == null && !Create())
            {
                return;
            }

            ApplyColor();

            // The controller transform sits at the capsule's centre, which is also where a default
            // Unity capsule's pivot is, so the two line up without a vertical fudge.
            marker.transform.position = controller.transform.position;
            marker.transform.rotation = Quaternion.Euler(0f, controller.transform.rotation.eulerAngles.y, 0f);

            if (!marker.activeSelf)
            {
                marker.SetActive(true);
            }
        }

        internal static void Hide()
        {
            if (marker != null && marker.activeSelf)
            {
                marker.SetActive(false);
            }
        }

        internal static void Destroy()
        {
            if (marker != null)
            {
                Object.Destroy(marker);
            }

            marker = null;
            markerRenderer = null;
            appliedColor = null;
        }

        private static bool Create()
        {
            Shader shader = Shader.Find("Transparent/Diffuse");
            if (shader == null)
            {
                Plugin.Log.LogWarning("Transparent/Diffuse is missing from the build; the runner "
                    + "marker stays off and the free camera will show an empty level.");
                return false;
            }

            marker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            marker.name = "SeumFreeCamRunner";
            marker.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(marker);

            // A collider here would push the runner around, and the runner is being replayed, so it
            // would desynchronise the very run we are watching.
            Collider collider = marker.GetComponent<Collider>();
            if (collider != null)
            {
                Object.Destroy(collider);
            }

            markerRenderer = marker.GetComponent<MeshRenderer>();
            markerRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            markerRenderer.receiveShadows = false;
            markerRenderer.material = new Material(shader);

            // A default capsule is two units tall, so this lands a touch under the 1.8 the
            // character controller occupies.
            marker.transform.localScale = new Vector3(0.75f, 0.9f, 0.75f);
            return true;
        }

        private static void ApplyColor()
        {
            string wanted = FreeCamConfig.MarkerColor.Value;
            if (markerRenderer == null || wanted == appliedColor)
            {
                return;
            }

            appliedColor = wanted;

            Color color;
            if (!ColorUtility.TryParseHtmlString(wanted.StartsWith("#") ? wanted : "#" + wanted, out color))
            {
                color = new Color(1f, 0.25f, 0.1f, 0.63f);
                Plugin.Log.LogWarning("MarkerColor '" + wanted + "' is not RRGGBBAA hex; using the default.");
            }

            markerRenderer.material.color = color;
        }
    }
}

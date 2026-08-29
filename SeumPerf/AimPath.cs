using UnityEngine;

namespace SeumPerf
{
    /// <summary>
    /// Guard for the aim-line prediction in <c>FPSInputController.Update</c>.
    ///
    /// Vanilla calls <c>Projectile.generatePath</c> unconditionally every frame. That method runs a
    /// loop of up to 100 steps and issues a <c>Physics.SphereCastNonAlloc</c> on every one of them.
    /// Its output is only ever consumed when the slow-motion aim is engaged:
    ///
    /// <code>
    /// bool flag3 = Projectile.generatePath(characterView.path, ...);
    /// bool flag4 = (slowTimeTimestamp > 0f) &amp; flag2;
    /// characterView.trailHit.SetActive(flag3 &amp; flag4);
    /// ...
    /// characterView.trail.SetActive(flag4);
    /// </code>
    ///
    /// With <c>flag4</c> false both objects are inactive, so the trail renderer's LateUpdate never
    /// runs and nothing else reads <c>characterView.path</c> - it has exactly one consumer,
    /// <c>projectileTrail.path = path</c> in CharacterView. So when <c>slowTimeTimestamp &lt;= 0</c>
    /// the whole prediction is thrown away, and skipping it cannot be observed.
    ///
    /// The guard reads the same field the very next line reads, so there is no staleness: on the
    /// frame slow motion starts, the prediction runs as usual before the trail is switched on.
    /// </summary>
    public static class AimPath
    {
        public static bool Generate(
            Path path,
            Vector3 projectileOrigin,
            Vector3 velocity,
            float updateModifier,
            GameObject owner,
            bool straight,
            out Vector3 hitLocation,
            out Vector3 hitNormal)
        {
            FPSInputController controller = owner == null ? null : Cached.CompGO<FPSInputController>(owner);
            if (controller != null && controller.slowTimeTimestamp <= 0f)
            {
                hitLocation = Vector3.zero;
                hitNormal = Vector3.up;
                return false;
            }

            return Projectile.generatePath(
                path, projectileOrigin, velocity, updateModifier, owner, straight, out hitLocation, out hitNormal);
        }
    }
}

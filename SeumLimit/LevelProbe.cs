using UnityEngine;

namespace SeumLimit
{
    /// <summary>
    /// Asks the loaded level whether the character could walk a straight line, using the same
    /// capsule the character controller has. Everything is read live from the scene, so it works
    /// on any level, including workshop ones, without parsing level files.
    ///
    /// Deliberately strict: anything solid in the way blocks, a step higher than the controller's
    /// stepOffset blocks, a gap in the floor blocks, and anything with <c>CollisionRule.canKill</c>
    /// blocks whether it is solid or a trigger. Moving geometry is taken where it is now - on the
    /// aim screen, before anything has moved or opened - which also errs on the blocking side.
    /// </summary>
    internal sealed class LevelProbe
    {
        // Everything except CharacterCollision (10) and Projectile (13) - the value the game's own
        // CharacterMotor.notCharacterLayer holds.
        private const int Mask = ~((1 << 10) | (1 << 13));
        private const float FloorSample = 0.5f;

        private readonly float radius;
        private readonly float halfHeight;
        private readonly float stepOffset;
        private readonly RaycastHit[] hits = new RaycastHit[32];

        private LevelProbe(CharacterController cc)
        {
            radius = cc.radius;
            halfHeight = cc.height * 0.5f;
            stepOffset = cc.stepOffset;
        }

        internal static LevelProbe Create()
        {
            CharacterController cc = Object.FindObjectOfType<CharacterController>();
            return cc == null ? null : new LevelProbe(cc);
        }

        /// <summary>Positions are the character's transform, which is the capsule's centre.</summary>
        internal bool IsWalkable(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            float distance = delta.magnitude;
            if (distance < 0.01f)
            {
                return true;
            }

            if (FreeDistance(from, delta / distance, distance) < distance)
            {
                return false;
            }

            // Floor all the way: the controller would fall through a gap even if nothing blocks.
            int samples = Mathf.Max(1, Mathf.CeilToInt(distance / FloorSample));
            float reach = halfHeight + stepOffset + 0.1f;
            for (int s = 0; s <= samples; s++)
            {
                Vector3 p = Vector3.Lerp(from, to, s / (float)samples);
                RaycastHit floor;
                if (!Physics.Raycast(p, Vector3.down, out floor, reach, Mask, QueryTriggerInteraction.Ignore)
                    || IsDeadly(floor.collider))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// How far the capsule can move from <paramref name="from"/> towards <paramref name="to"/>
        /// before something solid or deadly stops it. No floor needed: this is for the air.
        /// </summary>
        internal float ClearDistance(Vector3 from, Vector3 to)
        {
            Vector3 delta = to - from;
            float distance = delta.magnitude;
            return distance < 0.01f ? distance : Mathf.Min(distance, FreeDistance(from, delta / distance, distance));
        }

        /// <summary>
        /// Distance to the first thing along the way that stops the character: anything solid
        /// ahead, or anything with canKill, solid or not. Infinity when the way is clear.
        /// </summary>
        private float FreeDistance(Vector3 from, Vector3 direction, float distance)
        {
            // The bottom sphere is lifted by stepOffset: the controller climbs anything lower than
            // that, and the floor under the start must not count as an obstacle.
            Vector3 top = from + Vector3.up * (halfHeight - radius);
            Vector3 bottom = from + Vector3.up * (-halfHeight + radius + stepOffset);
            if (bottom.y > top.y)
            {
                bottom = top;
            }

            float free = float.PositiveInfinity;
            int count = Physics.CapsuleCastNonAlloc(top, bottom, radius * 0.98f, direction, hits, distance,
                Mask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = hits[i];
                if (hit.collider == null)
                {
                    continue;
                }

                // Distance 0 means the capsule started inside it (the floor it stands on at the
                // start): not a wall ahead - unless it is deadly, which is never fine.
                bool deadly = IsDeadly(hit.collider);
                if (deadly || (!hit.collider.isTrigger && hit.distance > 0f))
                {
                    free = Mathf.Min(free, hit.distance);
                }
            }

            return free;
        }

        private static bool IsDeadly(Collider collider)
        {
            CollisionRule rule = collider.GetComponentInParent<CollisionRule>();
            return rule != null && rule.canKill;
        }
    }
}

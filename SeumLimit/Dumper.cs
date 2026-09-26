using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using IOPath = System.IO.Path;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeumLimit
{
    /// <summary>
    /// Stage 0 diagnostics: dumps everything the limit analysis has to be fitted to, so the
    /// numbers come from the running game instead of from reading IL.
    ///
    /// Writes <c>&lt;stamp&gt;_info.txt</c> always, and <c>&lt;stamp&gt;_frames.csv</c> +
    /// <c>&lt;stamp&gt;_events.csv</c> when <see cref="Replay.replay"/> holds a session.
    /// </summary>
    internal static class Dumper
    {
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private static readonly string[] EventNames =
        {
            "FIRE1", "FIRE2", "GRAVITY_POWER", "PICKUP", "PICKUP_PROJECTILE", "TRIGGER",
            "RING_TRIGGER", "GHOST_PLATFORM_DESTROY", "SPAWNED_PLATFORM_DESTROY", "CRACKED_DESTROY",
            "ON_LAND", "UNGROUND", "FLIP", "ROAR", "ROCKET",
        };

        internal static string Write()
        {
            string dir = IOPath.Combine(Paths.CachePath, IOPath.Combine("SeumLimit", "dump"));
            Directory.CreateDirectory(dir);

            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", Inv) + "_" + SafeName(SceneManager.GetActiveScene().name);
            string infoPath = IOPath.Combine(dir, stamp + "_info.txt");

            StringBuilder sb = new StringBuilder();
            DumpTime(sb);
            DumpCharacter(sb);
            DumpColliders(sb);
            DumpReplay(sb, dir, stamp);

            File.WriteAllText(infoPath, sb.ToString(), Encoding.UTF8);
            return infoPath;
        }

        private static void DumpTime(StringBuilder sb)
        {
            sb.AppendLine("== Time / physics");
            Line(sb, "scenes", string.Join(", ", Enumerable.Range(0, SceneManager.sceneCount)
                .Select(i => SceneManager.GetSceneAt(i).name).ToArray()));
            Line(sb, "Time.fixedDeltaTime", Time.fixedDeltaTime);
            Line(sb, "Time.maximumDeltaTime", Time.maximumDeltaTime);
            Line(sb, "Time.timeScale", Time.timeScale);
            Line(sb, "Physics.gravity", Physics.gravity);
            Line(sb, "Physics.defaultContactOffset", Physics.defaultContactOffset);
            Line(sb, "FPSInputController.GRAVITY", FPSInputController.GRAVITY);
            sb.AppendLine();
        }

        private static void DumpCharacter(StringBuilder sb)
        {
            sb.AppendLine("== Character");

            CharacterController cc = UnityEngine.Object.FindObjectOfType<CharacterController>();
            if (cc == null)
            {
                sb.AppendLine("no CharacterController in the scene (dump from inside a level)");
                sb.AppendLine();
                return;
            }

            Line(sb, "object", cc.gameObject.name + " layer=" + LayerMask.LayerToName(cc.gameObject.layer)
                + " (" + cc.gameObject.layer + ") tag=" + cc.gameObject.tag);
            Line(sb, "position", cc.transform.position);
            Line(sb, "lossyScale", cc.transform.lossyScale);
            Line(sb, "cc.radius", cc.radius);
            Line(sb, "cc.height", cc.height);
            Line(sb, "cc.center", cc.center);
            Line(sb, "cc.stepOffset", cc.stepOffset);
            Line(sb, "cc.slopeLimit", cc.slopeLimit);
            Line(sb, "cc.skinWidth", cc.skinWidth);
            Line(sb, "cc.minMoveDistance", cc.minMoveDistance);

            sb.Append("collides with layers:");
            for (int i = 0; i < 32; i++)
            {
                string name = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(name) && !Physics.GetIgnoreLayerCollision(cc.gameObject.layer, i))
                {
                    sb.Append(' ').Append(name).Append('(').Append(i).Append(')');
                }
            }

            sb.AppendLine();

            CharacterMotor motor = cc.GetComponent<CharacterMotor>();
            if (motor != null)
            {
                DumpFields(sb, "motor", motor);
                DumpFields(sb, "motor.movement", motor.movement);
                DumpFields(sb, "motor.jumping", motor.jumping);
                DumpFields(sb, "motor.sliding", motor.sliding);
                DumpFields(sb, "motor.movingPlatform", motor.movingPlatform);
            }

            FPSInputController input = cc.GetComponent<FPSInputController>();
            if (input != null)
            {
                DumpFields(sb, "input", input);
            }

            sb.AppendLine();
        }

        /// <summary>
        /// Groups every active collider by how it is marked - layer, tag, trigger flag, collider
        /// type and the game scripts on it or its parents - which is what tells walls, hazards,
        /// finish and moving platforms apart.
        /// </summary>
        private static void DumpColliders(StringBuilder sb)
        {
            sb.AppendLine("== Colliders (grouped)");

            Dictionary<string, List<Collider>> groups = new Dictionary<string, List<Collider>>();
            foreach (Collider c in UnityEngine.Object.FindObjectsOfType<Collider>())
            {
                string scripts = string.Join("+", c.GetComponentsInParent<MonoBehaviour>(true)
                    .Where(m => m != null && m.GetType().Assembly == typeof(Replay).Assembly)
                    .Select(m => m.GetType().Name).Distinct().OrderBy(n => n).ToArray());

                string key = "layer=" + LayerMask.LayerToName(c.gameObject.layer)
                    + " tag=" + c.gameObject.tag
                    + (c.isTrigger ? " TRIGGER" : "")
                    + " " + c.GetType().Name
                    + " scripts=[" + scripts + "]"
                    + RuleFlags(c.GetComponentInParent<CollisionRule>())
                    + PlatformInfo(c.GetComponentInParent<MovingPlatform>());

                List<Collider> list;
                if (!groups.TryGetValue(key, out list))
                {
                    list = new List<Collider>();
                    groups.Add(key, list);
                }

                list.Add(c);
            }

            foreach (KeyValuePair<string, List<Collider>> g in groups.OrderByDescending(p => p.Value.Count))
            {
                Bounds b = g.Value[0].bounds;
                foreach (Collider c in g.Value)
                {
                    b.Encapsulate(c.bounds);
                }

                sb.AppendLine(g.Value.Count.ToString(Inv).PadLeft(5) + "  " + g.Key);
                sb.AppendLine("       bounds " + Fmt(b.min) + " .. " + Fmt(b.max)
                    + "  e.g. " + string.Join(", ", g.Value.Take(4).Select(c => c.gameObject.name).ToArray()));
            }

            sb.AppendLine();
        }

        private static void DumpReplay(StringBuilder sb, string dir, string stamp)
        {
            sb.AppendLine("== Replay");

            Replay.ReplaySession session = Replay.replay;
            if (session == null)
            {
                sb.AppendLine("Replay.replay is null (open a leaderboard replay to dump one)");
                return;
            }

            Line(sb, "playing", Replay.playing);

            FieldInfo scoreField = AccessTools.Field(typeof(Hud), "selectedReplayScore");
            WriteSession(sb, session, scoreField == null ? null : scoreField.GetValue(null) as Score, dir, stamp);
        }

        /// <summary>
        /// Writes a replay as <c>&lt;stamp&gt;_frames.csv</c> + <c>&lt;stamp&gt;_events.csv</c> and
        /// its summary into <paramref name="sb"/>. Shared by the F7 dump and the top-run collector,
        /// so everything the plugin saves can be fed to the same offline tooling.
        /// </summary>
        internal static void WriteSession(StringBuilder sb, Replay.ReplaySession session, Score score, string dir, string stamp)
        {
            float dt = Time.fixedDeltaTime;
            Line(sb, "waitTime", session.waitTime);
            Line(sb, "frameCount", session.frameCount);
            Line(sb, "frameCount * fixedDeltaTime", session.frameCount * dt);
            Line(sb, "eventCount", session.eventCount);
            Line(sb, "projectileDataCount", session.projectileDataCount);

            if (score != null)
            {
                Line(sb, "score", score.name + " place=" + score.place + " time=" + score.time
                    + " ugc=" + score.replayUGC + " hardcore=" + score.hardcore);
            }

            Dictionary<string, int> counts = new Dictionary<string, int>();
            StringBuilder events = new StringBuilder("index,type,name,data,timestamp,runFrame\n");
            for (int i = 0; i < session.eventCount; i++)
            {
                Replay.ReplayEventRecord e = session.events[i / 100][i % 100];
                string name = e.type >= 0 && e.type < EventNames.Length ? EventNames[e.type] : "?" + e.type;
                string key = name + (e.type == 3 ? "(data=" + e.data + ")" : "");
                int n;
                counts.TryGetValue(key, out n);
                counts[key] = n + 1;

                events.Append(i.ToString(Inv)).Append(',').Append(e.type.ToString(Inv)).Append(',')
                    .Append(name).Append(',').Append(e.data.ToString(Inv)).Append(',')
                    .Append(e.timestamp.ToString("R", Inv)).Append(',')
                    // Event timestamps count from the level restart (Time.time - Game.restartTimestamp),
                    // frames from the first input - waitTime is the gap between the two.
                    .Append(dt > 0f ? ((e.timestamp - session.waitTime) / dt).ToString("0.###", Inv) : "").Append('\n');
            }

            foreach (KeyValuePair<string, int> c in counts.OrderBy(p => p.Key))
            {
                Line(sb, "  event " + c.Key, c.Value);
            }

            float maxH = 0f;
            StringBuilder frames = new StringBuilder("frame,t,x,y,z,rotX,rotY,forwardVelocity,hSpeed,vSpeed,accel\n");
            Vector3 prevVelocity = Vector3.zero;
            for (int i = 0; i < session.frameCount; i++)
            {
                Replay.ReplayFullFrame f = session.frames[i / 60][i % 60];
                float h = 0f, v = 0f, accel = 0f;
                if (i + 1 < session.frameCount && dt > 0f)
                {
                    Vector3 velocity = (session.frames[(i + 1) / 60][(i + 1) % 60].position - f.position) / dt;
                    h = new Vector2(velocity.x, velocity.z).magnitude;
                    v = velocity.y;

                    // Normal movement stays within ground acceleration + gravity (well under
                    // 100 m/s^2); a teleport, a roar launch or a wall stop shows up as a spike here,
                    // which is the only trace a teleport leaves - it has no event of its own.
                    accel = i > 0 ? (velocity - prevVelocity).magnitude / dt : 0f;
                    prevVelocity = velocity;
                }

                maxH = Mathf.Max(maxH, h);
                frames.Append(i.ToString(Inv)).Append(',').Append((i * dt).ToString("0.#####", Inv)).Append(',')
                    .Append(f.position.x.ToString("R", Inv)).Append(',')
                    .Append(f.position.y.ToString("R", Inv)).Append(',')
                    .Append(f.position.z.ToString("R", Inv)).Append(',')
                    .Append(f.rotationX.ToString("R", Inv)).Append(',')
                    .Append(f.rotationY.ToString("R", Inv)).Append(',')
                    .Append(f.forwardVelocity.ToString("R", Inv)).Append(',')
                    .Append(h.ToString("0.####", Inv)).Append(',')
                    .Append(v.ToString("0.####", Inv)).Append(',')
                    .Append(accel.ToString("0.#", Inv)).Append('\n');
            }

            Line(sb, "max horizontal speed (from positions)", maxH);

            File.WriteAllText(IOPath.Combine(dir, stamp + "_frames.csv"), frames.ToString(), Encoding.UTF8);
            File.WriteAllText(IOPath.Combine(dir, stamp + "_events.csv"), events.ToString(), Encoding.UTF8);
            sb.AppendLine("frames/events written next to this file");
        }

        /// <summary>
        /// The flags that give a collider its meaning - canKill, isExit, isBounce and so on - as
        /// the game's own collision handling reads them. Only the ones that are set.
        /// </summary>
        private static string RuleFlags(CollisionRule rule)
        {
            if (rule == null)
            {
                return "";
            }

            List<string> set = new List<string>();
            foreach (FieldInfo field in typeof(CollisionRule).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object value = field.GetValue(rule);
                if (value is bool && (bool)value)
                {
                    set.Add(field.Name);
                }
                else if ((value is float && (float)value != 0f) || (value is int && (int)value != 0)
                    || (value != null && value.GetType().IsEnum && Convert.ToInt32(value, Inv) != 0))
                {
                    set.Add(field.Name + "=" + Convert.ToString(value, Inv));
                }
            }

            return " rule{" + string.Join(",", set.ToArray()) + "}";
        }

        private static string PlatformInfo(MovingPlatform platform)
        {
            if (platform == null)
            {
                return "";
            }

            return " platform{start=" + platform.startActiveValue
                + ",triggersRequired=" + platform.triggersRequired
                + ",loop=" + platform.loop
                + ",waypoints=" + (platform.waypoints == null ? 0 : platform.waypoints.Length) + "}";
        }

        /// <summary>Every simple field of an object, public or not, instance and static.</summary>
        private static void DumpFields(StringBuilder sb, string prefix, object target)
        {
            if (target == null)
            {
                Line(sb, prefix, "null");
                return;
            }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            foreach (FieldInfo field in target.GetType().GetFields(flags))
            {
                Type t = field.FieldType;
                object value = field.IsStatic ? field.GetValue(null) : field.GetValue(target);

                if (t.IsPrimitive || t.IsEnum || t == typeof(Vector3) || t == typeof(Vector2)
                    || t == typeof(Quaternion) || t == typeof(string))
                {
                    Line(sb, prefix + "." + field.Name, value);
                }
                else if (t == typeof(AnimationCurve) && value != null)
                {
                    Line(sb, prefix + "." + field.Name, string.Join(" ", ((AnimationCurve)value).keys
                        .Select(k => "(" + k.time.ToString("0.###", Inv) + "," + k.value.ToString("0.###", Inv) + ")")
                        .ToArray()));
                }
            }
        }

        private static void Line(StringBuilder sb, string name, object value)
        {
            string text = value is Vector3 ? Fmt((Vector3)value)
                : value is float ? ((float)value).ToString("R", Inv)
                : Convert.ToString(value, Inv);
            sb.Append(name).Append(" = ").AppendLine(text);
        }

        private static string Fmt(Vector3 v)
        {
            return "(" + v.x.ToString("0.###", Inv) + ", " + v.y.ToString("0.###", Inv) + ", " + v.z.ToString("0.###", Inv) + ")";
        }

        private static string SafeName(string name)
        {
            foreach (char c in IOPath.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }

            return string.IsNullOrEmpty(name) ? "scene" : name;
        }
    }
}

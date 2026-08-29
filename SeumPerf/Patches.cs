using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace SeumPerf
{
    /// <summary>
    /// The patches that need real logic rather than a mechanical IL substitution.
    /// Each is registered individually so one failure never takes the rest down with it.
    /// </summary>
    internal static class Patches
    {
        internal static void Apply(Harmony harmony)
        {
            if (PerfConfig.TrimProjectilePaths.Value)
            {
                Patch(harmony, typeof(Projectile), "addPathPoint",
                    prefix: nameof(GrowProjectilePath));
                Patch(harmony, typeof(Projectile), "generatePath",
                    prefix: nameof(EnsureGeneratePathCapacity));
            }

            if (PerfConfig.ReuseTrailMeshBuffers.Value)
            {
                Patch(harmony, typeof(ProjectileTrail), "LateUpdate",
                    prefix: nameof(TrailLateUpdate));
            }

            if (PerfConfig.RingTriggerOnStateChange.Value)
            {
                Patch(harmony, typeof(RingTrigger), "Update",
                    prefix: nameof(RingTriggerUpdate));
            }

            if (PerfConfig.HellikuEarlyOut.Value)
            {
                Patch(harmony, typeof(Helliku), "Update",
                    prefix: nameof(HellikuUpdate));
            }

            if (PerfConfig.DedupeAudioMixerWrites.Value)
            {
                Patch(harmony, typeof(AudioManager), "setGroupVolume",
                    prefix: nameof(SetGroupVolume));
            }

            if (PerfConfig.CacheLevelData.Value)
            {
                Patch(harmony, typeof(Game), "getCurrentLevelData",
                    prefix: nameof(LevelDataPrefix), postfix: nameof(LevelDataPostfix));
            }

            if (PerfConfig.CacheSteamUserId.Value)
            {
                Patch(harmony, typeof(LeaderboardsSteamBackend), "isScoreCurrentUser",
                    prefix: nameof(IsScoreCurrentUser));
            }

            if (PerfConfig.CacheLevelNames.Value)
            {
                Patch(harmony, typeof(Hud), "levelNameString",
                    prefix: nameof(LevelNamePrefix), postfix: nameof(LevelNamePostfix));
            }

            if (PerfConfig.CachePostEffectResourceChecks.Value)
            {
                Patch(harmony, typeof(GhostOverlayEffect), "CheckResources",
                    prefix: nameof(GhostCheckResources));
                Patch(harmony, typeof(ShadowWorldOverlayEffect), "CheckResources",
                    prefix: nameof(ShadowCheckResources));
                Patch(harmony, typeof(ExposureCorrection), "CheckResources",
                    prefix: nameof(ExposureCheckResources));
            }
        }

        private static void Patch(Harmony harmony, Type type, string method, string prefix = null, string postfix = null)
        {
            string label = type.Name + "." + method;
            try
            {
                MethodBase original = AccessTools.Method(type, method);
                if (original == null)
                {
                    Plugin.Log.LogWarning($"SeumPerf: could not resolve '{label}', skipping.");
                    return;
                }

                harmony.Patch(
                    original,
                    prefix == null ? null : new HarmonyMethod(AccessTools.Method(typeof(Patches), prefix)),
                    postfix == null ? null : new HarmonyMethod(AccessTools.Method(typeof(Patches), postfix)));

                if (PerfConfig.VerboseLogging.Value)
                {
                    Plugin.Log.LogInfo($"SeumPerf: patched {label}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"SeumPerf: failed to patch '{label}': {ex.Message}");
            }
        }

        // ---------------------------------------------------------------- projectile path buffer

        private const int PathHardCap = 4096;

        /// <summary>
        /// Restores the capacity the shrunk Path constructor gave up, on demand. The original
        /// behaviour - refuse the point and log once the buffer is full - is preserved exactly,
        /// it just happens at 4096 entries like before instead of at the smaller starting size.
        /// </summary>
        private static void GrowProjectilePath(Projectile __instance)
        {
            Path path = __instance.path;
            if (path == null || path.points == null)
            {
                return;
            }

            if (path.count < (uint)path.points.Length || path.points.Length >= PathHardCap)
            {
                return;
            }

            Array.Resize(ref path.points, Math.Min(PathHardCap, Math.Max(16, path.points.Length * 2)));
        }

        /// <summary>generatePath writes up to 100 points without any bounds check of its own.</summary>
        private static void EnsureGeneratePathCapacity(Path path)
        {
            if (path?.points != null && path.points.Length < 128)
            {
                Array.Resize(ref path.points, 128);
            }
        }

        // ---------------------------------------------------------------------- projectile trails

        private sealed class TrailState
        {
            internal Mesh Mesh;
            internal readonly List<Vector3> Vertices = new List<Vector3>(256);
            internal readonly List<Vector2> Uvs = new List<Vector2>(256);
            internal readonly List<Color> Colors = new List<Color>(256);
            internal readonly List<int> Triangles = new List<int>(768);
        }

        private static readonly ConditionalWeakTable<ProjectileTrail, TrailState> TrailStates =
            new ConditionalWeakTable<ProjectileTrail, TrailState>();

        /// <summary>
        /// Same mesh, same maths, no garbage. The original allocated a Vector3[], a Vector2[], a
        /// Color[] and an int[] every frame for every live trail, all sized by the projectile's
        /// path length - which keeps growing for as long as the projectile is alive.
        /// </summary>
        private static bool TrailLateUpdate(ProjectileTrail __instance)
        {
            ProjectileTrail trail = __instance;
            Path path = trail.path;

            float life = trail.lifeTime;
            if (trail.death != -1f)
            {
                if (trail.death - trail.trailStartTime < 0.25f)
                {
                    life *= 0.25f;
                }
                else if (trail.death - trail.trailStartTime < 0.5f)
                {
                    life *= 0.5f;
                }
            }

            float age = Time.time - trail.trailStartTime;

            if (path != null && path.count > 1)
            {
                TrailState state = TrailStates.GetOrCreateValue(trail);
                if (state.Mesh == null)
                {
                    MeshFilter filter = trail.GetComponent<MeshFilter>();
                    if (filter == null)
                    {
                        // Start() has not run yet; let the original method deal with it.
                        return true;
                    }

                    state.Mesh = filter.mesh;
                }

                int count = (int)path.count;
                List<Vector3> vertices = state.Vertices;
                List<Vector2> uvs = state.Uvs;
                List<Color> colors = state.Colors;
                List<int> triangles = state.Triangles;

                vertices.Clear();
                uvs.Clear();
                colors.Clear();
                triangles.Clear();

                float firstStamp = path.points[0].timeStamp;
                float span = path.points[count - 1].timeStamp - firstStamp;
                float invSpan = 1f / span;
                Color transparent = new Color(1f, 1f, 1f, 0f);

                for (int i = 0; i < count; i++)
                {
                    PathPoint point = path.points[i];
                    float localTime = point.timeStamp - firstStamp;
                    float u = localTime * invSpan;

                    Vector3 direction = (i != 0)
                        ? (path.points[i - 1].position - point.position)
                        : (point.position - path.points[i + 1].position);
                    Vector3 side = new Vector3(0f - direction.z, 0f, direction.x);
                    side.Normalize();
                    Vector3 wave = side * Mathf.Sin(point.timeStamp * 100f) * 0.025f;

                    vertices.Add(point.position + side * trail.size * 0.5f + wave * trail.waveRatio);
                    vertices.Add(point.position + -side * trail.size * 0.5f + wave * trail.waveRatio);

                    Color color = (i == 0 || i == count - 1 || age > localTime + life)
                        ? transparent
                        : (!(age > localTime) ? Color.white : new Color(1f, 1f, 1f, 1f - (age - localTime) / life));
                    colors.Add(color);
                    colors.Add(color);

                    uvs.Add(new Vector2(u * trail.uvScale, 0f));
                    uvs.Add(new Vector2(u * trail.uvScale, 1f));

                    if (i <= 0)
                    {
                        continue;
                    }

                    if (point.split)
                    {
                        // The original left this stretch of the freshly allocated index buffer at
                        // zero, producing a degenerate triangle. Reproduce that exactly.
                        for (int z = 0; z < 6; z++)
                        {
                            triangles.Add(0);
                        }
                    }
                    else
                    {
                        triangles.Add(i * 2 - 2);
                        triangles.Add(i * 2 - 1);
                        triangles.Add(i * 2);
                        triangles.Add(i * 2 + 1);
                        triangles.Add(i * 2);
                        triangles.Add(i * 2 - 1);
                    }
                }

                Mesh mesh = state.Mesh;
                mesh.Clear();
                mesh.SetVertices(vertices);
                mesh.SetUVs(0, uvs);
                mesh.SetColors(colors);
                mesh.SetTriangles(triangles, 0);
            }

            if (trail.death != -1f && Time.time - trail.death > life)
            {
                UnityEngine.Object.Destroy(trail.gameObject);
            }

            return false;
        }

        // ------------------------------------------------------------------------- ring triggers

        private sealed class RingState
        {
            internal TriggerState Last;
            internal bool Applied;
            internal int NextReassertFrame;
        }

        private static readonly ConditionalWeakTable<RingTrigger, RingState> RingStates =
            new ConditionalWeakTable<RingTrigger, RingState>();

        private static Action<RingTrigger> syncRotation;

        /// <summary>
        /// Vanilla re-asserts SetActive on every object of all three state arrays every frame.
        /// The visible result only changes when triggeredState changes, so drive it off that -
        /// with a periodic re-assert (staggered per instance) so anything that flips those objects
        /// behind our back still recovers within a fraction of a second.
        /// </summary>
        private static bool RingTriggerUpdate(RingTrigger __instance)
        {
            if (syncRotation == null)
            {
                MethodInfo method = AccessTools.Method(typeof(RingTrigger), "syncRotation");
                if (method == null)
                {
                    return true;
                }

                syncRotation = AccessTools.MethodDelegate<Action<RingTrigger>>(method);
            }

            RingState state = RingStates.GetOrCreateValue(__instance);
            int frame = Time.frameCount;

            if (!state.Applied || state.Last != __instance.triggeredState || frame >= state.NextReassertFrame)
            {
                ApplyRingState(__instance);
                state.Last = __instance.triggeredState;
                state.Applied = true;
                state.NextReassertFrame = frame + 30 + (Math.Abs(__instance.GetInstanceID()) % 30);
            }

            syncRotation(__instance);
            return false;
        }

        private static void ApplyRingState(RingTrigger ring)
        {
            switch (ring.triggeredState)
            {
                case TriggerState.INACTIVE:
                    SetAll(ring.objectInActiveState, false);
                    SetAll(ring.objectInTriggeredState, false);
                    SetAll(ring.objectInInactiveState, true);
                    break;
                case TriggerState.ACTIVE:
                    SetAll(ring.objectInInactiveState, false);
                    SetAll(ring.objectInTriggeredState, false);
                    SetAll(ring.objectInActiveState, true);
                    break;
                case TriggerState.TRIGGERED:
                    SetAll(ring.objectInInactiveState, false);
                    SetAll(ring.objectInActiveState, false);
                    SetAll(ring.objectInTriggeredState, true);
                    break;
            }
        }

        private static void SetAll(GameObject[] objects, bool active)
        {
            if (objects == null)
            {
                return;
            }

            for (int i = 0; i < objects.Length; i++)
            {
                GameObject go = objects[i];
                if (go != null && go.activeSelf != active)
                {
                    go.SetActive(active);
                }
            }
        }

        // ------------------------------------------------------------------------------- helliku

        private static readonly AccessTools.FieldRef<Helliku, bool> HellikuDone =
            AccessTools.FieldRefAccess<Helliku, bool>("lastStateHelliku");

        /// <summary>
        /// The vanilla body is a one-shot material swap, but it evaluates
        /// Game.getCurrentLevelData() before checking whether it already ran.
        /// </summary>
        private static bool HellikuUpdate(Helliku __instance)
        {
            return !HellikuDone(__instance);
        }

        // ----------------------------------------------------------------------------- audio mix

        private static readonly Dictionary<string, float> LastGroupVolume = new Dictionary<string, float>(8);
        private static int lastVolumeRefreshFrame;

        /// <summary>
        /// Five AudioMixer.SetFloat calls a frame, almost always with the value they already hold.
        /// The periodic forced refresh keeps this honest if a mixer write ever fails silently.
        /// </summary>
        private static bool SetGroupVolume(string group, float volume)
        {
            int frame = Time.frameCount;
            if (frame - lastVolumeRefreshFrame > 120)
            {
                lastVolumeRefreshFrame = frame;
                LastGroupVolume.Clear();
                return true;
            }

            float previous;
            if (LastGroupVolume.TryGetValue(group, out previous) && previous == volume)
            {
                return false;
            }

            LastGroupVolume[group] = volume;
            return true;
        }

        // ---------------------------------------------------------------------------- level data

        private static LevelData cachedLevelData;
        private static int cachedLevelDataFrame = -1;
        private static int cachedLevelIndex;
        private static StartedFrom cachedStartedFrom;
        private static ulong cachedUgcId;
        private static ulong cachedUgcRev;
        private static object cachedAccount;

        private static bool LevelDataPrefix(ref LevelData __result)
        {
            if (cachedLevelData != null
                && cachedLevelDataFrame == Time.frameCount
                && cachedLevelIndex == Game.currentLevel
                && cachedStartedFrom == Game.startedFrom
                && cachedUgcId == Game.ugcLevelId
                && cachedUgcRev == Game.ugcLevelRev
                && ReferenceEquals(cachedAccount, Accounts.current))
            {
                __result = cachedLevelData;
                return false;
            }

            return true;
        }

        private static void LevelDataPostfix(LevelData __result)
        {
            cachedLevelData = __result;
            cachedLevelDataFrame = Time.frameCount;
            cachedLevelIndex = Game.currentLevel;
            cachedStartedFrom = Game.startedFrom;
            cachedUgcId = Game.ugcLevelId;
            cachedUgcRev = Game.ugcLevelRev;
            cachedAccount = Accounts.current;
        }

        // ------------------------------------------------------------------- post effect support

        private static readonly AccessTools.FieldRef<GhostOverlayEffect, Material> GhostMaterial =
            AccessTools.FieldRefAccess<GhostOverlayEffect, Material>("overlayMaterial");

        private static readonly AccessTools.FieldRef<ShadowWorldOverlayEffect, Material> ShadowMaterial =
            AccessTools.FieldRefAccess<ShadowWorldOverlayEffect, Material>("overlayMaterial");

        private static readonly AccessTools.FieldRef<ExposureCorrection, Material> ExposureMaterial =
            AccessTools.FieldRefAccess<ExposureCorrection, Material>("exposureMaterial");

        // Once the material exists the shader compiled and the platform supports the effect, so the
        // SystemInfo round trips these do on every OnRenderImage cannot change their answer.
        private static bool GhostCheckResources(GhostOverlayEffect __instance, ref bool __result)
        {
            return NeedsResourceCheck(GhostMaterial(__instance), ref __result);
        }

        private static bool ShadowCheckResources(ShadowWorldOverlayEffect __instance, ref bool __result)
        {
            return NeedsResourceCheck(ShadowMaterial(__instance), ref __result);
        }

        private static bool ExposureCheckResources(ExposureCorrection __instance, ref bool __result)
        {
            return NeedsResourceCheck(ExposureMaterial(__instance), ref __result);
        }

        private static bool NeedsResourceCheck(Material material, ref bool result)
        {
            if (material == null)
            {
                return true;
            }

            result = true;
            return false;
        }

        // ------------------------------------------------------------------------ steam identity

        private static ulong steamUserId;
        private static bool steamUserIdKnown;

        // SteamManager is internal to the game assembly, so its Initialized property is reached
        // through reflection. It is only consulted until the id has been read successfully.
        private static readonly MethodInfo SteamInitializedGetter = ResolveSteamInitialized();

        private static MethodInfo ResolveSteamInitialized()
        {
            Type type = TypeResolver.ResolveType("SteamManager");
            return type == null ? null : AccessTools.PropertyGetter(type, "Initialized");
        }

        /// <summary>
        /// The leaderboard asks Steam who the local user is once per row per OnGUI event. Measured
        /// on the level-start screen: ~40 calls and ~3 ms per frame - by far the most expensive
        /// thing on that screen, because every call goes through
        /// <c>InteropHelp.TestIfAvailableClient()</c> and two native P/Invokes.
        ///
        /// The logged-in Steam id cannot change while the process runs, so it is read once. Until
        /// it can be read (Steam not initialised yet) the original runs untouched, which also keeps
        /// the "not initialised means false" behaviour intact.
        /// </summary>
        private static bool IsScoreCurrentUser(Score score, ref bool __result)
        {
            if (!steamUserIdKnown)
            {
                try
                {
                    if (SteamInitializedGetter == null
                        || !(bool)SteamInitializedGetter.Invoke(null, null))
                    {
                        return true;
                    }

                    steamUserId = SteamUser.GetSteamID().m_SteamID;
                }
                catch (Exception)
                {
                    return true;
                }

                if (steamUserId == 0UL)
                {
                    return true;
                }

                steamUserIdKnown = true;
            }

            __result = score != null && score.id == steamUserId;
            return false;
        }

        // ----------------------------------------------------------------------------- level names

        private static readonly Dictionary<int, string> LevelNames = new Dictionary<int, string>(64);

        static Patches()
        {
            CacheRegistry.Register(LevelNames.Clear);
        }

        // Level 136 renders as "???" until a skull earned elsewhere unlocks it, so it is the one
        // entry whose answer depends on mutable state - never cached.
        private const int VariableNameLevel = 136;

        private static bool LevelNamePrefix(int level, int mode, ref string __result)
        {
            if (level == VariableNameLevel)
            {
                return true;
            }

            string cached;
            if (LevelNames.TryGetValue(level * 8 + mode, out cached))
            {
                __result = cached;
                return false;
            }

            return true;
        }

        private static void LevelNamePostfix(int level, int mode, string __result)
        {
            if (level != VariableNameLevel)
            {
                LevelNames[level * 8 + mode] = __result;
            }
        }
    }
}

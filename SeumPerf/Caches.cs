using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeumPerf
{
    /// <summary>
    /// Registry of every lookup cache in the mod, so a level change can wipe all of them at once.
    ///
    /// Everything in this file has to behave exactly like the Unity API it replaces - the whole
    /// point of the mod is that frames get cheaper without the game behaving differently. Where a
    /// cached value can legitimately go stale (destroyed components, hands swapped in and out,
    /// level reloads) the cache revalidates instead of trusting itself.
    /// </summary>
    internal static class CacheRegistry
    {
        private static readonly List<Action> Clears = new List<Action>();

        internal static void Register(Action clear)
        {
            lock (Clears)
            {
                Clears.Add(clear);
            }
        }

        internal static void ClearAll()
        {
            lock (Clears)
            {
                for (int i = 0; i < Clears.Count; i++)
                {
                    Clears[i]();
                }
            }
        }
    }

    /// <summary>
    /// One dictionary per closed generic - a static field on a generic type gives a per-T store
    /// without any Type key hashing on the hot path.
    /// </summary>
    internal static class ComponentCache<T> where T : Component
    {
        // Levels hold a few thousand objects; well past that we are just retaining stale ids.
        private const int MaxEntries = 8192;

        private static readonly Dictionary<int, T> Self = new Dictionary<int, T>();
        private static readonly Dictionary<int, T> Children = new Dictionary<int, T>();
        private static readonly Dictionary<int, T> Parents = new Dictionary<int, T>();

        static ComponentCache()
        {
            CacheRegistry.Register(Clear);
        }

        internal static void Clear()
        {
            Self.Clear();
            Children.Clear();
            Parents.Clear();
        }

        internal static T Get(GameObject go)
        {
            if (go == null)
            {
                return null;
            }

            int id = go.GetInstanceID();
            T cached;
            // The Unity null check also covers "component was destroyed since we cached it".
            if (Self.TryGetValue(id, out cached) && cached != null)
            {
                return cached;
            }

            T found = go.GetComponent<T>();
            if (Self.Count >= MaxEntries)
            {
                Self.Clear();
            }

            Self[id] = found;
            return found;
        }

        internal static T GetInChildren(GameObject go)
        {
            if (go == null)
            {
                return null;
            }

            // GetComponentInChildren only walks active objects, so an inactive root has to go
            // through the real call every time (it returns null anyway), and an inactive cache hit
            // means the hierarchy changed under us - e.g. the player swapped hands.
            if (!go.activeInHierarchy)
            {
                return go.GetComponentInChildren<T>();
            }

            int id = go.GetInstanceID();
            T cached;
            if (Children.TryGetValue(id, out cached) && cached != null && cached.gameObject.activeInHierarchy)
            {
                return cached;
            }

            T found = go.GetComponentInChildren<T>();
            if (Children.Count >= MaxEntries)
            {
                Children.Clear();
            }

            Children[id] = found;
            return found;
        }

        internal static T GetInParent(GameObject go)
        {
            if (go == null)
            {
                return null;
            }

            int id = go.GetInstanceID();
            T cached;
            if (Parents.TryGetValue(id, out cached) && cached != null)
            {
                return cached;
            }

            T found = go.GetComponentInParent<T>();
            if (Parents.Count >= MaxEntries)
            {
                Parents.Clear();
            }

            Parents[id] = found;
            return found;
        }
    }

    /// <summary>Static entry points the transpilers emit calls to.</summary>
    public static class Cached
    {
        public static T CompGO<T>(GameObject go) where T : Component
        {
            return ComponentCache<T>.Get(go);
        }

        public static T CompC<T>(Component c) where T : Component
        {
            return c == null ? null : ComponentCache<T>.Get(c.gameObject);
        }

        public static T ChildGO<T>(GameObject go) where T : Component
        {
            return ComponentCache<T>.GetInChildren(go);
        }

        public static T ChildC<T>(Component c) where T : Component
        {
            return c == null ? null : ComponentCache<T>.GetInChildren(c.gameObject);
        }

        public static T ParentGO<T>(GameObject go) where T : Component
        {
            return ComponentCache<T>.GetInParent(go);
        }

        public static T ParentC<T>(Component c) where T : Component
        {
            return c == null ? null : ComponentCache<T>.GetInParent(c.gameObject);
        }

        private static Camera mainCamera;

        /// <summary>
        /// Camera.main is a tag search over the scene on Unity 2018 (it only started caching in
        /// 2020.2). Projectile.Update and Dart.FixedUpdate call it once per instance per frame.
        /// The isActiveAndEnabled check reproduces the "only enabled MainCamera-tagged cameras
        /// count" rule, so a disabled camera falls back to a real lookup.
        /// </summary>
        public static Camera MainCamera()
        {
            if (mainCamera == null || !mainCamera.isActiveAndEnabled)
            {
                mainCamera = Camera.main;
            }

            return mainCamera;
        }

        /// <summary>Keeps the second argument of a two-string Concat, dropping the literal prefix.</summary>
        public static string Second(string a, string b)
        {
            return b;
        }

        /// <summary>Keeps the first argument of a two-string Concat, dropping the literal suffix.</summary>
        public static string First(string a, string b)
        {
            return a;
        }

        /// <summary>
        /// Input.inputString allocates a fresh string on every read. Its only caller reads it once
        /// a frame purely to look for cheat-code letters, so skip the allocation on the frames
        /// where no key is held at all.
        /// </summary>
        public static string InputString()
        {
            return Input.anyKey ? Input.inputString : string.Empty;
        }

        private static readonly Dictionary<int, Material[]> PortalMaterials = new Dictionary<int, Material[]>();

        static Cached()
        {
            CacheRegistry.Register(delegate
            {
                PortalMaterials.Clear();
                mainCamera = null;
            });
        }

        /// <summary>
        /// Renderer.materials allocates a new array on every read (and instantiates the materials
        /// on the first one). PortalRenderer reads it once per portal per rendering camera per
        /// frame just to re-assign the same render texture.
        /// </summary>
        public static Material[] Materials(Renderer r)
        {
            if (r == null)
            {
                return null;
            }

            int id = r.GetInstanceID();
            Material[] cached;
            if (PortalMaterials.TryGetValue(id, out cached) && cached != null && cached.Length > 0 && cached[0] != null)
            {
                return cached;
            }

            Material[] found = r.materials;
            PortalMaterials[id] = found;
            return found;
        }
    }

    /// <summary>
    /// Shader.PropertyToID caching. The string overloads of Material.SetX do that lookup natively
    /// on every call; hoisting it into a managed dictionary skips a marshalled string per call.
    /// </summary>
    public static class ShaderIds
    {
        private static readonly Dictionary<string, int> Ids = new Dictionary<string, int>(64);

        public static int Id(string name)
        {
            int id;
            if (Ids.TryGetValue(name, out id))
            {
                return id;
            }

            id = Shader.PropertyToID(name);
            Ids[name] = id;
            return id;
        }

        public static void SetColor(Material m, string name, Color value)
        {
            m.SetColor(Id(name), value);
        }

        public static void SetFloat(Material m, string name, float value)
        {
            m.SetFloat(Id(name), value);
        }

        public static void SetVector(Material m, string name, Vector4 value)
        {
            m.SetVector(Id(name), value);
        }

        public static void SetTexture(Material m, string name, Texture value)
        {
            m.SetTexture(Id(name), value);
        }

        public static void SetMatrix(Material m, string name, Matrix4x4 value)
        {
            m.SetMatrix(Id(name), value);
        }

        public static bool HasProperty(Material m, string name)
        {
            return m.HasProperty(Id(name));
        }
    }
}

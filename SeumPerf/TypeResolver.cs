using System;
using System.Reflection;
using HarmonyLib;

namespace SeumPerf
{
    /// <summary>
    /// Resolves "Type:Member" specs. The game assembly is searched first on purpose: several of the
    /// type names involved (Path, Game) collide with BCL types, so a global search would happily
    /// return System.IO.Path. Other loaded assemblies are searched afterwards, which is how the
    /// profiler reaches types belonging to the other plugins in this repository.
    /// </summary>
    internal static class TypeResolver
    {
        private static readonly Assembly GameAssembly = typeof(Projectile).Assembly;

        internal static MethodBase ResolveMethod(string spec)
        {
            int split = spec.IndexOf(':');
            if (split <= 0)
            {
                return null;
            }

            Type type = ResolveType(spec.Substring(0, split));
            if (type == null)
            {
                return null;
            }

            string member = spec.Substring(split + 1);
            return member == ".ctor"
                ? (MethodBase)AccessTools.Constructor(type, Type.EmptyTypes)
                : AccessTools.Method(type, member);
        }

        internal static Type ResolveType(string fullName)
        {
            Type type = GameAssembly.GetType(fullName, throwOnError: false);
            if (type != null)
            {
                return type;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly == GameAssembly || assembly.IsDynamic)
                {
                    continue;
                }

                string name = assembly.GetName().Name;
                if (name.StartsWith("System", StringComparison.Ordinal)
                    || name.StartsWith("Unity", StringComparison.Ordinal)
                    || name == "mscorlib"
                    || name == "netstandard")
                {
                    continue;
                }

                try
                {
                    type = assembly.GetType(fullName, throwOnError: false);
                    if (type != null)
                    {
                        return type;
                    }
                }
                catch (Exception)
                {
                    // A half-loaded assembly is not worth failing the whole lookup over.
                }
            }

            return null;
        }
    }
}

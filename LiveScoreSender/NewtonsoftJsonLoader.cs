using System;
using System.IO;
using System.Reflection;

namespace LiveScoreSender
{
    // Релизная сборка BepInEx не несёт Newtonsoft.Json (несмотря на ожидания
    // из старого комментария в csproj), поэтому сборка встроена в
    // LiveScoreSender.dll как embedded resource и подгружается вручную через
    // AssemblyResolve — без лишнего файла рядом с плагином. Регистрировать
    // обработчик нужно из статического конструктора Plugin, а не из Awake():
    // Awake() сам обращается к типам Newtonsoft.Json, и CLR резолвит эту
    // сборку ещё до выполнения первой строки метода.
    internal static class NewtonsoftJsonLoader
    {
        private const string ResourceName = "LiveScoreSender.Newtonsoft.Json.dll";
        private const string AssemblySimpleName = "Newtonsoft.Json";

        private static Assembly _cached;
        private static bool _registered;

        internal static void EnsureRegistered()
        {
            if (_registered)
            {
                return;
            }

            _registered = true;
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (new AssemblyName(args.Name).Name != AssemblySimpleName)
            {
                return null;
            }

            if (_cached != null)
            {
                return _cached;
            }

            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName))
            {
                if (resourceStream == null)
                {
                    Plugin.Logger?.LogError($"[LiveScore] Embedded resource '{ResourceName}' не найден.");
                    return null;
                }

                byte[] bytes = new byte[resourceStream.Length];
                resourceStream.Read(bytes, 0, bytes.Length);
                _cached = Assembly.Load(bytes);
                return _cached;
            }
        }
    }
}

using System;
using System.Reflection;
using System.Text;
using Rewired;

namespace SeumInput
{
    /// <summary>
    /// Dumps Rewired's runtime configuration once, so the settings that shape input latency stop
    /// being invisible. Everything is read reflectively: the interesting values live on a helper
    /// object whose exact surface differs between Rewired versions, and a missing property should
    /// produce a gap in a log line rather than an exception on startup.
    /// </summary>
    internal static class RewiredReport
    {
        internal static void Log()
        {
            try
            {
                if (!ReInput.isReady)
                {
                    Plugin.Log.LogInfo("SeumInput: Rewired is not ready yet, skipping the report.");
                    return;
                }

                StringBuilder sb = new StringBuilder(512);
                sb.Append("SeumInput: Rewired ").Append(ReInput.programVersion);
                sb.Append(", usingUnityInput=").Append(ReInput.usingUnityInput);

                object configuration = ReInput.configuration;
                if (configuration != null)
                {
                    AppendProperties(sb, configuration);
                }

                Plugin.Log.LogInfo(sb.ToString());
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning($"SeumInput: could not read the Rewired configuration: {ex.Message}");
            }
        }

        private static void AppendProperties(StringBuilder sb, object target)
        {
            PropertyInfo[] properties = target.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (!property.CanRead || property.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                Type type = property.PropertyType;
                // Only the scalars are worth a log line; the object graph behind the rest is large
                // and says nothing about latency.
                if (!type.IsPrimitive && !type.IsEnum && type != typeof(string))
                {
                    continue;
                }

                try
                {
                    sb.Append("\n    ").Append(property.Name).Append(" = ").Append(property.GetValue(target, null));
                }
                catch (Exception)
                {
                    // A property that throws on read tells us nothing; skip it.
                }
            }
        }
    }
}

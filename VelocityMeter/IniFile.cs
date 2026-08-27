using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;

internal sealed class IniFile
{
    // The real Settings.ini written by the original mod stores everything under
    // "[Assembly-CSharp]" (apparently the original internal IniFile auto-namespaced by the
    // calling assembly). ModLoader.cs calls Read/Write/KeyExists without ever passing a
    // section, so without this default every lookup went to the empty section instead and
    // silently found nothing - settings loaded, but always came out empty.
    private const string DefaultSection = "Assembly-CSharp";

    private readonly string filePath;

    public IniFile(string fileName)
    {
        // Environment.CurrentDirectory isn't reliable under BepInEx's doorstop injection -
        // it doesn't always match the game folder the way it would for code running inside
        // the game's own assembly. Paths.GameRootPath is the same folder the original,
        // fully-in-DLL mod would have read/written Settings.ini from (next to the game exe).
        filePath = System.IO.Path.Combine(Paths.GameRootPath, fileName);
    }

    public bool KeyExists(string key, string section = null)
    {
        string value;
        return TryRead(key, section, out value);
    }

    public string Read(string key, string section = null)
    {
        string value;
        return TryRead(key, section, out value) ? value : string.Empty;
    }

    public void Write(string key, string value, string section = null)
    {
        Dictionary<string, Dictionary<string, string>> data = Load();
        string sectionName = section ?? DefaultSection;
        Dictionary<string, string> values;

        if (!data.TryGetValue(sectionName, out values))
        {
            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            data[sectionName] = values;
        }

        values[key] = value ?? string.Empty;
        Save(data);
    }

    private bool TryRead(string key, string section, out string value)
    {
        Dictionary<string, Dictionary<string, string>> data = Load();
        Dictionary<string, string> values;
        value = null;

        if (!data.TryGetValue(section ?? DefaultSection, out values))
        {
            return false;
        }

        return values.TryGetValue(key, out value);
    }

    private Dictionary<string, Dictionary<string, string>> Load()
    {
        var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        string sectionName = string.Empty;

        if (!File.Exists(filePath))
        {
            return result;
        }

        foreach (string rawLine in File.ReadAllLines(filePath, Encoding.UTF8))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line[0] == ';' || line[0] == '#')
            {
                continue;
            }

            if (line[0] == '[' && line[line.Length - 1] == ']')
            {
                sectionName = line.Substring(1, line.Length - 2).Trim();
                continue;
            }

            int separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            Dictionary<string, string> values;
            if (!result.TryGetValue(sectionName, out values))
            {
                values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                result[sectionName] = values;
            }

            string key = line.Substring(0, separator).Trim();
            string value = line.Substring(separator + 1).Trim();
            values[key] = value;
        }

        return result;
    }

    private void Save(Dictionary<string, Dictionary<string, string>> data)
    {
        using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> section in data)
            {
                if (section.Key.Length > 0)
                {
                    writer.WriteLine("[" + section.Key + "]");
                }

                foreach (KeyValuePair<string, string> entry in section.Value)
                {
                    writer.WriteLine(entry.Key + "=" + entry.Value);
                }
            }
        }
    }
}

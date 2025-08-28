using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CodeRebirthLib.Internal;
static class TagExporter
{
    const string _datetimeFormat = "dd_MM_yyyy-HH_mm";
    
    private static StreamWriter _outputFile;
    internal static void Init()
    {
        Directory.CreateDirectory(GetFolder());
        _outputFile = new StreamWriter(Path.Combine(GetFolder(), GetFileName(".md")));

        Application.quitting += () =>
        {
            _outputFile.Close();
            _outputFile.Dispose();
        };
        
        AddRegistry("Enemies", LethalContent.Enemies);
        AddRegistry("Moons", LethalContent.Moons);
        AddRegistry("MapObjects", LethalContent.MapObjects);
        AddRegistry("Items", LethalContent.Items);
        AddRegistry("Weathers", LethalContent.Weathers);
        AddRegistry("Dungeons", LethalContent.Dungeons);
        AddRegistry("Unlockables", LethalContent.Unlockables);
    }
    
    static string GetFileName(string extension) {
        return $"{DateTime.Now.ToString(_datetimeFormat)}{extension}";
    }

    static string GetFolder() {
        return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "tag-exports");
    }
    
    static void AddRegistry<T>(string name, TaggedRegistry<T> registry) where T : CRBaseInfo<T>
    {
        registry.AfterTagging += () =>
        {
            _outputFile.WriteLine($"## {name}");
            foreach ((NamespacedKey<T> key, T value) in registry)
            {
                _outputFile.WriteLine($"{key}:");
                WriteList(_outputFile, value.AllTags().Select(it => it.ToString()));
                _outputFile.WriteLine("");
                _outputFile.WriteLine("");
            }
            _outputFile.Flush();
        };
    }
    
    static void WriteList(StreamWriter stream, IEnumerable<string> list) {
        stream.WriteLine(string.Join("<br/>\n", list.Select(it => "- " + it)));
    }
}
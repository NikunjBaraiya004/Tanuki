using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class NamespaceInjector : AssetModificationProcessor
{
    private const string settingsAssetPath = "Assets/Editor/NamespaceSettings.asset";

    public static void OnWillCreateAsset(string path)
    {
        if (!path.EndsWith(".cs.meta")) return;

        path = path.Replace(".meta", "");
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), path);
        if (!File.Exists(fullPath)) return;

        string scriptText = File.ReadAllText(fullPath);
        if (scriptText.Contains("namespace")) return;

        string namespaceName = LoadNamespaceFromSettings();
        if (string.IsNullOrEmpty(namespaceName)) return;

        var lines = scriptText.Split('\n').ToList();

        // Extract using directives
        var usingLines = lines.TakeWhile(l => l.Trim().StartsWith("using")).ToList();
        var bodyLines = lines.Skip(usingLines.Count).ToList();

        // Indent class body
        for (int i = 0; i < bodyLines.Count; i++)
            bodyLines[i] = "    " + bodyLines[i];

        string finalText = string.Join("\n", usingLines) + "\n\n" +
                           $"namespace {namespaceName}\n{{\n" +
                           string.Join("\n", bodyLines) + "\n}";

        File.WriteAllText(fullPath, finalText);
        AssetDatabase.Refresh();
    }

    private static string LoadNamespaceFromSettings()
    {
        NamespaceSettings settings = AssetDatabase.LoadAssetAtPath<NamespaceSettings>(settingsAssetPath);
        return settings != null ? settings.defaultNamespace : null;
    }
}

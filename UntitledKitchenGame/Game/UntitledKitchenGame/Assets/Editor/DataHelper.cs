using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class DataHelper
{
    public static void MarkChangesForSaving(Object target)
    {
        Undo.RecordObject(target, "Data Import");
        EditorUtility.SetDirty(target);
    }

    public static T GetOrCreateAsset<T>(string name, Dictionary<string, T> existing, string folder) where T : ScriptableObject {
        if (!existing.TryGetValue(name, out T asset)) {
            
            asset = ScriptableObject.CreateInstance<T>();

            if (folder.StartsWith("Assets/")) folder = folder.Substring(7);
            if (folder.EndsWith('/')) folder = folder.Substring(0, folder.Length - 1);

            string folderPath = $"Assets/{folder}";
            if (!AssetDatabase.IsValidFolder(folderPath)) {
                Debug.LogWarning($"Folder path '{folderPath}' does not exist - creating it now.");
                var folders = folder.Split('/');
                folderPath = "Assets";
                foreach(var newFolder in folders) {
                    AssetDatabase.CreateFolder(folderPath, newFolder);
                    folderPath = $"{folderPath}/{newFolder}";
                }                
            }
            AssetDatabase.CreateAsset(asset, $"{folderPath}/{name}.asset");
            existing.Add(name, asset);
        }

        MarkChangesForSaving(asset);
        return asset;
    }

    public static Dictionary<string, T> GetAllAssetsOfType<T>() where T : Object {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        var collection = new Dictionary<string, T>(guids.Length);

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            collection.Add(asset.name, asset);
        }

        return collection;
    }
}

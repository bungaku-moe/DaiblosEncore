using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSpine;
using Il2CppSpine.Unity;
using Il2CppSystem.IO;
using UnityEngine;
using Directory = Il2CppSystem.IO.Directory;
using File = Il2CppSystem.IO.File;
using Path = Il2CppSystem.IO.Path;
using SearchOption = System.IO.SearchOption;

namespace DaiblosEncore.Patches;

public class SpineReplacer
{
    // ----- Patch Skeleton Data Binary if has .skel file -----
    [HarmonyPatch(typeof(SkeletonDataAsset), "ReadSkeletonData", typeof(byte[]), typeof(AttachmentLoader),
        typeof(float))]
    private static bool Prefix(
        SkeletonDataAsset __instance,
        ref byte[] bytes,
        AttachmentLoader attachmentLoader,
        float scale)
    {
        var spinePath = Path.Combine(Core.SpineModPath, __instance.name.Replace("SkeletonData", "spine").ToLower());
        var skelPath = Path.Combine(spinePath,
            Directory.GetFiles(spinePath).FirstOrDefault(path => path.EndsWith(".skel")));

        if (!File.Exists(skelPath))
        {
            Core.Log.Error($"Unable to read Skeleton Data file! {skelPath} does not exist!");
            return true;
        }
        
        try
        {
            bytes = File.ReadAllBytes(skelPath);
        }
        catch (Exception e)
        {
            Core.Log.Error(e);
        }

        return true;
    }

    [HarmonyPatch(typeof(SkeletonDataAsset), "GetSkeletonData")]
    private class Patch_SkeletonDataAsset_GetSkeletonData
    {
        private static void Prefix(SkeletonDataAsset __instance)
        {
            // ----- The assets should be exported by container name and keep the prefab folder name unchanged. -----
            var baseName = __instance.name.Replace("SkeletonData", "spine").ToLower();
            var spinePath = Path.Combine(Core.SpineModPath, baseName);
            string skelPath = "", jsonPath = "", atlasPath = "";
            var texturesPath = new Il2CppSystem.Collections.Generic.List<string>();
            
            var files = System.IO.Directory.GetFiles(spinePath, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var file in files.Where(f =>
                         f.EndsWith(".skel") || f.EndsWith(".json") || f.EndsWith(".atlas") || f.EndsWith(".png")))
                if (file.EndsWith(".skel"))
                    skelPath = file;
                else if (file.EndsWith(".json"))
                    jsonPath = file;
                else if (file.EndsWith(".atlas"))
                    atlasPath = file;
                else if (file.EndsWith(".png"))
                    texturesPath.Add(file);

            if (!File.Exists(atlasPath) || !File.Exists(jsonPath) || texturesPath.Count == 0)
            {
                Core.Log.Error($"Missing asset file(s) to use custom Spine model!");
                Core.Log.Msg($"Atlas: {atlasPath}");
                Core.Log.Msg($"Skeleton Data: {jsonPath}");
                Core.Log.Msg($"Skeleton Data Binary (Optional): {skelPath}");
                Core.Log.Msg($"Texture(s): {texturesPath.Count} texture(s)");
                return;
            }

            try
            {
                Core.Log.Msg($"Loading custom Spine model: {spinePath}");
                
                // ----- Load texture(s) -----
                var textures = new Texture2D[texturesPath.Count];
                for (var i = 0; i < textures.Length; i++)
                {
                    var texture = new Texture2D(1, 1)
                    {
                        name = Path.GetFileNameWithoutExtension(texturesPath[i])
                    };
                    texture.LoadImage(File.ReadAllBytes(texturesPath[i]));
                    texture.Apply(false, true);
                    textures[i] = texture;
                }

                // ----- Create atlas asset(s) -----
                var atlasAssetsBase = new Il2CppSystem.Collections.Generic.List<AtlasAssetBase>();
                foreach (var atlasAssetBase in __instance.atlasAssets)
                {
                    // ----- Copy original material(s) then update the texture -----
                    Material[] materials = atlasAssetBase.Materials.ToArray();
                    for (var j = 0; j < materials.Length; j++)
                        materials[j].mainTexture = textures[j];

                    var atlas = File.ReadAllText(atlasPath);
                    var atlasTextAsset = new TextAsset
                    {
                        name = Path.GetFileNameWithoutExtension(atlasPath)
                    };
                    TextAsset.Internal_CreateInstance(atlasTextAsset, atlas);
                    var atlasAsset = SpineAtlasAsset.CreateRuntimeInstance(
                        atlasTextAsset,
                        materials,
                        true
                    );
                    atlasAssetsBase.Add(atlasAsset);
                }

                var atlasAssets = new Il2CppReferenceArray<AtlasAssetBase>(atlasAssetsBase.Count);
                for (var i = 0; i < atlasAssetsBase.Count; i++)
                    atlasAssets[i] = atlasAssetsBase[i];

                __instance.atlasAssets = atlasAssets;

                if (File.Exists(skelPath))
                {
                    // ----- Trick Skeleton Reader 'flag' to read the file as binary file by having the skeletonJSON
                    // name contain .skel extension -----
                    Core.Log.Msg("Spine model has .skel file, use it over .json file.");
                    __instance.skeletonJSON.name = Path.GetFileName(skelPath);
                }
                else if (File.Exists(jsonPath))
                {
                    var skeletonJson = new TextAsset
                    {
                        name = Path.GetFileName(jsonPath)
                    };
                    TextAsset.Internal_CreateInstance(skeletonJson, File.ReadAllText(jsonPath));
                    __instance.skeletonJSON = skeletonJson;
                }

                // Clear cached data so Spine rebuilds it
                __instance.Clear();
            }
            catch (Exception e)
            {
                Core.Log.Error(e);
                return;
            }
        }
    }
}
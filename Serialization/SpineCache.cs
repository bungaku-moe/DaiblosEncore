using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSpine.Unity;
using UnityEngine;

namespace DaiblosEncore.Serialization;

internal class SpineCache
{
    public Il2CppReferenceArray<AtlasAssetBase>? AtlasAssets;
    public TextAsset? SkeletonJson;
}
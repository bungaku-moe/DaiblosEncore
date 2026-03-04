using DaiblosEncore;
using MelonLoader;
using MelonLoader.Logging;
using MelonLoader.Utils;

[assembly: MelonInfo(
    typeof(Core),
    "Daiblos Encore",
    "1.0.0",
    "kiraio",
    "https://github.com/kiraio-moe/DaiblosEncore/releases"
)]
[assembly: MelonGame("Megagame", "DAIBLOSCORE")]
[assembly: MelonColor(1, 48, 133, 213)]
[assembly: MelonAuthorColor(1, 255, 42, 122)]

namespace DaiblosEncore;

public class Core : MelonMod
{
    public static readonly MelonLogger.Instance Log = new("DaiblosEncore", ColorARGB.FromArgb(48, 133, 213));   
    
    private static string? _basePath;
    public static string? SpineModPath;
    
    public override void OnInitializeMelon()
    {
        _basePath = Path.Combine(MelonEnvironment.ModsDirectory, "DaiblosEncore");
        Directory.CreateDirectory(_basePath);

        SpineModPath = Path.Combine(_basePath, "Spine");
        Directory.CreateDirectory(SpineModPath);

        HarmonyInstance.PatchAll();
    }
}
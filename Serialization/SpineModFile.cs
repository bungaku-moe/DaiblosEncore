using UnityEngine;

namespace DaiblosEncore.Serialization;

[Serializable]
public class SpineModFile
{
    public SpineSettings Spine = new();

    public class SpineSettings
    {
        public Vector3 Position = Vector3.zero;
        public Vector3 Rotation = Vector3.zero;
        public Vector3 Scale = Vector3.one;
    }
}
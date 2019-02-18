using UnityEngine;
using QuickVR;

public class QuickUnityVRBaseAsset : ScriptableObject
{
 
    public System.Int32 _priority;
    public UnityEngine.Texture2D _calibrationTexture;
    public UnityEngine.LayerMask _visibleLayers;
    public System.Boolean _applyHeadRotation;
    public System.Boolean _applyHeadPosition;
    public UnityEngine.Camera _pfCamera;
    public System.Single _cameraNearPlane;
    public System.Single _cameraFarPlane;
    [BitMask(typeof(QuickVR.IKLimbBones), 0, 4)]
    public System.Int32 _trackedJoints;
    public System.Boolean _useFootprints;
    public UnityEngine.Transform _footprints;
 
}

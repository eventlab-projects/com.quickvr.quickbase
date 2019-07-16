using UnityEngine;
using QuickVR;

public class QuickUnityVRAsset : ScriptableObject
{
 
    public System.Int32 _priority;
    public UnityEngine.Texture2D _calibrationTexture;
    public UnityEngine.LayerMask _visibleLayers;
    public System.Boolean _applyHeadRotation;
    public System.Boolean _applyHeadPosition;
    public UnityEngine.Camera _pfCamera;
    public System.Single _cameraNearPlane;
    public System.Single _cameraFarPlane;
    [BitMask(typeof(QuickVR.IKLimbBones), 0, 5)]
    public System.Int32 _trackedJoints;
    public System.Boolean _useFootprints;
    public UnityEngine.Transform _footprints;
    public System.Boolean _displaceWithCamera;
    public System.Boolean _rotateWithCamera;
    public System.Boolean _isStanding;
    public QuickVR.QuickUnityVR.UpdateMode _updateMode;
 
}

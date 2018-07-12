using UnityEngine;
using QuickVR;

public class QuickUnityVRHandsAsset : ScriptableObject
{
 
    public System.Int32 _priority;
    public UnityEngine.Texture2D _calibrationTexture;
    public UnityEngine.LayerMask _visibleLayers;
    public System.Boolean _applyHeadRotation;
    public System.Boolean _applyHeadPosition;
    public System.Single _cameraNearPlane;
    public System.Single _cameraFarPlane;
    public UnityEngine.Vector3 _handControllerPositionOffset;
    [BitMask(typeof(QuickVR.IKLimbBones), 0, 4)]
    public System.Int32 _trackedJoints;
    public QuickVR.QuickVRHand _pfHandLeft;
    public QuickVR.QuickVRHand _pfHandRight;
 
}

using UnityEngine;
using QuickVR;

public class QuickUnityVRBodyTrackingAsset : ScriptableObject
{
 
    public System.Int32 _priority;
    public QuickVR.QuickBodyTracking.DebugMode _debugMode;
    [BitMask(typeof(QuickVR.QuickJointType), 0, 22)]
    public System.Int32 _trackedJointsBodyMask;
    [BitMask(typeof(QuickVR.QuickJointType), 23, 37)]
    public System.Int32 _trackedJointsLeftHandMask;
    [BitMask(typeof(QuickVR.QuickJointType), 38, 52)]
    public System.Int32 _trackedJointsRightHandMask;
    public System.Boolean _applyRootMotionX;
    public System.Boolean _applyRootMotionY;
    public System.Boolean _applyRootMotionZ;
    public UnityEngine.Vector3 _handControllerPositionOffset;
    public System.Boolean _useFootprints;
    public UnityEngine.Transform _footprints;
 
}

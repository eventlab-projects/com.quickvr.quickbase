using UnityEngine;
using QuickVR;

public class ErcBodyTrackingAsset : ScriptableObject
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
    public UnityEngine.TextAsset _dataFile;
    public System.Single _samplingRate;
    public System.Boolean _autoPlay;
    public System.Int32 _currentFrameID;
    public ErcBodyTracking.PlayMode _playMode;
    public System.Boolean _feetOnFloor;
    public System.Boolean _mirror;
    public System.Int32 _numFrames;
 
}

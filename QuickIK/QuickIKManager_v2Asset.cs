using UnityEngine;
using QuickVR;

public class QuickIKManager_v2Asset : ScriptableObject
{
 
    public System.Int32 _priority;
    public System.Boolean _ikActive;
    [BitMask(typeof(QuickVR.IKLimbBones), 0, 4)]
    public System.Int32 _ikMask;
    [BitMask(typeof(QuickVR.IKLimbBones), 0, 4)]
    public System.Int32 _ikHintMaskUpdate;
 
}

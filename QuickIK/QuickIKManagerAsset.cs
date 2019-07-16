using UnityEngine;
using QuickVR;

public class QuickIKManagerAsset : ScriptableObject
{
 
    public System.Int32 _priority;
    public System.Boolean _ikActive;
    [BitMask(typeof(QuickVR.IKLimbBones), 0, 5)]
    public System.Int32 _ikMaskBody;
    [BitMask(typeof(QuickVR.IKLimbBones), 0, 5)]
    public System.Int32 _ikHintMaskUpdate;
 
}

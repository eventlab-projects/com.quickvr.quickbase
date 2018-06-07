using UnityEngine;
using QuickVR;

public class QuickIKManagerHumanoidAsset : ScriptableObject
{
 
    public System.Int32 _priority;
    public System.Boolean _ikActive;
    [BitMask(typeof(QuickVR.IKLimbBones), 0, 4)]
    public System.Int32 _ikMask;
 
}

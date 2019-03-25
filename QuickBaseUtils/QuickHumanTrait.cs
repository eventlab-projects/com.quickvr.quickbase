using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Extents the functionalities of Unity's HumanTrait class
public static class QuickHumanTrait
{

    #region PRIVATE ATTRIBUTES

    private static Dictionary<int, List<int>> _childBones = null;
    private static List<int> _bonesHierarchy = null;

    private static Dictionary<HumanBodyBones, HumanBodyBones> _lookAtBone = null;

    #endregion

    #region CREATION AND DESTRUCTION

    private static void CheckChildBones()
    {
        if (_childBones == null)
        {
            _childBones = new Dictionary<int, List<int>>();
            for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
            {
                int parentID = HumanTrait.GetParentBone(i);
                if (!_childBones.ContainsKey(parentID)) _childBones[parentID] = new List<int>();

                _childBones[parentID].Add(i);
            }
        }
    }

    private static void InitLookAtBones()
    {
        if (_lookAtBone == null)
        {
            _lookAtBone = new Dictionary<HumanBodyBones, HumanBodyBones>();

            //Spine
            _lookAtBone[HumanBodyBones.Hips] = HumanBodyBones.Spine;
            _lookAtBone[HumanBodyBones.Spine] = HumanBodyBones.Chest;
            _lookAtBone[HumanBodyBones.Chest] = HumanBodyBones.UpperChest;
            _lookAtBone[HumanBodyBones.UpperChest] = HumanBodyBones.Neck;
            _lookAtBone[HumanBodyBones.Neck] = HumanBodyBones.Head;

            //LeftArm
            _lookAtBone[HumanBodyBones.LeftShoulder] = HumanBodyBones.LeftUpperArm;
            _lookAtBone[HumanBodyBones.LeftUpperArm] = HumanBodyBones.LeftLowerArm;
            _lookAtBone[HumanBodyBones.LeftLowerArm] = HumanBodyBones.LeftHand;

            //RightArm
            _lookAtBone[HumanBodyBones.RightShoulder] = HumanBodyBones.RightUpperArm;
            _lookAtBone[HumanBodyBones.RightUpperArm] = HumanBodyBones.RightLowerArm;
            _lookAtBone[HumanBodyBones.RightLowerArm] = HumanBodyBones.RightHand;

            //LeftLeg
            _lookAtBone[HumanBodyBones.LeftUpperLeg] = HumanBodyBones.LeftLowerLeg;
            _lookAtBone[HumanBodyBones.LeftLowerLeg] = HumanBodyBones.LeftFoot;
            _lookAtBone[HumanBodyBones.LeftFoot] = HumanBodyBones.LeftToes;

            //RightLeg
            _lookAtBone[HumanBodyBones.RightUpperLeg] = HumanBodyBones.RightLowerLeg;
            _lookAtBone[HumanBodyBones.RightLowerLeg] = HumanBodyBones.RightFoot;
            _lookAtBone[HumanBodyBones.RightFoot] = HumanBodyBones.RightToes;

            //LeftHand
            _lookAtBone[HumanBodyBones.LeftThumbProximal] = HumanBodyBones.LeftThumbIntermediate;
            _lookAtBone[HumanBodyBones.LeftThumbIntermediate] = HumanBodyBones.LeftThumbDistal;
            _lookAtBone[HumanBodyBones.LeftIndexProximal] = HumanBodyBones.LeftIndexIntermediate;
            _lookAtBone[HumanBodyBones.LeftIndexIntermediate] = HumanBodyBones.LeftIndexDistal;
            _lookAtBone[HumanBodyBones.LeftMiddleProximal] = HumanBodyBones.LeftMiddleIntermediate;
            _lookAtBone[HumanBodyBones.LeftMiddleIntermediate] = HumanBodyBones.LeftMiddleDistal;
            _lookAtBone[HumanBodyBones.LeftRingProximal] = HumanBodyBones.LeftRingIntermediate;
            _lookAtBone[HumanBodyBones.LeftRingIntermediate] = HumanBodyBones.LeftRingDistal;
            _lookAtBone[HumanBodyBones.LeftLittleProximal] = HumanBodyBones.LeftLittleIntermediate;
            _lookAtBone[HumanBodyBones.LeftLittleIntermediate] = HumanBodyBones.LeftLittleDistal;

            //RightHand
            _lookAtBone[HumanBodyBones.RightThumbProximal] = HumanBodyBones.RightThumbIntermediate;
            _lookAtBone[HumanBodyBones.RightThumbIntermediate] = HumanBodyBones.RightThumbDistal;
            _lookAtBone[HumanBodyBones.RightIndexProximal] = HumanBodyBones.RightIndexIntermediate;
            _lookAtBone[HumanBodyBones.RightIndexIntermediate] = HumanBodyBones.RightIndexDistal;
            _lookAtBone[HumanBodyBones.RightMiddleProximal] = HumanBodyBones.RightMiddleIntermediate;
            _lookAtBone[HumanBodyBones.RightMiddleIntermediate] = HumanBodyBones.RightMiddleDistal;
            _lookAtBone[HumanBodyBones.RightRingProximal] = HumanBodyBones.RightRingIntermediate;
            _lookAtBone[HumanBodyBones.RightRingIntermediate] = HumanBodyBones.RightRingDistal;
            _lookAtBone[HumanBodyBones.RightLittleProximal] = HumanBodyBones.RightLittleIntermediate;
            _lookAtBone[HumanBodyBones.RightLittleIntermediate] = HumanBodyBones.RightLittleDistal;
        }
    }

    private static void InitBonesHierarchy(int boneID)
    {
        List<int> childBones = GetChildBones(boneID);
        _bonesHierarchy.AddRange(childBones);

        foreach (int c in childBones)
        {
            InitBonesHierarchy(c);
        }
    }

    #endregion

    #region GET AND SET

    public static List<int> GetBonesHierarchy()
    {
        if (_bonesHierarchy == null)
        {
            CheckChildBones();
            _bonesHierarchy = new List<int>();

            int initialBone = (int)HumanBodyBones.Hips;
            _bonesHierarchy.Add(initialBone);
            InitBonesHierarchy(initialBone);
        }

        return _bonesHierarchy;
    }

    public static int GetNumBones()
    {
        return HumanTrait.BoneCount;
    }

    public static string GetBoneName(int boneID)
    {
        return HumanTrait.BoneName[boneID];
    }

    public static int GetNumMuscles()
    {
        return HumanTrait.MuscleCount;
    }

    public static string GetMuscleName(int muscleID)
    {
        return HumanTrait.MuscleName[muscleID];
    }

    public static int GetRequiredBoneCount()
    {
        return HumanTrait.RequiredBoneCount;
    }

    public static int GetMuscleFromBone(int boneID, int dofID)
    {
        return HumanTrait.MuscleFromBone(boneID, dofID);
    }

    public static int GetBoneFromMuscle(int muscleID)
    {
        return HumanTrait.BoneFromMuscle(muscleID);
    }

    public static float GetBoneDefaultHierarchyMass(int boneID)
    {
        return HumanTrait.GetBoneDefaultHierarchyMass(boneID);
    }

    public static float GetMuscleDefaultMin(int muscleID)
    {
        return HumanTrait.GetMuscleDefaultMin(muscleID);
    }

    public static float GetMuscleDefaultMax(int muscleID)
    {
        return HumanTrait.GetMuscleDefaultMax(muscleID);
    }

    public static int GetParentBone(int boneID)
    {
        return HumanTrait.GetParentBone(boneID);
    }

    public static List<int> GetChildBones(int boneID)
    {
        CheckChildBones();

        return _childBones.ContainsKey(boneID)? _childBones[boneID] : new List<int>();
    }

    public static bool IsRequiredBone(int boneID)
    {
        return HumanTrait.RequiredBone(boneID);
    }

    public static HumanBodyBones? GetLookAtBone(HumanBodyBones boneID)
    {
        InitLookAtBones();

        HumanBodyBones? result = null;
        if (_lookAtBone.ContainsKey(boneID)) result = _lookAtBone[boneID];

        return result;
    }

    public static Vector3 GetDefaultLookAtDirection(HumanBodyBones boneID)
    {
        string bName = boneID.ToString();

        if (bName.Contains("Foot")) return Vector3.forward + Vector3.down;

        if (bName.Contains("Proximal") || bName.Contains("Intermediate") || bName.Contains("Distal"))
        {
            return bName.Contains("Left") ? Vector3.left : Vector3.right;
        }

        return Vector3.zero;
    }

    #endregion

}

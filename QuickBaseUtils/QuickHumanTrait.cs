using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Extents the functionalities of Unity's HumanTrait class
public static class QuickHumanTrait
{

    #region PRIVATE ATTRIBUTES

    private static Dictionary<int, List<int>> _childBones = null;
    private static List<int> _bonesHierarchy = null;

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

    #endregion

}

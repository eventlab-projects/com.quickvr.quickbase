using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{
    //Extents the functionalities of Unity's HumanTrait class

    public enum QuickHumanBodyBones
    {
        Hips = HumanBodyBones.Hips,

        //Spine bones
        Spine = HumanBodyBones.Spine,
        Chest = HumanBodyBones.Chest,
        UpperChest = HumanBodyBones.UpperChest,
        Neck = HumanBodyBones.Neck,
        Head = HumanBodyBones.Head,

        //Face bones
        LeftEye = HumanBodyBones.LeftEye,
        RightEye = HumanBodyBones.RightEye,
        Jaw = HumanBodyBones.Jaw,

        //Left leg bones
        LeftUpperLeg = HumanBodyBones.LeftUpperLeg,
        LeftLowerLeg = HumanBodyBones.LeftLowerLeg,
        LeftFoot = HumanBodyBones.LeftFoot,
        LeftToes = HumanBodyBones.LeftToes,

        //Right leg bones
        RightUpperLeg = HumanBodyBones.RightUpperLeg,
        RightLowerLeg = HumanBodyBones.RightLowerLeg,
        RightFoot = HumanBodyBones.RightFoot,
        RightToes = HumanBodyBones.RightToes,

        //Left arm bones
        LeftShoulder = HumanBodyBones.LeftShoulder,
        LeftUpperArm = HumanBodyBones.LeftUpperArm,
        LeftLowerArm = HumanBodyBones.LeftLowerArm,
        LeftHand = HumanBodyBones.LeftHand,

        //Right arm bones
        RightShoulder = HumanBodyBones.RightShoulder,
        RightUpperArm = HumanBodyBones.RightUpperArm,
        RightLowerArm = HumanBodyBones.RightLowerArm,
        RightHand = HumanBodyBones.RightHand,

        //Left hand finger bones        
        LeftThumbProximal = HumanBodyBones.LeftThumbProximal,
        LeftThumbIntermediate = HumanBodyBones.LeftThumbIntermediate,
        LeftThumbDistal = HumanBodyBones.LeftThumbDistal,
        LeftIndexProximal = HumanBodyBones.LeftIndexProximal,
        LeftIndexIntermediate = HumanBodyBones.LeftIndexIntermediate,
        LeftIndexDistal = HumanBodyBones.LeftIndexDistal,
        LeftMiddleProximal = HumanBodyBones.LeftMiddleProximal,
        LeftMiddleIntermediate = HumanBodyBones.LeftMiddleIntermediate,
        LeftMiddleDistal = HumanBodyBones.LeftMiddleDistal,
        LeftRingProximal = HumanBodyBones.LeftRingProximal,
        LeftRingIntermediate = HumanBodyBones.LeftRingIntermediate,
        LeftRingDistal = HumanBodyBones.LeftRingDistal,
        LeftLittleProximal = HumanBodyBones.LeftLittleProximal,
        LeftLittleIntermediate = HumanBodyBones.LeftLittleIntermediate,
        LeftLittleDistal = HumanBodyBones.LeftLittleDistal,

        //Right hand finger bones
        RightThumbProximal = HumanBodyBones.RightThumbProximal,
        RightThumbIntermediate = HumanBodyBones.RightThumbIntermediate,
        RightThumbDistal = HumanBodyBones.RightThumbDistal,
        RightIndexProximal = HumanBodyBones.RightIndexProximal,
        RightIndexIntermediate = HumanBodyBones.RightIndexIntermediate,
        RightIndexDistal = HumanBodyBones.RightIndexDistal,
        RightMiddleProximal = HumanBodyBones.RightMiddleProximal,
        RightMiddleIntermediate = HumanBodyBones.RightMiddleIntermediate,
        RightMiddleDistal = HumanBodyBones.RightMiddleDistal,
        RightRingProximal = HumanBodyBones.RightRingProximal,
        RightRingIntermediate = HumanBodyBones.RightRingIntermediate,
        RightRingDistal = HumanBodyBones.RightRingDistal,
        RightLittleProximal = HumanBodyBones.RightLittleProximal,
        RightLittleIntermediate = HumanBodyBones.RightLittleIntermediate,
        RightLittleDistal = HumanBodyBones.RightLittleDistal,
        
        //Left hand tip bones
        LeftThumbTip = HumanBodyBones.LastBone, 
        LeftIndexTip, 
        LeftMiddleTip,
        LeftRingTip,
        LeftLittleTip,

        //Right hand tip bones
        RightThumbTip,
        RightIndexTip,
        RightMiddleTip,
        RightRingTip,
        RightLittleTip,
    }

    public enum QuickHumanFingers
    {
        Thumb, 
        Index,
        Middle,
        Ring,
        Little,
    }

    public static class QuickHumanTrait
    {

        #region PRIVATE ATTRIBUTES

        private static Dictionary<QuickHumanBodyBones, QuickHumanBodyBones> _parentBone = null;
        private static Dictionary<int, List<int>> _childBones = null;
        private static List<int> _bonesHierarchy = null;

        private static Dictionary<HumanBodyBones, HumanBodyBones> _lookAtBone = null;

        private static List<string> _muscleNames = null;

        private static List<HumanBodyBones> _humanBodyBones = null;
        private static List<QuickHumanFingers> _fingers = null;
        private static List<QuickHumanBodyBones> _handBoneTips = null;
        private static List<QuickHumanBodyBones> _handBoneTipsLeft = null;
        private static List<QuickHumanBodyBones> _HandBoneTipsRight = null;

        private static Dictionary<QuickHumanFingers, List<QuickHumanBodyBones>> _bonesFromFingerLeft = null;
        private static Dictionary<QuickHumanFingers, List<QuickHumanBodyBones>> _bonesFromFingerRight = null;
        private static Dictionary<QuickHumanBodyBones, QuickHumanFingers> _fingerFromBone = new Dictionary<QuickHumanBodyBones, QuickHumanFingers>();
        private static HashSet<QuickHumanBodyBones> _fingerBonesLeft = new HashSet<QuickHumanBodyBones>();
        private static HashSet<QuickHumanBodyBones> _fingerBonesRight = new HashSet<QuickHumanBodyBones>();

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            InitMuscleNames();
            InitLookAtBones();
            InitBonesHierarchy();
            InitHandBoneTips();
        }

        private static Dictionary<QuickHumanFingers, List<QuickHumanBodyBones>> InitHumanFingers(bool isLeft)
        {
            Dictionary<QuickHumanFingers, List<QuickHumanBodyBones>> result = new Dictionary<QuickHumanFingers, List<QuickHumanBodyBones>>();
            string prefix = isLeft ? "Left" : "Right";
            string[] phalanges = { "Proximal", "Intermediate", "Distal", "Tip" };
            foreach (QuickHumanFingers f in GetHumanFingers())
            {
                result[f] = new List<QuickHumanBodyBones>();
                foreach (string p in phalanges)
                {
                    QuickHumanBodyBones fingerBone = QuickUtils.ParseEnum<QuickHumanBodyBones>(prefix + f.ToString() + p);
                    result[f].Add(fingerBone);
                    _fingerFromBone[fingerBone] = f;
                    if (isLeft) _fingerBonesLeft.Add(fingerBone);
                    else _fingerBonesRight.Add(fingerBone);
                }
            }

            return result;
        }

        private static void InitMuscleNames()
        {
            _muscleNames = new List<string>();

            for (int muscleID = 0; muscleID < HumanTrait.MuscleCount; muscleID++)
            {
                string name = HumanTrait.MuscleName[muscleID];
                for (int h = 0; h <= 1; h++)
                {
                    string hName = h == 0 ? "Left" : "Right";
                    foreach (QuickHumanFingers f in GetHumanFingers())
                    {
                        for (int i = 1; i <= 3; i++)
                        {
                            if (name == hName + " " + f.ToString() + " " + i.ToString() + " Stretched") name = hName + "Hand." + f.ToString() + "." + i.ToString() + " Stretched";
                        }
                    }
                    foreach (QuickHumanFingers f in GetHumanFingers())
                    {
                        if (name == hName + " " + f.ToString() + " Spread") name = hName + "Hand." + f.ToString() + ".Spread";
                    }
                }

                _muscleNames.Add(name);
            }
        }

        private static void InitLookAtBones()
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

        private static void InitParentBones()
        {
            _parentBone = new Dictionary<QuickHumanBodyBones, QuickHumanBodyBones>();
            _parentBone[QuickHumanBodyBones.LeftThumbTip] = QuickHumanBodyBones.LeftThumbDistal;
            _parentBone[QuickHumanBodyBones.LeftIndexTip] = QuickHumanBodyBones.LeftIndexDistal;
            _parentBone[QuickHumanBodyBones.LeftMiddleTip] = QuickHumanBodyBones.LeftMiddleDistal;
            _parentBone[QuickHumanBodyBones.LeftRingTip] = QuickHumanBodyBones.LeftRingDistal;
            _parentBone[QuickHumanBodyBones.LeftLittleTip] = QuickHumanBodyBones.LeftLittleDistal;

            _parentBone[QuickHumanBodyBones.RightThumbTip] = QuickHumanBodyBones.RightThumbDistal;
            _parentBone[QuickHumanBodyBones.RightIndexTip] = QuickHumanBodyBones.RightIndexDistal;
            _parentBone[QuickHumanBodyBones.RightMiddleTip] = QuickHumanBodyBones.RightMiddleDistal;
            _parentBone[QuickHumanBodyBones.RightRingTip] = QuickHumanBodyBones.RightRingDistal;
            _parentBone[QuickHumanBodyBones.RightLittleTip] = QuickHumanBodyBones.RightLittleDistal;
        }

        private static void InitBonesHierarchy()
        {
            _bonesHierarchy = new List<int>();

            int initialBone = (int)HumanBodyBones.Hips;
            _bonesHierarchy.Add(initialBone);
            InitBonesHierarchy(initialBone);
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

        private static void InitHandBoneTips()
        {
            _handBoneTipsLeft = InitHandBoneTips(true);
            _HandBoneTipsRight = InitHandBoneTips(false);

            _handBoneTips = new List<QuickHumanBodyBones>();
            _handBoneTips.AddRange(_handBoneTipsLeft);
            _handBoneTips.AddRange(_HandBoneTipsRight);
        }

        private static List<QuickHumanBodyBones> InitHandBoneTips(bool isLeft)
        {
            List<QuickHumanBodyBones> result = new List<QuickHumanBodyBones>();
            int initialBoneID = isLeft ? (int)QuickHumanBodyBones.LeftThumbTip : (int)QuickHumanBodyBones.LeftLittleTip;
            int finalBoneID = isLeft ? (int)QuickHumanBodyBones.RightThumbTip : (int)QuickHumanBodyBones.RightLittleTip;
            
            for (int i = initialBoneID; i <= finalBoneID; i++)
            {
                result.Add((QuickHumanBodyBones)i);
            }

            return result;
        }

        #endregion

        #region GET AND SET

        public static List<int> GetBonesHierarchy()
        {
            return _bonesHierarchy;
        }

        public static int GetNumBones()
        {
            return HumanTrait.BoneCount;
        }

        public static string GetBoneName(HumanBodyBones boneID)
        {
            return GetBoneName((int)boneID);
        }

        public static string GetBoneName(int boneID)
        {
            return HumanTrait.BoneName[boneID];
        }

        public static int GetNumMuscles()
        {
            return HumanTrait.MuscleCount;
        }

        public static List<string> GetMuscleNames()
        {
            return _muscleNames;
        }

        public static string GetMuscleName(int muscleID)
        {
            return GetMuscleNames()[muscleID];
        }

        public static int GetRequiredBoneCount()
        {
            return HumanTrait.RequiredBoneCount;
        }

        public static int GetMuscleFromBone(QuickHumanBodyBones boneID, int dofID)
        {
            return (int)boneID < (int)HumanBodyBones.LastBone ? GetMuscleFromBone((HumanBodyBones)boneID, dofID) : -1;
        }

        public static int GetMuscleFromBone(HumanBodyBones boneID, int dofID)
        {
            return HumanTrait.MuscleFromBone((int)boneID, dofID);
        }

        public static HumanBodyBones GetBoneFromMuscle(int muscleID)
        {
            return (HumanBodyBones)HumanTrait.BoneFromMuscle(muscleID);
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

        public static List<HumanBodyBones> GetHumanBodyBones()
        {
            if (_humanBodyBones == null)
            {
                _humanBodyBones = QuickUtils.GetEnumValues<HumanBodyBones>();
                _humanBodyBones.RemoveAt(_humanBodyBones.Count - 1);  //Remove the LastBone, which is not a valid HumanBodyBone ID
            }

            return _humanBodyBones;
        }

        public static int GetParentBone(int boneID)
        {
            return HumanTrait.GetParentBone(boneID);
        }

        public static HumanBodyBones GetParentBone(HumanBodyBones boneID)
        {
            return (HumanBodyBones)GetParentBone((int)boneID);
        }

        public static QuickHumanBodyBones GetParentBone(QuickHumanBodyBones boneID)
        {
            if ((int)boneID < (int)HumanBodyBones.LastBone)
            {
                return (QuickHumanBodyBones)GetParentBone((int)boneID);
            }

            if (_parentBone == null)
            {
                InitParentBones();
            }

            return _parentBone[boneID];
        }

        public static List<HumanBodyBones> GetChildBones(HumanBodyBones boneID)
        {
            List<int> tmp = GetChildBones((int)boneID);

            List<HumanBodyBones> result = new List<HumanBodyBones>();
            foreach (int i in tmp)
            {
                result.Add((HumanBodyBones)i);
            }

            return result;
        }

        public static List<int> GetChildBones(int boneID)
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

            return _childBones.ContainsKey(boneID) ? _childBones[boneID] : new List<int>();
        }

        public static bool IsRequiredBone(int boneID)
        {
            return HumanTrait.RequiredBone(boneID);
        }

        public static HumanBodyBones? GetLookAtBone(HumanBodyBones boneID)
        {
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

        public static List<QuickHumanBodyBones> GetHandBoneTips()
        {
            return _handBoneTips;
        }

        public static List<QuickHumanBodyBones> GetHandBoneTips(bool isLeft)
        {
            return isLeft ? _handBoneTipsLeft : _HandBoneTipsRight;
        }

        public static Transform GetBoneTransform(this Animator animator, QuickHumanBodyBones boneID)
        {
            Transform result = null;

            if ((int)boneID < (int)HumanBodyBones.LastBone)
            {
                result = animator.GetBoneTransform((HumanBodyBones)boneID);
            }
            else
            {
                //At this point, boneID is a bone finger tip
                result = animator.GetBoneTransform(GetParentBone(boneID)).GetChild(0);
            }

            return result;
        }

        public static List<QuickHumanFingers> GetHumanFingers()
        {
            if (_fingers == null)
            {
                _fingers = QuickUtils.GetEnumValues<QuickHumanFingers>();
            }

            return _fingers;
        }

        public static List<QuickHumanBodyBones> GetBonesFromFinger(QuickHumanFingers finger, bool isLeft)
        {
            if (isLeft)
            {
                if (_bonesFromFingerLeft == null)
                {
                    _bonesFromFingerLeft = InitHumanFingers(true);
                }

                return _bonesFromFingerLeft[finger];
            }

            if (_bonesFromFingerRight == null)
            {
                _bonesFromFingerRight = InitHumanFingers(false);
            }

            return _bonesFromFingerRight[finger];
        }

        public static QuickHumanFingers GetFingerFromBone(QuickHumanBodyBones bone)
        {
            return _fingerFromBone[bone];
        }

        public static bool IsBoneFingerLeft(QuickHumanBodyBones boneID)
        {
            return _fingerBonesLeft.Contains(boneID);
        }

        public static bool IsBoneFingerLeft(HumanBodyBones boneID)
        {
            return IsBoneFingerLeft((QuickHumanBodyBones)boneID);
        }

        public static bool IsBoneFingerRight(QuickHumanBodyBones boneID)
        {
            return _fingerBonesRight.Contains(boneID);
        }

        public static bool IsBoneFingerRight(HumanBodyBones boneID)
        {
            return IsBoneFingerRight((QuickHumanBodyBones)boneID);
        }

        public static bool IsFingerBoneLeft(QuickHumanBodyBones bone)
        {
            return _fingerBonesLeft.Contains(bone);
        }

        #endregion

        #region ANIMATOR EXTENSIONS

        public static void CreateMissingBones(this Animator animator)
        {
            animator.CreateEyes();
            animator.CreateFingerTips();
        }

        private static void CreateEyes(this Animator animator)
        {
            CreateEye(animator, true);
            CreateEye(animator, false);
        }

        private static void CreateEye(this Animator animator, bool eyeLeft)
        {
            if (!animator.GetEye(eyeLeft))
            {
                HumanBodyBones eyeBoneID = eyeLeft ? HumanBodyBones.LeftEye : HumanBodyBones.RightEye;
                if (!animator.GetBoneTransform(eyeBoneID))
                {
                    Transform tHead = animator.GetBoneTransform(HumanBodyBones.Head);
                    Transform tEye = tHead.CreateChild(eyeBoneID.ToString());

                    //The eye center position
                    tEye.position = tHead.position + animator.transform.forward * 0.15f + animator.transform.up * 0.13f;

                    //Account for the Eye Separation
                    float sign = eyeLeft ? -1.0f : 1.0f;
                    tEye.position += sign * animator.transform.right * 0.032f;
                }
            }
        }

        private static void CreateFingerTips(this Animator animator)
        {
            HumanBodyBones[] distalBones =
            {
                HumanBodyBones.LeftThumbDistal,
                HumanBodyBones.LeftIndexDistal,
                HumanBodyBones.LeftMiddleDistal,
                HumanBodyBones.LeftRingDistal,
                HumanBodyBones.LeftLittleDistal,

                HumanBodyBones.RightThumbDistal,
                HumanBodyBones.RightIndexDistal,
                HumanBodyBones.RightMiddleDistal,
                HumanBodyBones.RightRingDistal,
                HumanBodyBones.RightLittleDistal
            };

            foreach (HumanBodyBones boneID in distalBones)
            {
                Transform tBoneDistal = animator.GetBoneTransform(boneID);
                if (!tBoneDistal.GetChild(0))
                {
                    Transform tBoneIntermediate = animator.GetBoneTransform(GetParentBone(boneID));
                    Vector3 v = tBoneDistal.position - tBoneIntermediate.position;

                    Transform tBoneTip = tBoneDistal.CreateChild("__FingerTip__");
                    tBoneTip.position = tBoneDistal.position + v;
                }
            }
        }

        public static Transform GetEye(this Animator animator, bool eyeLeft)
        {
            HumanBodyBones eyeBoneID = eyeLeft ? HumanBodyBones.LeftEye : HumanBodyBones.RightEye;
            Transform eye = animator.GetBoneTransform(eyeBoneID);
            if (!eye)
            {
                eye = animator.GetBoneTransform(HumanBodyBones.Head).Find(eyeBoneID.ToString());
            }

            return eye;
        }

        public static Vector3 GetEyeCenterPosition(this Animator animator)
        {
            Transform lEye = animator.GetEye(true);
            Transform rEye = animator.GetEye(false);
            if (lEye && rEye)
            {
                return Vector3.Lerp(lEye.position, rEye.position, 0.5f);
            }

            return animator.transform.position;
        }

        #endregion

    }

    public static class QuickHumanPoseHandler
    {

        #region PROTECTED ATTRIBUTES

        private static Dictionary<Animator, HumanPoseHandler> _poseHandlers = new Dictionary<Animator, HumanPoseHandler>();

        #endregion

        #region GET AND SET

        public static HumanPoseHandler GetHumanPoseHandler(Animator animator)
        {
            if (!_poseHandlers.ContainsKey(animator))
            {
                _poseHandlers[animator] = new HumanPoseHandler(animator.avatar, animator.transform);
            }

            return _poseHandlers[animator];
        }

        public static void GetHumanPose(Animator animator, ref HumanPose result)
        {
            //Save the current transform properties
            Transform tmpParent = animator.transform.parent;
            Vector3 tmpPos = animator.transform.position;
            Quaternion tmpRot = animator.transform.rotation;

            //Set the transform to the world origin
            animator.transform.parent = null;
            animator.transform.position = Vector3.zero;
            animator.transform.rotation = Quaternion.identity;

            //Copy the pose
            GetHumanPoseHandler(animator).GetHumanPose(ref result);

            //Restore the transform properties
            animator.transform.parent = tmpParent;
            animator.transform.position = tmpPos;
            animator.transform.rotation = tmpRot;

            animator.transform.localScale = Vector3.one;
        }

        public static void SetHumanPose(Animator animator, ref HumanPose pose)
        {
            GetHumanPoseHandler(animator).SetHumanPose(ref pose);
        }

        #endregion

    }
}

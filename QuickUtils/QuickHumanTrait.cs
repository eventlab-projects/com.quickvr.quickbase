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

        private static string[] _muscleNames = null;

        private static HumanBodyBones[] _humanBodyBones = null;
        private static QuickHumanFingers[] _fingers = null;
        
        private static Dictionary<QuickHumanFingers, List<QuickHumanBodyBones>> _bonesFromFingerLeft = null;
        private static Dictionary<QuickHumanFingers, List<QuickHumanBodyBones>> _bonesFromFingerRight = null;
        private static Dictionary<QuickHumanBodyBones, QuickHumanFingers> _fingerFromBone = new Dictionary<QuickHumanBodyBones, QuickHumanFingers>();

        #endregion

        #region CREATION AND DESTRUCTION

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
                }
            }

            return result;
        }

        private static void InitMuscleNames()
        {
            _muscleNames = new string[HumanTrait.MuscleCount];

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

                _muscleNames[muscleID] = name;
            }
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

        #endregion

        #region GET AND SET

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

        public static string[] GetMuscleNames()
        {
            if (_muscleNames == null)
            {
                InitMuscleNames();
            }
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

        public static HumanBodyBones[] GetHumanBodyBones()
        {
            if (_humanBodyBones == null)
            {
                List<HumanBodyBones> tmp = QuickUtils.GetEnumValues<HumanBodyBones>();
                tmp.RemoveAt(tmp.Count - 1);  //Remove the LastBone, which is not a valid HumanBodyBone ID
                _humanBodyBones = tmp.ToArray();
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

        public static QuickHumanFingers[] GetHumanFingers()
        {
            if (_fingers == null)
            {
                _fingers = QuickUtils.GetEnumValues<QuickHumanFingers>().ToArray();
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
            int i = (int)boneID;

            return 
                (
                (i >= (int)QuickHumanBodyBones.LeftThumbProximal && i <= (int)QuickHumanBodyBones.LeftLittleDistal) ||
                (i >= (int)QuickHumanBodyBones.LeftThumbTip && i <= (int)QuickHumanBodyBones.LeftLittleTip) 
                );
        }

        public static bool IsBoneFingerLeft(HumanBodyBones boneID)
        {
            return IsBoneFingerLeft((QuickHumanBodyBones)boneID);
        }

        public static bool IsBoneFingerRight(QuickHumanBodyBones boneID)
        {
            int i = (int)boneID;

            return
                (
                (i >= (int)QuickHumanBodyBones.RightThumbProximal && i <= (int)QuickHumanBodyBones.RightLittleDistal) ||
                (i >= (int)QuickHumanBodyBones.RightThumbTip && i <= (int)QuickHumanBodyBones.RightLittleTip)
                );
        }

        public static bool IsBoneFingerRight(HumanBodyBones boneID)
        {
            return IsBoneFingerRight((QuickHumanBodyBones)boneID);
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
                if (tBoneDistal.childCount == 0)
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

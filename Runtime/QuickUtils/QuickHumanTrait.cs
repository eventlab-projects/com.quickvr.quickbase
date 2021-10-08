using System.Collections;
using System.Collections.Generic;
using System.Reflection;

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

        LastBone,
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
        private static Dictionary<QuickHumanBodyBones, QuickHumanBodyBones> _nextBoneInChain = null;

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

        private static void InitNextBones()
        {
            _nextBoneInChain = new Dictionary<QuickHumanBodyBones, QuickHumanBodyBones>();

            //Spine chain
            _nextBoneInChain[QuickHumanBodyBones.Hips] = QuickHumanBodyBones.Spine;
            _nextBoneInChain[QuickHumanBodyBones.Spine] = QuickHumanBodyBones.Head;

            //Left arm chain
            _nextBoneInChain[QuickHumanBodyBones.LeftUpperArm] = QuickHumanBodyBones.LeftLowerArm;
            _nextBoneInChain[QuickHumanBodyBones.LeftLowerArm] = QuickHumanBodyBones.LeftHand;

            //Right arm chain
            _nextBoneInChain[QuickHumanBodyBones.RightUpperArm] = QuickHumanBodyBones.RightLowerArm;
            _nextBoneInChain[QuickHumanBodyBones.RightLowerArm] = QuickHumanBodyBones.RightHand;

            //Left leg chain
            _nextBoneInChain[QuickHumanBodyBones.LeftUpperLeg] = QuickHumanBodyBones.LeftLowerLeg;
            _nextBoneInChain[QuickHumanBodyBones.LeftLowerLeg] = QuickHumanBodyBones.LeftFoot;

            //Right leg chain
            _nextBoneInChain[QuickHumanBodyBones.RightUpperLeg] = QuickHumanBodyBones.RightLowerLeg;
            _nextBoneInChain[QuickHumanBodyBones.RightLowerLeg] = QuickHumanBodyBones.RightFoot;

            //Hands chains
            foreach (bool b in new bool[]{ true, false}) 
            {
                foreach (QuickHumanFingers f in GetHumanFingers())
                {
                    List<QuickHumanBodyBones> fingerBones = GetBonesFromFinger(f, b);
                    for (int i = 0; i < fingerBones.Count - 1; i++)
                    {
                        _nextBoneInChain[fingerBones[i]] = fingerBones[i + 1];
                    }
                }
            }
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
            return muscleID >= 0? GetMuscleNames()[muscleID] : "Undefined";
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

        public static QuickHumanBodyBones GetNextBoneInChain(QuickHumanBodyBones boneID)
        {
            if (_nextBoneInChain == null)
            {
                InitNextBones();
            }

            bool b = _nextBoneInChain.TryGetValue(boneID, out QuickHumanBodyBones result);

            return b? result : QuickHumanBodyBones.LastBone;
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

        public static HumanBodyBones ToHumanBodyBones(this Animator animator, Transform tBone)
        {
            HumanBodyBones result = HumanBodyBones.LastBone;
            for (HumanBodyBones boneID = 0; result == HumanBodyBones.LastBone && boneID < HumanBodyBones.LastBone; boneID++)
            {
                if (tBone == animator.GetBoneTransform(boneID))
                {
                    result = boneID;
                }
            }

            return result;
        }

        public static Transform GetBoneTransform(this Animator animator, QuickHumanBodyBones boneID)
        {
            Transform result = null;

            if (boneID == QuickHumanBodyBones.LeftEye)
            {
                result = animator.GetEye(true);
            }
            else if (boneID == QuickHumanBodyBones.RightEye)
            {
                result = animator.GetEye(false);
            }
            else
            {
                if ((int)boneID < (int)HumanBodyBones.LastBone)
                {
                    result = animator.GetBoneTransform((HumanBodyBones)boneID);
                }
                else
                {
                    //At this point, boneID is a bone finger tip
                    Transform tBoneDistal = animator.GetBoneTransform(GetParentBone(boneID));
                    if (tBoneDistal)
                    {
                        result = tBoneDistal.GetChild(0);
                    }
                }
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

        public static void Compress(this AnimationCurve aCurve, float epsilon)
        {
            Keyframe[] keyFrames = aCurve.keys;
            if (keyFrames.Length > 2)
            {
                aCurve.keys = new Keyframe[] { };

                aCurve.AddKey(keyFrames[0]);
                int iPrev = 0;

                for (int i = 1; i < keyFrames.Length - 1; i++)
                {
                    Keyframe k = keyFrames[i];
                    Keyframe kPrev = keyFrames[iPrev];
                    Keyframe kNext = keyFrames[i + 1];

                    float t = (k.time - kPrev.time) / (kNext.time - kPrev.time);
                    float v = Mathf.Lerp(kPrev.value, kNext.value, t);

                    if (Mathf.Abs(k.value - v) > epsilon)
                    {
                        aCurve.AddKey(k);
                        iPrev = i;
                    }
                }

                aCurve.AddKey(keyFrames[keyFrames.Length - 1]);
            }
        }

        public static void CreateMissingBones(this Animator animator)
        {
            animator.EnforceTPose();

            animator.CreateEyes();
            animator.CreateFingerTips();

            animator.CreateRotationReferences();
        }

        private static void CreateEyes(this Animator animator)
        {
            animator.CreateEye(true);
            animator.CreateEye(false);

            animator.CreateEyeCenter();
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
                if (tBoneDistal && tBoneDistal.childCount == 0)
                {
                    Transform tBoneIntermediate = animator.GetBoneTransform(GetParentBone(boneID));
                    Vector3 v = tBoneDistal.position - tBoneIntermediate.position;

                    Transform tBoneTip = tBoneDistal.CreateChild("__FingerTip__");
                    tBoneTip.position = tBoneDistal.position + v;
                }
            }
        }

        private static void CreateRotationReferences(this Animator animator)
        {
            animator.CreateRotationReference(QuickHumanBodyBones.Head);
            animator.CreateRotationReference(QuickHumanBodyBones.LeftHand);
            animator.CreateRotationReference(QuickHumanBodyBones.RightHand);
            animator.CreateRotationReference(QuickHumanBodyBones.LeftFoot);
            animator.CreateRotationReference(QuickHumanBodyBones.RightFoot);
        }

        public static Transform GetRotationReference(this Animator animator, QuickHumanBodyBones boneID)
        {
            return animator.GetBoneTransform(boneID).Find("__RotationReference__");
        }

        private static void CreateRotationReference(this Animator animator, QuickHumanBodyBones boneID)
        {
            Transform t = animator.GetBoneTransform(boneID).CreateChild("__RotationReference__");
            if (boneID == QuickHumanBodyBones.Head || boneID == QuickHumanBodyBones.LeftFoot || boneID == QuickHumanBodyBones.RightFoot)
            {
                t.LookAt(t.position + animator.transform.forward, animator.transform.up);
            }
            else if (boneID == QuickHumanBodyBones.LeftHand)
            {
                t.LookAt(t.position - animator.transform.right, animator.transform.up);
            }
            else if (boneID == QuickHumanBodyBones.RightHand)
            {
                t.LookAt(t.position + animator.transform.right, animator.transform.up);
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

        private static Transform CreateEyeCenter(this Animator animator)
        {
            Transform tHead = animator.GetBoneTransform(HumanBodyBones.Head);
            Transform tResult = tHead.CreateChild("__EyeCenter__");
            
            tResult.rotation = animator.transform.rotation;
            tResult.position = Vector3.Lerp(animator.GetEye(true).position, animator.GetEye(false).position, 0.5f);

            return tResult;
        }

        public static Transform GetEyeCenter(this Animator animator)
        {
            return animator.GetBoneTransform(HumanBodyBones.Head).Find("__EyeCenter__");
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

        public static void EnforceTPose(this Animator animator)
        {
            HumanPoseHandler poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);

            HumanPose pose = new HumanPose();
            poseHandler.GetHumanPose(ref pose);

            pose.bodyPosition = new Vector3(0.0023505474f, 0.97386265f, -0.015300978f);
            pose.bodyRotation = Quaternion.identity;

            EnforceTPoseSpine(ref pose);
            EnforceTPoseLegs(ref pose);
            EnforceTPoseArms(ref pose);
            EnforceTPoseFingers(ref pose);
            EnforceTPoseFace(ref pose);

            poseHandler.SetHumanPose(ref pose);

            //RuntimeAnimatorController tmp = animator.runtimeAnimatorController;
            //animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Animations/TPose");
            //animator.Update(0);
            //animator.runtimeAnimatorController = tmp;

            //Enforce the TPose of the Spine
            //animator.EnforceTPose(QuickHumanBodyBones.Hips, QuickHumanBodyBones.Head, animator.transform.up);

            ////Enforce the TPose of the LeftArm
            //animator.EnforceTPose(QuickHumanBodyBones.LeftUpperArm, QuickHumanBodyBones.LeftLowerArm, -animator.transform.right);
            //animator.EnforceTPose(QuickHumanBodyBones.LeftLowerArm, QuickHumanBodyBones.LeftHand, -animator.transform.right);

            ////Enforce the TPose of the RightArm
            //animator.EnforceTPose(QuickHumanBodyBones.RightUpperArm, QuickHumanBodyBones.RightLowerArm, animator.transform.right);
            //animator.EnforceTPose(QuickHumanBodyBones.RightLowerArm, QuickHumanBodyBones.RightHand, animator.transform.right);

            ////Enforce the TPose of the Hands
            //animator.EnforceTPoseHand(true);
            //animator.EnforceTPoseHand(false);

            ////Enforce the TPose of the Fingers
            //animator.EnforceTPoseFingers(true);
            //animator.EnforceTPoseFingers(false);

            ////Enforce the TPose of the LeftLeg
            //animator.EnforceTPose(QuickHumanBodyBones.LeftUpperLeg, QuickHumanBodyBones.LeftLowerLeg, -animator.transform.up);
            //animator.EnforceTPose(QuickHumanBodyBones.LeftLowerLeg, QuickHumanBodyBones.LeftFoot, -animator.transform.up);

            ////Enforce the TPose of the RightLeg
            //animator.EnforceTPose(QuickHumanBodyBones.RightUpperLeg, QuickHumanBodyBones.RightLowerLeg, -animator.transform.up);
            //animator.EnforceTPose(QuickHumanBodyBones.RightLowerLeg, QuickHumanBodyBones.RightFoot, -animator.transform.up);
        }

        private static void EnforceTPoseSpine(ref HumanPose pose)
        {
            for (HumanBodyBones boneID = HumanBodyBones.Spine; boneID <= HumanBodyBones.Head; boneID++)
            {
                for (int i = 0; i < 3; i++)
                {
                    int muscleID = GetMuscleFromBone(boneID, i);
                    if (muscleID >= 0)
                    {
                        pose.muscles[muscleID] = 0;
                    }
                }
            }
        }

        private static void EnforceTPoseLegs(ref HumanPose pose)
        {
            //Upper Leg
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg })
            {
                pose.muscles[GetMuscleFromBone(boneID, 0)] = 0;
                pose.muscles[GetMuscleFromBone(boneID, 1)] = 0;
                pose.muscles[GetMuscleFromBone(boneID, 2)] = 0.56844866f;
            }

            //Lower Leg
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftLowerLeg, HumanBodyBones.RightLowerLeg })
            {
                pose.muscles[GetMuscleFromBone(boneID, 0)] = 0;
                pose.muscles[GetMuscleFromBone(boneID, 2)] = 0.95906335f;
            }

            //Foot
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot })
            {
                pose.muscles[GetMuscleFromBone(boneID, 1)] = 0;
                pose.muscles[GetMuscleFromBone(boneID, 2)] = -0.03370474f;
            }

            //Toes
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot })
            {
                pose.muscles[GetMuscleFromBone(boneID, 2)] = 0;
            }
        }

        private static void EnforceTPoseArms(ref HumanPose pose)
        {
            //Shoulder
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftShoulder, HumanBodyBones.RightShoulder})
            {
                pose.muscles[GetMuscleFromBone(boneID, 1)] = 0;
                pose.muscles[GetMuscleFromBone(boneID, 2)] = 0;
            }

            //Upper Arm
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftUpperArm, HumanBodyBones.RightUpperArm })
            {
                pose.muscles[GetMuscleFromBone(boneID, 0)] = 0;
                pose.muscles[GetMuscleFromBone(boneID, 1)] = 0.3f;
                pose.muscles[GetMuscleFromBone(boneID, 2)] = 0.4f;
            }

            //Lower Arm
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftLowerArm, HumanBodyBones.RightLowerArm })
            {
                pose.muscles[GetMuscleFromBone(boneID, 0)] = -0.06501173f;
                pose.muscles[GetMuscleFromBone(boneID, 2)] = 1;
            }

            //Hand
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftHand, HumanBodyBones.RightHand })
            {
                pose.muscles[GetMuscleFromBone(boneID, 1)] = 0.058751374f;
                pose.muscles[GetMuscleFromBone(boneID, 2)] = -0.042013716f;
            }
        }

        private static void EnforceTPoseFingers(ref HumanPose pose)
        {
            //Thumb Proximal
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftThumbProximal, HumanBodyBones.RightThumbProximal })
            {
                pose.muscles[GetMuscleFromBone(boneID, 1)] = -0.10919621f;
                pose.muscles[GetMuscleFromBone(boneID, 2)] = -1.2577168f;
            }

            //Thumb Intermediate
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.RightThumbIntermediate })
            {
                pose.muscles[GetMuscleFromBone(boneID, 2)] = 0.7628888f;
            }

            //Thumb Distal
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftThumbDistal, HumanBodyBones.RightThumbDistal })
            {
                pose.muscles[GetMuscleFromBone(boneID, 2)] = 0.7313669f;
            }

            //Index to Little Stretched
            foreach (HumanBodyBones boneID in new HumanBodyBones[]
            {
                HumanBodyBones.LeftIndexProximal,
                HumanBodyBones.LeftMiddleProximal,
                HumanBodyBones.LeftRingProximal, 
                HumanBodyBones.LeftLittleProximal,

                HumanBodyBones.RightIndexProximal, 
                HumanBodyBones.RightMiddleProximal,
                HumanBodyBones.RightRingProximal, 
                HumanBodyBones.RightLittleProximal
            })
            {
                pose.muscles[GetMuscleFromBone(boneID, 2)] = 0.735f;
            }

            //Index to Little Intermediate and Distal phalanges
            foreach (HumanBodyBones boneID in new HumanBodyBones[] 
            { 
                HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexDistal,
                HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleDistal,
                HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingDistal,
                HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleDistal,

                HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexDistal,
                HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleDistal,
                HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingDistal,
                HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleDistal
            })
            {
                pose.muscles[GetMuscleFromBone(boneID, 2)] = 0.8116841f;
            }

            //Index Spread
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftIndexProximal, HumanBodyBones.RightIndexProximal })
            {
                pose.muscles[GetMuscleFromBone(boneID, 1)] = -0.32665434f;
            }

            //Middle Spread
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftMiddleProximal, HumanBodyBones.RightMiddleProximal })
            {
                pose.muscles[GetMuscleFromBone(boneID, 1)] = -0.26266485f;
            }

            //Ring Spread
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftRingProximal, HumanBodyBones.RightRingProximal })
            {
                pose.muscles[GetMuscleFromBone(boneID, 1)] = -0.9577875f;
            }

            //Little Spread
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftLittleProximal, HumanBodyBones.RightLittleProximal })
            {
                pose.muscles[GetMuscleFromBone(boneID, 1)] = -0.5872235f;
            }
        }

        private static void EnforceTPoseFace(ref HumanPose pose)
        {
            //Jaw
            pose.muscles[GetMuscleFromBone(HumanBodyBones.Jaw, 1)] = 0;
            pose.muscles[GetMuscleFromBone(HumanBodyBones.Jaw, 2)] = 1;

            //Eyes
            foreach (HumanBodyBones boneID in new HumanBodyBones[] { HumanBodyBones.LeftEye, HumanBodyBones.RightEye })
            {
                for (int i = 0; i < 3; i++)
                {
                    int muscleID = GetMuscleFromBone(boneID, i);
                    if (muscleID >= 0)
                    {
                        pose.muscles[muscleID] = 0;
                    }
                }
            }
        }

        //private static void EnforceTPose(this Animator animator, QuickHumanBodyBones boneTargetID, QuickHumanBodyBones boneUpperID, QuickHumanBodyBones boneLowerID, Vector3 vTarget)
        //{
        //    Transform tTarget = animator.GetBoneTransform(boneTargetID);
        //    Transform tUpper = animator.GetBoneTransform(boneUpperID);
        //    Transform tLower = animator.GetBoneTransform(boneLowerID);

        //    if (tTarget && tUpper && tLower)
        //    {
        //        Vector3 vUpperArm = tLower.position - tUpper.position;

        //        tTarget.Rotate(Vector3.Cross(vUpperArm, vTarget).normalized, Vector3.Angle(vUpperArm, vTarget), Space.World);
        //    }
        //}

        //private static void EnforceTPose(this Animator animator, QuickHumanBodyBones boneUpperID, QuickHumanBodyBones boneLowerID, Vector3 vTarget)
        //{
        //    animator.EnforceTPose(boneUpperID, boneUpperID, boneLowerID, vTarget);
        //}

        //private static void EnforceTPoseHand(this Animator animator, bool isLeft)
        //{
        //    Transform tHand = animator.GetBoneTransform(isLeft? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
        //    Transform tIndex = animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftIndexProximal : HumanBodyBones.RightIndexProximal);
        //    Transform tMiddle = animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.RightMiddleProximal);
        //    Transform tLittle = animator.GetBoneTransform(isLeft ? HumanBodyBones.LeftLittleProximal : HumanBodyBones.RightLittleProximal);

        //    Transform tPose = tHand.CreateChild("__TPose__");
        //    Vector3 v = tIndex.position - tHand.position;
        //    Vector3 w = tLittle.position - tHand.position;
        //    Vector3 n = Vector3.Cross(v, w);
            
        //    if (isLeft)
        //    {
        //        n *= -1;
        //    }
            
        //    tPose.LookAt(tMiddle.position, n);

        //    //Align the forward vector
        //    float sign = isLeft ? -1 : 1;
        //    tHand.Rotate(Vector3.Cross(tPose.forward, sign * animator.transform.right), Vector3.Angle(tPose.forward, sign * animator.transform.right), Space.World);

        //    //Align the up vector
        //    tHand.Rotate(Vector3.Cross(tPose.up, animator.transform.up), Vector3.Angle(tPose.up, animator.transform.up), Space.World);
        //}

        //private static void EnforceTPoseFingers(this Animator animator, bool isLeft)
        //{
        //    foreach (QuickHumanFingers f in GetHumanFingers())
        //    {
        //        List<QuickHumanBodyBones> bones = GetBonesFromFinger(f, isLeft);
        //        for (int i = 0; i < bones.Count - 1; i++)
        //        {
        //            Transform tBoneStart = animator.GetBoneTransform(bones[i]);
        //            Transform tBoneEnd = animator.GetBoneTransform(bones[i + 1]);
        //            EnforceTPose(animator, bones[i], bones[i + 1], Vector3.ProjectOnPlane(tBoneEnd.position - tBoneStart.position, animator.transform.up));
        //        }
        //    }
        //}

        //https://forum.unity.com/threads/recording-humanoid-animations-with-foot-ik.545015/
        public static void GetIKGoalFromBodyPose(this Animator animator, AvatarIKGoal avatarIKGoal, Vector3 bodyPosition, Quaternion bodyRotation, out Vector3 goalPos, out Quaternion goalRot, float humanScale = 1)
        {
            HumanBodyBones boneID = ToHumanBodyBones(avatarIKGoal);
            if (boneID == HumanBodyBones.LastBone)
            {
                throw new System.InvalidOperationException("Invalid bone id.");
            }

            MethodInfo methodGetAxisLength = typeof(Avatar).GetMethod("GetAxisLength", BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodGetAxisLength == null)
            {
                throw new System.InvalidOperationException("Cannot find GetAxisLength method.");
            }

            MethodInfo methodGetPostRotation = typeof(Avatar).GetMethod("GetPostRotation", BindingFlags.Instance | BindingFlags.NonPublic);
            if (methodGetPostRotation == null)
            {
                throw new System.InvalidOperationException("Cannot find GetPostRotation method.");
            }

            Quaternion postRotation = (Quaternion)methodGetPostRotation.Invoke(animator.avatar, new object[] { (int)boneID });

            Transform tBone = animator.GetBoneTransform(boneID);
            goalPos = tBone.position;
            goalRot = tBone.rotation * postRotation;

            if (avatarIKGoal == AvatarIKGoal.LeftFoot || avatarIKGoal == AvatarIKGoal.RightFoot)
            {
                // Here you could use animator.leftFeetBottomHeight or animator.rightFeetBottomHeight rather than GetAxisLenght
                // Both are equivalent but GetAxisLength is the generic way and work for all human bone
                float axislength = (float)methodGetAxisLength.Invoke(animator.avatar, new object[] { (int)boneID });
                Vector3 footBottom = new Vector3(axislength, 0, 0);
                goalPos += (goalRot * footBottom);
            }

            // IK goal are in avatar body local space
            Quaternion invRootQ = Quaternion.Inverse(bodyRotation);
            goalPos = invRootQ * (goalPos - bodyPosition);
            goalRot = invRootQ * goalRot;
            goalPos /= humanScale;
        }

        private static HumanBodyBones ToHumanBodyBones(AvatarIKGoal avatarIKGoal)
        {
            HumanBodyBones boneID = HumanBodyBones.LastBone;
            switch (avatarIKGoal)
            {
                case AvatarIKGoal.LeftFoot: boneID = HumanBodyBones.LeftFoot; break;
                case AvatarIKGoal.RightFoot: boneID = HumanBodyBones.RightFoot; break;
                case AvatarIKGoal.LeftHand: boneID = HumanBodyBones.LeftHand; break;
                case AvatarIKGoal.RightHand: boneID = HumanBodyBones.RightHand; break;
            }

            return boneID;
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
            Vector3 tmpScale = animator.transform.localScale;

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
            animator.transform.localScale = tmpScale;
        }

        public static void SetHumanPose(Animator animator, ref HumanPose pose)
        {
            GetHumanPoseHandler(animator).SetHumanPose(ref pose);
        }

        #endregion

    }
}

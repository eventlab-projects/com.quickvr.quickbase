using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace QuickVR {

    public class QuickCopyPoseBase : MonoBehaviour {

        #region PUBLIC PARAMETERS

        public enum TrackedJointBody
        {
            Hips, 
            
            Spine,
            Chest,
            UpperChest,
            Neck,
            Head, 
            
            LeftUpperArm, 
            LeftLowerArm,
            LeftHand,

            RightUpperArm,
            RightLowerArm,
            RightHand,

            LeftUpperLeg,
            LeftLowerLeg,
            LeftFoot,

            RightUpperLeg,
            RightLowerLeg,
            RightFoot,
        }

        [BitMask(typeof(TrackedJointBody))]
        public int _trackedJointsBody = -1;

        [BitMask(typeof(QuickHumanFingers))]
        public int _trackedJointsHandLeft = -1;

        [BitMask(typeof(QuickHumanFingers))]
        public int _trackedJointsHandRight = -1;

        #endregion

        #region PROTECTED PARAMETERS

        protected static List<QuickHumanBodyBones> _allJoints = null;
        protected static Dictionary<QuickHumanBodyBones, TrackedJointBody> _toTrackedJointBody = new Dictionary<QuickHumanBodyBones, TrackedJointBody>();
        
        protected Animator _source = null;
        protected Animator _dest = null;

        protected Transform _sourceOrigin = null;
        protected Transform _destOrigin = null;

        protected HumanPose _poseSource = new HumanPose();
        protected HumanPose _poseDest = new HumanPose();
        protected HumanPose _initialPoseDest = new HumanPose();

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            _allJoints = QuickUtils.ParseEnum<QuickHumanBodyBones, TrackedJointBody>();
            foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
            {
                _allJoints.AddRange(QuickHumanTrait.GetBonesFromFinger(f, true));
                _allJoints.AddRange(QuickHumanTrait.GetBonesFromFinger(f, false));
            }

            foreach (TrackedJointBody j in QuickUtils.GetEnumValues<TrackedJointBody>())
            {
                _toTrackedJointBody[QuickUtils.ParseEnum<QuickHumanBodyBones>(j.ToString())] = j;
            }
        }

        protected virtual void Awake()
        {
            _destOrigin = transform.CreateChild("__DestOrigin__");
            _sourceOrigin = transform.CreateChild("__SourceOrigin__");
        }

        #endregion

        #region GET AND SET

        public virtual Animator GetAnimatorSource()
        {
            return _source;
        }

        public virtual Animator GetAnimatorDest()
        {
            return _dest;
        }

        public virtual void SetAnimatorSource(Animator animator)
        {
            _source = animator;
        }

        public virtual void SetAnimatorDest(Animator animator)
        {
            //Restore the initial HumanPose that _dest had at the begining
            if (_dest)
            {
                QuickHumanPoseHandler.SetHumanPose(_dest, ref _initialPoseDest);
            }

            _dest = animator;

            //Save the current HumanPose of the new _dest
            if (_dest)
            {
                QuickHumanPoseHandler.GetHumanPose(_dest, ref _initialPoseDest);
            }
        }

        protected virtual bool IsTrackedJointBody(TrackedJointBody joint)
        {
            return IsTrackedJoint(_trackedJointsBody, (int)joint);
        }

        protected virtual bool IsTrackedJointHandLeft(QuickHumanFingers joint)
        {
            return IsTrackedJoint(_trackedJointsHandLeft, (int)joint);
        }

        protected virtual bool IsTrackedJointHandRight(QuickHumanFingers joint)
        {
            return IsTrackedJoint(_trackedJointsHandRight, (int)joint);
        }

        protected virtual bool IsTrackedJoint(int mask, int jointID)
        {
            return ((_trackedJointsBody & (1 << jointID)) != 0);
        }

        protected virtual bool IsTrackedJoint(QuickHumanBodyBones boneID)
        {
            if (QuickHumanTrait.IsBoneFingerLeft(boneID)) return IsTrackedJointHandLeft(QuickHumanTrait.GetFingerFromBone(boneID));
            else if (QuickHumanTrait.IsBoneFingerRight(boneID)) return IsTrackedJointHandRight(QuickHumanTrait.GetFingerFromBone(boneID));
            return IsTrackedJointBody(_toTrackedJointBody[boneID]);
        }

        #endregion

        #region UPDATE

        public virtual void CopyPose()
        {
            if (_source && _dest && _source != _dest)
            {
                CopyPoseImp();
            }
        }

        protected virtual void CopyPoseImp()
        {
            //For some obscure reason, we have to set source and dest to null parent in order to work. 
            QuickHumanPoseHandler.GetHumanPose(_source, ref _poseSource);
            QuickHumanPoseHandler.GetHumanPose(_dest, ref _poseDest);

            foreach (QuickHumanBodyBones boneID in _allJoints)
            {
                if (!IsTrackedJoint(boneID))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        int m = QuickHumanTrait.GetMuscleFromBone(boneID, i);
                        if (m != -1)
                        {
                            _poseSource.muscles[m] = _poseDest.muscles[m];
                        }
                    }
                }
            }

            //The hips is a special case, it modifies the bodyPosition and bodyRotation fields
            if (!IsTrackedJointBody(TrackedJointBody.Hips))
            {
                _poseSource.bodyPosition = _poseDest.bodyPosition;
                _poseSource.bodyRotation = _poseDest.bodyRotation;
            }

            QuickHumanPoseHandler.SetHumanPose(_dest, ref _poseSource);
        }

        #endregion

    }

}

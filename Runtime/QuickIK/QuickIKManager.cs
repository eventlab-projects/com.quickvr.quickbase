using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.XR;

namespace QuickVR {

	[System.Serializable]
    
    //public enum IKLimbBones
    //{
    //    Hips,
    //    Head,
    //    LeftHand,
    //    RightHand,
    //    LeftFoot,
    //    RightFoot,
    //};

    public enum IKBone
    {
        //--> Main body IK bones
        Hips,
        Head,
        LeftHand,
        RightHand,
        LeftFoot,
        RightFoot,
        //<--

        //--> Left hand IK bones
        LeftThumbDistal,
        LeftIndexDistal,
        LeftRingDistal,
        LeftMiddleDistal,
        LeftLittleDistal,
        //<--

        //--> Right hand IK bones
        RightThumbDistal,
        RightIndexDistal,
        RightRingDistal,
        RightMiddleDistal,
        RightLittleDistal,
        //<--

        //--> Face IK bones
        LeftEye,
        RightEye,
        //<--

        LastBone,

        //<-- QuickAccess Bones
        StartBody = Hips,
        EndBody = RightFoot,

        StartLeftHandFingers = LeftThumbDistal,
        EndLeftHandFingers = LeftLittleDistal,

        StartRightHandFingers = RightThumbDistal,
        EndRightHandFingers = RightLittleDistal,

        StartFace = LeftEye,
        EndFace = RightEye,
    };

    [ExecuteInEditMode]
    public class QuickIKManager : QuickBaseTrackingManager 
    {

        #region PUBLIC PARAMETERS

#if UNITY_EDITOR

        [SerializeField, HideInInspector]
        public bool _showControlsBody = false;

        [SerializeField, HideInInspector]
        public bool _showControlsFingersLeftHand = false;

        [SerializeField, HideInInspector]
        public bool _showControlsFingersRightHand = false;

#endif

        #endregion

        #region PROTECTED PARAMETERS

        protected HumanBodyBones[] _allIKBones =
        {
           HumanBodyBones.Hips,
           HumanBodyBones.Head,
           HumanBodyBones.LeftHand,
           HumanBodyBones.RightHand,
           HumanBodyBones.LeftFoot,
           HumanBodyBones.RightFoot,

           HumanBodyBones.LeftThumbDistal,
           HumanBodyBones.LeftIndexDistal,
           HumanBodyBones.LeftRingDistal,
           HumanBodyBones.LeftMiddleDistal,
           HumanBodyBones.LeftLittleDistal,

           HumanBodyBones.RightThumbDistal,
           HumanBodyBones.RightIndexDistal,
           HumanBodyBones.RightRingDistal,
           HumanBodyBones.RightMiddleDistal,
           HumanBodyBones.RightLittleDistal,

           HumanBodyBones.LeftEye,
           HumanBodyBones.RightEye,
        };

        protected HashSet<HumanBodyBones> _allIKBonesSet = null;

        protected Transform _ikTargetsRoot
        {
            get
            {
                if (!m_ikTargetsRoot) m_ikTargetsRoot = transform.CreateChild("__IKTargets__");
                return m_ikTargetsRoot;
            }
        }
        protected Transform m_ikTargetsRoot;

        protected Transform _ikTargetsLeftHand
        {
            get
            {
                if (!m_ikTargetsLeftHand) m_ikTargetsLeftHand = transform.CreateChild("__IKTargetsLeftHand__");
                return m_ikTargetsLeftHand;
            }
        }
        protected Transform m_ikTargetsLeftHand;

        protected Transform _ikTargetsRightHand
        {
            get
            {
                if (!m_ikTargetsRightHand) m_ikTargetsRightHand = transform.CreateChild("__IKTargetsRightHand__");
                return m_ikTargetsRightHand;
            }
        }
        protected Transform m_ikTargetsRightHand;

        protected Transform _ikTargetsFace
        {
            get
            {
                if (!m_ikTargetsFace) m_ikTargetsFace = transform.CreateChild("__IKTargetsFace__");
                return m_ikTargetsFace;
            }
        }
        protected Transform m_ikTargetsFace;

        protected Dictionary<HumanBodyBones, QuickIKSolver> _ikSolvers = new Dictionary<HumanBodyBones, QuickIKSolver>();
        protected static List<HumanBodyBones> _toHumanBodyBones = new List<HumanBodyBones>();
        protected static Dictionary<HumanBodyBones, IKBone> _toIKBone = new Dictionary<HumanBodyBones, IKBone>();

        protected List<QuickIKSolver> _ikSolversBody = null;
        protected List<QuickIKSolver> _ikSolversLeftHand = null;
        protected List<QuickIKSolver> _ikSolversRightHand = null;

        protected static List<HumanBodyBones> _ikLimbBones = null;

        [SerializeField, HideInInspector]
        protected Vector3 _initialHipsLocalPosition = Vector3.zero;

        [SerializeField, HideInInspector]
        protected bool _isInitialized = false;

        #endregion

        #region CONSTANTS

        protected static string IK_SOLVERS_ROOT_NAME = "__IKSolvers__";
        public static string IK_SOLVER_PREFIX = "_IKSolver_";
        public static string IK_TARGET_PREFIX = "_IKTarget_";
        public static float DEFAULT_TARGET_HINT_DISTANCE = 0.25f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            Reset();

            SavePose();
        }

        protected virtual void Reset()
        {
            CreateIKSolvers();

            if (!_isInitialized)
            {
                LoadTPose();
                SavePose();

                _initialHipsLocalPosition = _animator.GetBoneTransform(HumanBodyBones.Hips).localPosition;

                _isInitialized = true;
            }
        }

        protected virtual void CreateIKSolvers()
        {
            foreach (HumanBodyBones boneID in _allIKBones)
            {
                if (!_ikSolvers.ContainsKey(boneID))
                {
                    _ikSolvers[boneID] = CreateIKSolver(boneID);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            //ResetTPose();

            QuickUtils.Destroy(transform.Find(IK_SOLVERS_ROOT_NAME));
            
            QuickUtils.Destroy(m_ikTargetsRoot);
            QuickUtils.Destroy(m_ikTargetsLeftHand);
            QuickUtils.Destroy(m_ikTargetsRightHand);
            QuickUtils.Destroy(m_ikTargetsFace);
        }

        protected virtual QuickIKSolver CreateIKSolver(HumanBodyBones boneID)
        {
            QuickIKSolver result = null;
            if (_animator.GetBoneTransform(boneID) == null)
            {
                Debug.LogWarning("[QuickIKManager]: Could not create IKSolver for " + boneID + " because this bone in the Animator is null!!");
            }
            else
            {
                if (boneID == HumanBodyBones.Hips) result = CreateIKSolver<QuickIKSolverHips>(boneID);
                else if (boneID == HumanBodyBones.LeftHand || boneID == HumanBodyBones.RightHand)
                {
                    result = CreateIKSolver<QuickIKSolverHand>(boneID);
                }
                else if (boneID == HumanBodyBones.LeftEye || boneID == HumanBodyBones.RightEye)
                {
                    result = CreateIKSolver<QuickIKSolverEye>(boneID);
                }
                else
                {
                    result = CreateIKSolver<QuickIKSolver>(boneID);
                }
            }
            
            return result;
        }

        protected virtual T CreateIKSolver<T>(HumanBodyBones boneLimb) where T : QuickIKSolver
        {
            Transform ikSolversRoot = transform.CreateChild(IK_SOLVERS_ROOT_NAME);
            T ikSolver = ikSolversRoot.CreateChild(IK_SOLVER_PREFIX + boneLimb.ToString()).GetOrCreateComponent<T>();

            //And configure it according to the bone
            ikSolver._boneUpper = GetBoneUpper(boneLimb);
            ikSolver._boneMid = GetBoneMid(boneLimb);
            ikSolver._boneLimb = _animator.GetBoneTransform(boneLimb);

            ikSolver._weightIKPos = 1.0f;
            ikSolver._weightIKRot = 1.0f;

            return ikSolver;
        }

        #endregion

        #region GET AND SET

        public virtual void SavePose()
        {
            foreach (HumanBodyBones boneID in _allIKBones)
            {
                QuickIKSolver ikSolver = GetIKSolver(boneID);
                if (ikSolver)
                {
                    ikSolver.SavePose();
                }
            }
        }

        public virtual void LoadPose()
        {
            foreach (HumanBodyBones boneID in _allIKBones)
            {
                QuickIKSolver ikSolver = GetIKSolver(boneID);
                if (ikSolver)
                {
                    ikSolver.LoadPose();
                }
            }
        }

        public virtual void ResetIKTarget(HumanBodyBones boneID)
        {
            QuickIKSolver ikSolver = GetIKSolver(boneID);
            ikSolver.ResetIKChain();
            ResetIKTarget(boneID, ikSolver._targetLimb);
            if (ikSolver._targetHint)
            {
                ResetIKTarget(QuickHumanTrait.GetParentBone(boneID), ikSolver._targetHint);
            }
            if (ikSolver._targetLimb.childCount > 0)
            {
                ikSolver._targetLimb.GetChild(0).rotation = _animator.GetBoneTransform(ikSolver._boneID).rotation;
            }
        }

        protected virtual void ResetIKTarget(HumanBodyBones boneID, Transform ikTarget)
        {
            if ((int)boneID != -1)
            {
                ikTarget.name = IK_TARGET_PREFIX + boneID;

                Transform bone = _animator.GetBoneTransform(boneID);
                string boneName = boneID.ToString();

                //Set the position of the IKTarget
                ikTarget.position = bone.position;

                //Set the rotation of the IKTarget
                ikTarget.rotation = transform.rotation;
                if (boneID == HumanBodyBones.LeftHand)
                {
                    ikTarget.LookAt(ikTarget.position - transform.right, transform.up);
                }
                else if (boneID == HumanBodyBones.RightHand)
                {
                    ikTarget.LookAt(ikTarget.position + transform.right, transform.up);
                }
                else if (boneName.Contains("Spine"))
                {
                    ikTarget.position -= transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
                }
                else if (boneID == HumanBodyBones.LeftLowerArm || boneID == HumanBodyBones.RightLowerArm)
                {
                    ikTarget.position += Vector3.Lerp(-transform.forward, -transform.up, 0.35f).normalized * DEFAULT_TARGET_HINT_DISTANCE;
                }
                else if (boneID == HumanBodyBones.LeftLowerLeg || boneID == HumanBodyBones.RightLowerLeg)
                {
                    ikTarget.position += transform.forward * DEFAULT_TARGET_HINT_DISTANCE;
                }
                else if (boneName.Contains("Distal"))
                {
                    ikTarget.up = transform.up;
                    ikTarget.forward = _animator.GetBoneTransform(boneID).position - _animator.GetBoneTransform(boneID - 1).position;
                }
                else if (boneName.Contains("Intermediate"))
                {
                    if (boneName.Contains("Thumb"))
                    {
                        Vector3 v = Vector3.Cross(_animator.GetBoneTransform(boneID - 1).position - _animator.GetBoneTransform(boneID).position, transform.up).normalized;
                        float sign = boneName.Contains("Left") ? 1 : -1;
                        ikTarget.position += sign * v * 0.1f;
                    }
                    else
                    {
                        ikTarget.position += transform.up * 0.1f;
                    }
                }
            }
        }

        public virtual void LoadTPose()
        {
            ResetIKTargets(false);
        }

        public virtual void LoadAnimPose()
        {
            ResetIKTargets(true);
        }
                
        public virtual void ResetIKTargets(bool applyAnim)
        {
            //Restore the TPose
            _ikTargetsRoot.ResetTransformation();
            
            _ikTargetsLeftHand.parent = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _ikTargetsLeftHand.ResetTransformation();

            _ikTargetsRightHand.parent = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _ikTargetsRightHand.ResetTransformation();

            _ikTargetsFace.parent = _animator.GetBoneTransform(HumanBodyBones.Head);
            _ikTargetsFace.ResetTransformation();

            _animator.EnforceTPose();

            //Temporally set the parent of each ikTargetLimb to be the boneLimb. This way, the 
            //target is automatically moved to the bone position when the animation is applied. 
            foreach (HumanBodyBones boneID in _allIKBones)
            {
                QuickIKSolver ikSolver = GetIKSolver(boneID);
                if (ikSolver)
                {
                    ikSolver.SaveInitialBoneRotations();
                    ResetIKTarget(boneID);
                    ikSolver._targetLimb.parent = ikSolver._boneLimb;
                }
            }

            //If we have an animatorcontroller defined, the targets are moved at the position of the 
            //initial frame of the current animation in such controller. 
            if (applyAnim && _animator.runtimeAnimatorController)
            {
                Quaternion jawLocalRot = Quaternion.identity;
                Transform tJaw = _animator.GetBoneTransform(HumanBodyBones.Jaw);
                if (tJaw)
                {
                    jawLocalRot = tJaw.localRotation;
                }

                _animator.Update(0);

                //Force the mouth to be closed
                if (tJaw)
                {
                    tJaw.localRotation = jawLocalRot;
                }
            }

            //Check the rotation of the parents of the targets of the finger bones
            for (HumanBodyBones boneID = HumanBodyBones.LeftThumbDistal; boneID <= HumanBodyBones.LeftLittleDistal; boneID += 3)
            {
                Transform t = GetIKTargetParent(boneID);
                t.position = _animator.GetBoneTransform(boneID - 2).position;
                t.LookAt(_animator.GetBoneTransform(boneID - 1), transform.up);
            }
            for (HumanBodyBones boneID = HumanBodyBones.RightThumbDistal; boneID <= HumanBodyBones.RightLittleDistal; boneID += 3)
            {
                Transform t = GetIKTargetParent(boneID);
                t.position = _animator.GetBoneTransform(boneID - 2).position;
                t.LookAt(_animator.GetBoneTransform(boneID - 1), transform.up);
            }

            //Restore the ikTargetLimb real parent. 
            foreach (HumanBodyBones boneID in _allIKBones)
            {
                QuickIKSolver ikSolver = GetIKSolver(boneID);
                if (ikSolver)
                {
                    ikSolver._targetLimb.parent = GetIKTargetParent(ikSolver._boneID);
                    ikSolver._targetLimb.localScale = Vector3.one;
                    if (QuickHumanTrait.IsBoneFingerLeft(ikSolver._boneID) || QuickHumanTrait.IsBoneFingerRight(ikSolver._boneID))
                    {
                        ikSolver._targetLimb.localRotation = Quaternion.identity;
                    }
                }
            }

            _ikTargetsLeftHand.parent = transform;
            _ikTargetsLeftHand.localScale = Vector3.one;

            _ikTargetsRightHand.parent = transform;
            _ikTargetsRightHand.localScale = Vector3.one;

            _ikTargetsFace.parent = transform;
            _ikTargetsFace.localScale = Vector3.one;
        }

        protected virtual Transform GetIKTargetParent(HumanBodyBones boneID)
        {
            if (boneID == HumanBodyBones.LeftEye || boneID == HumanBodyBones.RightEye) return _ikTargetsFace;
            if (QuickHumanTrait.IsBoneFingerLeft(boneID)) return _ikTargetsLeftHand.CreateChild("_Axis_" + boneID.ToString());
            if (QuickHumanTrait.IsBoneFingerRight(boneID)) return _ikTargetsRightHand.CreateChild("_Axis_" + boneID.ToString());
            return _ikTargetsRoot;
        }

        protected virtual Vector3 GetIKTargetHipsOffset()
        {
            return Vector3.zero;
        }

        protected virtual Transform GetBoneUpper(HumanBodyBones boneLimbID)
        {
            if (boneLimbID == HumanBodyBones.Hips || boneLimbID == HumanBodyBones.Head) return _animator.GetBoneTransform(HumanBodyBones.Spine);
            return _animator.GetBoneTransform(QuickHumanTrait.GetParentBone(QuickHumanTrait.GetParentBone(boneLimbID)));
        }

        protected virtual Transform GetBoneMid(HumanBodyBones boneLimbID)
        {
            if (boneLimbID == HumanBodyBones.Hips || boneLimbID == HumanBodyBones.Head) return _animator.GetBoneTransform(HumanBodyBones.Spine);
            return _animator.GetBoneTransform(QuickHumanTrait.GetParentBone(boneLimbID));
        }

        public override void Calibrate()
        {
            foreach (HumanBodyBones boneID in _allIKBones)
            {
                QuickIKSolver ikSolver = GetIKSolver(boneID);
                if (ikSolver)
                {
                    ikSolver.Calibrate();
                }
            }
        }

        //public static List<HumanBodyBones> GetIKLimbBones()
        //{
        //    if (_ikLimbBones == null)
        //    {
        //        _ikLimbBones = new List<HumanBodyBones>();
        //        foreach (IKLimbBones boneID in QuickUtils.GetEnumValues<IKLimbBones>())
        //        {
        //            _ikLimbBones.Add(QuickUtils.ParseEnum<HumanBodyBones>(boneID.ToString()));
        //        }
        //    }

        //    return _ikLimbBones;
        //}

        public static HumanBodyBones ToHumanBodyBones(IKBone ikBone)
        {
            if (_toHumanBodyBones.Count == 0)
            {
                for (IKBone b = 0; b < IKBone.LastBone; b++)
                {
                    _toHumanBodyBones.Add(QuickUtils.ParseEnum<HumanBodyBones>(b.ToString()));
                }
            }

            return _toHumanBodyBones[(int)ikBone];
        }

        public static IKBone ToIKBone(HumanBodyBones boneID)
        {
            if (_toIKBone.Count == 0)
            {
                for (IKBone b = 0; b < IKBone.LastBone; b++)
                {
                    _toIKBone[QuickUtils.ParseEnum<HumanBodyBones>(b.ToString())] = b;
                }
            }

            return _toIKBone[boneID];
        }

        public virtual QuickIKSolver GetIKSolver(IKBone ikBone)
        {
            return GetIKSolver(ToHumanBodyBones(ikBone));
        }

        public virtual QuickIKSolver GetIKSolver(HumanBodyBones boneID)
        {
            QuickIKSolver result = null;

            if (IsIKBone(boneID))
            {
                if (!_ikSolvers.ContainsKey(boneID))
                {
                    _ikSolvers[boneID] = CreateIKSolver(boneID);
                }

                result = _ikSolvers[boneID];
            }
            
            return result;
        }

        public virtual List<QuickIKSolver> GetIKSolversBody()
        {
            if (_ikSolversBody == null)
            {
                _ikSolversBody = new List<QuickIKSolver>();
                for (IKBone ikBone = IKBone.StartBody; ikBone <= IKBone.EndBody; ikBone++)
                {
                    _ikSolversBody.Add(GetIKSolver(ikBone));
                }
            }

            return _ikSolversBody;
        }

        public virtual List<QuickIKSolver> GetIKSolversHand(bool isLeftHand)
        {
            List<QuickIKSolver> result = isLeftHand ? _ikSolversLeftHand : _ikSolversRightHand;
            if (result == null)
            {
                result = new List<QuickIKSolver>();
                IKBone ikBoneStart = isLeftHand ? IKBone.StartLeftHandFingers : IKBone.StartRightHandFingers;
                IKBone ikBoneEnd = isLeftHand ? IKBone.EndLeftHandFingers : IKBone.EndRightHandFingers;

                for (IKBone ikBone = ikBoneStart; ikBone <= ikBoneEnd; ikBone++)
                {
                    result.Add(GetIKSolver(ikBone));
                }
            }

            return result;
        }

        protected virtual bool IsIKBone(HumanBodyBones boneID)
        {
            if (_allIKBonesSet == null)
            {
                _allIKBonesSet = new HashSet<HumanBodyBones>(_allIKBones);
            }

            return _allIKBonesSet.Contains(boneID);
        }

        public virtual void CopyLeftHandPoseToRightHand()
        {
            CopyHandPose(HumanBodyBones.LeftHand, HumanBodyBones.RightHand);
        }

        public virtual void CopyRightHandPoseToLeftHand()
        {
            CopyHandPose(HumanBodyBones.RightHand, HumanBodyBones.LeftHand);
        }

        protected virtual void CopyHandPose(HumanBodyBones srcHandBoneID, HumanBodyBones dstHandBoneID)
        {
            //Copy the hand pose
            MirrorPose(GetIKSolver(srcHandBoneID), GetIKSolver(dstHandBoneID));
            
            //Copy the fingers pose
            List<QuickIKSolver> srcHandIKSolvers = GetIKSolversHand(srcHandBoneID == HumanBodyBones.LeftHand);
            List<QuickIKSolver> dstHandIKSolvers = GetIKSolversHand(dstHandBoneID == HumanBodyBones.LeftHand);
            for (int i = 0; i < srcHandIKSolvers.Count; i++)
            {
                MirrorPose(srcHandIKSolvers[i], dstHandIKSolvers[i]);
            }
        }

        public virtual void CopyLeftFootPoseToRightFoot()
        {
            MirrorPose(GetIKSolver(HumanBodyBones.LeftFoot), GetIKSolver(HumanBodyBones.RightFoot));
        }

        public virtual void CopyRightFootPoseToLeftFoot()
        {
            MirrorPose(GetIKSolver(HumanBodyBones.RightFoot), GetIKSolver(HumanBodyBones.LeftFoot));
        }

        protected virtual void MirrorPose(QuickIKSolver srcIKSolver, QuickIKSolver dstIKSolver)
        {
            Vector3 srcPos = srcIKSolver._targetLimb.localPosition;
            Quaternion srcRot = srcIKSolver._targetLimb.localRotation;
            dstIKSolver._targetLimb.localPosition = Vector3.Scale(new Vector3(-1, 1, 1), srcPos);
            dstIKSolver._targetLimb.localRotation = new Quaternion(srcRot.x, -srcRot.y, -srcRot.z, srcRot.w);
            //if (dstIKSolver._targetHint && srcIKSolver._targetHint)
            //{
            //    dstIKSolver._targetHint.localPosition = Vector3.Scale(new Vector3(-1, 1, 1), srcIKSolver._targetHint.localPosition);
            //}
        }

        #endregion

        #region UPDATE

        protected virtual void LateUpdate()
        {
            UpdateTracking();
        }

        public override void UpdateTracking()
        {
            //Update the IK for the body controllers
            QuickIKSolver ikSolverHips = GetIKSolver(HumanBodyBones.Hips);
            ikSolverHips.UpdateIK();

            QuickIKSolver ikSolverHead = GetIKSolver(HumanBodyBones.Head);
            ikSolverHead.UpdateIK();
            
            ikSolverHips._targetLimb.position += GetIKTargetHipsOffset();
            ikSolverHips.UpdateIK();

            GetIKSolver(HumanBodyBones.LeftHand).UpdateIK();
            GetIKSolver(HumanBodyBones.RightHand).UpdateIK();
            GetIKSolver(HumanBodyBones.LeftFoot).UpdateIK();
            GetIKSolver(HumanBodyBones.RightFoot).UpdateIK();
            
            //Update the IK for the fingers controllers
            Transform leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _ikTargetsLeftHand.position = leftHand.position;
            _ikTargetsLeftHand.rotation = leftHand.rotation;

            Transform rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _ikTargetsRightHand.position = rightHand.position;
            _ikTargetsRightHand.rotation = rightHand.rotation;

            for (IKBone ikBone = IKBone.StartLeftHandFingers; ikBone <= IKBone.EndLeftHandFingers; ikBone++)
            {
                GetIKSolver(ikBone).UpdateIK();
            }

            for (IKBone ikBone = IKBone.StartRightHandFingers; ikBone <= IKBone.EndRightHandFingers; ikBone++)
            {
                GetIKSolver(ikBone).UpdateIK();
            }

            //Update the IK for the face controllers
            Transform head = _animator.GetBoneTransform(HumanBodyBones.Head);
            _ikTargetsFace.position = head.position;
            _ikTargetsFace.rotation = head.rotation;
        }

        #endregion

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            foreach (HumanBodyBones boneID in _allIKBones)
            {
                QuickIKSolver ikSolver = GetIKSolver(boneID);
                if (ikSolver && ikSolver._boneUpper && ikSolver._targetLimb) Gizmos.DrawLine(ikSolver._boneUpper.position, ikSolver._targetLimb.position);
            }

            Gizmos.color = Color.magenta;
            foreach (HumanBodyBones boneID in _allIKBones)
            {
                QuickIKSolver ikSolver = GetIKSolver(boneID);
                if (ikSolver && ikSolver._boneUpper && ikSolver._boneMid) Gizmos.DrawLine(ikSolver._boneUpper.position, ikSolver._boneMid.position);
                if (ikSolver && ikSolver._boneMid && ikSolver._boneLimb) Gizmos.DrawLine(ikSolver._boneMid.position, ikSolver._boneLimb.position);
            }

            Gizmos.color = Color.yellow;
            foreach (HumanBodyBones boneID in _allIKBones)
            {
                QuickIKSolver ikSolver = GetIKSolver(boneID);
                if (ikSolver && ikSolver._boneMid && ikSolver._targetHint) Gizmos.DrawLine(ikSolver._boneMid.position, ikSolver._targetHint.position);
            }
        }

        #endregion

    }

}
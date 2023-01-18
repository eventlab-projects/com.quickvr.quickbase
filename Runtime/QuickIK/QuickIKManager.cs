using System.Collections.Generic;
using UnityEngine;

namespace QuickVR {

    [System.Serializable]

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
    };

    [RequireComponent(typeof(QuickIKManagerExecuteInEditMode))]
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

        [SerializeField, HideInInspector]
        public bool _showControlsFace = false;

#endif

        #endregion

        #region PROTECTED PARAMETERS

        protected Transform _ikSolversRoot
        {
            get
            {
                if (!m_IKSolversRoot)
                {
                    m_IKSolversRoot = transform.CreateChild(IK_SOLVERS_ROOT_NAME);
                }

                return m_IKSolversRoot;
            }
        }
        protected Transform m_IKSolversRoot = null;

        protected Transform _ikTargetsRoot
        {
            get
            {
                if (!m_ikTargetsRoot) m_ikTargetsRoot = transform.CreateChild("__IKTargets__");
                return m_ikTargetsRoot;
            }
        }
        protected Transform m_ikTargetsRoot;

        protected static List<HumanBodyBones> _toHumanBodyBones = new List<HumanBodyBones>();
        protected static Dictionary<HumanBodyBones, IKBone> _toIKBone = new Dictionary<HumanBodyBones, IKBone>();

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
        public static float DEFAULT_TARGET_HINT_FINGER_DISTANCE = 0.1f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            Reset();
        }

        protected virtual void Reset()
        {
            InitAnimator();
            _animator.CreateMissingBones();

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
            //Ensure that the ikSolvers are childs of _ikSolversRoot. 
            foreach (QuickIKSolver ikSolver in GetComponentsInChildren<QuickIKSolver>(true))
            {
                ikSolver.transform.parent = _ikSolversRoot;
            }

            for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
            {
                QuickIKSolver ikSolver = CreateIKSolver(ikBone);
                ikSolver.transform.SetSiblingIndex((int)ikBone);
            }

            for (IKBone ikBone = IKBone.LeftThumbDistal; ikBone <= IKBone.LeftLittleDistal; ikBone++)
            {
                GetIKSolver(ikBone)._targetHint.parent = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            }

            for (IKBone ikBone = IKBone.RightThumbDistal; ikBone <= IKBone.RightLittleDistal; ikBone++)
            {
                GetIKSolver(ikBone)._targetHint.parent = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            }
        }

        protected virtual void OnDestroy()
        {
            //ResetTPose();

            QuickUtils.Destroy(transform.Find(IK_SOLVERS_ROOT_NAME));
            
            QuickUtils.Destroy(m_ikTargetsRoot);
        }

        protected virtual QuickIKSolver CreateIKSolver(IKBone ikBone)
        {
            QuickIKSolver result;

            if (ikBone == IKBone.Hips)
            {
                result = CreateIKSolver<QuickIKSolverHips>(ikBone);
            }
            else if (ikBone == IKBone.Head)
            {
                result = CreateIKSolver<QuickIKSolverHead>(ikBone);
            }
            else if (ikBone == IKBone.LeftHand || ikBone == IKBone.RightHand)
            {
                result = CreateIKSolver<QuickIKSolverHand>(ikBone);
            }
            else if (ikBone == IKBone.LeftEye || ikBone == IKBone.RightEye)
            {
                result = CreateIKSolver<QuickIKSolverEye>(ikBone);
            }
            else
            {
                result = CreateIKSolver<QuickIKSolver>(ikBone);
            }
            
            return result;
        }

        protected virtual T CreateIKSolver<T>(IKBone ikBone) where T : QuickIKSolver
        {
            Transform ikSolversRoot = transform.CreateChild(IK_SOLVERS_ROOT_NAME);
            Transform t = ikSolversRoot.CreateChild(IK_SOLVER_PREFIX + ikBone.ToString());
            T ikSolver = t.GetComponent<T>();

            if (!ikSolver)
            {
                ikSolver = t.gameObject.AddComponent<T>();

                //And configure it according to the bone
                HumanBodyBones boneLimb = ToHumanBodyBones(ikBone);
                ikSolver._boneUpper = _animator.GetBoneTransform(GetBoneUpperID(boneLimb));
                ikSolver._boneMid = _animator.GetBoneTransform(GetBoneMidID(boneLimb));
                ikSolver._boneLimb = _animator.GetBoneTransform((QuickHumanBodyBones)boneLimb);
            }
            
            return ikSolver;
        }

        #endregion

        #region GET AND SET

        public virtual void SavePose()
        {
            for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
            {
                GetIKSolver(ikBone).SavePose();
            }
        }

        public virtual void LoadPose()
        {
            for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
            {
                GetIKSolver(ikBone).LoadPose();
            }
        }

        protected virtual void ResetIKTarget(HumanBodyBones boneID, Transform ikTarget)
        {
            if ((int)boneID != -1)
            {
                ikTarget.name = IK_TARGET_PREFIX + boneID;
                Transform bone = _animator.GetBoneTransform((QuickHumanBodyBones)boneID);
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
                else if (boneID == HumanBodyBones.Spine)
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
                    //ikTarget.forward = _animator.GetBoneTransform(boneID).position - _animator.GetBoneTransform(boneID - 1).position;
                    ikTarget.forward = _animator.GetBoneTransformFingerTip(boneID).position - _animator.GetBoneTransform(boneID).position;
                    if (boneName.Contains("Thumb"))
                    {
                        float sign = boneName.Contains("Left") ? -1 : 1;
                        ikTarget.Rotate(ikTarget.forward, sign * 90, Space.World);
                    }
                }
                else if (boneName.Contains("Intermediate"))
                {
                    if (boneName.Contains("Thumb"))
                    {
                        Vector3 v = Vector3.Cross(_animator.GetBoneTransform(boneID - 1).position - _animator.GetBoneTransform(boneID).position, transform.up).normalized;
                        float sign = boneName.Contains("Left") ? 1 : -1;
                        ikTarget.position += sign * v * DEFAULT_TARGET_HINT_FINGER_DISTANCE;
                    }
                    else
                    {
                        ikTarget.position += transform.up * DEFAULT_TARGET_HINT_FINGER_DISTANCE;
                    }
                }
            }
        }

        public virtual void LoadTPose()
        {
            _animator.EnforceTPose();

            //Reset the IKTargets 
            for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
            {
                QuickIKSolver ikSolver = GetIKSolver(ikBone);
                HumanBodyBones boneID = ToHumanBodyBones(ikBone);
                ResetIKTarget(boneID, ikSolver._targetLimb);
                ikSolver._targetLimb.parent = GetIKTargetParent(boneID);

                if (ikSolver._targetLimb.childCount > 0)
                {
                    ikSolver._targetLimb.GetChild(0).rotation = ikSolver._boneLimb.rotation;
                }
                
                if (ikSolver._targetHint)
                {
                    ResetIKTarget(GetBoneMidID(boneID), ikSolver._targetHint);
                }
            }
        }

        public virtual void LoadAnimPose()
        {
            //Restore the TPose
            LoadTPose();

            _ikTargetsRoot.ResetTransformation();

            //Temporally set the parent of each ikTargetLimb to be the boneLimb. This way, the 
            //target is automatically moved to the bone position when the animation is applied. 
            for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
            {
                QuickIKSolver ikSolver = GetIKSolver(ikBone);
                ikSolver._targetLimb.parent = ikSolver._boneLimb;
            }

            //If we have an animatorcontroller defined, the targets are moved at the position of the 
            //initial frame of the current animation in such controller. 
            if (_animator.runtimeAnimatorController)
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
            for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
            {
                HumanBodyBones boneID = ToHumanBodyBones(ikBone);
                QuickIKSolver ikSolver = GetIKSolver(ikBone);
                ikSolver._targetLimb.parent = GetIKTargetParent(boneID);
                ikSolver._targetLimb.localScale = Vector3.one;
            }

            //Recalculate the ikTargetHint position of the arms and legs
            //for (IKBone ikBone = IKBone.LeftHand; ikBone <= IKBone.RightFoot; ikBone++)
            //{
            //    QuickIKSolver ikSolver = GetIKSolver(ikBone);
            //    if (ikSolver._targetHint)
            //    {
            //        Vector3 u = (ikSolver._boneMid.position - ikSolver._boneLimb.position).normalized;
            //        Vector3 v = (ikSolver._boneMid.position - ikSolver._boneUpper.position).normalized;

            //        if (Vector3.Angle(u, v) < 175)
            //        {
            //            ikSolver._targetHint.position = ikSolver._boneMid.position + (u + v).normalized * DEFAULT_TARGET_HINT_DISTANCE;
            //        }
            //    }
            //}
        }
                
        protected virtual Transform GetIKTargetParent(HumanBodyBones boneID)
        {
            if (boneID == HumanBodyBones.LeftEye || boneID == HumanBodyBones.RightEye) return GetIKSolver(IKBone.Head)._targetLimb;
            if (QuickHumanTrait.IsBoneFingerLeft(boneID)) return GetIKSolver(IKBone.LeftHand)._targetLimb;
            if (QuickHumanTrait.IsBoneFingerRight(boneID)) return GetIKSolver(IKBone.RightHand)._targetLimb;
            return _ikTargetsRoot;
        }

        protected virtual HumanBodyBones GetBoneUpperID(HumanBodyBones boneLimbID)
        {
            HumanBodyBones boneUpperID;

            if (boneLimbID == HumanBodyBones.Hips)
            {
                boneUpperID = HumanBodyBones.Spine;
            }
            else if (boneLimbID == HumanBodyBones.Head)
            {
                boneUpperID = HumanBodyBones.Spine;
            }
            else if (boneLimbID == HumanBodyBones.LeftEye || boneLimbID == HumanBodyBones.RightEye)
            {
                boneUpperID = HumanBodyBones.Head;
            }
            else
            {
                boneUpperID = QuickHumanTrait.GetParentBone(QuickHumanTrait.GetParentBone(boneLimbID));
            }

            return boneUpperID;
        }

        protected virtual HumanBodyBones GetBoneMidID(HumanBodyBones boneLimbID)
        {
            HumanBodyBones boneMidID;

            if (boneLimbID == HumanBodyBones.Hips || boneLimbID == HumanBodyBones.Head)
            {
                boneMidID = HumanBodyBones.Spine;
            }
            else if (boneLimbID == HumanBodyBones.LeftEye || boneLimbID == HumanBodyBones.RightEye)
            {
                boneMidID = HumanBodyBones.Head;
            }
            else
            {
                boneMidID = QuickHumanTrait.GetParentBone(boneLimbID);
            }

            return boneMidID;
        }

        public override void Calibrate()
        {
            LoadPose();
        }

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
            return _ikSolversRoot.GetChild((int)ikBone).GetComponent<QuickIKSolver>();
        }

        public virtual QuickIKSolver GetIKSolver(HumanBodyBones boneID)
        {
            return GetIKSolver(ToIKBone(boneID));
        }

        public virtual void CopyLeftHandPoseToRightHand()
        {
            CopyHandPose(IKBone.LeftHand, IKBone.RightHand);
        }

        public virtual void CopyRightHandPoseToLeftHand()
        {
            CopyHandPose(IKBone.RightHand, IKBone.LeftHand);
        }

        protected virtual void CopyHandPose(IKBone srcHandBoneID, IKBone dstHandBoneID)
        {
            //Copy the hand pose
            MirrorPose(GetIKSolver(srcHandBoneID), GetIKSolver(dstHandBoneID));

            //Copy the fingers pose
            IKBone srcIKBoneStart = srcHandBoneID == IKBone.LeftHand? IKBone.LeftThumbDistal : IKBone.RightThumbDistal;
            IKBone dstIKBoneStart = dstHandBoneID == IKBone.LeftHand ? IKBone.LeftThumbDistal : IKBone.RightThumbDistal;

            for (int i = 0; i < 5; i++)
            {
                MirrorPose(GetIKSolver(srcIKBoneStart + i), GetIKSolver(dstIKBoneStart + i));
            }
        }

        public virtual void CopyLeftFootPoseToRightFoot()
        {
            MirrorPose(GetIKSolver(IKBone.LeftFoot), GetIKSolver(IKBone.RightFoot));
        }

        public virtual void CopyRightFootPoseToLeftFoot()
        {
            MirrorPose(GetIKSolver(IKBone.RightFoot), GetIKSolver(IKBone.LeftFoot));
        }

        protected virtual void MirrorPose(QuickIKSolver srcIKSolver, QuickIKSolver dstIKSolver)
        {
            Transform srcParent = srcIKSolver._targetLimb.parent;
            Transform dstParent = dstIKSolver._targetLimb.parent;
            srcIKSolver._targetLimb.parent = dstIKSolver._targetLimb.parent = transform;
            
            Transform srcHintParent = null;
            Transform dstHintParent = null;

            if (srcIKSolver._targetHint && dstIKSolver._targetHint)
            {
                srcHintParent = srcIKSolver._targetHint.parent;
                dstHintParent = dstIKSolver._targetHint.parent;
                srcIKSolver._targetHint.parent = dstIKSolver._targetHint.parent = transform;
            }

            MirrorIKTarget(srcIKSolver._targetLimb, dstIKSolver._targetLimb);
            MirrorIKTarget(srcIKSolver._targetHint, dstIKSolver._targetHint);

            srcIKSolver.UpdateIK();
            dstIKSolver.UpdateIK();

            //Restore the parent for the IKTargetLimbs
            srcIKSolver._targetLimb.parent = srcParent;
            dstIKSolver._targetLimb.parent = dstParent;
            srcIKSolver._targetLimb.localScale = dstIKSolver._targetLimb.localScale = Vector3.one;

            //Restore the parent for the IKTargetHints
            if (srcIKSolver._targetHint && dstIKSolver._targetHint)
            {
                srcIKSolver._targetHint.parent = srcHintParent;
                dstIKSolver._targetHint.parent = dstHintParent;
                srcIKSolver._targetHint.localScale = dstIKSolver._targetHint.localScale = Vector3.one;
            }
        }

        protected virtual void MirrorIKTarget(Transform srcIKTarget, Transform dstIKTarget)
        {
            if (srcIKTarget && dstIKTarget)
            {
                Vector3 srcPos = srcIKTarget.localPosition;
                Quaternion srcRot = srcIKTarget.localRotation;

                dstIKTarget.localPosition = Vector3.Scale(new Vector3(-1, 1, 1), srcPos);
                dstIKTarget.localRotation = new Quaternion(srcRot.x, -srcRot.y, -srcRot.z, srcRot.w);
            }
        }

        #endregion

        #region UPDATE

        protected virtual void LateUpdate()
        {
            UpdateTracking();
        }

        public override void UpdateTracking()
        {
            ////if (_animator.runtimeAnimatorController == null)
            //{
            //    for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
            //    {
            //        QuickIKSolver ikSolver = GetIKSolver(ikBone);
            //        if (ikSolver._enableIK)
            //        {
            //            GetIKSolver(ikBone).ResetIKChain();
            //        }
            //    }
            //}

            QuickIKSolver ikSolverHips = GetIKSolver(IKBone.Hips);
            QuickIKSolver ikSolverHead = GetIKSolver(IKBone.Head);

            //float chainLength = Vector3.Distance(_animator.GetBoneTransform(HumanBodyBones.Hips).position, _animator.GetBoneTransform(HumanBodyBones.Head).position);
            //Vector3 v = (ikSolverHips._targetLimb.position - ikSolverHead._targetLimb.position).normalized;
            //ikSolverHips._targetLimb.position = ikSolverHead._targetLimb.position + v * chainLength;

            //Update the IK for the body controllers
            ikSolverHips.UpdateIK();
            ikSolverHead.UpdateIK();

            GetIKSolver(IKBone.LeftHand).UpdateIK();
            GetIKSolver(IKBone.RightHand).UpdateIK();
            GetIKSolver(IKBone.LeftFoot).UpdateIK();
            GetIKSolver(IKBone.RightFoot).UpdateIK();

            //Update the IK for the fingers controllers
            UpdateIKFingers();

            //Update the IK for the face controllers
            for (IKBone ikBone = IKBone.LeftEye; ikBone <= IKBone.RightEye; ikBone++)
            {
                GetIKSolver(ikBone).UpdateIK();
            }
        }

        protected virtual void UpdateIKFingers()
        {
            for (IKBone ikBone = IKBone.LeftThumbDistal; ikBone <= IKBone.LeftLittleDistal; ikBone++)
            {
                GetIKSolver(ikBone).UpdateIK();
            }

            for (IKBone ikBone = IKBone.RightThumbDistal; ikBone <= IKBone.RightLittleDistal; ikBone++)
            {
                GetIKSolver(ikBone).UpdateIK();
            }
        }

        #endregion

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
            {
                QuickIKSolver ikSolver = GetIKSolver(ikBone);
                if (ikSolver._boneUpper && ikSolver._targetLimb) Gizmos.DrawLine(ikSolver._boneUpper.position, ikSolver._targetLimb.position);
            }

            Gizmos.color = Color.magenta;
            for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
            {
                QuickIKSolver ikSolver = GetIKSolver(ikBone);
                if (ikSolver._boneUpper && ikSolver._boneMid) Gizmos.DrawLine(ikSolver._boneUpper.position, ikSolver._boneMid.position);
                if (ikSolver._boneMid && ikSolver._boneLimb) Gizmos.DrawLine(ikSolver._boneMid.position, ikSolver._boneLimb.position);
            }

            Gizmos.color = Color.yellow;
            for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
            {
                QuickIKSolver ikSolver = GetIKSolver(ikBone);
                if (ikSolver._boneMid && ikSolver._targetHint) Gizmos.DrawLine(ikSolver._boneMid.position, ikSolver._targetHint.position);
            }
        }

        #endregion

    }

}
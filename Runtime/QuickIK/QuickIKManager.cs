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

        #region EVENTS

        public delegate void OnAddQuickIKManagerCallback(QuickIKManager ikManager);
        public delegate void OnRemoveQuickIKManagerCallback(QuickIKManager ikManager);

        public static OnAddQuickIKManagerCallback OnAdd;
        public static OnRemoveQuickIKManagerCallback OnRemove;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            Reset();

            SavePose();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (OnAdd != null)
            {
                OnAdd(this);
            }
        }

        protected virtual void OnDisable()
        {
            if (OnRemove != null)
            {
                OnRemove(this);
            }
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

        protected virtual QuickIKSolver CreateIKSolver(IKBone ikBone)
        {
            QuickIKSolver result;
            
            if (ikBone == IKBone.Hips) result = CreateIKSolver<QuickIKSolverHips>(ikBone);
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
                ikSolver._boneUpper = GetBoneUpper(boneLimb);
                ikSolver._boneMid = GetBoneMid(boneLimb);
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
                ikSolver._targetLimb.GetChild(0).rotation = _animator.GetBoneTransform((QuickHumanBodyBones)boneID).rotation;
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
            ResetIKTargets(false);
        }

        public virtual void LoadAnimPose()
        {
            ResetIKTargets(true);
        }
                
        public virtual void ResetIKTargets(bool applyAnim)
        {
            for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++ )
            {
                GetIKSolver(ikBone).ResetIKChain();
            }

            //Restore the TPose
            _ikTargetsRoot.ResetTransformation();
            
            _ikTargetsLeftHand.parent = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _ikTargetsLeftHand.ResetTransformation();

            _ikTargetsRightHand.parent = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _ikTargetsRightHand.ResetTransformation();

            _ikTargetsFace.parent = _animator.GetBoneTransform(HumanBodyBones.Head);
            _ikTargetsFace.ResetTransformation();
            _ikTargetsFace.rotation = transform.rotation;

            _animator.EnforceTPose();

            //Temporally set the parent of each ikTargetLimb to be the boneLimb. This way, the 
            //target is automatically moved to the bone position when the animation is applied. 
            for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
            {
                QuickIKSolver ikSolver = GetIKSolver(ikBone);
                ikSolver.SaveInitialBoneRotations();
                ResetIKTarget(ToHumanBodyBones(ikBone));
                ikSolver._targetLimb.parent = ikSolver._boneLimb;
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
            for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
            {
                HumanBodyBones boneID = ToHumanBodyBones(ikBone);
                QuickIKSolver ikSolver = GetIKSolver(ikBone);
                ikSolver._targetLimb.parent = GetIKTargetParent(boneID);
                ikSolver._targetLimb.localScale = Vector3.one;
                if (QuickHumanTrait.IsBoneFingerLeft(boneID) || QuickHumanTrait.IsBoneFingerRight(boneID))
                {
                    ikSolver._targetLimb.localRotation = Quaternion.identity;
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

        protected virtual Transform GetBoneUpper(HumanBodyBones boneLimbID)
        {
            if (boneLimbID == HumanBodyBones.Hips || boneLimbID == HumanBodyBones.Head)
            {
                return _animator.GetBoneTransform(HumanBodyBones.Spine);
            }
            else if (boneLimbID == HumanBodyBones.LeftEye || boneLimbID == HumanBodyBones.RightEye)
            {
                return _animator.GetBoneTransform(HumanBodyBones.Head);
            }
            
            return _animator.GetBoneTransform(QuickHumanTrait.GetParentBone(QuickHumanTrait.GetParentBone(boneLimbID)));
        }

        protected virtual Transform GetBoneMid(HumanBodyBones boneLimbID)
        {
            if (boneLimbID == HumanBodyBones.Hips || boneLimbID == HumanBodyBones.Head)
            {
                return _animator.GetBoneTransform(HumanBodyBones.Spine);
            }
            else if (boneLimbID == HumanBodyBones.LeftEye || boneLimbID == HumanBodyBones.RightEye)
            {
                return _animator.GetBoneTransform(HumanBodyBones.Head);
            }

            return _animator.GetBoneTransform(QuickHumanTrait.GetParentBone(boneLimbID));
        }

        public override void Calibrate()
        {
            //for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
            //{
            //    GetIKSolver(ikBone).Calibrate();
            //}

            ResetIKTargets(true);
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
            UpdateIKTargetsRootHands();

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
            if (_animator.runtimeAnimatorController == null)
            {
                for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
                {
                    GetIKSolver(ikBone).ResetIKChain();
                }
            }

            QuickIKSolver ikSolverHips = GetIKSolver(IKBone.Hips);
            QuickIKSolver ikSolverHead = GetIKSolver(IKBone.Head);

            float chainLength = Vector3.Distance(_animator.GetBoneTransform(HumanBodyBones.Hips).position, _animator.GetBoneTransform(HumanBodyBones.Head).position);
            Vector3 v = (ikSolverHips._targetLimb.position - ikSolverHead._targetLimb.position).normalized;
            ikSolverHips._targetLimb.position = ikSolverHead._targetLimb.position + v * chainLength;

            //Update the IK for the body controllers
            ikSolverHips.UpdateIK();
            ikSolverHead.UpdateIK();

            GetIKSolver(IKBone.LeftHand).UpdateIK();
            GetIKSolver(IKBone.RightHand).UpdateIK();
            GetIKSolver(IKBone.LeftFoot).UpdateIK();
            GetIKSolver(IKBone.RightFoot).UpdateIK();

            //Update the IK for the fingers controllers
            UpdateIKTargetsRootHands();

            for (IKBone ikBone = IKBone.LeftThumbDistal; ikBone <= IKBone.LeftLittleDistal; ikBone++)
            {
                GetIKSolver(ikBone).UpdateIK();
            }

            for (IKBone ikBone = IKBone.RightThumbDistal; ikBone <= IKBone.RightLittleDistal; ikBone++)
            {
                GetIKSolver(ikBone).UpdateIK();
            }

            //Update the IK for the face controllers
            Transform head = _animator.GetBoneTransform(HumanBodyBones.Head);
            _ikTargetsFace.position = head.position;
            _ikTargetsFace.rotation = GetIKSolver(IKBone.Head)._targetLimb.rotation;

            for (IKBone ikBone = IKBone.LeftEye; ikBone <= IKBone.RightEye; ikBone++)
            {
                GetIKSolver(ikBone).UpdateIK();
            }
        }

        protected virtual void UpdateIKTargetsRootHands()
        {
            Transform leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            _ikTargetsLeftHand.position = leftHand.position;
            _ikTargetsLeftHand.rotation = leftHand.rotation;

            Transform rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _ikTargetsRightHand.position = rightHand.position;
            _ikTargetsRightHand.rotation = rightHand.rotation;
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
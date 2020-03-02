using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace QuickVR {

	public abstract class QuickBodyTracking : QuickBaseTrackingManager {

		#region PUBLIC PARAMETERS

        public enum DebugMode
        {
            None, 
            OnlySkeleton,
            All,
        }
        public DebugMode _debugMode = DebugMode.None;

        [BitMask(typeof(QuickJointType), QuickJointType.SPINE_BASE, QuickJointType.FOOT_RIGHT)]
        public int _trackedJointsBodyMask = -1;

        [BitMask(typeof(QuickJointType), QuickJointType.THUMB1_LEFT, QuickJointType.LITTLE3_LEFT)]
        public int _trackedJointsLeftHandMask = -1;

        [BitMask(typeof(QuickJointType), QuickJointType.THUMB1_RIGHT, QuickJointType.LITTLE3_RIGHT)]
        public int _trackedJointsRightHandMask = -1;

        [Header("Root Motion")]
        public bool _applyRootMotionX = false;
        public bool _applyRootMotionY = false;
        public bool _applyRootMotionZ = false;

		#endregion

		#region CREATION AND DESTRUCTION

        protected override void Start()
        {
            base.Start();

            _vrManager.AddBodyTrackingSystem(this);
        }

        #endregion

	}

	public abstract class QuickBodyTracking<V> : QuickBodyTracking
												where V : struct
	{

		#region PROTECTED PARAMETERS

		protected QuickJointsSkeleton _skeleton = null;

		protected Dictionary<QuickJointType, Quaternion> _initialJointRotations = new Dictionary<QuickJointType, Quaternion>();
		protected Dictionary<QuickJointType, Quaternion> _initialJointLocalRotations = new Dictionary<QuickJointType, Quaternion>();

		protected QuickUnityDictionary<QuickJointType, DoubleExponentialFilterVector3> _filters = new QuickUnityDictionary<QuickJointType, DoubleExponentialFilterVector3>();

		protected Dictionary<QuickJointType, V> _toTracking = new Dictionary<QuickJointType, V>();
        protected Dictionary<V, QuickJointType> _toQuickJoint = new Dictionary<V, QuickJointType>();
        protected Dictionary<QuickJointType, HumanBodyBones> _toUnity = new Dictionary<QuickJointType, HumanBodyBones>();

		protected Vector3 _initialForward = Vector3.forward;
		protected Quaternion _calibrationRotation = Quaternion.identity;
		protected Quaternion _initialRootRotation = Quaternion.identity;

		#endregion

		#region CREATION AND DESTRUCTION

		protected override void Awake() {
			base.Awake();
			CreateSkeleton();
            _initialRootRotation = transform.rotation;
            _initialForward = ComputeRootForward();
            CreateIntermediateSkeletonMap();
            CreateUnitySkeletonMap();
            InitInitialRotations();
            InitFilters();
		}

		protected virtual void CreateSkeleton() {
			GameObject go = new GameObject("__QuickJointsSkeleton__");
			go.transform.position = Vector3.zero;
			go.transform.rotation = Quaternion.identity;
			_skeleton = go.AddComponent<QuickJointsSkeleton>();
			_skeleton.transform.rotation = Quaternion.identity;
		}

		protected abstract void CreateIntermediateSkeletonMap();

		protected virtual void CreateUnitySkeletonMap() {
			//Spine Mapping
			CreateJointMapUnity(QuickJointType.SPINE_BASE, HumanBodyBones.Hips);
			CreateJointMapUnity(QuickJointType.SPINE_MID, HumanBodyBones.Spine);
            CreateJointMapUnity(QuickJointType.CHEST, HumanBodyBones.Chest);
			CreateJointMapUnity(QuickJointType.NECK, HumanBodyBones.Neck);
			CreateJointMapUnity(QuickJointType.HEAD, HumanBodyBones.Head);
            CreateJointMapUnity(QuickJointType.EYE_LEFT, HumanBodyBones.LeftEye);
            CreateJointMapUnity(QuickJointType.EYE_RIGHT, HumanBodyBones.RightEye);
			
			//Left Arm Mapping
			CreateJointMapUnity(QuickJointType.CLAVICLE_LEFT, HumanBodyBones.LeftShoulder);
			CreateJointMapUnity(QuickJointType.SHOULDER_LEFT, HumanBodyBones.LeftUpperArm);
			CreateJointMapUnity(QuickJointType.ELBOW_LEFT, HumanBodyBones.LeftLowerArm);
			CreateJointMapUnity(QuickJointType.HAND_LEFT, HumanBodyBones.LeftHand);
			
			//Right Arm Mapping
			CreateJointMapUnity(QuickJointType.CLAVICLE_RIGHT, HumanBodyBones.RightShoulder);
			CreateJointMapUnity(QuickJointType.SHOULDER_RIGHT, HumanBodyBones.RightUpperArm);
			CreateJointMapUnity(QuickJointType.ELBOW_RIGHT, HumanBodyBones.RightLowerArm);
			CreateJointMapUnity(QuickJointType.HAND_RIGHT, HumanBodyBones.RightHand);
			
			//Left Leg Mapping
			CreateJointMapUnity(QuickJointType.HIP_LEFT, HumanBodyBones.LeftUpperLeg);
			CreateJointMapUnity(QuickJointType.KNEE_LEFT, HumanBodyBones.LeftLowerLeg);
			CreateJointMapUnity(QuickJointType.ANKLE_LEFT, HumanBodyBones.LeftFoot);
			CreateJointMapUnity(QuickJointType.FOOT_LEFT, HumanBodyBones.LeftToes);
			
			//Left Leg Mapping
			CreateJointMapUnity(QuickJointType.HIP_RIGHT, HumanBodyBones.RightUpperLeg);
			CreateJointMapUnity(QuickJointType.KNEE_RIGHT, HumanBodyBones.RightLowerLeg);
			CreateJointMapUnity(QuickJointType.ANKLE_RIGHT, HumanBodyBones.RightFoot);
			CreateJointMapUnity(QuickJointType.FOOT_RIGHT, HumanBodyBones.RightToes);
			
			//Left Hand Mapping
			CreateJointMapUnity(QuickJointType.THUMB1_LEFT, HumanBodyBones.LeftThumbProximal);
			CreateJointMapUnity(QuickJointType.THUMB2_LEFT, HumanBodyBones.LeftThumbIntermediate);
			CreateJointMapUnity(QuickJointType.THUMB3_LEFT, HumanBodyBones.LeftThumbDistal);
			
			CreateJointMapUnity(QuickJointType.INDEX1_LEFT, HumanBodyBones.LeftIndexProximal);
			CreateJointMapUnity(QuickJointType.INDEX2_LEFT, HumanBodyBones.LeftIndexIntermediate);
			CreateJointMapUnity(QuickJointType.INDEX3_LEFT, HumanBodyBones.LeftIndexDistal);
			
			CreateJointMapUnity(QuickJointType.MIDDLE1_LEFT, HumanBodyBones.LeftMiddleProximal);
			CreateJointMapUnity(QuickJointType.MIDDLE2_LEFT, HumanBodyBones.LeftMiddleIntermediate);
			CreateJointMapUnity(QuickJointType.MIDDLE3_LEFT, HumanBodyBones.LeftMiddleDistal);
			
			CreateJointMapUnity(QuickJointType.RING1_LEFT, HumanBodyBones.LeftRingProximal);
			CreateJointMapUnity(QuickJointType.RING2_LEFT, HumanBodyBones.LeftRingIntermediate);
			CreateJointMapUnity(QuickJointType.RING3_LEFT, HumanBodyBones.LeftRingDistal);
			
			CreateJointMapUnity(QuickJointType.LITTLE1_LEFT, HumanBodyBones.LeftLittleProximal);
			CreateJointMapUnity(QuickJointType.LITTLE2_LEFT, HumanBodyBones.LeftLittleIntermediate);
			CreateJointMapUnity(QuickJointType.LITTLE3_LEFT, HumanBodyBones.LeftLittleDistal);

			//Right Hand Mapping
			CreateJointMapUnity(QuickJointType.THUMB1_RIGHT, HumanBodyBones.RightThumbProximal);
			CreateJointMapUnity(QuickJointType.THUMB2_RIGHT, HumanBodyBones.RightThumbIntermediate);
			CreateJointMapUnity(QuickJointType.THUMB3_RIGHT, HumanBodyBones.RightThumbDistal);
			
			CreateJointMapUnity(QuickJointType.INDEX1_RIGHT, HumanBodyBones.RightIndexProximal);
			CreateJointMapUnity(QuickJointType.INDEX2_RIGHT, HumanBodyBones.RightIndexIntermediate);
			CreateJointMapUnity(QuickJointType.INDEX3_RIGHT, HumanBodyBones.RightIndexDistal);
			
			CreateJointMapUnity(QuickJointType.MIDDLE1_RIGHT, HumanBodyBones.RightMiddleProximal);
			CreateJointMapUnity(QuickJointType.MIDDLE2_RIGHT, HumanBodyBones.RightMiddleIntermediate);
			CreateJointMapUnity(QuickJointType.MIDDLE3_RIGHT, HumanBodyBones.RightMiddleDistal);
			
			CreateJointMapUnity(QuickJointType.RING1_RIGHT, HumanBodyBones.RightRingProximal);
			CreateJointMapUnity(QuickJointType.RING2_RIGHT, HumanBodyBones.RightRingIntermediate);
			CreateJointMapUnity(QuickJointType.RING3_RIGHT, HumanBodyBones.RightRingDistal);
			
			CreateJointMapUnity(QuickJointType.LITTLE1_RIGHT, HumanBodyBones.RightLittleProximal);
			CreateJointMapUnity(QuickJointType.LITTLE2_RIGHT, HumanBodyBones.RightLittleIntermediate);
			CreateJointMapUnity(QuickJointType.LITTLE3_RIGHT, HumanBodyBones.RightLittleDistal);
		}

		protected virtual void CreateJointMapQuickVR(QuickJointType intermediateJoint, V trackingJoint) {
			_toTracking[intermediateJoint] = trackingJoint;
            _toQuickJoint[trackingJoint] = intermediateJoint;
		}

		protected virtual void CreateJointMapUnity(QuickJointType intermediateJoint, HumanBodyBones unityBoneID) {
			_toUnity[intermediateJoint] = unityBoneID;
		}

		protected virtual void InitInitialRotations() {
			Quaternion tmpRot = transform.rotation;
			transform.rotation = Quaternion.identity;

            foreach (QuickJointType qJointID in QuickJoint.GetAllTypes())
            {
				if (!_toUnity.ContainsKey(qJointID)) continue;

				HumanBodyBones uBoneID = _toUnity[qJointID];
				_initialJointRotations[qJointID] = _animator.GetBoneTransform(uBoneID).rotation;
				_initialJointLocalRotations[qJointID] = _animator.GetBoneTransform(uBoneID).localRotation;
			}

			transform.rotation = tmpRot;
		}

		protected virtual void InitFilters() {
            foreach (QuickJointType qJointID in QuickJoint.GetAllTypes())
            {
				CheckFilter(qJointID);
				DoubleExponentialFilterVector3 filter = null; 
				_filters.TryGetValue(qJointID, out filter); 
				_skeleton.SetFilter(qJointID, filter);
			}
		}

		protected virtual void CheckFilter(QuickJointType jointID) {
			if (!_filters.ContainsKey(jointID)) {
				_filters.Add(jointID, new DoubleExponentialFilterVector3());
			}
		}

		#endregion

		#region GET AND SET

		public virtual QuickJointsSkeleton GetIntermediateSkeleton() {
			return _skeleton;
		}

		public virtual bool IsTrackedJoint(QuickJointType qJointID) {
			int mask = -1;
			QuickJointType startJointID = QuickJointType.SPINE_BASE;

			//Determine the mask where qJointID is found. 
			if (qJointID <= QuickJointType.FOOT_RIGHT) {
				startJointID = QuickJointType.SPINE_BASE;
				mask = _trackedJointsBodyMask;
			}
			else if (qJointID <= QuickJointType.LITTLE3_LEFT) {
				startJointID = QuickJointType.THUMB1_LEFT;
				mask = _trackedJointsLeftHandMask;
			}
			else {
				startJointID = QuickJointType.THUMB1_RIGHT;
				mask = _trackedJointsRightHandMask;
			}
			int value = (int)qJointID - (int)startJointID;

			return ((mask & (1 << value)) != 0);
		}

		protected virtual Vector3 ComputeTorsoForward() {
			return Vector3.ProjectOnPlane(_skeleton.GetForward(), transform.up).normalized;
		}

		protected virtual Vector3 ComputeRootForward() {
			return Vector3.ProjectOnPlane(transform.forward, transform.up).normalized;
		}

		public virtual DoubleExponentialFilterVector3 GetFilter(QuickJointType qJointID) {
			DoubleExponentialFilterVector3 filter = null;
			_filters.TryGetValue(qJointID, out filter);
			return filter;
		}

		protected abstract bool IsTrackingDataWorldSpace();
		protected abstract Vector3 GetTrackingJointPosition(V tJointID);
		protected abstract Quaternion GetTrackingJointRotation(V tJointID);

		protected virtual QuickJoint.State GetTrackingJointState(V tJointID) {
            return _toQuickJoint.ContainsKey(tJointID) ? QuickJoint.State.TRACKED : QuickJoint.State.UNTRACKED;
		}

		protected abstract bool IsTrackingConnected();

		public override void Calibrate() {
			foreach (var pair in _toUnity) {
				_animator.GetBoneTransform(pair.Value).localRotation = _initialJointLocalRotations[pair.Key];
			}

			_calibrationRotation = Quaternion.Inverse(_initialRootRotation) * Quaternion.FromToRotation(ComputeTorsoForward(), _initialForward);
		}

        protected virtual Vector3 GetRootDisplacement()
        {
            Vector3 rootVel = _skeleton.GetJoint(QuickJointType.SPINE_BASE).GetVelocity();
            Vector3 vMask = new Vector3((_applyRootMotionX) ? 1 : 0, (_applyRootMotionY) ? 1 : 0, (_applyRootMotionZ) ? 1 : 0);
            Vector3 disp = _calibrationRotation * new Vector3(rootVel.x * vMask.x, rootVel.y * vMask.y, rootVel.z * vMask.z);

            return disp;
        }

		#endregion

		#region UPDATE

		public override void UpdateTracking() {
			if (!IsTrackingConnected()) return;

            //Vector3 fwd = ComputeTorsoForward();
            //if (fwd != Vector3.zero) transform.forward = fwd;
            
			UpdateIntermediateSkeleton();
			UpdateUnitySkeleton();

			UpdateRootPosition();

            //transform.rotation *= _initialRootRotation;
		}

		protected virtual void UpdateIntermediateSkeleton() {
            //Apply the tracking data to the intermediate skeleton. 
            foreach (QuickJointType qJointID in QuickJoint.GetAllTypes())
            {
				ApplyTrackingData(qJointID);
			}

            ScaleIntermediateSkeleton(QuickJointType.SPINE_BASE);

            foreach (QuickJointType qJointID in QuickJoint.GetAllTypes())
            {
                //_skeleton.GetJoint(qJointID).FilterPosition();
                //_skeleton.GetJoint(qJointID).FilterRotation();
            }
		}

        protected virtual void ScaleIntermediateSkeleton(QuickJointType qJointID)
        {
            QuickJointType qJointParentID = _skeleton.GetParentID(qJointID);
            Transform t = _skeleton.GetJoint(qJointID).GetTransform();
            Transform tParent = _skeleton.GetParent(qJointID).GetTransform();

            Transform uBone = _animator.GetBoneTransform(_toUnity[qJointID]);
            Transform uBoneParent = _animator.GetBoneTransform(_toUnity[qJointParentID]);

            float avatarBoneLength = Vector3.Distance(uBone.position, uBoneParent.position);
            t.position = tParent.position + (uBone.position - uBoneParent.position).normalized * avatarBoneLength;

            List<QuickJointType> childs = _skeleton.GetChilds(qJointID);
            foreach (QuickJointType c in childs)
            {
                ScaleIntermediateSkeleton(c);
            }
        }

        protected virtual void UpdateUnitySkeleton() {
            //Apply the intermediate skeleton data to the specific Unity skeleton. 
            foreach (QuickJointType qJointID in QuickJoint.GetAllTypes())
            { 
				if (_toUnity.ContainsKey(qJointID) && IsTrackedJoint(qJointID))	UpdateBoneRotation(qJointID);
			}
		}

		protected virtual void ApplyTrackingData(QuickJointType qJointID) {
			if (!_toTracking.ContainsKey(qJointID)) return;

			V tJoint = _toTracking[qJointID];

			Vector3 rawPos = GetTrackingJointPosition(tJoint);
			Quaternion rawRot = GetTrackingJointRotation(tJoint);
            _skeleton.GetJoint(qJointID)._state = GetTrackingJointState(tJoint);

            Transform t = _skeleton.GetJoint(qJointID).GetTransform();
            if (IsTrackingDataWorldSpace())
            {
                t.position = rawPos;
                t.rotation = rawRot;
            }
            else
            {
                t.localPosition = rawPos;
                t.localRotation = rawRot;
            }
		}

		protected virtual void UpdateBoneRotation(QuickJointType qJointID) {
            Transform uBone = _animator.GetBoneTransform(_toUnity[qJointID]);

			QuickJoint srcJoint = _skeleton.GetJoint(qJointID);

			uBone.rotation = _calibrationRotation * srcJoint.GetTransform().rotation * _initialJointRotations[qJointID];
		}

		protected virtual void UpdateRootPosition() {
			transform.Translate(GetRootDisplacement(), Space.Self);
		}

		#endregion

		#region DEBUG

		protected virtual void OnDrawGizmos() {
            if (!_skeleton) return;

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers) r.enabled = (_debugMode != DebugMode.OnlySkeleton);

            if (_debugMode == DebugMode.None) return;

            Transform tBase = _skeleton.GetJoint(QuickJointType.SPINE_BASE).GetTransform();
            Vector3 tmpPos = tBase.position;
            Quaternion tmpRot = tBase.rotation;

            tBase.Rotate(transform.up, Vector3.SignedAngle(ComputeTorsoForward(), transform.forward, transform.up), Space.World);
            tBase.position = _animator.GetBoneTransform(HumanBodyBones.Hips).position;

            _skeleton.UpdateDebug();

            tBase.rotation = tmpRot;
            tBase.position = tmpPos;
        }

        #endregion

    }
	
}
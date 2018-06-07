using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {
	
	public class QuickJointsSkeleton : MonoBehaviour {
		
		#region PUBLIC PARAMETERS
		
		public Color _debugJointTrackedColor = Color.green;
		public Color _debugJointInferredColor = Color.yellow;
		public Color _debugJointUntrackedColor = Color.red;
		
		public Color _debugBoneColor = Color.grey;
		
		#endregion
		
		#region PROTECTED PARAMETERS
		
		protected Dictionary<QuickJointType, QuickJoint> _joints = new Dictionary<QuickJointType, QuickJoint>();
		
		protected Dictionary<QuickJointType, List<QuickJointType>> _childJoints = new Dictionary<QuickJointType, List<QuickJointType>>();
		protected Dictionary<QuickJointType, QuickJointType> _parentJoint = new Dictionary<QuickJointType, QuickJointType>();
		
		#endregion
		
		#region CREATION AND DESTRUCTION
		
		protected virtual void Awake() {
			InitParentBones();
			InitChildBones();
			CreateJoint(QuickJointType.SPINE_BASE, transform);
		}
		
		protected virtual void InitParentBones() {
			InitParentBonesSpine();
			InitParentBonesLeftArm();
			InitParentBonesRightArm();
			InitParentBonesLeftLeg();
			InitParentBonesRightLeg();
			InitParentBonesLeftHand();
			InitParentBonesRightHand();
		}
		
		protected virtual void InitParentBonesSpine() {
			//SPINE BONES
			_parentJoint[QuickJointType.SPINE_MID] = QuickJointType.SPINE_BASE;
            _parentJoint[QuickJointType.CHEST] = QuickJointType.SPINE_MID;
			_parentJoint[QuickJointType.NECK] = QuickJointType.CHEST;
			_parentJoint[QuickJointType.HEAD] = QuickJointType.NECK;
            _parentJoint[QuickJointType.EYE_LEFT] = QuickJointType.HEAD;
            _parentJoint[QuickJointType.EYE_RIGHT] = QuickJointType.HEAD;
        }
		
		protected virtual void InitParentBonesLeftArm() {
			//LEFT ARM
			_parentJoint[QuickJointType.CLAVICLE_LEFT] = QuickJointType.NECK;
			_parentJoint[QuickJointType.SHOULDER_LEFT] = QuickJointType.CLAVICLE_LEFT;
			_parentJoint[QuickJointType.ELBOW_LEFT] = QuickJointType.SHOULDER_LEFT;
			_parentJoint[QuickJointType.HAND_LEFT] = QuickJointType.ELBOW_LEFT;
		}
		
		protected virtual void InitParentBonesRightArm() {
			//RIGHT ARM
			_parentJoint[QuickJointType.CLAVICLE_RIGHT] = QuickJointType.NECK;
			_parentJoint[QuickJointType.SHOULDER_RIGHT] = QuickJointType.CLAVICLE_RIGHT;
			_parentJoint[QuickJointType.ELBOW_RIGHT] = QuickJointType.SHOULDER_RIGHT;
			_parentJoint[QuickJointType.HAND_RIGHT] = QuickJointType.ELBOW_RIGHT;
		}
		
		protected virtual void InitParentBonesLeftLeg() {
			//LEFT LEG
			_parentJoint[QuickJointType.HIP_LEFT] = QuickJointType.SPINE_BASE;
			_parentJoint[QuickJointType.KNEE_LEFT] = QuickJointType.HIP_LEFT;
			_parentJoint[QuickJointType.ANKLE_LEFT] = QuickJointType.KNEE_LEFT;
			_parentJoint[QuickJointType.FOOT_LEFT] = QuickJointType.ANKLE_LEFT;
		}
		
		protected virtual void InitParentBonesRightLeg() {
			//RIGHT LEG
			_parentJoint[QuickJointType.HIP_RIGHT] = QuickJointType.SPINE_BASE;
			_parentJoint[QuickJointType.KNEE_RIGHT] = QuickJointType.HIP_RIGHT;
			_parentJoint[QuickJointType.ANKLE_RIGHT] = QuickJointType.KNEE_RIGHT;
			_parentJoint[QuickJointType.FOOT_RIGHT] = QuickJointType.ANKLE_RIGHT;
		}
		
		protected virtual void InitParentBonesLeftHand() {
			_parentJoint[QuickJointType.THUMB1_LEFT] = QuickJointType.HAND_LEFT;
			_parentJoint[QuickJointType.THUMB2_LEFT] = QuickJointType.THUMB1_LEFT;
			_parentJoint[QuickJointType.THUMB3_LEFT] = QuickJointType.THUMB2_LEFT;
			
			_parentJoint[QuickJointType.INDEX1_LEFT] = QuickJointType.HAND_LEFT;
			_parentJoint[QuickJointType.INDEX2_LEFT] = QuickJointType.INDEX1_LEFT;
			_parentJoint[QuickJointType.INDEX3_LEFT] = QuickJointType.INDEX2_LEFT;
			
			_parentJoint[QuickJointType.MIDDLE1_LEFT] = QuickJointType.HAND_LEFT;
			_parentJoint[QuickJointType.MIDDLE2_LEFT] = QuickJointType.MIDDLE1_LEFT;
			_parentJoint[QuickJointType.MIDDLE3_LEFT] = QuickJointType.MIDDLE2_LEFT;
			
			_parentJoint[QuickJointType.RING1_LEFT] = QuickJointType.HAND_LEFT;
			_parentJoint[QuickJointType.RING2_LEFT] = QuickJointType.RING1_LEFT;
			_parentJoint[QuickJointType.RING3_LEFT] = QuickJointType.RING2_LEFT;
			
			_parentJoint[QuickJointType.LITTLE1_LEFT] = QuickJointType.HAND_LEFT;
			_parentJoint[QuickJointType.LITTLE2_LEFT] = QuickJointType.LITTLE1_LEFT;
			_parentJoint[QuickJointType.LITTLE3_LEFT] = QuickJointType.LITTLE2_LEFT;
		}
		
		protected virtual void InitParentBonesRightHand() {
			_parentJoint[QuickJointType.THUMB1_RIGHT] = QuickJointType.HAND_RIGHT;
			_parentJoint[QuickJointType.THUMB2_RIGHT] = QuickJointType.THUMB1_RIGHT;
			_parentJoint[QuickJointType.THUMB3_RIGHT] = QuickJointType.THUMB2_RIGHT;
			
			_parentJoint[QuickJointType.INDEX1_RIGHT] = QuickJointType.HAND_RIGHT;
			_parentJoint[QuickJointType.INDEX2_RIGHT] = QuickJointType.INDEX1_RIGHT;
			_parentJoint[QuickJointType.INDEX3_RIGHT] = QuickJointType.INDEX2_RIGHT;
			
			_parentJoint[QuickJointType.MIDDLE1_RIGHT] = QuickJointType.HAND_RIGHT;
			_parentJoint[QuickJointType.MIDDLE2_RIGHT] = QuickJointType.MIDDLE1_RIGHT;
			_parentJoint[QuickJointType.MIDDLE3_RIGHT] = QuickJointType.MIDDLE2_RIGHT;
			
			_parentJoint[QuickJointType.RING1_RIGHT] = QuickJointType.HAND_RIGHT;
			_parentJoint[QuickJointType.RING2_RIGHT] = QuickJointType.RING1_RIGHT;
			_parentJoint[QuickJointType.RING3_RIGHT] = QuickJointType.RING2_RIGHT;
			
			_parentJoint[QuickJointType.LITTLE1_RIGHT] = QuickJointType.HAND_RIGHT;
			_parentJoint[QuickJointType.LITTLE2_RIGHT] = QuickJointType.LITTLE1_RIGHT;
			_parentJoint[QuickJointType.LITTLE3_RIGHT] = QuickJointType.LITTLE2_RIGHT;
		}
		
		protected virtual void InitChildBones() {
			foreach (var pair in _parentJoint) {
				if (!_childJoints.ContainsKey(pair.Value)) _childJoints[pair.Value] = new List<QuickJointType>();
				_childJoints[pair.Value].Add(pair.Key);
			}
		}
		
		protected virtual void CreateJoint(QuickJointType qJointID, Transform parent) {
			Transform t = new GameObject(qJointID.ToString()).transform;
			t.parent = parent;
			t.ResetTransformation();
			
			List<QuickJointType> childs = GetChilds(qJointID);
			foreach (QuickJointType c in childs) CreateJoint(c, t);
			QuickJoint q = new QuickJoint(t);
			_joints[qJointID] = q;
		}
		
		#endregion
		
		#region GET AND SET
		
		public List<QuickJointType> GetChilds(QuickJointType jointID) {
			if (!_childJoints.ContainsKey(jointID)) return new List<QuickJointType>();
			return _childJoints[jointID];
		}

        public QuickJoint GetParent(QuickJointType jointID)
        {
            return GetJoint(GetParentID(jointID));
        }

        public QuickJointType GetParentID(QuickJointType jointID)
        {
            if (jointID == QuickJointType.SPINE_BASE) return QuickJointType.SPINE_BASE;

            QuickJointType qParentID = _parentJoint[jointID];
            if (GetJoint(qParentID).GetTransform().localPosition != Vector3.zero)
            {
                return qParentID;
            }

            return GetParentID(qParentID);
        }
		
		public virtual QuickJoint GetJoint(QuickJointType jointID) {
			return _joints[jointID];
		}
		
		protected virtual Color GetColorFromState(QuickJoint.State state) {
			if (state == QuickJoint.State.TRACKED) return _debugJointTrackedColor;
			if (state == QuickJoint.State.INFERRED) return _debugJointInferredColor;
			return _debugJointUntrackedColor;
		}
		
		public virtual void SetFilter(QuickJointType qJointID, DoubleExponentialFilterVector3 filter) {
			GetJoint(qJointID).SetSmoothPositionMethod(filter);
		}

        public virtual Vector3 GetForward()
        {
            Vector3 rShoulderPos = GetJoint(QuickJointType.SHOULDER_RIGHT).GetTransform().position;
            Vector3 lShoulderPos = GetJoint(QuickJointType.SHOULDER_LEFT).GetTransform().position;
            Vector3 hipsPos = GetJoint(QuickJointType.SPINE_BASE).GetTransform().position;

            Vector3 u = rShoulderPos - hipsPos;
            Vector3 w = lShoulderPos - hipsPos;

            return Vector3.Cross(u, w).normalized;
        }
		
		#endregion
		
		#region UPDATE
		
		public virtual void Scale(QuickJointsSkeleton skel) {
			List<QuickJointType> rootChilds = GetChilds(QuickJointType.SPINE_BASE);
			foreach (QuickJointType child in rootChilds) Scale(child, skel);
		}
		
		public virtual void Scale(QuickJointType qJointID, QuickJointsSkeleton skel) {
			Transform srcTransform = GetJoint(qJointID).GetTransform();
			Transform dstTransform = skel.GetJoint(qJointID).GetTransform();
			
			Vector3 boneDir = srcTransform.localPosition.normalized;
			if (boneDir == Vector3.zero) {
				//If the boneDir is zero, it means that the tracking does not have a mapping for this joint. 
				//So we use the dst bone dir. 
				boneDir = dstTransform.localPosition.normalized;
			}
			float dstBoneLength = dstTransform.localPosition.magnitude;
			srcTransform.localPosition = boneDir * dstBoneLength;
			
			List<QuickJointType> childs = GetChilds(qJointID);
			foreach (QuickJointType child in childs) Scale(child, skel);
		}

        #endregion

        #region DEBUG

        public virtual void UpdateDebug()
        {
            foreach (QuickJointType qJointID in QuickJoint.GetAllTypes())
            {
                UpdateDebug(qJointID);
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(GetJoint(QuickJointType.SPINE_BASE).GetTransform().position, GetForward());
        }

        protected virtual void UpdateDebug(QuickJointType qJoint)
        {
            QuickJoint q = GetJoint(qJoint);
            Transform t = q.GetTransform();

            //Draw the bone as a cube. 
            Gizmos.color = GetColorFromState(q._state);
            Gizmos.DrawCube(t.position, Vector3.one * 0.025f);

            //Draw the coordinate system of the bone. 
            float axisLength = 0.05f;
            Debug.DrawRay(t.position, t.forward * axisLength, Color.blue);
            Debug.DrawRay(t.position, t.right * axisLength, Color.red);
            Debug.DrawRay(t.position, t.up * axisLength, Color.green);

            List<QuickJointType> childs = GetChilds(qJoint);
            foreach (QuickJointType c in childs)
            {
                QuickJoint tChild = GetJoint(c);
                Gizmos.color = _debugBoneColor;
                Gizmos.DrawLine(q.GetTransform().position, tChild.GetTransform().position);
            }
        }

        //		protected virtual void UpdateDebugAverage() {
        //			UpdateDebugAverage(Kinect.JointType.SpineBase);
        //		}
        //			
        //		protected virtual void UpdateDebugAverage(Kinect.JointType qJoint) {
        //			QuickJoint q = GetJoint(qJoint);
        //			Gizmos.color = Color.red;
        //			Gizmos.DrawCube(q.GetAccumulatedAveragePosition(), Vector3.one * 0.025f);
        //			List<Kinect.JointType> childs = _jointsHierarchy.GetChilds(qJoint);
        //			foreach (Kinect.JointType c in childs) {
        //				QuickJoint tChild = GetJoint(c);
        //				Gizmos.color = _debugBoneColor;
        //				Gizmos.DrawLine(q.GetAccumulatedAveragePosition(), tChild.GetAccumulatedAveragePosition());
        //				UpdateDebugAverage(c);
        //			}
        //		}

        #endregion

    }
	
}

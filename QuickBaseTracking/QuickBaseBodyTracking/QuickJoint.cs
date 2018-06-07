using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace QuickVR {

	public enum QuickJointType {
		//SPINE JOINTS
		SPINE_BASE = 0, 
		SPINE_MID, 
        CHEST,
		NECK, 
		HEAD,
        EYE_LEFT,
        EYE_RIGHT,

        //LEFT ARM
        CLAVICLE_LEFT, 
		SHOULDER_LEFT, 
		ELBOW_LEFT, 
		HAND_LEFT, 
		
		//RIGHT ARM
		CLAVICLE_RIGHT, 
		SHOULDER_RIGHT, 
		ELBOW_RIGHT, 
		HAND_RIGHT, 
		
		//LEFT LEG
		HIP_LEFT, 
		KNEE_LEFT, 
		ANKLE_LEFT, 
		FOOT_LEFT, 
		
		//RIGHT LEG
		HIP_RIGHT, 
		KNEE_RIGHT, 
		ANKLE_RIGHT, 
		FOOT_RIGHT, 
		
		//LEFT HAND FINGERS
		THUMB1_LEFT, 
		THUMB2_LEFT, 
		THUMB3_LEFT, 
		INDEX1_LEFT, 
		INDEX2_LEFT, 
		INDEX3_LEFT,
		MIDDLE1_LEFT, 
		MIDDLE2_LEFT, 
		MIDDLE3_LEFT, 
		RING1_LEFT, 
		RING2_LEFT, 
		RING3_LEFT, 
		LITTLE1_LEFT,
		LITTLE2_LEFT,
		LITTLE3_LEFT,

		//RIGHT HAND FINGERS
		THUMB1_RIGHT, 
		THUMB2_RIGHT, 
		THUMB3_RIGHT, 
		INDEX1_RIGHT, 
		INDEX2_RIGHT, 
		INDEX3_RIGHT,
		MIDDLE1_RIGHT, 
		MIDDLE2_RIGHT, 
		MIDDLE3_RIGHT, 
		RING1_RIGHT, 
		RING2_RIGHT, 
		RING3_RIGHT, 
		LITTLE1_RIGHT,
		LITTLE2_RIGHT,
		LITTLE3_RIGHT,
	}

	public class QuickJoint {

		#region PUBLIC PARAMETERS

		public enum State {
			TRACKED,
			INFERRED,
			UNTRACKED,
		};
		public State _state = State.TRACKED;

		#endregion
		
		#region PROTECTED PARAMETERS

		protected Transform _transform = null;
		protected AccumulatedAverageVector3 _accumulatedAveragePosition = null;
		protected DoubleExponentialFilterVector3 _smoothPositionMethod = null;

		protected Quaternion _initialRotation = Quaternion.identity;
		protected Quaternion _initialLocalRotation = Quaternion.identity;
		protected List<Vector3> _historyPosition = new List<Vector3>();
		protected int _untrackedStateCounter = 0;	//Frames in a row the joint must be tracked in order to determine it is really tracked

		protected bool _dirtyInitialRotation = true;
		protected bool _dirtyInitialLocalRotation = true;

        protected static List<QuickJointType> _allTypes = null;

		#endregion
		
		#region CREATION AND DESTRUCTION
		
		public QuickJoint(Transform t) {
			_transform = t;
			_accumulatedAveragePosition = new AccumulatedAverageVector3();
		}
		
		#endregion
		
		#region GET AND SET

        public static List<QuickJointType> GetAllTypes()
        {
            if (_allTypes == null) _allTypes = QuickUtils.GetEnumValues<QuickJointType>();

            return _allTypes;
        }

		public virtual void SetSmoothPositionMethod(DoubleExponentialFilterVector3 method) {
			_smoothPositionMethod = method;
		}

		public virtual Transform GetTransform() {
			return _transform;
		}

		public virtual Vector3 GetAccumulatedAveragePosition() {
			return _accumulatedAveragePosition.GetCurrentValue();
		}

		public virtual Vector3 GetVelocity() {
			if (_historyPosition.Count < 2) return Vector3.zero;
			return (_historyPosition[1] - _historyPosition[0]);
		}

        public virtual void SetData(Vector3 rawPosition, Quaternion rawRotation, State state, bool isWorldSpace)
        {
            UpdateState(state);

        }

        #endregion

        #region UPDATE

        public virtual void Update(Vector3 rawPosition, Quaternion rawRotation, State state, bool isWorldSpace) {
			UpdateState(state);
			//UpdatePosition(rawPosition, isWorldSpace);
			//UpdateRotation(rawRotation, isWorldSpace);
		}

		protected virtual void UpdateState(State newState) {
			if (_state == State.TRACKED) {
				if (newState != State.TRACKED) {
					_state = newState;
					_untrackedStateCounter = 15;
				}
			}
			else {
				if (newState == State.TRACKED) {
					_untrackedStateCounter--;
					if (_untrackedStateCounter == 0) _state = State.TRACKED;
				}
				else _untrackedStateCounter = 15;
			}
		}

		protected virtual void UpdatePosition(Vector3 rawPosition, bool isWorldSpace) {
			Vector3 pos = rawPosition;
            if (_smoothPositionMethod != null) pos = _smoothPositionMethod.Filter(rawPosition);
            if (Vector3.Distance(pos, _accumulatedAveragePosition.GetCurrentValue()) > 0.15f)
            {
                _accumulatedAveragePosition.Reset();
            }
            _accumulatedAveragePosition.AddSample(pos);

            if (isWorldSpace) _transform.position = pos;
			else _transform.localPosition = pos;

			UpdateLastPosition(_transform.position);
		}

		protected virtual void UpdateRotation(Quaternion rawRotation, bool isWorldSpace) {
			if (isWorldSpace) _transform.rotation = rawRotation;
			else _transform.localRotation = rawRotation;
		}

		protected virtual void UpdateLastPosition(Vector3 pos) {
			if (_historyPosition.Count < 2) _historyPosition.Add(pos);
			else {
				_historyPosition[0] = _historyPosition[1];
				_historyPosition[1] = pos;
			}
		}
		
		#endregion
	}

}

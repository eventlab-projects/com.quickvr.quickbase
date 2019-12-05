using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	[System.Serializable]
    public abstract class QuickBaseTrackingManager : MonoBehaviour {

		#region PUBLIC PARAMETERS

		public int _priority = 0;

		#endregion

		#region PROTECTED PARAMETERS

		[SerializeField, HideInInspector] 
        protected Animator _animator = null;	

		[SerializeField, HideInInspector] 
        protected bool _isCalibrated = false;

		#endregion

		#region ABSTRACT

		public abstract void UpdateTracking();
        protected abstract int GetDefaultPriority();

		#endregion

		#region CREATION AND DESTRUCTION

        protected virtual void Reset()
        {
            _priority = GetDefaultPriority();
            _animator = GetComponent<Animator>();
        }

        protected virtual void Awake() {
            _animator = GetComponent<Animator>();
            if (_animator) _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		}

		protected virtual void Start() {
            SkinnedMeshRenderer[] renderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer r in renderers)
            {
                r.updateWhenOffscreen = true;
            }
            QuickSingletonManager.GetInstance<QuickVRManager>().AddTrackingManager(_priority, this);
		}

        protected virtual void OnEnable() {
			QuickVRManager.OnPreUpdateTracking += UpdateInput;
		}

		protected virtual void OnDisable() {
			QuickVRManager.OnPreUpdateTracking -= UpdateInput;
		}

		#endregion

		#region GET AND SET

		public virtual bool IsHumanoid() {
			return _animator && _animator.isHuman;
		}

		public virtual void Calibrate() {
			_isCalibrated = true;
		}

		public virtual bool IsCalibrated() {
			return _isCalibrated;
		}

        public virtual void SetCalibrated(bool isCalibrated)
        {
            _isCalibrated = isCalibrated;
        }

		#endregion

		#region UPDATE

		protected virtual void UpdateInput() {
			if (InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CALIBRATE)) _isCalibrated = false;
		}

		#endregion

	}

}
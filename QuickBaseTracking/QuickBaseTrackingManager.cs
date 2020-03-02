using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	[System.Serializable]
    public abstract class QuickBaseTrackingManager : MonoBehaviour {

		#region PROTECTED PARAMETERS

		[SerializeField, HideInInspector] 
        protected Animator _animator = null;	

        protected QuickVRManager _vrManager = null;

		#endregion

		#region ABSTRACT

		public abstract void UpdateTracking();

		#endregion

		#region CREATION AND DESTRUCTION

        protected virtual void Reset()
        {
            _animator = GetComponent<Animator>();
        }

        protected virtual void Awake() {
            _animator = GetComponent<Animator>();
            _vrManager = QuickSingletonManager.GetInstance<QuickVRManager>();
            if (_animator) _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		}

		protected virtual void Start() {
            SkinnedMeshRenderer[] renderers = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer r in renderers)
            {
                r.updateWhenOffscreen = true;
            }
		}

        #endregion

		#region GET AND SET

		public virtual bool IsHumanoid() {
			return _animator && _animator.isHuman;
		}

        public abstract void Calibrate();

		#endregion

	}

}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	[System.Serializable]
    public abstract class QuickBaseTrackingManager : MonoBehaviour {

        #region PROTECTED PARAMETERS

        [SerializeField, HideInInspector]
        protected Animator _animator = null;
        
        protected QuickVRManager _vrManager
        {
            get
            {
                return QuickSingletonManager.GetInstance<QuickVRManager>();
            }
        }

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            InitAnimator();
        }

        protected virtual void OnEnable()
        {
            SkinnedMeshRenderer[] smRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer r in smRenderers)
            {
                r.updateWhenOffscreen = true;
            }
        }

        public virtual void InitAnimator()
        {
            if (!_animator)
            {
                _animator = GetComponent<Animator>();
                _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
        }

        #endregion

        #region GET AND SET

        public abstract void Calibrate();

        #endregion

        #region UPDATE

        public virtual void UpdateTracking()
        {

        }

        #endregion

    }

}
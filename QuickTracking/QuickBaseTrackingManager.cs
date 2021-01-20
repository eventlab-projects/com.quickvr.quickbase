using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	[System.Serializable]
    public abstract class QuickBaseTrackingManager : MonoBehaviour {

        #region PROTECTED PARAMETERS

        protected Animator _animator
        {
            get
            {
                if (!m_animator)
                {
                    m_animator = GetComponent<Animator>();
                    m_animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                }

                return m_animator;
            }
        }

        protected QuickVRManager _vrManager
        {
            get
            {
                return QuickSingletonManager.GetInstance<QuickVRManager>();
            }
        }

        #endregion

        #region PRIVATE ATTRIBUTES

        Animator m_animator = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _animator.CreateMissingBones();
        }

        protected virtual void OnEnable()
        {
            SkinnedMeshRenderer[] smRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer r in smRenderers)
            {
                r.updateWhenOffscreen = true;
            }

            RegisterTrackingManager();
        }

        protected abstract void RegisterTrackingManager();

        #endregion

        #region GET AND SET

        public abstract void Calibrate();

        #endregion

        #region UPDATE

        public virtual void UpdateTrackingEarly()
        {

        }

        public virtual void UpdateTrackingLate()
        {

        }

        #endregion

    }

}
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
                if (!m_Animator)
                {
                    m_Animator = GetComponent<Animator>();
                    m_Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                }

                return m_Animator;
            }
        }
        [SerializeField, HideInInspector]
        protected Animator m_Animator = null;
        
        protected QuickVRManager _vrManager
        {
            get
            {
                return QuickSingletonManager.GetInstance<QuickVRManager>();
            }
        }

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            SkinnedMeshRenderer[] smRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer r in smRenderers)
            {
                r.updateWhenOffscreen = true;
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
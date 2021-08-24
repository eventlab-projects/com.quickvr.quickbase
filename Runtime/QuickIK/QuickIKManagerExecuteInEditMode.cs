using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{
    [ExecuteInEditMode]
    public class QuickIKManagerExecuteInEditMode : MonoBehaviour
    {

        #region PROTECTED ATTRIBUTES

        public QuickIKManager _ikManager
        {
            get
            {
                if (!m_IKManager)
                {
                    m_IKManager = GetComponent<QuickIKManager>();
                }

                return m_IKManager;
            }
        }
        protected QuickIKManager m_IKManager = null;

        #endregion

        #region EVENTS

        public delegate void OnAddQuickIKManagerCallback(QuickIKManagerExecuteInEditMode ikManager);
        public delegate void OnRemoveQuickIKManagerCallback(QuickIKManagerExecuteInEditMode ikManager);

        public static OnAddQuickIKManagerCallback OnIKManagerAdded;
        public static OnRemoveQuickIKManagerCallback OnIKManagerRemoved;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            if (OnIKManagerAdded != null)
            {
                OnIKManagerAdded(this);
            }
        }

        protected virtual void OnDisable()
        {
            if (OnIKManagerRemoved != null)
            {
                OnIKManagerRemoved(this);
            }
        }

        #endregion

        #region UPDATE

        protected virtual void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                _ikManager.UpdateTracking();
            }
        }

        #endregion

    }
}

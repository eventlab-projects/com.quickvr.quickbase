using UnityEngine;
using System.Collections;

namespace QuickVR
{

    [System.Serializable]
    public abstract class QuickMonoBehaviour : MonoBehaviour
    {

        #region PROTECTED PARAMETERS

        [SerializeField, HideInInspector]
        protected bool _initialized = false;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Reset()
        {
            if (!_initialized) Init();
        }

        protected virtual void Awake()
        {
            if (!_initialized) Init();
        }

        public virtual void Init()
        {
            _initialized = true;
        }

        #endregion

        #region GET AND SET

        public virtual bool IsInitialized()
        {
            return _initialized;
        }

        #endregion
    }

}

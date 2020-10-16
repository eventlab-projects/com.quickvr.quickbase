using QuickVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VRKeys;

namespace QuickVR
{
    public class QuickVRKeyboard : MonoBehaviour
    {

        #region PROTECTED ATTRIBUTES

        [SerializeField, HideInInspector]
        protected Keyboard _vrKeyboard = null;

        protected QuickVRManager _vrManager = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            if (!_vrKeyboard)
            {
                _vrKeyboard = Instantiate<Keyboard>(Resources.Load<Keyboard>("Prefabs/pf_QuickVRKeyboard"));
                _vrKeyboard.transform.parent = transform;
                _vrKeyboard.transform.ResetTransformation();
            }

            _vrManager = QuickSingletonManager.GetInstance<QuickVRManager>();
        }

        #endregion

        #region GET AND SET

        public virtual void Enable(bool enable)
        {
            if (enable)
            {
                Animator animator = _vrManager.GetAnimatorTarget();
                if (animator)
                {
                    Transform tHead = animator.GetBoneTransform(HumanBodyBones.Head);
                    _vrKeyboard.transform.rotation = animator.transform.rotation;
                    _vrKeyboard.transform.position = tHead.position;
                    _vrKeyboard.transform.position += animator.transform.forward * 2;
                }
                _vrKeyboard.Enable();
            }
            else
            {
                _vrKeyboard.Disable();
            }
        }

        public virtual bool IsEnabled()
        {
            return !_vrKeyboard.disabled;
        }

        #endregion

    }
}



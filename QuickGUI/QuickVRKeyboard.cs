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

        protected virtual IEnumerator Start()
        {
            while (!_vrKeyboard.IsInitialized())
            {
                yield return null;
            }

            foreach (Key k in _vrKeyboard.GetKeys())
            {
                QuickUIButton button = k.GetOrCreateComponent<QuickUIButton>();
                button.OnDown += k.DoAction;

                RectTransform t = k.GetComponent<RectTransform>();
                BoxCollider collider = k.GetOrCreateComponent<BoxCollider>();
                collider.size = new Vector3(t.rect.width, t.rect.height, 0);
                collider.center = new Vector3(t.rect.width / 2, -t.rect.height / 2, 0);
                
                Rigidbody rBody = k.GetOrCreateComponent<Rigidbody>();
                rBody.isKinematic = true;
            }
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



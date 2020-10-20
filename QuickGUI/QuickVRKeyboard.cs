﻿using QuickVR;
using System;
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

        #region ACTIONS

        public Action OnSubmit;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            if (!_vrKeyboard)
            {
                _vrKeyboard = Instantiate<Keyboard>(Resources.Load<Keyboard>("Prefabs/pf_QuickVRKeyboard"));
                _vrKeyboard.transform.SetParent(transform, false);
                _vrKeyboard.transform.ResetTransformation();
            }
            _vrKeyboard.OnSubmit.AddListener(ActionSubmit);

            _vrManager = QuickSingletonManager.GetInstance<QuickVRManager>();
        }

        protected virtual void Start()
        {
            foreach (Key k in _vrKeyboard.GetKeys())
            {
                QuickUIButton button = k.GetOrCreateComponent<QuickUIButton>();
                button.OnDown += k.DoAction;
            }

            Enable(false);
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
                
            }

            _vrKeyboard.Enable(enable);
        }

        public virtual bool IsEnabled()
        {
            return _vrKeyboard.IsEnabled();
        }

        public virtual string GetText()
        {
            return _vrKeyboard.GetText();
        }

        protected virtual void ActionSubmit(string text)
        {
            if (OnSubmit != null)
            {
                OnSubmit();
            }
        }

        #endregion

    }
}



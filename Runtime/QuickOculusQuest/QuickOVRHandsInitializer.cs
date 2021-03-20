using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace QuickVR
{

    public class QuickOVRHandsInitializer : QuickBaseTrackingManager
    {

        #region PUBLIC ATTRIBUTES

        public bool _debug = false;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickOVRHand _leftHand = null;
        protected QuickOVRHand _rightHand = null;
        protected QuickUnityVR _hTracking
        {
            get
            {
                if (!m_hTracking)
                {
                    m_hTracking = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorSource().GetComponent<QuickUnityVR>();
                }

                return m_hTracking;
            }
        }
        protected QuickUnityVR m_hTracking = null;
        
        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
#if UNITY_ANDROID
            QuickVRManager.OnSourceAnimatorSet += OnSourceAnimatorSet;
#endif
        }

        protected static void OnSourceAnimatorSet(Animator animator)
        {
            animator.GetOrCreateComponent<QuickOVRHandsInitializer>();
            QuickSingletonManager.GetInstance<InputManager>().CreateDefaultImplementation<InputManagerOVRHands>();
        }

        protected virtual void Start()
        {
            QuickVRPlayArea vrPlayArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();
            _leftHand = Instantiate<QuickOVRHand>(Resources.Load<QuickOVRHand>("Prefabs/pf_QuickOVRHandLeft"), vrPlayArea.GetVRNode(HumanBodyBones.LeftHand).transform);
            _leftHand.transform.ResetTransformation();

            _rightHand = Instantiate<QuickOVRHand>(Resources.Load<QuickOVRHand>("Prefabs/pf_QuickOVRHandRight"), vrPlayArea.GetVRNode(HumanBodyBones.RightHand).transform);
            _rightHand.transform.ResetTransformation();

            _vrManager.AddHandTrackingSystem(this);
        }

#endregion

        #region GET AND SET

        public override void Calibrate()
        {

        }

        public virtual QuickOVRHand GetOVRHand(bool left)
        {
            return left ? _leftHand : _rightHand;
        }

        #endregion

        #region UPDATE

        public override void UpdateTracking()
        {
            if (_hTracking && _hTracking._handTrackingMode == QuickUnityVR.HandTrackingMode.Hands)
            {
                OVRInput.Update();
                if (_leftHand) _leftHand.UpdateTracking();
                if (_rightHand) _rightHand.UpdateTracking();
            }
        }

        #endregion

    }
}

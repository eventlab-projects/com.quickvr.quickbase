using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace QuickVR
{

    public class QuickOVRHandsInitializer : QuickBaseTrackingManager
    {

        #region PUBLIC ATTRIBUTES

        public bool _debug = true;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickOVRHand _leftHand = null;
        protected QuickOVRHand _rightHand = null;
        protected QuickUnityVR _hTracking
        {
            get
            {
                return QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorSource().GetComponent<QuickUnityVR>();
            }
        }

        private static Dictionary<QuickHumanBodyBones, OVRSkeleton.BoneId> _toOVR = new Dictionary<QuickHumanBodyBones, OVRSkeleton.BoneId>();

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            List<QuickHumanFingers> fingers = QuickHumanTrait.GetHumanFingers();
            foreach (bool isLeft in new bool[]{ true, false}) 
            {
                for (int i = 0; i < fingers.Count; i++)
                {
                    QuickHumanFingers f = fingers[i];
                    string infix = i < 4 ? f.ToString() : "Pinky";
                    List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(f, isLeft);
                    for (int j = 0; j < fingerBones.Count; j++)
                    {
                        string sufix = j < 3 ? (j + 1).ToString() : "Tip";
                        QuickHumanBodyBones b = fingerBones[j];
                        OVRSkeleton.BoneId ovrBoneID = QuickUtils.ParseEnum<OVRSkeleton.BoneId>("Hand_" + infix + sufix);
                        _toOVR[b] = ovrBoneID;
                    }
                }
            }

#if UNITY_ANDROID
            QuickVRManager.OnSourceAnimatorSet += OnSourceAnimatorSet;
#endif
        }

        protected static void OnSourceAnimatorSet()
        {
            QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorSource().GetOrCreateComponent<QuickOVRHandsInitializer>();
        }

        protected virtual void Start()
        {
            QuickVRPlayArea vrPlayArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();
            _leftHand = Instantiate<QuickOVRHand>(Resources.Load<QuickOVRHand>("Prefabs/pf_QuickOVRHandLeft"), vrPlayArea.GetVRNode(HumanBodyBones.LeftHand).transform);
            _leftHand.transform.ResetTransformation();

            _rightHand = Instantiate<QuickOVRHand>(Resources.Load<QuickOVRHand>("Prefabs/pf_QuickOVRHandRight"), vrPlayArea.GetVRNode(HumanBodyBones.RightHand).transform);
            _rightHand.transform.ResetTransformation();
        }

        protected override void RegisterTrackingManager()
        {
            _vrManager.AddHandTrackingSystem(this);
        }
        
#endregion

#region GET AND SET

        public static OVRSkeleton.BoneId ToOVR(QuickHumanBodyBones boneID)
        {
            return _toOVR[boneID];
        }

        public override void Calibrate()
        {

        }

        public virtual QuickOVRHand GetOVRHand(bool left)
        {
            return left ? _leftHand : _rightHand;
        }

#endregion

#region UPDATE

        public override void UpdateTrackingLate()
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

using UnityEngine;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

    public class QuickUnityVRHands : QuickUnityVRBase
    {

        #region CONSTANTS

        protected const string DEFAULT_PF_HAND_LEFT = "Prefabs/_pfVRHandMale_Left";
        protected const string DEFAULT_PF_HAND_RIGHT = "Prefabs/_pfVRHandMale_Right";

        #endregion

        #region PUBLIC PARAMETERS

        public QuickVRHand _pfHandLeft = null;
        public QuickVRHand _pfHandRight = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void CreateVRHands()
        {
            if (!_pfHandLeft) _pfHandLeft = Resources.Load<QuickVRHand>(DEFAULT_PF_HAND_LEFT);
            if (!_pfHandRight) _pfHandRight = Resources.Load<QuickVRHand>(DEFAULT_PF_HAND_RIGHT);

            _vrHandLeft = CreateVRHand(_pfHandLeft, "__vrHandLeft__");
            _vrHandRight = CreateVRHand(_pfHandRight, "__vrHandRight__");

            base.CreateVRHands();
        }

        protected virtual QuickVRHand CreateVRHand(QuickVRHand handModel, string rootName)
        {
            Transform handRoot = transform.CreateChild(rootName);
            QuickVRHand hand = Instantiate<QuickVRHand>(handModel);
            hand.transform.rotation *= transform.rotation;

            handRoot.position = hand._handBone.position;
            hand.transform.parent = handRoot;

            return hand;
        }

        protected override void Start()
        {
            base.Start();

            _vrHandLeft.gameObject.SetActive(false);
            _vrHandRight.gameObject.SetActive(false);
        }

        #endregion

        #region GET AND SET

        public override Vector3 GetEyeCenterPosition()
        {
            float yOffset = GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject().transform.position.y - _vrNodesOrigin.position.y;
            return transform.position + transform.up * yOffset;
        }

        protected override Vector3 GetDisplacement()
        {
            return GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject().GetDisplacement();
        }

        protected override float GetRotationOffset()
        {
            QuickVRNode hmdNode = GetQuickVRNode(QuickVRNode.Type.Head);

            Vector3 currentForward = Vector3.ProjectOnPlane(_vrNodesOrigin.forward, _vrNodesOrigin.up);
            Vector3 targetForward = Vector3.ProjectOnPlane(hmdNode.transform.forward, _vrNodesOrigin.up);

            return Vector3.SignedAngle(currentForward, targetForward, _vrNodesOrigin.up);
        }

        protected override void CalibrateVRNodeHead(QuickVRNode node)
        {
            base.CalibrateVRNodeHead(node);

            QuickTrackedObject tObject = node.GetTrackedObject();

            SettingsBase.HeightMode hMode = SettingsBase.GetHeightMode();
            float yOffset = 0.0f;
            if (hMode == SettingsBase.HeightMode.FromTrackingSystem)
            {
                yOffset = _camera.transform.localPosition.y;
            }
            else if (hMode == SettingsBase.HeightMode.FromSubject)
            {
                float sf = HUMAN_HEADS_TALL_EYES / HUMAN_HEADS_TALL;
                yOffset = SettingsBase.GetSubjectHeight() * sf;
            }

            _vrNodesOrigin.position = tObject.transform.position - transform.up * yOffset;
        }

        #endregion

        #region UPDATE

        protected override void UpdateTransformRoot()
        {
            base.UpdateTransformRoot();

            CalibrateCameraForward();
        }

        protected override void UpdateTransformNodes()
        {
            UpdateVRHand(QuickVRNode.Type.LeftHand);
            UpdateVRHand(QuickVRNode.Type.RightHand);
        }

        protected virtual void UpdateVRHand(QuickVRNode.Type nType)
        {
            QuickVRNode node = GetQuickVRNode(nType);
            if (node.IsTracked())
            {
                Transform vrHandRoot = GetVRHand(nType).transform.parent;

                QuickTrackedObject tObject = node.GetTrackedObject();
                Vector3 posOffset = tObject.transform.position - _vrNodesOrigin.position;
                vrHandRoot.position = transform.position + ToAvatarSpace(posOffset);
                vrHandRoot.rotation = ToAvatarSpace(tObject.transform.rotation);
            }
        }

        protected override void OnLeftHandConnected()
        {
            _vrHandLeft.gameObject.SetActive(true);

            base.OnLeftHandConnected();
        }

        protected override void OnRightHandConnected()
        {
            _vrHandRight.gameObject.SetActive(true);

            base.OnRightHandConnected();
        }

        #endregion

    }

}

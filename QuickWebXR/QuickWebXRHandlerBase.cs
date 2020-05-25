using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using WebXR;

namespace QuickVR
{

    public class QuickWebXRHandlerBase : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public HumanBodyBones _role = HumanBodyBones.LastBone;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickVRPlayArea _vrPlayArea = null;
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _vrPlayArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();
            Reset();
        }

        protected virtual void Reset()
        {

        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPreUpdateTrackingEarly += UpdateVRNodeRequest;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPreUpdateTrackingEarly -= UpdateVRNodeRequest;
        }

        #endregion

        #region UPDATE

        private void UpdateVRNodeRequest()
        {
            if (WebXRManager.Instance.xrState == WebXRState.ENABLED)
            {
                UpdateVRNode();
            }
        }

        protected virtual void UpdateVRNode()
        {
            UpdateVRNode(_role, transform);
        }

        protected virtual void UpdateVRNode(HumanBodyBones role, Transform t)
        {
            QuickVRNode vrNode = _vrPlayArea.GetVRNode(role);
            vrNode.transform.position = t.position;
            vrNode.transform.rotation = t.rotation;
            vrNode.SetTracked(true);
        }

        #endregion

    }

}



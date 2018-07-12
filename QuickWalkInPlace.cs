using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickWalkInPlace : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public QuickVRNode.Type _targetNode = QuickVRNode.Type.Head;

        public float _thresholdSpeed = 0.05f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickUnityVRBase _hTracking = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _hTracking = GetComponent<QuickUnityVRBase>();
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostUpdateTracking += UpdateTranslation;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostUpdateTracking -= UpdateTranslation;
        }

        #endregion

        #region UPDATE

        protected virtual void UpdateTranslation()
        {
            QuickVRNode node = _hTracking.GetQuickVRNode(_targetNode);
            QuickTrackedObject tObject = _hTracking.GetQuickVRNode(_targetNode).GetTrackedObject();

            if (node.IsTracked())
            {
                float s = Mathf.Abs(tObject.GetVelocity().y);
                //if (s < _thresholdSpeed) s = 0.0f;

                Debug.Log("s = " + s.ToString("f3"));

                transform.Translate(Vector3.forward * s * Time.deltaTime, Space.Self);
            }


            
        }

        #endregion

    }

}

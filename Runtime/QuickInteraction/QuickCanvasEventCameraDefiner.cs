using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    [RequireComponent(typeof(Canvas))]
    public class QuickCanvasEventCameraDefiner : MonoBehaviour
    {

        #region PROTECTED ATTRIBUTES

        protected Canvas _canvas = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            if (!_canvas.worldCamera)
            {
                _canvas.worldCamera = Camera.main;
            }
        }

        #endregion

    }

}



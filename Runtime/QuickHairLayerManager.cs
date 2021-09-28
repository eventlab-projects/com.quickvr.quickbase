using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{
    public class QuickHairLayerManager : MonoBehaviour
    {

        #region PROTECTED ATTRIBUTES

        protected QuickVRManager _vrManager = null;
        protected Animator _animator = null;
        protected Renderer _hairRenderer = null;
                
        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _vrManager = QuickSingletonManager.GetInstance<QuickVRManager>();
            QuickVRCameraController cameraController = QuickSingletonManager.GetInstance<QuickVRCameraController>();
            cameraController._visibleLayers &= ~(1 << (int)LayerMask.NameToLayer("Hair"));

            _animator = GetComponent<Animator>();
            
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            foreach (Renderer r in renderers)
            {
                if (r.name == "Hair")
                {
                    _hairRenderer = r;
                    break;
                }
            }
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostCameraUpdate += UpdateHairVisibility;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostCameraUpdate -= UpdateHairVisibility;
        }

        #endregion

        #region UPDATE

        protected virtual void UpdateHairVisibility()
        {
            if (_hairRenderer)
            {
                _hairRenderer.gameObject.layer = LayerMask.NameToLayer(_animator == _vrManager.GetAnimatorTarget() ? "Hair" : "Default");
            }
        }

        #endregion

    }

}

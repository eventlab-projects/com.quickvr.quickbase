using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace QuickVR
{
    public class QuickVRControllerInteractor : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public XRNode _xrNode = XRNode.LeftHand;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected XRDirectInteractor _directInteractor = null;
        protected QuickXRRayInteractor _rayInteractorTeleport = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Start()
        {
            CreateDirectInteractor();
            CreateRayInteractor();
        }

        protected virtual void CreateDirectInteractor()
        {
            Transform tInteractor = CreateInteractorTransform("__DirectInteractor__");

            //Add the components to be able to catch close objects. 
            SphereCollider collider = tInteractor.gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.2f;
            _directInteractor = tInteractor.gameObject.AddComponent<XRDirectInteractor>();
        }

        protected virtual void CreateRayInteractor()
        {
            Transform tInteractor = CreateInteractorTransform("__RayInteractor__");

            LineRenderer lRenderer = tInteractor.gameObject.AddComponent<LineRenderer>();
            lRenderer.material = Resources.Load<Material>("Materials/QuickDefaultLine");
            lRenderer.numCornerVertices = 4;
            lRenderer.numCapVertices = 4;
            lRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lRenderer.receiveShadows = false;
            lRenderer.sortingOrder = 5;
            
            _rayInteractorTeleport = tInteractor.gameObject.AddComponent<QuickXRRayInteractor>();
            _rayInteractorTeleport._interactionType = QuickXRRayInteractor.InteractionType.Teleport;
            tInteractor.gameObject.AddComponent<XRInteractorLineVisual>();
        }

        protected Transform CreateInteractorTransform(string tName)
        {
            Transform tDirectInteractor = transform.CreateChild(tName);
            XRController controller = tDirectInteractor.gameObject.AddComponent<XRController>();
            controller.enableInputTracking = false;
            controller.controllerNode = _xrNode;

            return tDirectInteractor;
        }

        #endregion

    }
}



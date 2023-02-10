using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

using UnityEngine.InputSystem;

namespace QuickVR
{
    public enum InteractorType
    {
        GrabDirect,
        Grab,
        Teleport,
        UI,
        Generic, 
    }

    public class QuickVRInteractor : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public virtual XRNode _xrNode { get; set; }
        
        #endregion

        #region PROTECTED ATTRIBUTES

        protected Dictionary<InteractorType, XRBaseControllerInteractor> _interactors = new Dictionary<InteractorType, XRBaseControllerInteractor>();

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            CreateInteractors();

            //By default, disable all the interactors
            foreach (var pair in _interactors)
            {
                SetInteractorEnabled(pair.Key, false);
            }
        }

        protected virtual void CreateInteractors()
        {
            //QuickVRInteractionManager interactionManager = QuickSingletonManager.GetInstance<QuickVRInteractionManager>();

            //_interactors[InteractorType.GrabDirect] = CreateInteractor(interactionManager._pfInteractorGrabDirect);
            //_interactors[InteractorType.Grab] = CreateInteractor(interactionManager._pfInteractorGrabRay);
            //_interactors[InteractorType.Teleport] = CreateInteractor(interactionManager._pfInteractorTeleportRay);
            //_interactors[InteractorType.UI] = CreateInteractor(interactionManager._pfInteractorUIRay);

            Transform tInteractor = transform.CreateChild("__InteractorGeneric__");
            _interactors[InteractorType.Generic] = tInteractor.GetOrCreateComponent<XRRayInteractor>();
        }

        #endregion

        #region GET AND SET

        public virtual XRBaseControllerInteractor GetInteractor(InteractorType type)
        {
            _interactors.TryGetValue(type, out XRBaseControllerInteractor result);

            return result;
        }

        public virtual void SetInteractorEnabled(bool enabled)
        {
            SetInteractorEnabled(InteractorType.Generic, enabled);
        }

        public virtual void SetInteractorEnabled(InteractorType type, bool enabled)
        {
            //ActionBasedController interactor = GetInteractor(type);
            //if (interactor && interactor.gameObject.activeSelf != enabled)
            //{
            //    interactor.gameObject.SetActive(enabled);
            //}

            XRBaseControllerInteractor interactor = GetInteractor(type);
            if (interactor && interactor.gameObject.activeSelf != enabled)
            {
                interactor.gameObject.SetActive(enabled);

                //Disable all the interactors
                HashSet<XRBaseControllerInteractor> enabledInteractors = new HashSet<XRBaseControllerInteractor>();
                foreach (var pair in _interactors)
                {
                    if (pair.Value.gameObject.activeSelf)
                    {
                        enabledInteractors.Add(pair.Value);
                    }
                    pair.Value.gameObject.SetActive(false);
                }

                //Reenable the interactors that were enabled at the begining. 
                foreach (XRBaseControllerInteractor tmp in enabledInteractors)
                {
                    tmp.gameObject.SetActive(true);
                }
            }
        }

        public virtual bool IsEnabledInteractor(InteractorType type)
        {
            XRBaseControllerInteractor interactor = GetInteractor(type);
            return interactor ? interactor.gameObject.activeSelf : false;
        }

        #endregion

    }
}



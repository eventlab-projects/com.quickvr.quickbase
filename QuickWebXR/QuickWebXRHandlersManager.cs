using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WebXR;

namespace QuickVR
{
    public class QuickWebXRHandlersManager : MonoBehaviour
    {

        #region PROTECTED ATTRIBUTES

        protected QuickWebXRHandlerHead _handlerHead = null;
        protected QuickWebXRHandlerController _handlerHandLeft = null;
        protected QuickWebXRHandlerController _handlerHandRight = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _handlerHead = CreateHandlerHead();
            _handlerHandLeft = CreateHandlerController(true);
            _handlerHandRight = CreateHandlerController(false);
        }

        protected virtual QuickWebXRHandlerHead CreateHandlerHead()
        {
            QuickWebXRHandlerHead result = GetComponentInChildren<QuickWebXRHandlerHead>();
            if (!result)
            {
                result = transform.CreateChild("Head").GetOrCreateComponent<QuickWebXRHandlerHead>();
            }

            return result;
        }

        protected virtual QuickWebXRHandlerController CreateHandlerController(bool isLeft)
        {
            WebXRController[] controllers = GetComponentsInChildren<WebXRController>();
            WebXRController controller = null;
            for (int i = 0; i < controllers.Length && !controller; i++)
            {
                WebXRController c = controllers[i];
                if (
                    (isLeft && c.hand == WebXRControllerHand.LEFT) ||
                    (!isLeft && c.hand == WebXRControllerHand.RIGHT)
                    )
                {
                    controller = c;
                }
            }

            if (!controller)
            {
                controller = transform.CreateChild(isLeft ? "HandLeft" : "HandRight").gameObject.AddComponent<WebXRController>();
                controller.hand = isLeft ? WebXRControllerHand.LEFT : WebXRControllerHand.RIGHT;
            }

            return controller.GetOrCreateComponent<QuickWebXRHandlerController>();
        }

        #endregion

        #region GET AND SET

        public virtual QuickWebXRHandlerController GetHandlerController(bool isLeft)
        {
            return isLeft ? _handlerHandLeft : _handlerHandRight;
        }

        public virtual QuickWebXRHandlerHead GetHandlerHead()
        {
            return _handlerHead;
        }

        #endregion

    }

}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using System;
using System.Collections.Generic;

namespace QuickVR
{

    // The Cursor is a small point at the centre of the screen.
    // It is used as a visual aid for aiming. The position of the
    // Cursor is either at a default position in space or on the
    // surface of a VRInteractiveItem as determined by the VRRayCaster.
    public class QuickUICursor : MonoBehaviour
    {

        #region PUBLIC PARAMETERS

        public float _DefaultDistance = 5.0f;           // The default distance away from the camera the Cursor is placed.

        public float _RayLength = 500f;                 // How far into the scene the ray is cast.
        public LayerMask _RayCastMask = -1 & ~(1 << 2);	// Layers to exclude from the raycast.
        public bool _drawRay = false;
        public bool _drawCursor = true;

        public string _TriggerVirtualKey = InputManager.DEFAULT_BUTTON_CONTINUE;

        public bool _hideIfNoHit = true;

        public Transform _vrGUICursor = null;

        public enum Role
        {
            Head,
            LeftHand,
            RightHand,
        };

        #endregion

        #region CONSTANTS

        protected const string DEFAULT_RAY_SHADER = "UI/OverOverlay";

        #endregion

        #region PROTECTED PARAMETERS

        protected static Dictionary<Role, QuickUICursor> _vrCursors = new Dictionary<Role, QuickUICursor>();

        protected static Canvas _canvasCursors = null;

        protected Transform _CursorTransform;      // We need to affect the Cursor's transform.

        protected QuickUIInteractiveItem _CurrentInteractible;                //The current interactive item
        protected QuickUIInteractiveItem _LastInteractible;                   //The last interactive item

        protected Vector3 _OriginalScale;                           // Since the scale of the Cursor changes, the original scale needs to be stored.

        protected RaycastHit _raycastResult;
        protected Ray _ray;

        protected LineRenderer _rayRender = null;

        protected Mouse _mouse
        {
            get
            {
                if (m_Mouse == null)
                {
                    m_Mouse = Mouse.current;
                }

                return m_Mouse;
            }
        }
        protected Mouse m_Mouse = null;

        #endregion

        #region EVENTS

        public event Action OnTriggerDown;  // Called when the trigger assigned to this cursor is pressed
        public event Action OnTriggerUp;    // Called when the trigger assigned to this cursor is released

        #endregion

        #region CREATION AND DESTRUCTION

        public static QuickUICursor CreateVRCursor(Role cType, Transform cTransform)
        {
            QuickUICursor vrCursor = cTransform.gameObject.GetOrCreateComponent<QuickUICursor>();
            vrCursor._TriggerVirtualKey = InputManager.DEFAULT_BUTTON_CONTINUE;
            vrCursor._drawRay = (cType == Role.LeftHand || cType == Role.RightHand);

            _vrCursors[cType] = vrCursor;
            vrCursor.enabled = false;

            return vrCursor;
        }

        protected virtual void Awake()
        {
            if (!_vrGUICursor) _vrGUICursor = Resources.Load<Transform>("Prefabs/pf_GUICursor");
            CreateCanvasCursors();
            CreateVRGUICursor();

            _rayRender = gameObject.GetOrCreateComponent<LineRenderer>();
            _rayRender.startWidth = _rayRender.endWidth = 0.01f;
            _rayRender.material = new Material(Shader.Find(DEFAULT_RAY_SHADER));

            SetColor(Color.green);
        }

        protected virtual void CreateCanvasCursors()
        {
            if (!_canvasCursors)
            {
                _canvasCursors = new GameObject("__CanvasCursor__").GetOrCreateComponent<Canvas>();
                _canvasCursors.gameObject.layer = LayerMask.NameToLayer("UI");
                _canvasCursors.renderMode = RenderMode.WorldSpace;
                _canvasCursors.sortingLayerName = "GUI";
            }
        }

        protected virtual void CreateVRGUICursor()
        {
            _CursorTransform = Instantiate<Transform>(_vrGUICursor);
            _CursorTransform.name = "__GUICursor__";
            _CursorTransform.SetParent(_canvasCursors.GetComponent<RectTransform>());
            _OriginalScale = _CursorTransform.localScale;
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostUpdateTracking += UpdateRaycast;
            _CursorTransform.gameObject.SetActive(true);
            _rayRender.enabled = true;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostUpdateTracking -= UpdateRaycast;
            if (_CursorTransform) _CursorTransform.gameObject.SetActive(false);
            _rayRender.enabled = false;
        }

        #endregion

        #region GET AND SET

        public static QuickUICursor GetVRCursorMain()
        {
            return GetVRCursor(QuickVRManager.IsXREnabled() ? Role.RightHand : Role.Head);
        }

        public static QuickUICursor GetVRCursor(Role cType)
        {
            if (!_vrCursors.ContainsKey(cType)) return null;

            return _vrCursors[cType];
        }

        public virtual Image GetImage()
        {
            return _CursorTransform.GetComponentInChildren<Image>();
        }

        public virtual QuickUIInteractiveItem GetCurrentInteractible()
        {
            return _CurrentInteractible;
        }

        // This overload of SetPosition is used when the the VREyeRaycaster hasn't hit anything.
        public void SetPosition(Vector3 pos, float distance)
        {
            // Set the position of the reticle to the default distance in front of the camera.
            _CursorTransform.position = pos;
            _CursorTransform.localScale = _OriginalScale * distance;
        }

        public virtual RaycastHit GetRaycastResult()
        {
            return _raycastResult;
        }

        public virtual Ray GetRay()
        {
            return _ray;
        }

        protected virtual Ray ComputeRay()
        {
            if ( !QuickVRManager.IsXREnabled() && GetVRCursor(Role.Head) == this)
            {
                return QuickVRCameraController.GetCamera().ScreenPointToRay(new Vector3(_mouse.position.x.ReadValue(), _mouse.position.y.ReadValue()));
            }

            return new Ray(transform.position, _CursorTransform.position - transform.position);
        }

        public virtual void SetColor(Color c)
        {
            _rayRender.material.color = c;
            GetImage().color = c;
        }

        #endregion

        #region UPDATE

        protected virtual void UpdateRaycast()
        {
            EyeRaycast();

            _CursorTransform.LookAt(transform.position);

            _rayRender.SetPosition(0, transform.position);
            _rayRender.SetPosition(1, _CursorTransform.position);

            bool hit = _raycastResult.collider != null;
            bool draw = (hit || (!hit && !_hideIfNoHit));
            _CursorTransform.gameObject.SetActive(draw && _drawCursor);
            _rayRender.enabled = draw && _drawRay;
        }

        protected virtual void LateUpdate()
        {
            if (InputManager.GetButtonDown(_TriggerVirtualKey)) HandleDown();
            else if (InputManager.GetButtonUp(_TriggerVirtualKey)) HandleUp();
        }

        protected virtual void EyeRaycast()
        {
            SetPosition(transform.position + transform.forward * _DefaultDistance, _DefaultDistance);

            // Create a ray that points forwards from the camera.
            _ray = ComputeRay();
            float rLength = _RayLength;

            // Do the raycast forweards to see if we hit an interactive item
            if (Physics.Raycast(_ray, out _raycastResult, Mathf.Infinity, _RayCastMask))
            {
                QuickUIInteractiveItem interactible = _raycastResult.collider.GetComponent<QuickUIInteractiveItem>(); //attempt to get the VRInteractiveItem on the hit object
                _CurrentInteractible = interactible;

                // If we hit an interactive item and it's not the same as the last interactive item, then call Over
                if (interactible && interactible != _LastInteractible) interactible.Over();

                // Deactive the last interactive item 
                if (interactible != _LastInteractible) DeactiveLastInteractible();

                _LastInteractible = interactible;

                SetPosition(_raycastResult.point, _raycastResult.distance);
                rLength = _raycastResult.distance;
            }
            else
            {
                // Nothing was hit, deactive the last interactive item.
                DeactiveLastInteractible();
                _CurrentInteractible = null;
            }

            Debug.DrawRay(_ray.origin, _ray.direction * rLength, Color.blue, 1.0f);
        }

        protected virtual void DeactiveLastInteractible()
        {
            if (_LastInteractible == null) return;

            _LastInteractible.Out();
            _LastInteractible = null;
        }

        protected virtual void HandleUp()
        {
            if (_CurrentInteractible != null) _CurrentInteractible.Up();
            if (OnTriggerUp != null) OnTriggerUp();
        }

        protected void HandleDown()
        {
            if (_CurrentInteractible != null) _CurrentInteractible.Down();
            if (OnTriggerDown != null) OnTriggerDown();
        }

        #endregion
    }

}
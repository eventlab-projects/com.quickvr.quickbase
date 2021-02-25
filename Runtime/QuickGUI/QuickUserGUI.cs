using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuickVR
{

    public class QuickUserGUI : MonoBehaviour
    {

        #region CONSTANTS

        protected const string NAME_INSTRUCTIONS_TRANSFORM = "__Instructions__";

        #endregion

        #region PUBLIC ATTRIBUTES

        public bool _followCamera = true;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Canvas _canvas = null;
        protected Text _instructions = null;

        protected static HashSet<QuickUserGUI> _guis = new HashSet<QuickUserGUI>(); //All the GUIS present in the game. 

        protected Camera _camera
        {
            get
            {
                return Camera.main;
            }
        }

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            RegisterGUI(this);

            Reset();
        }

        protected static void RegisterGUI(QuickUserGUI userGUI)
        {
            _guis.Add(userGUI);
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostCameraUpdate += ActionPostCameraUpdate;

            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (Button b in buttons)
            {
                b.GetOrCreateComponent<QuickUIButton>();
            }
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostCameraUpdate -= ActionPostCameraUpdate;
        }

        protected virtual void Reset()
        {
            gameObject.layer = LayerMask.NameToLayer("UI");
            _canvas = CreateCanvas();
            _instructions = CreateInstructionsText();
            gameObject.GetOrCreateComponent<QuickCanvasEventCameraDefiner>();
        }

        protected virtual Canvas CreateCanvas()
        {
            Canvas result = gameObject.GetOrCreateComponent<Canvas>();
            RectTransform t = result.GetComponent<RectTransform>();
            t.localPosition = new Vector3(0.0f, 1.5f, 3.0f);
            t.sizeDelta = new Vector2(5.0f, 3.0f);
            result.renderMode = RenderMode.WorldSpace;

            CanvasScaler cScaler = gameObject.GetOrCreateComponent<CanvasScaler>();
            cScaler.dynamicPixelsPerUnit = 1;
            cScaler.referencePixelsPerUnit = 10;

            GraphicRaycaster rCaster = gameObject.GetOrCreateComponent<GraphicRaycaster>();

            return result;
        }

        protected virtual Text CreateInstructionsText()
        {
            Text result = transform.CreateChild(NAME_INSTRUCTIONS_TRANSFORM).GetOrCreateComponent<Text>();
            RectTransform t = result.GetComponent<RectTransform>();
            t.sizeDelta = new Vector2(15, 12);
            t.anchorMin = new Vector2(0.5f, 1.0f);
            t.anchorMax = new Vector2(0.5f, 1.0f);
            t.pivot = new Vector2(0.5f, 1.0f);
            
            t.localScale = Vector3.one * 0.25f;

            result.font = Resources.Load<Font>("Fonts/arial");
            result.fontSize = 1;
            result.material = Instantiate(Resources.Load<Material>("Materials/GUIText"));

            return result;
        }

        #endregion

        #region GET AND SET

        public virtual void SetTextInstructions(string text)
        {
            _instructions.text = text;
        }

        public virtual void ClearAllText()
        {
            foreach (Text t in _canvas.GetComponentsInChildren<Text>()) 
            {
                t.text = "";
            }
        }

        public virtual void EnableInstructions(bool enable)
        {
            _instructions.gameObject.SetActive(enable);
        }

        public virtual bool IsEnabledInstructions()
        {
            return _instructions.gameObject.activeSelf;
        }

        public virtual void ShowAll(bool show)
        {
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(show);
            }
        }

        public virtual QuickUIButton GetButton(string buttonName)
        {
            return transform.Find(buttonName).GetOrCreateComponent<QuickUIButton>();
        }

        public virtual void ResetPosition()
        {
            Animator animator = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorTarget();
            //Vector3 fwd = animator.transform.forward;
            Vector3 fwd = Vector3.ProjectOnPlane(Camera.main.transform.forward, animator.transform.up);
            transform.position = animator.GetBoneTransform(HumanBodyBones.Head).position + fwd * 3;
            transform.forward = fwd;
        }

        #endregion

        #region UPDATE

        protected virtual void ActionPostCameraUpdate()
        {
            if (_followCamera)
            {
                Vector3 fwd = _camera.transform.forward;
                transform.position = _camera.transform.position + fwd * 3;
                //transform.forward = fwd;
                transform.rotation = _camera.transform.rotation;
            }
        }

        #endregion

    }

}


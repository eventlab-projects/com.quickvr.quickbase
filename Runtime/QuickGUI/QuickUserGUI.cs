using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace QuickVR
{

    public class QuickUserGUI : MonoBehaviour
    {

        #region CONSTANTS

        public const string MAT_PARAM_GUI_ZTEST_MODE = "unity_GUIZTestMode";

        protected const string NAME_INSTRUCTIONS_TRANSFORM = "__Instructions__";

        #endregion

        #region PUBLIC ATTRIBUTES

        public bool _followCamera = true;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Canvas _canvas = null;
        protected TextMeshProUGUI _instructions = null;

        protected static HashSet<QuickUserGUI> _guis = new HashSet<QuickUserGUI>(); //All the GUIS present in the game. 

        protected Camera _camera
        {
            get
            {
                return QuickVRCameraController.GetCamera();
            }
        }

        protected TextMeshProUGUI[] _textMeshes;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            RegisterGUI(this);

            Reset();

            _textMeshes = GetComponentsInChildren<TextMeshProUGUI>(true);
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
            Canvas result = gameObject.GetComponent<Canvas>();
            if (!result)
            {
                result = gameObject.AddComponent<Canvas>();
                RectTransform t = result.GetComponent<RectTransform>();
                t.localPosition = new Vector3(0.0f, 1.5f, 3.0f);
                t.sizeDelta = new Vector2(5.0f, 3.0f);
                result.renderMode = RenderMode.WorldSpace;

                CanvasScaler cScaler = gameObject.GetOrCreateComponent<CanvasScaler>();
                cScaler.dynamicPixelsPerUnit = 1;
                cScaler.referencePixelsPerUnit = 10;

                GraphicRaycaster rCaster = gameObject.GetOrCreateComponent<GraphicRaycaster>();
            }
            
            return result;
        }

        protected virtual TextMeshProUGUI CreateInstructionsText()
        {
            Transform tInstructions = transform.CreateChild(NAME_INSTRUCTIONS_TRANSFORM);
            TextMeshProUGUI result = tInstructions.GetComponent<TextMeshProUGUI>();
            if (!result)
            {
                result = tInstructions.gameObject.AddComponent<TextMeshProUGUI>();

                RectTransform tCanvas = _canvas.GetComponent<RectTransform>();
                RectTransform t = result.GetComponent<RectTransform>();
                float sf = 100;
                t.sizeDelta = new Vector2(tCanvas.GetWidth() * sf, tCanvas.GetHeight() * sf);
                t.anchorMin = new Vector2(0.5f, 1.0f);
                t.anchorMax = new Vector2(0.5f, 1.0f);
                t.pivot = new Vector2(0.5f, 1.0f);

                t.localScale = Vector3.one * (1.0f / sf);
            }

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
            QuickUIButton result = null;

            Button[] buttons = transform.GetComponentsInChildren<Button>(true);
            foreach (Button b in buttons)
            {
                if (b.name == buttonName)
                {
                    result = b.GetOrCreateComponent<QuickUIButton>();        
                }
            }

            return result;
        }

        public virtual void ResetPosition()
        {
            Animator animator = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorTarget();
            //Vector3 fwd = animator.transform.forward;
            Vector3 fwd = Vector3.ProjectOnPlane(QuickVRCameraController.GetCamera().transform.forward, animator.transform.up);
            transform.position = animator.GetBoneTransform(HumanBodyBones.Head).position + fwd * 3;
            transform.forward = fwd;
        }

        #endregion

        #region UPDATE

        protected virtual void ActionPostCameraUpdate()
        {
            UpdateZTestMode();

            if (_followCamera)
            {
                Vector3 fwd = _camera.transform.forward;
                transform.position = _camera.transform.position + fwd * 3;
                //transform.forward = fwd;
                transform.rotation = _camera.transform.rotation;
            }
        }

        protected virtual void UpdateZTestMode()
        {
            int zTestValue = (int)UnityEngine.Rendering.CompareFunction.Always;

            foreach (TextMeshProUGUI t in _textMeshes)
            {
                Material mat = t.materialForRendering;
                if (mat && mat.HasProperty(MAT_PARAM_GUI_ZTEST_MODE) && mat.GetInt(MAT_PARAM_GUI_ZTEST_MODE) != zTestValue)
                {
                    mat.SetInt(MAT_PARAM_GUI_ZTEST_MODE, zTestValue);
                }
            }
        }

        #endregion

    }

}


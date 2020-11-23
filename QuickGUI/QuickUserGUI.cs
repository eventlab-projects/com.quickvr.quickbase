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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        protected static void Init()
        {
            QuickVRManager.OnPreCameraUpdate += ActionPreCameraUpdate;
        }

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

        public virtual bool IsBlockView()
        {
            return true;
        }

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

        #endregion

        #region UPDATE

        protected static void ActionPreCameraUpdate()
        {
            if (!QuickVRManager.IsXREnabled())
            {
                bool blockCamera = false;
                foreach (QuickUserGUI g in _guis)
                {
                    blockCamera |= g.gameObject.activeInHierarchy && g.IsBlockView(); 
                }

                QuickSingletonManager.GetInstance<QuickVRCameraController>()._rotateCamera = blockCamera ? false : true;
            }
        }

        protected virtual void ActionPostCameraUpdate()
        {
            Vector3 fwd = _camera.transform.forward;
            transform.position = _camera.transform.position + fwd * 3;
            transform.forward = fwd;
        }

        #endregion

    }

}


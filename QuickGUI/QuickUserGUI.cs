using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Animations;

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

        protected ParentConstraint _constraint = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            Reset();
        }

        protected virtual void Reset()
        {
            _canvas = CreateCanvas();
            _instructions = CreateInstructionsText();
        }

        protected virtual IEnumerator Start()
        {
            while (!Camera.main) yield return null;

            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = Camera.main.transform;
            source.weight = 1.0f;

            _constraint = gameObject.GetOrCreateComponent<ParentConstraint>();
            _constraint.AddSource(source);
            _constraint.constraintActive = true;
            _constraint.SetTranslationOffset(0, new Vector3(0, 0, 3.0f));
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

        #endregion

    }

}


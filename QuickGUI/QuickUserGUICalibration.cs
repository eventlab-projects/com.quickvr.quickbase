using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuickVR
{

    public class QuickUserGUICalibration : QuickUserGUI
    {

        #region PUBLIC ATTRIBUTES

        public enum CalibrationStep
        {
            HMDAdjustment,
            ForwardDirection,
            TimeExpired,
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Text _hint = null;

        #endregion

        #region CONSTANTS

        protected const string NAME_HINT_TRANSFORM = "__Hint__";

        public const string INSTRUCTIONS_HMD_ADJUSTMENT_EN = "Adjust the HMD\n until you can read\n this text.";
        public const string INSTRUCTIONS_HMD_ADJUSTMENT_ES = "Ajusta el casco hasta que\n puedas leer este texto\n de forma nítida.";

        public const string INSTRUCTIONS_LOOK_FORWARD_EN = "Please, keep your head\n looking front, and do not\n move while the\n screen is in black.";
        public const string INSTRUCTIONS_LOOK_FORWARD_ES = "Por favor, mantén la cabeza\n mirando al frente, y no te\n muevas mientras la\n pantalla esté oscura.";

        public const string INSTRUCTIONS_TIME_EXPIRED_EN = "The expiration date\n of the application is reached\n and cannot be used anymore. ";
        public const string INSTRUCTIONS_TIME_EXPIRED_ES = "Se ha superado \n la fecha de expiración y\n la aplicación ya no se puede usar. ";

        public const string HINT_CONTROLLERS_EN = "Press the \"Right Trigger\" to continue.";
        public const string HINT_CONTROLLERS_ES = "Pulsa el \"Botón del mando derecho\" para continuar.";

        public const string HINT_HANDS_EN = "Make the \"Thumb Up\" sign to continue.";
        public const string HINT_HANDS_ES = "Haz el signo\"Pulgar Arriba\" para continuar.";

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Reset()
        {
            base.Reset();

            _hint = CreateHint();

            _instructions.alignment = TextAnchor.MiddleCenter;
            //_instructions.fontSize = 2;
        }

        protected virtual Text CreateHint()
        {
            Text result = transform.CreateChild(NAME_HINT_TRANSFORM).GetOrCreateComponent<Text>();
            RectTransform t = result.GetComponent<RectTransform>();
            t.sizeDelta = new Vector2(30, 6);
            t.anchorMin = new Vector2(0.5f, 1.0f);
            t.anchorMax = new Vector2(0.5f, 1.0f);
            t.pivot = new Vector2(0.5f, 1.0f);

            t.anchoredPosition3D = new Vector3(0, -2.5f, 0);
            t.localScale = Vector3.one * 0.125f;

            result.font = Resources.Load<Font>("Fonts/arial");
            result.fontSize = 1;
            result.alignment = TextAnchor.UpperCenter;
            result.material = Instantiate(Resources.Load<Material>("Materials/GUIText"));

            return result;
        }

        #endregion

        #region GET AND SET

        public virtual void SetCalibrationInstructions(CalibrationStep step, QuickUnityVR.HandTrackingMode handTrackingMode)
        {
            //Fill the instructions field
            bool isEnglish = SettingsBase.GetLanguage() == SettingsBase.Languages.ENGLISH;
            if (step == CalibrationStep.HMDAdjustment)
            {
                SetTextInstructions(isEnglish ? INSTRUCTIONS_HMD_ADJUSTMENT_EN : INSTRUCTIONS_HMD_ADJUSTMENT_ES);
            }
            else if (step == CalibrationStep.ForwardDirection)
            {
                SetTextInstructions(isEnglish ? INSTRUCTIONS_LOOK_FORWARD_EN : INSTRUCTIONS_LOOK_FORWARD_ES);
            }
            else if (step == CalibrationStep.TimeExpired)
            {
                SetTextInstructions(isEnglish ? INSTRUCTIONS_TIME_EXPIRED_EN : INSTRUCTIONS_TIME_EXPIRED_ES);
            }

            //Fill the hint field
            if (handTrackingMode == QuickUnityVR.HandTrackingMode.Controllers)
            {
                SetTextHint(isEnglish ? HINT_CONTROLLERS_EN : HINT_CONTROLLERS_ES);
            }
            else
            {
                SetTextHint(isEnglish ? HINT_HANDS_EN : HINT_HANDS_ES);
            }
        }

        public virtual void SetTextHint(string text)
        {
            _hint.text = text;
        }

        #endregion

    }

}



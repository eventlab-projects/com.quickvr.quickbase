using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickStageCalibration : QuickStagePreBase
    {

        #region PUBLIC ATTRIBUTES

        public AudioClip _headTrackingCalibrationInstructions = null;

        public List<Texture2D> _logos = new List<Texture2D>();
        public float _logoFadeTime = 0.5f;
        public float _logoStayTime = 2.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            if (!_headTrackingCalibrationInstructions)
            {
                _headTrackingCalibrationInstructions = Resources.Load<AudioClip>(GetDefaultHMDCalibrationInstructions());
            }

            base.Awake();
        }

        #endregion

        #region GET AND SET

        protected virtual string GetDefaultHMDCalibrationInstructions()
        {
            string path = "HMDCalibrationInstructions/";
            if (SettingsBase.GetLanguage() == SettingsBase.Languages.ENGLISH) path += "en/";
            else path += "es/";
            return path + "instructions";
        }

        #endregion

        #region UPDATE

        protected override IEnumerator CoUpdate()
        {
            _guiCalibration.gameObject.SetActive(true);

            //Show the logos if any
            yield return StartCoroutine(CoShowLogos());

            if (QuickVRManager.IsXREnabled())
            {
                yield return StartCoroutine(CoUpdateStateForwardDirection());    //Wait for the VR Devices Calibration
            }

            _guiCalibration.gameObject.SetActive(false);
            _vrManager.RequestCalibration();
        }

        protected virtual IEnumerator CoShowLogos()
        {
            //Show the logos
            //foreach (Texture2D logo in _logos)
            //{
            //    _cameraFade.SetTexture(logo);

            //    //fadeIN
            //    _cameraFade.FadeIn(_logoFadeTime);
            //    while (_cameraFade.IsFading()) yield return null;

            //    //fadeOUT
            //    _cameraFade.FadeOut(_logoFadeTime);
            //    while (_cameraFade.IsFading()) yield return null;
            //}

            yield break;
        }

        protected virtual IEnumerator CoUpdateStateForwardDirection()
        {
            //HMD Forward Direction calibration
            _instructionsManager.Play(_headTrackingCalibrationInstructions);
            _guiCalibration.SetCalibrationInstructions(QuickUserGUICalibration.CalibrationStep.ForwardDirection, _hTracking._handTrackingMode);

            while (_instructionsManager.IsPlaying() && !InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE)) yield return null;

            while (!InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE)) yield return null;

            _instructionsManager.Stop();
            yield return null;
        }
    }

    #endregion

}



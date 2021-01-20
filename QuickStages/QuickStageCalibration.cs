using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickStageCalibration : QuickStageBase
    {

        #region PUBLIC ATTRIBUTES

        public AudioClip _headTrackingCalibrationInstructions = null;

        public List<Texture2D> _logos = new List<Texture2D>();
        public float _logoFadeTime = 0.5f;
        public float _logoStayTime = 2.0f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected QuickUserGUICalibration _guiCalibration = null;
        protected QuickUnityVR _hTracking = null;
        protected QuickVRManager _vrManager = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            _guiCalibration = QuickSingletonManager.GetInstance<QuickUserGUICalibration>();
            _vrManager = QuickSingletonManager.GetInstance<QuickVRManager>();

            if (!_headTrackingCalibrationInstructions)
            {
                _headTrackingCalibrationInstructions = Resources.Load<AudioClip>(GetDefaultHMDCalibrationInstructions());
            }

            base.Awake();
        }

        public override void Init()
        {
            _hTracking = _gameManager.GetPlayer().GetComponent<QuickUnityVR>();

            base.Init();
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
#if UNITY_ANDROID
            if (_hTracking._handTrackingMode == QuickUnityVR.HandTrackingMode.Hands)
            {
                yield return StartCoroutine(CoUpdateHandTrackingMode());
            }
#endif

            if (QuickVRManager.IsXREnabled())
            {
                //Adjust the HMD
                yield return StartCoroutine(CoUpdateHMDAdjustment());
            }

            //Show the logos if any
            yield return StartCoroutine(CoShowLogos());

            if (QuickVRManager.IsXREnabled())
            {
                yield return StartCoroutine(CoUpdateStateForwardDirection());    //Wait for the VR Devices Calibration
            }

            _guiCalibration.gameObject.SetActive(false);
            _vrManager.RequestCalibration();
            _debugManager.Clear();
        }

        protected virtual IEnumerator CoUpdateHandTrackingMode()
        {
            _guiCalibration.SetCalibrationInstructions(QuickUserGUICalibration.CalibrationStep.HandTrackingMode, _hTracking._handTrackingMode);

            //HMD Adjustment
            while (!InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE)) yield return null;

            yield return null;
        }

        protected virtual IEnumerator CoUpdateHMDAdjustment()
        {
            _guiCalibration.SetCalibrationInstructions(QuickUserGUICalibration.CalibrationStep.HMDAdjustment, _hTracking._handTrackingMode);

            //HMD Adjustment
            _debugManager.Log("Adjusting HMD. Press CONTINUE when ready.");
            while (!InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE)) yield return null;

            yield return null;
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

            _debugManager.Log("[WAIT] Playing calibration instructions.", Color.red);
            while (_instructionsManager.IsPlaying() && !InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE)) yield return null;

            _debugManager.Log("Wait for the user to look forward. Press RETURN when ready.");
            while (!InputManager.GetButtonDown(InputManager.DEFAULT_BUTTON_CONTINUE)) yield return null;

            _instructionsManager.Stop();
            yield return null;
        }
    }

    #endregion

}



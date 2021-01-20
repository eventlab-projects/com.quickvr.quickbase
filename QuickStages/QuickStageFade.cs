using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickStageFade : QuickStageBase
    {

        #region PUBLIC PARAMETERS

        public float _fadeTime = 5.0f;

        public Color _color = Color.black;
        public Texture2D _texture = null;

        public enum FadeType
        {
            FadeIN,
            FadeOut,
        };

        public FadeType _fadeType = FadeType.FadeIN;

        public AnimationCurve _fadeCurve = null;

        #endregion

        #region PROTECTED PARAMETERS

        protected CameraFade _cameraFade = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Reset()
        {
            InitFadeCurve();
        }

        protected override void Awake()
        {
            base.Awake();

            _cameraFade = QuickSingletonManager.GetInstance<CameraFade>();
            InitFadeCurve();
        }

        protected virtual void InitFadeCurve()
        {
            if (_fadeCurve == null || _fadeCurve.length == 0)
            {
                _fadeCurve = new AnimationCurve();
                _fadeCurve.AddKey(0, 0);
                _fadeCurve.AddKey(1, 1);
            }
        }

        public override void Init()
        {
            base.Init();

            _cameraFade.SetTexture(_texture);
        }

        public override void Finish()
        {
            _cameraFade.SetColor(_fadeType == FadeType.FadeOut ? _color : Color.clear);
            
            base.Finish();
        }

        #endregion

        #region UPDATE

        protected override IEnumerator CoUpdate()
        {
            Color srcColor = _fadeType == FadeType.FadeOut ? Color.clear : _color;
            Color dstColor = _fadeType == FadeType.FadeOut ? _color : Color.clear;

            for (float elapsedTime = 0.0f; elapsedTime < _fadeTime; elapsedTime += Time.deltaTime)
            {
                float t = elapsedTime / _fadeTime;
                _cameraFade.SetColor(Color.Lerp(srcColor, dstColor, _fadeCurve.Evaluate(t)));

                yield return null;
            }
        }

        #endregion

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickWalkInPlace_v2 : QuickCharacterControllerBase
    {

        #region CONSTANTS

        protected const float DY_THRESHOLD = 0.004f;

        #endregion

        #region PUBLIC ATTRIBUTES

        public QuickSpeedCurveAsset _speedCurve = null;

        public float _speedMultiplier = 0.75f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected float _timeLastMin = -1;  //The time when the last local min has been detected. 

        protected float _speedYCicleStart = 0.0f;
        protected float _speedYLastFrame = 0.0f;

        [SerializeField, ReadOnly]
        protected float _desiredSpeed = 0.0f;

        protected QuickVRNode _node = null;

        protected float _timeStill = 0.0f;  //How many seconds the user remains (almost) still

        protected float _fStepMin = 0.0f;
        protected float _fStepMax = 0.0f;

        protected Coroutine _coUpdateTrackedNode = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            if (!_speedCurve) _speedCurve = Resources.Load<QuickSpeedCurveAsset>("QuickSpeedCurveDefault");
            _speedCurve._animationCurve.postWrapMode = WrapMode.Clamp;
            _speedCurve._animationCurve.preWrapMode = WrapMode.Clamp;

            Keyframe[] keys = _speedCurve._animationCurve.keys;
            _fStepMin = _fStepMax = keys[0].time;
            for (int i = 1; i < keys.Length; i++)
            {
                float t = keys[i].time;
                if (t < _fStepMin) _fStepMin = t;
                else if (t > _fStepMax) _fStepMax = t;
            }
        }

        protected virtual void Start()
        {
            QuickIKManager ikManager = GetComponent<QuickIKManager>();
            ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.LeftFoot);
            ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.RightFoot);
        }            

        protected override void OnEnable()
        {
            base.OnEnable();

            QuickUnityVRBase.OnCalibrate += Init;
            _coUpdateTrackedNode = StartCoroutine(CoUpdateTrackedNode());
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            QuickUnityVRBase.OnCalibrate -= Init;
            StopCoroutine(_coUpdateTrackedNode);
        }

        protected virtual void Init()
        {
            _speedYCicleStart = _speedYLastFrame = _node.GetTrackedObject().GetVelocity().y;
            _timeLastMin = -1;
            _timeStill = _fStepMax;
            _desiredSpeed = 0.0f;
        }

        #endregion

        #region GET AND SET

        protected override void ComputeTargetLinearVelocity()
        {
            if (_node && _node.IsTracked())
            {

                QuickTrackedObject tObject = _node.GetTrackedObject();

                float speedY = tObject.GetVelocity().y;
                if (speedY > _speedYLastFrame)
                {
                    if (_timeLastMin == -1)
                    {
                        //This is the first local min detected. 
                        _timeLastMin = Time.time;
                    }
                    else
                    {
                        //We have found a new local min
                        float dt = Time.time - _timeLastMin;
                        _timeLastMin = Time.time;

                        if (dt >= 0.2f && dt <= 2.0f)
                        {
                            //A valid step has been detected. 
                        }
                    }

                    //float dy = Mathf.Abs(posY - _posYCicleStart);
                    //if (dy > DY_THRESHOLD)
                    //{
                    //    //A real step has been detected
                    //    float dt = Time.time - _timeCicleStart;
                    //    _desiredSpeed = _speedCurve.Evaluate(dt);
                    //    _timeStill = 0.0f;
                    //}
                    //else
                    //{
                    //    //No step has been detected. The user is stopping. 
                    //    _timeStill = Mathf.Min(_timeStill + Time.deltaTime, _fStepMin);
                    //    _desiredSpeed = Mathf.Lerp(_speedCurve.Evaluate(_fStepMax), 0.0f, _timeStill / _fStepMin);
                    //    //_desiredSpeed = Mathf.Lerp(_desiredSpeed, 0.0f, _timeStill / _fStepMax);
                    //    //_targetLinearVelocity = _currentLinearVelocity = transform.forward * _desiredSpeed;
                    //}

                    //_desiredSpeed *= _speedMultiplier;

                    //_posYCicleStart = posY;
                    //_timeCicleStart = Time.time;
                }

                //_posYLastFrame = posY;
            }
            else _desiredSpeed = 0.0f;

            // Calculate how fast we should be moving
            _targetLinearVelocity = transform.forward * _desiredSpeed;
            _currentLinearVelocity = transform.forward * _currentLinearVelocity.magnitude;
        }

        protected override void ComputeTargetAngularVelocity()
        {
            
        }

        public override float GetMaxLinearSpeed()
        {
            return _speedCurve.Evaluate(0);
        }

        protected virtual IEnumerator CoUpdateTrackedNode()
        {
            QuickUnityVRBase hTracking = GetComponent<QuickUnityVRBase>();

            while (true)
            {
                QuickVRNode hipsNode = hTracking.GetQuickVRNode(QuickVRNode.Type.Waist);
                QuickVRNode n = hipsNode.IsTracked() ? hipsNode : hTracking.GetQuickVRNode(QuickVRNode.Type.Head);
                if (n != _node)
                {
                    _node = n;
                    Init();
                }

                yield return null;
            }
        }

        #endregion

    }

}

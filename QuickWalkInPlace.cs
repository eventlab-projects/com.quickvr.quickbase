using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickWalkInPlace : QuickCharacterControllerBase
    {

        #region CONSTANTS

        protected const float DY_THRESHOLD = 0.004f;

        #endregion

        #region PUBLIC ATTRIBUTES

        public QuickSpeedCurveAsset _speedCurve = null;

        public float _speedMultiplier = 0.75f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected float _timeCicleStart = 0.0f;

        protected float _posYCicleStart = 0.0f;
        protected float _posYLastFrame = 0.0f;

        [SerializeField, ReadOnly]
        protected float _desiredSpeed = 0.0f;

        protected QuickVRNode _node = null;

        protected bool _trend = false;  //true => going towards a local max; false => going towards a local min

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

        protected virtual void OnEnable()
        {
            QuickUnityVRBase.OnCalibrate += Init;
            _rigidBody.isKinematic = false;
            _coUpdateTrackedNode = StartCoroutine(CoUpdateTrackedNode());
        }

        protected virtual void OnDisable()
        {
            QuickUnityVRBase.OnCalibrate -= Init;
            _rigidBody.isKinematic = true;
            StopCoroutine(_coUpdateTrackedNode);
        }

        protected virtual void Init()
        {
            _posYCicleStart = _posYLastFrame = _node.GetTrackedObject().transform.position.y;
            _timeCicleStart = Time.time;
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

                float posY = tObject.transform.position.y;
                if ((_trend && (posY < _posYLastFrame)) || (!_trend && (posY > _posYLastFrame)))
                {
                    //1) If the trend is positive, but the current posY is less than posY at previous frame (_posYLastFrame), 
                    //we have found a local max

                    //2) If the trend is negative, but the current posY is greater than posY at previous frame (_posYLastFrame), 
                    //we have found a local min

                    //On either case, we have found the end of the current cicle. 

                    float dy = Mathf.Abs(posY - _posYCicleStart);
                    if (dy > DY_THRESHOLD)
                    {
                        //A real step has been detected
                        float dt = Time.time - _timeCicleStart;
                        _desiredSpeed = _speedCurve.Evaluate(dt);
                        _timeStill = 0.0f;
                    }
                    else
                    {
                        _timeStill = Mathf.Min(_timeStill + Time.deltaTime, _fStepMax);
                        _desiredSpeed = Mathf.Lerp(_speedCurve.Evaluate(_fStepMax), 0.0f, _timeStill / _fStepMax);
                    }

                    _desiredSpeed *= _speedMultiplier;

                    _posYCicleStart = posY;
                    _timeCicleStart = Time.time;

                    _trend = !_trend;
                }

                _posYLastFrame = posY;
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
                if (hipsNode)
                {
                    QuickVRNode n = hipsNode.IsTracked() ? hipsNode : hTracking.GetQuickVRNode(QuickVRNode.Type.Head);
                    if (n != _node)
                    {
                        _node = n;
                        Init();
                    }
                }

                yield return null;
            }
        }

        #endregion

    }

}

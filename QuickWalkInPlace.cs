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

        public enum WalkMethod
        {
            Head, 
            Hips, 
            Feet, 
        }

        public WalkMethod _walkMethod = WalkMethod.Head;
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
            QuickUnityVRBase hTracking = GetComponent<QuickUnityVRBase>();
            _node = hTracking.GetQuickVRNode(_walkMethod == WalkMethod.Head ? QuickVRNode.Type.Head : QuickVRNode.Type.Waist);
            if (_walkMethod != WalkMethod.Feet)
            {
                //If the walk method chosen is using the Head or the Hips nodes only, the feet 
                //are driven by the animation. Otherwise, they are driven by the real user movement. 
                hTracking._trackedJoints &= ~(1 << (int)IKLimbBones.LeftFoot);
                hTracking._trackedJoints &= ~(1 << (int)IKLimbBones.RightFoot);

                if (hTracking.GetType() == typeof(QuickUnityVR))
                {
                    QuickUnityVR uVR = (QuickUnityVR)hTracking;
                    uVR._rotateWithCamera = uVR._displaceWithCamera = _walkMethod == WalkMethod.Head;
                }
            }

            QuickIKManager ikManager = GetComponent<QuickIKManager>();
            ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.LeftFoot);
            ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.RightFoot);
        }            

        protected virtual void OnEnable()
        {
            QuickUnityVRBase.OnCalibrate += Init;
        }

        protected virtual void OnDisable()
        {
            QuickUnityVRBase.OnCalibrate -= Init;
        }

        protected virtual void Init()
        {
            _posYCicleStart = _posYLastFrame = _node.GetTrackedObject().transform.position.y;
            _timeCicleStart = Time.time;
            _timeStill = 0.0f;
        }

        #endregion

        #region GET AND SET

        protected override void ComputeTargetLinearVelocity()
        {
            if (_node.IsTracked())
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
                    //Debug.Log("dy = " + dy.ToString("f3"));
                    if (dy > DY_THRESHOLD)
                    {
                        //A real step has been detected
                        float dt = Time.time - _timeCicleStart;
                        _desiredSpeed = _speedCurve.Evaluate(dt);
                        _timeStill = 0.0f;
                    }
                    else
                    {
                        //We are still
                        _timeStill += Time.deltaTime;
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

        #endregion

    }

}

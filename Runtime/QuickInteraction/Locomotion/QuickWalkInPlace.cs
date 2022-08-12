using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickWalkInPlace : QuickCharacterControllerBase
    {

        #region CONSTANTS

        protected const float DY_THRESHOLD = 0.004f;

        protected const float MIN_TIME_STEP = 0.2f;    //5 steps per second
        protected const float MAX_TIME_STEP = 2.0f;    //0.5 steps per second

        #endregion

        #region PUBLIC ATTRIBUTES

        public float _speedMin = 1.0f;
        public float _speedMax = 5.0f;

        public float _speedMultiplier = 1.0f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected float _timeCicleStart = 0.0f;

        protected float _posYCicleStart = 0.0f;
        protected float _posYLastFrame = 0.0f;

        protected float _posYNewSample = 0.0f;

        [SerializeField, ReadOnly]
        protected float _desiredSpeed = 0.0f;

        protected QuickTrackedObject _trackedObject = null;

        protected bool _trend = false;  //true => going towards a local max; false => going towards a local min

        protected float _timeStill = 0.0f;  //How many seconds the user remains (almost) still

        protected Coroutine _coUpdateTrackedNode = null;

        protected QuickVRPlayArea _vrPlayArea = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            _vrPlayArea = QuickSingletonManager.GetInstance<QuickVRPlayArea>();

            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            QuickIKManager ikManager = GetComponent<QuickIKManager>();
            //ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.LeftFoot);
            //ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.RightFoot);
        }            

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostCalibrate += Init;
            StartCoroutine(CoUpdate());
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostCalibrate -= Init;
        }

        protected virtual void Init()
        {
            _posYCicleStart = _posYLastFrame = _posYNewSample = _trackedObject.transform.position.y;
            _timeCicleStart = Time.time;
            _timeStill = MIN_TIME_STEP;
            _desiredSpeed = 0.0f;
        }

        #endregion

        #region GET AND SET

        protected override void ComputeTargetLinearVelocity()
        {
            
        }

        protected override void ComputeTargetAngularVelocity()
        {
            
        }

        public override float GetMaxLinearSpeed()
        {
            return _speedMax;
        }
        
        protected virtual IEnumerator CoUpdate()
        {
            _trackedObject = _vrPlayArea.GetVRNode(HumanBodyBones.Head).GetTrackedObject();

            while (true)
            {
                CoUpdateTrackedNode();
                
                //Wait for a new sample
                yield return StartCoroutine(CoUpdateSample());

                CoUpdateTargetLinearVelocity();
            }
        }

        protected virtual void CoUpdateTrackedNode()
        {
            QuickUnityVR hTracking = GetComponent<QuickUnityVR>();
            QuickVRNode hipsNode = _vrPlayArea.GetVRNode(HumanBodyBones.Hips);
            if (hipsNode)
            {
                QuickTrackedObject tObject = hipsNode.IsTracked()? hipsNode.GetTrackedObject() : _vrPlayArea.GetVRNode(HumanBodyBones.Head).GetTrackedObject();
                if (tObject != _trackedObject)
                {
                    _trackedObject = tObject;

                    Init();
                }
            }
        }

        protected virtual void CoUpdateTargetLinearVelocity()
        {
            if ((_trend && (_posYNewSample < _posYLastFrame)) || (!_trend && (_posYNewSample > _posYLastFrame)))
            {
                //1) If the trend is positive, but the current posY is less than posY at previous frame (_posYLastFrame), 
                //we have found a local max

                //2) If the trend is negative, but the current posY is greater than posY at previous frame (_posYLastFrame), 
                //we have found a local min

                //On either case, we have found the end of the current cicle. 

                float dy = Mathf.Abs(_posYNewSample - _posYCicleStart);
                if (dy > DY_THRESHOLD)
                {
                    //A real step has been detected
                    float tStep = (Time.time - _timeCicleStart) * 2.0f;
                    if (tStep > MAX_TIME_STEP) _desiredSpeed = _speedMin;
                    else if (tStep < MIN_TIME_STEP) _desiredSpeed = _speedMax;
                    else
                    {
                        float t = MAX_TIME_STEP - MIN_TIME_STEP;
                        _desiredSpeed = (1.0f - ((tStep - MIN_TIME_STEP) / t)) * (_speedMax - _speedMin) + _speedMin;
                        _timeStill = 0.0f;
                    }
                }
                else
                {
                    _desiredSpeed = 0.0f;
                    //_targetLinearVelocity = Vector3.zero;
                    ////No step has been detected. The user is stopping. 
                    //_timeStill = Mathf.Min(_timeStill + Time.deltaTime, MIN_TIME_STEP);
                    //_desiredSpeed = Mathf.Lerp(_desiredSpeed, 0.0f, _timeStill / MIN_TIME_STEP);
                    ////_desiredSpeed = Mathf.Lerp(_desiredSpeed, 0.0f, _timeStill / _fStepMax);
                    ////_targetLinearVelocity = _currentLinearVelocity = transform.forward * _desiredSpeed;
                }

                _desiredSpeed *= _speedMultiplier;

                _posYCicleStart = _posYNewSample;
                _timeCicleStart = Time.time;

                _trend = !_trend;
            }

            _posYLastFrame = _posYNewSample;

            // Calculate how fast we should be moving
            _targetLinearVelocity = transform.forward * _desiredSpeed;
            _rigidBody.velocity = transform.forward * _rigidBody.velocity.magnitude;
        }

        protected virtual IEnumerator CoUpdateSample()
        {
            float posY = 0.0f;
            int numSamples = 5;

            for (int i = 0; i < numSamples; i++)
            {
                yield return null;

                posY += _trackedObject.transform.position.y;
            }
            posY /= (float)numSamples;

            _posYNewSample = posY;
        }

        #endregion

    }

}

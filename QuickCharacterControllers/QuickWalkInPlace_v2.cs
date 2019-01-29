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

        public float _speedMin = 1.4f;
        public float _speedMax = 5.0f;

        public float _speedMultiplier = 1.0f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected float _timeLastStep = -1; //The time when the last step has been detected

        [SerializeField, ReadOnly]
        protected float _desiredSpeed = 0.0f;

        protected QuickVRNode _node = null;

        protected Coroutine _coUpdateTrackedNode = null;

        protected float _minAcceleration = 0.0f;
        protected float _maxAcceleration = 0.0f;

        protected float _sampleNew = 0.0f;
        protected float _sampleOld = 0.0f;

        #endregion

        #region CONSTANTS

        protected const float MIN_TIME_STEP = 0.2f;    //5 steps per second
        protected const float MAX_TIME_STEP = 2.0f;    //0.5 steps per second
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Start()
        {
            QuickIKManager ikManager = GetComponent<QuickIKManager>();
            ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.LeftFoot);
            ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.RightFoot);
        }            

        protected virtual void OnEnable()
        {
            QuickUnityVRBase.OnCalibrate += Init;
            _coUpdateTrackedNode = StartCoroutine(CoUpdateTrackedNode());

            StartCoroutine(CoUpdateDynamicThreshold());
        }

        protected virtual void OnDisable()
        {
            QuickUnityVRBase.OnCalibrate -= Init;
            StopCoroutine(_coUpdateTrackedNode);
        }

        protected virtual void Init()
        {
            _timeLastStep = -1;
            _desiredSpeed = 0.0f;
        }

        #endregion

        #region GET AND SET

        protected virtual float GetDynamicThreshold()
        {
            return (_minAcceleration + _maxAcceleration) * 0.5f;
        }

        protected override void ComputeTargetLinearVelocity()
        {
            if (_node && _node.IsTracked())
            {

                QuickTrackedObject tObject = _node.GetTrackedObject();

                _sampleOld = _sampleNew;
                _sampleNew = tObject.GetAccelerationFull().y;

                float dynamicThreshold = GetDynamicThreshold();
                if 
                (
                    (_sampleNew < _sampleOld) && 
                    (_sampleOld >= dynamicThreshold) &&
                    (_sampleNew <= dynamicThreshold)
                )
                {
                    //A step has been detected if there is a negative slope of the acceleration plot (_sampleNew < _sampleOld) 
                    //when the acceleration curve crosses below the dynamic threshold
                    if (_timeLastStep == -1)
                    {
                        //This is the first step detected, no previous step has been detected yet. 
                        _timeLastStep = Time.time;
                    }
                    else
                    {
                        float tStep = Time.time - _timeLastStep;

                        if (tStep > MAX_TIME_STEP)
                        {
                            //A step has not been detected in some time (the user has been standing still before they took this step)
                            _desiredSpeed = _speedMin;
                        }
                        else if (tStep < MIN_TIME_STEP)
                        {
                            _desiredSpeed = _speedMax;
                        }
                        else
                        {
                            float t = MAX_TIME_STEP - MIN_TIME_STEP;
                            _desiredSpeed = (1.0f - ((tStep - MIN_TIME_STEP) / t)) * (_speedMax - _speedMin) + _speedMin;
                        }

                        _timeLastStep = Time.time;
                    }
                }
            }
            else _desiredSpeed = 0.0f;

            // Calculate how fast we should be moving
            _targetLinearVelocity = transform.forward * _desiredSpeed;
            _rigidBody.velocity = transform.forward * _rigidBody.velocity.magnitude;
        }

        protected override void ComputeTargetAngularVelocity()
        {
            
        }

        public override float GetMaxLinearSpeed()
        {
            return _speedMax;
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

        protected virtual IEnumerator CoUpdateDynamicThreshold()
        {
            QuickTrackedObject tObject = _node.GetTrackedObject();

            while (true)
            {
                float min = tObject.GetAccelerationFull().y;
                float max = tObject.GetAccelerationFull().y;

                for (int i = 0; i < 50; i++)
                {
                    yield return null;

                    float a = tObject.GetAccelerationFull().y;
                    if (a < min) min = a;
                    if (a > min) max = a;
                }

                _minAcceleration = min;
                _maxAcceleration = max;

                Debug.Log("minAcceleration = " + _minAcceleration.ToString("f3"));
                Debug.Log("maxAcceleration = " + _maxAcceleration.ToString("f3"));
            }
        }

        #endregion

    }

}

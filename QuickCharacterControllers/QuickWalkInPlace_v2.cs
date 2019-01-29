using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickWalkInPlace_v2 : QuickCharacterControllerBase
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

        protected float _timeLastStep = -1.0f;

        protected float _speedYLastSample = 0.0f;

        [SerializeField, ReadOnly]
        protected float _speedYNewSample = 0.0f;

        [SerializeField, ReadOnly]
        protected float _desiredSpeed = 0.0f;

        protected QuickTrackedObject _trackedObject = null;

        protected Coroutine _coUpdateTrackedNode = null;

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
            StartCoroutine(CoUpdate());
        }

        protected virtual void OnDisable()
        {
            QuickUnityVRBase.OnCalibrate -= Init;
        }

        protected virtual void Init()
        {
            _speedYLastSample = _speedYNewSample = 0.0f;
            _timeLastStep = -1;
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
            //Wait for the node of the head to be created. 
            QuickUnityVRBase hTracking = GetComponent<QuickUnityVRBase>();
            QuickVRNode nodeHead = null;
            while (nodeHead == null)
            {
                nodeHead = hTracking.GetQuickVRNode(QuickVRNode.Type.Head);
                yield return null;
            }
            _trackedObject = nodeHead.GetTrackedObject();

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
            QuickUnityVRBase hTracking = GetComponent<QuickUnityVRBase>();
            QuickVRNode hipsNode = hTracking.GetQuickVRNode(QuickVRNode.Type.Waist);
            if (hipsNode)
            {
                QuickTrackedObject tObject = hipsNode.IsTracked() ? hipsNode.GetTrackedObject() : hTracking.GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject();
                if (tObject != _trackedObject)
                {
                    _trackedObject = tObject;

                    Debug.Log("tObject = " + _trackedObject.transform.parent.name);

                    Init();
                }
            }
        }

        protected virtual void CoUpdateTargetLinearVelocity()
        {
            if (Mathf.Approximately(_speedYLastSample, _speedYNewSample))
            {
                _desiredSpeed = 0.0f;
            }
            else if ((_speedYLastSample <= 0) && (_speedYNewSample > 0))
            {
                //We have found a local min. 
                if (_timeLastStep == -1)
                {
                    //It is the first step, no previous step has been detected
                    _timeLastStep = Time.time;
                }
                else
                {
                    float tStep = Time.time - _timeLastStep;
                    _timeLastStep = Time.time;

                    if (tStep < MIN_TIME_STEP)
                    {
                        //The time between steps is considered to be too slow => noise. Discard it
                        _timeLastStep = -1;
                        //_desiredSpeed = 0.0f;
                    }
                    else if (tStep > MAX_TIME_STEP) _desiredSpeed = _speedMin;
                    else
                    {
                        float t = MAX_TIME_STEP - MIN_TIME_STEP;
                        _desiredSpeed = (1.0f - ((tStep - MIN_TIME_STEP) / t)) * (_speedMax - _speedMin) + _speedMin;
                    }
                }

                _desiredSpeed *= _speedMultiplier;
            }

            _speedYLastSample = _speedYNewSample;

            // Calculate how fast we should be moving
            _targetLinearVelocity = transform.forward * _desiredSpeed;
            _rigidBody.velocity = transform.forward * _rigidBody.velocity.magnitude;
        }

        protected virtual IEnumerator CoUpdateSample()
        {
            float speedY = 0.0f;
            int numSamples = 5;

            for (int i = 0; i < numSamples; i++)
            {
                yield return null;

                speedY += _trackedObject.GetVelocity().y;
            }
            speedY /= (float)numSamples;

            if (Mathf.Abs(speedY - _speedYNewSample) > 0.05f) _speedYNewSample = speedY;
        }

        #endregion

    }

}

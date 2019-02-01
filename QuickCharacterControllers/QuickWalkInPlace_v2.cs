using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickWalkInPlace_v2 : QuickCharacterControllerBase
    {

        #region CONSTANTS

        protected const float MIN_TIME_STEP = 0.2f;    //5 steps per second
        protected const float MAX_TIME_STEP = 1.0f;    //1 steps per second

        #endregion

        #region PUBLIC ATTRIBUTES

        public float _speedMin = 1.0f;
        public float _speedMax = 5.0f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected float _timeLastStep = -1.0f;
        protected float _timeStep = Mathf.Infinity;

        protected float _sampleLast = 0.0f;

        [SerializeField, ReadOnly]
        protected float _sampleNew = 0.0f;

        [SerializeField, ReadOnly]
        protected float _desiredSpeed = 0.0f;

        protected QuickTrackedObject _trackedObject = null;

        protected QuickUnityVRBase _headTracking = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Start()
        {
            base.Start();

            _headTracking = GetComponent<QuickUnityVRBase>();
            QuickIKManager ikManager = GetComponent<QuickIKManager>();
            ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.LeftFoot);
            ikManager._ikHintMaskUpdate &= ~(1 << (int)IKLimbBones.RightFoot);
        }

        protected virtual void OnEnable()
        {
            StartCoroutine(CoUpdate());
        }

        protected virtual void Init()
        {
            Debug.Log("trackedObject = " + _trackedObject.transform.parent.name);

            _sampleLast = _sampleNew = 0.0f;
            _timeLastStep = Time.time;
            _timeStep = Mathf.Infinity;
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

        protected virtual float ComputeDesiredSpeed(float tStep)
        {
            if (tStep > MAX_TIME_STEP) return 0.0f;

            return (1.0f - ((tStep - MIN_TIME_STEP) / (MAX_TIME_STEP - MIN_TIME_STEP))) * (_speedMax - _speedMin) + _speedMin;
        }

        protected virtual float GetSample()
        {
            return _trackedObject.GetVelocity().y;
        }

        #endregion

        #region UPDATE

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
                //Debug.Log("disp = " + disp.ToString("f3"));

                CoUpdateTrackedNode();

                //Wait for a new sample
                yield return StartCoroutine(CoUpdateSample());

                CoUpdateTargetLinearVelocity();

                //Check the real displacement of the user in the room. If it is big enough, the contribution
                //of the WiP is ignored. 
                Vector3 disp = Vector3.Scale(_headTracking.GetDisplacement(), Vector3.forward + Vector3.right);

                if (disp.magnitude > 0.005f)
                {
                    _rigidBody.velocity = Vector3.Scale(_rigidBody.velocity, Vector3.up);
                    Init();
                }
            }
        }

        protected virtual void CoUpdateTrackedNode()
        {
            QuickVRNode hipsNode = _headTracking.GetQuickVRNode(QuickVRNode.Type.Waist);
            if (hipsNode)
            {
                QuickTrackedObject tObject = hipsNode.IsTracked() ? hipsNode.GetTrackedObject() : _headTracking.GetQuickVRNode(QuickVRNode.Type.Head).GetTrackedObject();
                if (tObject != _trackedObject)
                {
                    _trackedObject = tObject;

                    Init();
                }
            }
        }

        protected virtual void CoUpdateTargetLinearVelocity()
        {
            if ((_sampleLast <= 0) && (_sampleNew > 0))
            {
                _timeStep = Time.time - _timeLastStep;
                _timeLastStep = Time.time;

                if (_timeStep >= MIN_TIME_STEP)
                {
                    _desiredSpeed = ComputeDesiredSpeed(_timeStep);
                }
            }

            _sampleLast = _sampleNew;


            float timeSinceLastStep = Time.time - _timeLastStep;
            if (timeSinceLastStep >= MIN_TIME_STEP)
            {
                _desiredSpeed = Mathf.Lerp(ComputeDesiredSpeed(_timeStep), 0.0f, timeSinceLastStep / _timeStep);
            }

            // Calculate how fast we should be moving
            _targetLinearVelocity = transform.forward * _desiredSpeed;
            _rigidBody.velocity = transform.forward * _rigidBody.velocity.magnitude;
        }

        protected virtual IEnumerator CoUpdateSample()
        {
            float sample = 0.0f;
            int numSamples = 5;

            for (int i = 0; i < numSamples; i++)
            {
                yield return null;

                sample += GetSample();
            }
            sample /= (float)numSamples;

            if (Mathf.Abs(sample - _sampleNew) > 0.05f) _sampleNew = sample;
        }

        #endregion

    }

}

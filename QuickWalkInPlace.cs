using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickWalkInPlace : QuickCharacterControllerBase
    {

        #region CONSTANTS

        protected const float DY_THRESHOLD = 0.004f;
        protected const int NUM_FRAMES_STILL = 3;

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

        public float _acceleration = 8.0f;
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

        protected int _numStillFrames = 0;  //Counts how many consecutive frames the reference transform has been (almost) still

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            if (!_speedCurve) _speedCurve = Resources.Load<QuickSpeedCurveAsset>("QuickSpeedCurveDefault");
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
            QuickVRManager.OnPostUpdateTracking += UpdateTranslation;
            QuickUnityVRBase.OnCalibrate += Init;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostUpdateTracking -= UpdateTranslation;
            QuickUnityVRBase.OnCalibrate -= Init;
        }

        protected virtual void Init()
        {
            _posYCicleStart = _posYLastFrame = _node.GetTrackedObject().transform.position.y;
            _timeCicleStart = Time.time;
            _numStillFrames = 0;
        }

        #endregion

        #region GET AND SET

        protected override void ComputeTargetLinearVelocity()
        {
            // Calculate how fast we should be moving
            _targetLinearVelocity = transform.forward * _desiredSpeed;
        }

        protected override void ComputeTargetAngularVelocity()
        {
            float cAXis = InputManager.GetAxis(InputManager.DEFAULT_AXIS_HORIZONTAL);
            _targetAngularVelocity = Vector3.zero; //transform.up * cAXis * _angularAcceleration * Mathf.Deg2Rad;
        }

        public override float GetMaxLinearSpeed()
        {
            return _speedCurve.Evaluate(0);
        }

        #endregion

            #region UPDATE

        protected virtual void UpdateTranslation()
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
                        float dt = Time.time - _timeCicleStart;
                        _desiredSpeed = _speedCurve.Evaluate(dt);
                        _numStillFrames = 0;
                    }
                    else
                    {
                        _desiredSpeed = 0.0f;
                        _numStillFrames++;
                    }

                    _desiredSpeed *= _speedMultiplier;

                    _posYCicleStart = posY;
                    _timeCicleStart = Time.time;

                    _trend = !_trend;
                }

                _posYLastFrame = posY;
            }
            else _desiredSpeed = 0.0f;
        }

        #endregion

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickWalkInPlace : MonoBehaviour
    {

        #region CONSTANTS

        protected const float DY_THRESHOLD = 0.004f;
        protected const int NUM_FRAMES_STILL = 3;

        #endregion

        #region PUBLIC ATTRIBUTES

        public QuickVRNode.Type _targetNode = QuickVRNode.Type.Head;
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
        protected Coroutine _coUpdateSpeed = null;

        protected int _numStillFrames = 0;  //Counts how many consecutive frames the reference transform has been (almost) still

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            if (!_speedCurve) _speedCurve = Resources.Load<QuickSpeedCurveAsset>("QuickSpeedCurveDefault");
        }

        protected virtual void Start()
        {
            _node = GetComponent<QuickUnityVRBase>().GetQuickVRNode(_targetNode);
            _node.OnConnected += Init;
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPostUpdateTracking += UpdateTranslation;
            if (_node) _node.OnConnected += Init;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPostUpdateTracking -= UpdateTranslation;
            if (_node) _node.OnConnected -= Init;
            if (_coUpdateSpeed != null) StopCoroutine(_coUpdateSpeed);
        }

        protected virtual void Init()
        {
            _posYCicleStart = _posYLastFrame = _node.GetTrackedObject().transform.position.y;
            _timeCicleStart = Time.time;
            _coUpdateSpeed = StartCoroutine(CoUpdateSpeed());
            _numStillFrames = 0;
        }

        #endregion

        #region GET AND SET

        //protected override void ComputeTargetLinearVelocity()
        //{
        //    // Calculate how fast we should be moving
        //    float hAxis = InputManager.GetAxis(InputManager.DEFAULT_AXIS_HORIZONTAL);
        //    float vAxis = InputManager.GetAxis(InputManager.DEFAULT_AXIS_VERTICAL);
        //    _targetLinearVelocity = new Vector3(hAxis, 0, vAxis);
        //    _targetLinearVelocity.Normalize();
        //    _targetLinearVelocity = transform.TransformDirection(_targetLinearVelocity);
        //    _targetLinearVelocity *= _maxLinearSpeed;
        //}

        //protected override void ComputeTargetAngularVelocity()
        //{
        //    //float cAXis = InputManager.GetAxis(InputManager.DEFAULT_AXIS_HORIZONTAL_RIGHT);
        //    float cAXis = InputManager.GetAxis(InputManager.DEFAULT_AXIS_HORIZONTAL);
        //    _targetAngularVelocity = transform.up * cAXis * _angularAcceleration * Mathf.Deg2Rad;
        //}

        #endregion

        #region UPDATE

        protected virtual void UpdateTranslation()
        {
            if (!_node.IsTracked()) return;
            
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

        protected virtual IEnumerator CoUpdateSpeed()
        {
            float currentSpeed = 0.0f;
            while (true)
            {
                if (_numStillFrames < NUM_FRAMES_STILL)
                {
                    float ds = _acceleration * Time.deltaTime;
                    currentSpeed = (currentSpeed < _desiredSpeed) ? Mathf.Min(currentSpeed + ds, _desiredSpeed) : Mathf.Max(currentSpeed - ds, _desiredSpeed);
                    transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime, Space.Self);
                }

                yield return null;
            }
        }

        #endregion

    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    [ExecuteInEditMode]
    [System.Serializable]
    public class QuickVRHandAnimator : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public bool _isLeft = false;

        public QuickHandPose _handPoseNeutral = null;
        public QuickHandPose _handPoseClosed = null;
        public QuickHandPose _handPoseThumbUp = null;

        [Range(0.0f, 1.0f)]
        public float _closeFactor = 0;

        public bool _isPointing = false;
        public bool _isThumbUp = false;

        [System.Serializable]
        public class FingerBones
        {
            public Transform _proximal = null;
            public Transform _intermediate = null;
            public Transform _distal = null;
            public Transform _tip = null;

            public enum RotAxis
            {
                Right,
                Up,
                Forward,
                Left,
                Down,
                Back
            }

            public RotAxis _axisClose;
            public RotAxis _axisSeparate;

            private static Vector3[] _rotAxisToVector3 = { Vector3.right, Vector3.up, Vector3.forward, Vector3.left, Vector3.down, Vector3.back };

            public virtual Transform this[int boneIndex]
            {
                get
                {
                    if (boneIndex == 0) return _proximal;
                    if (boneIndex == 1) return _intermediate;
                    if (boneIndex == 2) return _distal;
                    if (boneIndex == 3) return _tip;

                    return null;
                }
            }

            public virtual Vector3 GetRotAxisClose()
            {
                return _rotAxisToVector3[(int)_axisClose];
            }

            public virtual Vector3 GetRotAxisSeparate()
            {
                return _rotAxisToVector3[(int)_axisSeparate];
            }

            public virtual bool CheckTransforms()
            {
                return _proximal && _intermediate && _distal && _tip;
            }

        }

        public Transform _handOrigin = null;

        public FingerBones _fingerBonesThumb = new FingerBones();
        public FingerBones _fingerBonesIndex = new FingerBones();
        public FingerBones _fingerBonesMiddle = new FingerBones();
        public FingerBones _fingerBonesRing = new FingerBones();
        public FingerBones _fingerBonesLittle = new FingerBones();

        #endregion

        #region CONSTANTS

        protected const float INPUT_RATE_CHANGE = 20.0f;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected static QuickHumanFingers[] _humanFingers
        {
            get
            {
                if (m_Fingers == null)
                {
                    m_Fingers = QuickHumanTrait.GetHumanFingers();
                }

                return m_Fingers;
            }
            
        }
        protected static QuickHumanFingers[] m_Fingers = null;

        protected Dictionary<QuickHumanFingers, FingerBones> _fingerBoneTransforms 
        {
            get 
            {
                if (m_FingerBoneTransforms == null)
                {
                    m_FingerBoneTransforms = new Dictionary<QuickHumanFingers, FingerBones>();
                    m_FingerBoneTransforms[QuickHumanFingers.Thumb] = _fingerBonesThumb;
                    m_FingerBoneTransforms[QuickHumanFingers.Index] = _fingerBonesIndex;
                    m_FingerBoneTransforms[QuickHumanFingers.Middle] = _fingerBonesMiddle;
                    m_FingerBoneTransforms[QuickHumanFingers.Ring] = _fingerBonesRing;
                    m_FingerBoneTransforms[QuickHumanFingers.Little] = _fingerBonesLittle;
                }

                return m_FingerBoneTransforms;
            }
        }

        protected Dictionary<QuickHumanFingers, FingerBones> m_FingerBoneTransforms = null;

        protected QuickHandGestureSettings _handGestureSettings
        {
            get
            {
                QuickHandGestureSettings result = null;
                if (Application.isPlaying)
                {
                    Animator animatorSrc = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorSource();
                    if (animatorSrc)
                    {
                        QuickUnityVR unityVR = animatorSrc.GetComponent<QuickUnityVR>();
                        result = _isLeft ? unityVR._gestureSettingsLeftHand : unityVR._gestureSettingsRightHand;
                    }
                }

                return result;
            }

        }

        protected float _blendPoint = 0;
        protected float _blendThumbUp = 0;

        protected Collider[] _colliders = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            Reset();

            _colliders = GetComponentsInChildren<Collider>(true);
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnSourceAnimatorSet += ActionAnimatorSourceSet;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnSourceAnimatorSet -= ActionAnimatorSourceSet;
        }

        protected virtual void Reset()
        {
            if (_handPoseNeutral == null)
            {
                _handPoseNeutral = Resources.Load<QuickHandPose>("HandPoses/HandPose_Neutral");
            }
            if (_handPoseClosed == null)
            {
                _handPoseClosed = Resources.Load<QuickHandPose>("HandPoses/HandPose_Closed");
            }
            if (_handPoseThumbUp == null)
            {
                _handPoseThumbUp = Resources.Load<QuickHandPose>("HandPoses/HandPose_ThumbUp");
            }
        }

        #endregion

        #region GET AND SET

        protected virtual void ActionAnimatorSourceSet(Animator animator)
        {
            Debug.Log("HOLA!!!");
            foreach (QuickHumanFingers fingerID in QuickUtils.GetEnumValues<QuickHumanFingers>())
            {
                InitFingerRotation(animator, fingerID);
            }
        }

        protected virtual void InitFingerRotation(Animator animator, QuickHumanFingers fingerID)
        {
            HumanBodyBones proximalBoneID;
            if (fingerID == QuickHumanFingers.Thumb)
            {
                proximalBoneID = _isLeft ? HumanBodyBones.LeftThumbProximal : HumanBodyBones.RightThumbProximal;
            }
            else if (fingerID == QuickHumanFingers.Index)
            {
                proximalBoneID = _isLeft ? HumanBodyBones.LeftIndexProximal : HumanBodyBones.RightIndexProximal;
            }
            else if (fingerID == QuickHumanFingers.Middle)
            {
                proximalBoneID = _isLeft ? HumanBodyBones.LeftMiddleProximal : HumanBodyBones.RightMiddleProximal;
            }
            else if (fingerID == QuickHumanFingers.Ring)
            {
                proximalBoneID = _isLeft ? HumanBodyBones.LeftRingProximal : HumanBodyBones.RightRingProximal;
            }
            else //if (fingerID == QuickHumanFingers.Little)
            {
                proximalBoneID = _isLeft ? HumanBodyBones.LeftLittleProximal : HumanBodyBones.RightLittleProximal;
            }

            Transform tBoneSrc0 = animator.GetBoneTransform(proximalBoneID);
            Transform tBoneSrc1 = animator.GetBoneTransform(proximalBoneID + 1);
            Vector3 v = tBoneSrc1.position - tBoneSrc0.position;

            FingerBones fBones = _fingerBoneTransforms[fingerID];
            fBones._proximal.LookAt(fBones._proximal.position + v, fBones._proximal.up);
        }

        protected virtual bool CheckHandPoses()
        {
            return _handPoseNeutral && _handPoseClosed && _handPoseThumbUp;
        }

        public FingerBones this[int i]
        {
            get
            {
                if (i == 0) return _fingerBonesThumb;
                if (i == 1) return _fingerBonesIndex;
                if (i == 2) return _fingerBonesMiddle;
                if (i == 3) return _fingerBonesRing;

                return _fingerBonesLittle;
            }
        }

        protected virtual void GetAngleLimitsClose(QuickHumanFingers finger, out float min, out float max)
        {
            if (finger == QuickHumanFingers.Thumb)
            {
                min = -45;
                max = 45;
            }
            else
            {
                min = 0;
                max = 90;
            }
        }

        protected virtual float GetMaxAngleSeparation(QuickHumanFingers finger)
        {
            return finger == QuickHumanFingers.Thumb ? 60.0f : 10.0f;
        }

        public virtual Collider[] GetColliders()
        {
            return _colliders;
        }

        #endregion

        #region UPDATE

        public virtual void Update()
        {
            if (CheckHandPoses())
            {
                UpdateInputStates();
                UpdateFingers();
            }
        }

        protected virtual void UpdateInputStates()
        {
            if (_handGestureSettings != null)
            {
                _isPointing = _handGestureSettings.IsPointing();
                _isThumbUp = _handGestureSettings.IsThumbUp();
                _closeFactor = InputManagerVR.GetAxis(_isLeft ? InputManagerVR.AxisCode.LeftGrip : InputManagerVR.AxisCode.RightGrip);
            }

            _blendPoint = InputValueRateChange(_isPointing, _blendPoint);
            _blendThumbUp = InputValueRateChange(_isThumbUp, _blendThumbUp);
        }

        private float InputValueRateChange(bool isDown, float value)
        {
            float rateDelta = Time.deltaTime * INPUT_RATE_CHANGE;
            float sign = isDown ? 1.0f : -1.0f;
            return Mathf.Clamp01(value + rateDelta * sign);
        }

        protected virtual void UpdateFingers()
        {
            for (int i = 0; i < 5; i++)
            {
                QuickHumanFingers f = _humanFingers[i];
                FingerBones fBones = _fingerBoneTransforms[f];
                QuickFingerPose fPose = new QuickFingerPose();

                //Close factor
                for (int j = 0; j < 3; j++)
                {
                    if (i == 0 && _blendThumbUp > 0)
                    {
                        fPose.SetCloseFactor(j, Mathf.Lerp(_handPoseNeutral[i].GetCloseFactor(j), _handPoseThumbUp[i].GetCloseFactor(j), _blendThumbUp));
                    }
                    else if (i == 1 && _blendPoint > 0)
                    {
                        fPose.SetCloseFactor(j, Mathf.Lerp(_handPoseNeutral[i].GetCloseFactor(j), 0, _blendPoint));
                    }
                    else
                    {
                        fPose.SetCloseFactor(j, Mathf.Lerp(_handPoseNeutral[i].GetCloseFactor(j), _handPoseClosed[i].GetCloseFactor(j), _closeFactor));
                    }
                }

                //Separation factor
                if (i == 0 && _blendThumbUp > 0)
                {
                    fPose._separation = Mathf.Lerp(_handPoseNeutral[i]._separation, _handPoseThumbUp[i]._separation, _blendThumbUp);
                }
                else
                {
                    fPose._separation = Mathf.Lerp(_handPoseNeutral[i]._separation, _handPoseClosed[i]._separation, _closeFactor);
                }

                if (fBones.CheckTransforms())
                {
                    ResetFingerRotation(fBones);
                    UpdateFingerClose(f, fPose, fBones);
                    UpdateFingerSeparation(f, fPose, fBones);
                }
            }
        }

        protected virtual void ResetFingerRotation(FingerBones fBones)
        {
            for (int i = 0; i < QuickHumanTrait.NUM_BONES_PER_FINGER; i++)
            {
                fBones[i].localRotation = Quaternion.identity;
            }
        }

        protected virtual void UpdateFingerClose(QuickHumanFingers f, QuickFingerPose fPose, FingerBones fBones)
        {
            Vector3 rotAxis = fBones.GetRotAxisClose();

            GetAngleLimitsClose(f, out float minAngle, out float maxAngle);
            
            for (int i = 0; i < 3; i++)
            {
                float rotAngle = Mathf.Lerp(minAngle, maxAngle, fPose.GetCloseFactor(i));
                fBones[i].Rotate(rotAxis, rotAngle, Space.Self);
            }
        }

        protected virtual void UpdateFingerSeparation(QuickHumanFingers f, QuickFingerPose fPose, FingerBones fBones)
        {
            Vector3 rotAxis = fBones.GetRotAxisSeparate();
            float t = (fPose._separation + 1) / 2.0f;
            float maxAngle = GetMaxAngleSeparation(f);
            float rotAngle = Mathf.Lerp(-maxAngle, maxAngle, t);

            fBones[0].Rotate(rotAxis, rotAngle, Space.Self);
        }

        #endregion

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            foreach (var pair in _fingerBoneTransforms)
            {
                if (pair.Value.CheckTransforms())
                {
                    for (int i = 1; i < QuickHumanTrait.NUM_BONES_PER_FINGER; i++)
                    {
                        Gizmos.DrawLine(pair.Value[i - 1].position, pair.Value[i].position);
                    }
                }
            }
        }

        #endregion

    }

}



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

        public QuickHandPose _handPose = null;

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

            public virtual Transform[] ToArray()
            {
                return new Transform[] { _proximal, _intermediate, _distal, _tip };
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

        public FingerBones _fingerBonesThumb = new FingerBones();
        public FingerBones _fingerBonesIndex = new FingerBones();
        public FingerBones _fingerBonesMiddle = new FingerBones();
        public FingerBones _fingerBonesRing = new FingerBones();
        public FingerBones _fingerBonesLittle = new FingerBones();

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

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual float GetMaxAngleClose(QuickHumanFingers finger)
        {
            return finger == QuickHumanFingers.Thumb? 45.0f : 90.0f;
        }

        protected virtual float GetMaxAngleSeparation(QuickHumanFingers finger)
        {
            return finger == QuickHumanFingers.Thumb ? 60.0f : 10.0f;
        }

        #endregion

        #region UPDATE

        public virtual void Update()
        {
            if (_handPose)
            {
                for (int i = 0; i < 5; i++)
                {
                    QuickHumanFingers f = _humanFingers[i];
                    FingerBones fBones = _fingerBoneTransforms[f];
                    QuickFingerPose fPose = _handPose[i];

                    if (fBones.CheckTransforms())
                    {
                        ResetFingerRotation(fBones);
                        UpdateFingerClose(f, fPose, fBones);
                        UpdateFingerSeparation(f, fPose, fBones);
                    }
                }
            }
        }

        protected virtual void ResetFingerRotation(FingerBones fBones)
        {
            foreach (Transform t in fBones.ToArray())
            {
                t.localRotation = Quaternion.identity;
            }
        }

        protected virtual void UpdateFingerClose(QuickHumanFingers f, QuickFingerPose fPose, FingerBones fBones)
        {
            Vector3 rotAxis = fBones.GetRotAxisClose();
            
            float maxAngle = GetMaxAngleClose(f);
            
            for (int i = 0; i < 3; i++)
            {
                float rotAngle = Mathf.Lerp(0.0f, maxAngle, fPose.GetCloseFactor(i));
                if (i == 0 && f == QuickHumanFingers.Thumb)
                {
                    rotAngle = Mathf.Min(25.0f, rotAngle);
                }

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
                    DebugFinger(pair.Value.ToArray());
                }
            }
        }

        protected virtual void DebugFinger(Transform[] fBones)
        {
            for (int i = 1; i < fBones.Length; i++)
            {
                Gizmos.DrawLine(fBones[i - 1].position, fBones[i].position);
            }
        }

        #endregion

    }

}



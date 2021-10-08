using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QuickVR
{
    [System.Serializable]
    public class QuickTransformAnimationCurve
    {
        #region PROTECTED PARAMETERS

        [SerializeField]
        protected AnimationCurve _positionXCurve = new AnimationCurve();

        [SerializeField]
        protected AnimationCurve _positionYCurve = new AnimationCurve();

        [SerializeField] 
        protected AnimationCurve _positionZCurve = new AnimationCurve();

        protected AnimationCurve _rotationXCurve = new AnimationCurve();
        protected AnimationCurve _rotationYCurve = new AnimationCurve();
        protected AnimationCurve _rotationZCurve = new AnimationCurve();
        protected AnimationCurve _rotationWCurve = new AnimationCurve();

        #endregion

        #region CREATION AND DESTRUCTION

        public QuickTransformAnimationCurve()
        {
                        
        }

        public QuickTransformAnimationCurve(ref List<Vector3> positions, ref List<Quaternion> rotations, ref List<float> timeStamps)
        {
            if (positions.Count != timeStamps.Count || rotations.Count != timeStamps.Count)
            {
                QuickVRManager.LogWarning("Lists of positions, rotations and timeStamps have different size");
                return;
            }

            for (int i = 0; i < timeStamps.Count; i++) // for every captured keyframe
            {
                AddPosition(timeStamps[i], positions[i]);
                AddRotation(timeStamps[i], rotations[i]);
            }
        }

        #endregion

        #region GET AND SET

        public virtual void SetWrapMode(WrapMode mode)
        {
            _positionXCurve.postWrapMode = _positionYCurve.postWrapMode = _positionZCurve.postWrapMode = mode;
            _rotationXCurve.postWrapMode = _rotationYCurve.postWrapMode = _rotationZCurve.postWrapMode = _rotationWCurve.postWrapMode = mode;
        }

        public virtual void AddPosition(float time, Vector3 pos)
        {
            _positionXCurve.AddKey(time, pos.x);
            _positionYCurve.AddKey(time, pos.y);
            _positionZCurve.AddKey(time, pos.z);
        }

        public virtual void AddRotation(float time, Quaternion rot)
        {
            _rotationXCurve.AddKey(time, rot.x);
            _rotationYCurve.AddKey(time, rot.y);
            _rotationZCurve.AddKey(time, rot.z);
            _rotationWCurve.AddKey(time, rot.w);
        }

        public virtual Vector3 SamplePosition(float time)
        {
            return new Vector3(_positionXCurve.Evaluate(time), _positionYCurve.Evaluate(time), _positionZCurve.Evaluate(time));
        }

        public virtual Quaternion SampleRotation(float time)
        {
            float timeA = 0.0f;
            int i = 0;
            Keyframe[] kFrames = _rotationXCurve.keys;
            while (i < kFrames.Length && kFrames[i].time < time)
            {
                timeA = kFrames[i].time;
                i++;
            }
            float timeB = timeA;
            if ((i + 1) < kFrames.Length)
                timeB = kFrames[i + 1].time;

            Quaternion rotationA = GetQuaternion(timeA);
            Quaternion rotationB = GetQuaternion(timeB);

            float t = (time - timeA) / (timeB - timeA);

            return Quaternion.Slerp(rotationA, rotationB, t);
            
        }

        protected virtual Quaternion GetQuaternion(float time)
        {
            float x = _rotationXCurve.Evaluate(time);
            float y = _rotationYCurve.Evaluate(time);
            float z = _rotationZCurve.Evaluate(time);
            float w = _rotationWCurve.Evaluate(time);

            Quaternion sampledRotation = new Quaternion(x, y, z, w);
            return sampledRotation;
        }

        #endregion
    }
}



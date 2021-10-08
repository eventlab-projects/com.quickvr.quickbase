using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QuickVR
{
    public class QuickIKAnimationCurve
    {
        #region PROTECTED PARAMETERS

        protected AnimationCurve _positionXCurve;
        protected AnimationCurve _positionYCurve;
        protected AnimationCurve _positionZCurve;

        protected AnimationCurve _forwardXCurve;
        protected AnimationCurve _forwardYCurve;
        protected AnimationCurve _forwardZCurve;

        #endregion

        #region CREATION AND DESTRUCTION

        public QuickIKAnimationCurve(ref List<Vector3> positions, ref List<Vector3> forwards, ref List<float> timeStamps)
        {
            if (positions.Count != timeStamps.Count || forwards.Count != timeStamps.Count)
            {
                QuickVRManager.LogError("Lists of positions, rotations and timeStamps have different size");
                return;
            }

            _positionXCurve = new AnimationCurve();
            _positionYCurve = new AnimationCurve();
            _positionZCurve = new AnimationCurve();

            _forwardXCurve = new AnimationCurve();
            _forwardYCurve = new AnimationCurve();
            _forwardZCurve = new AnimationCurve();

            for (int i = 0; i < timeStamps.Count; i++) // for every captured keyframe
            {
                _positionXCurve.AddKey(timeStamps[i], positions[i].x);
                _positionYCurve.AddKey(timeStamps[i], positions[i].y);
                _positionZCurve.AddKey(timeStamps[i], positions[i].z);

                _forwardXCurve.AddKey(timeStamps[i], forwards[i].x);
                _forwardYCurve.AddKey(timeStamps[i], forwards[i].y);
                _forwardZCurve.AddKey(timeStamps[i], forwards[i].z);
            }
        }

        #endregion

        #region GET AND SET

        public virtual Vector3 SamplePosition(float time)
        {
            float x = _positionXCurve.Evaluate(time);
            float y = _positionYCurve.Evaluate(time);
            float z = _positionZCurve.Evaluate(time);

            Vector3 sampledPosition = new Vector3(x, y, z);
            return sampledPosition;
        }

        public virtual Vector3 SampleForward(float time)
        {
            float x = _forwardXCurve.Evaluate(time);
            float y = _forwardYCurve.Evaluate(time);
            float z = _forwardZCurve.Evaluate(time);

            Vector3 sampledForward = new Vector3(x, y, z);
            return sampledForward;
        }

        #endregion
    }
}



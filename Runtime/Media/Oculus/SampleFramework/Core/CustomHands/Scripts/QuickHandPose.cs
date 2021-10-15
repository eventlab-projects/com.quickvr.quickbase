using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{
    [System.Serializable]
    public class QuickFingerPose
    {

        #region PUBLIC ATTRIBUTES

        [Range(0.0f, 1.0f)]
        public float _close0 = 0;

        [Range(0.0f, 1.0f)]
        public float _close1 = 0;

        [Range(0.0f, 1.0f)]
        public float _close2 = 0;

        [Range(-1.0f, 1.0f)]
        public float _separation = 0;

        #endregion

        #region GET AND SET

        public virtual float GetCloseFactor(int i)
        {
            if (i == 0) return _close0;
            if (i == 1) return _close1;

            return _close2;
        }

        #endregion

    }

    [CreateAssetMenu(fileName = "HandPose", menuName = "QuickVR/QuickHandPose", order = 1)]
    public class QuickHandPose : ScriptableObject
    {

        public QuickFingerPose _thumbPose = new QuickFingerPose();
        public QuickFingerPose _indexPose = new QuickFingerPose();
        public QuickFingerPose _middlePose = new QuickFingerPose();
        public QuickFingerPose _ringPose = new QuickFingerPose();
        public QuickFingerPose _littlePose = new QuickFingerPose();

        #region GET AND SET

        public QuickFingerPose this[int i] 
        {
            get
            {
                if (i == 0) return _thumbPose;
                if (i == 1) return _indexPose;
                if (i == 2) return _middlePose;
                if (i == 3) return _ringPose;
                if (i == 4) return _littlePose;

                return null;
            }
        }

        #endregion

    }

}



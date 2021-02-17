using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace QuickVR
{

    [CreateAssetMenu(fileName = "QuickSpeedCurve", menuName = "QuickVR/QuickSpeedCurve", order = 1)]
    [System.Serializable]
    public class QuickSpeedCurveAsset : ScriptableObject
    {

        public AnimationCurve _animationCurve = new AnimationCurve();

        #region GET AND SET

        public virtual float Evaluate(float dt)
        {
            return _animationCurve.Evaluate(dt);
        }

        #endregion

    }

}

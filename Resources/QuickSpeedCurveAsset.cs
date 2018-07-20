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

    }

}

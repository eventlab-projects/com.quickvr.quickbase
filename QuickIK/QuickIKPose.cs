using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    [System.Serializable]
    public class QuickIKData
    {
        public Vector3 _targetLimbLocalPosition = Vector3.zero;
        public Quaternion _targetLimbLocalRotation = Quaternion.identity;
        public Vector3 _targetHintLocalPosition = Vector3.zero;
    }

}

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

        public QuickIKData(Vector3 limbLocalPos, Quaternion limbLocalRot, Vector3 hintLocalPos)
        {
            _targetLimbLocalPosition = limbLocalPos;
            _targetLimbLocalRotation = limbLocalRot;
            _targetHintLocalPosition = hintLocalPos;
        }
    }

    public class QuickIKPose : ScriptableObject
    {
        public QuickIKData _ikDataHead = null;
        public QuickIKData _ikDataLeftHand = null;
        public QuickIKData _ikDataRightHand = null;
        public QuickIKData _ikDataLeftFoot = null;
        public QuickIKData _ikDataRightFoot = null;
    }

}

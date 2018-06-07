using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    [System.Serializable]
    public class QuickIKData
    {
        public Vector3 _targetLimbPosition = Vector3.zero;
        public Quaternion _targetLimbRotation = Quaternion.identity;
        public Vector3 _targetHintPosition = Vector3.zero;

        public QuickIKData(Vector3 limbPos, Quaternion limbRot, Vector3 hintPos)
        {
            _targetLimbPosition = limbPos;
            _targetLimbRotation = limbRot;
            _targetHintPosition = hintPos;
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

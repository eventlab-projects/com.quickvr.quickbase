using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickIKSolverHand : QuickIKSolver
    {

        protected Animator _animator
        {
            get
            {
                if (!m_Animator)
                {
                    m_Animator = GetComponentInParent<Animator>();
                }

                return m_Animator;
            }
        }
        protected Animator m_Animator;

        protected bool _isLeftHand
        {
            get
            {
                return _boneLimb == _animator.GetBoneTransform(HumanBodyBones.LeftHand);
            }
        }

        protected Transform _foreArmCorrector
        {
            get
            {
                if (!m_foreArmCorrector) m_foreArmCorrector = transform.CreateChild("__ForeArmCorrector__");
                return m_foreArmCorrector;
            }
        }
        protected Transform m_foreArmCorrector;

        protected Transform _upperBoneReference
        {
            get
            {
                if (!m_UpperBoneReference)
                {
                    m_UpperBoneReference = _boneUpper.CreateChild("__BoneUpperReference__");
                    m_UpperBoneReference.LookAt(_boneMid, Vector3.up);
                }

                return m_UpperBoneReference;
            }
        }
        protected Transform m_UpperBoneReference = null;

        #region GET AND SET

        protected override Vector3 GetIKTargetHintPosition(float ikAngle)
        {
            //float r = GetChainLength();
            //float maxY = _boneUpper.position.y + r;

            //float t = Mathf.Clamp01((maxY - _targetLimb.position.y) / (r));

            ////return Vector3.Lerp(_targetLimb.position - Vector3.up * (GetMidLength() + QuickIKManager.DEFAULT_TARGET_HINT_DISTANCE), _targetHint.position, t);
            ////return Vector3.Lerp(_boneUpper.position + Vector3.Lerp(_animator.transform.forward, Vector3.down, 0.5f).normalized * (QuickIKManager.DEFAULT_TARGET_HINT_DISTANCE), _targetHint.position, t);
            //return Vector3.Lerp(_boneMid.position - Vector3.up * (GetMidLength() + QuickIKManager.DEFAULT_TARGET_HINT_DISTANCE), _targetHint.position, t);

            float uLength = GetUpperLength();
            int sign = _isLeftHand ? -1 : 1;
            Vector3 elbowPos = _boneUpper.position + _upperBoneReference.forward * (uLength * Mathf.Cos(ikAngle)) + sign * _upperBoneReference.right * (uLength * Mathf.Sin(ikAngle));

            _targetHint.position = elbowPos;

            return _targetHint.position;
        }

        #endregion

        #region UPDATE

        public override void UpdateIK()
        {
            base.UpdateIK();

            //Correct the rotations of the wrist and forearm by applying human body constraints
            float boneMidWeight = 0.5f;
            float rotAngle = _targetLimb.localEulerAngles.z * boneMidWeight;
            Vector3 rotAxis = (_boneLimb.position - _boneMid.position);

            //Apply the rotation to the forearm
            Quaternion limbRot = _boneLimb.rotation;
            _foreArmCorrector.forward = rotAxis;
            Vector3 upBefore = _foreArmCorrector.up;
            _foreArmCorrector.Rotate(rotAxis, rotAngle, Space.World);
            if (Vector3.Dot(upBefore, _foreArmCorrector.up) < 0)
            {
                rotAngle += 180.0f;
            }
            _boneMid.Rotate(rotAxis, rotAngle, Space.World);

            //Restore the rotation of the limb
            _boneLimb.rotation = limbRot;
        }

        #endregion

    }

}

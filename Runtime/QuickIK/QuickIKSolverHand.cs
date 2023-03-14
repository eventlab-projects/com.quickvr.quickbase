using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

    public class QuickIKSolverHand : QuickIKSolver
    {

        protected Transform _foreArmCorrector
        {
            get
            {
                if (!m_foreArmCorrector) m_foreArmCorrector = transform.CreateChild("__ForeArmCorrector__");
                return m_foreArmCorrector;
            }
        }
        protected Transform m_foreArmCorrector;

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

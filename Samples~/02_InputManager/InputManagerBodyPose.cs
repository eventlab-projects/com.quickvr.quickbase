using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using QuickVR;

public class InputManagerBodyPose : BaseInputManager 
{

    #region PUBLIC ATTRIBUTES

    public Animator _animator = null;

    public float _deadLeanAngle = 5.0f;
    public float _maxLeanAngle = 15.0f;

    public float _tPoseThresholdAngle = 25.0f;

    public enum AxisCodes
    {
        LeanForward,
        LeanSide,
    }

    public enum ButtonCodes
    {
        TPose,
    }

    #endregion

    #region GET AND SET

    public override string[] GetAxisCodes()
    {
        return GetCodes<AxisCodes>();
    }

    public override string[] GetButtonCodes()
    {
        return GetCodes<ButtonCodes>();
    }

    #endregion

    #region INPUT MANAGEMENT

    protected override float ImpGetAxis(string axis)
    {
        float result = 0;

        if (_animator)
        {
            Transform tHead = _animator.GetBoneTransform(HumanBodyBones.Head);
            Transform tHips = _animator.GetBoneTransform(HumanBodyBones.Hips);

            Vector3 bodyPose = tHead.position - tHips.position;
            float angle = 0;

            if (axis == AxisCodes.LeanForward.ToString())
            {
                //Compute the lean angle forward
                Vector3 v = Vector3.ProjectOnPlane(bodyPose, _animator.transform.right);
                angle = Vector3.SignedAngle(_animator.transform.up, v, _animator.transform.right);
            }
            else if (axis == AxisCodes.LeanSide.ToString())
            {
                //Compute the lean angle to the side
                Vector3 v = Vector3.ProjectOnPlane(bodyPose, _animator.transform.forward);
                angle = Vector3.SignedAngle(_animator.transform.up, v, -_animator.transform.forward);
            }

            if (Mathf.Abs(angle) > _deadLeanAngle)
            {
                //Normalize the angle value
                result = Mathf.Sign(angle) * Mathf.Clamp01(Mathf.Abs(angle) / _maxLeanAngle);
            }
        }

        return result;
    }

    protected override bool ImpGetButton(string buttonName)
    {
        bool result = false;
        if (buttonName == ButtonCodes.TPose.ToString())
        {
            bool isTPoseLeft = IsArmTPose(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftHand);
            bool isTPoseRight = IsArmTPose(HumanBodyBones.RightUpperArm, HumanBodyBones.RightHand);
            result = isTPoseLeft && isTPoseRight;
        }

        return result;
    }

    protected virtual bool IsArmTPose(HumanBodyBones boneUpperID, HumanBodyBones boneHandID)
    {
        Vector3 v = _animator.GetBoneTransform(boneHandID).position - _animator.GetBoneTransform(boneUpperID).position;

        float angleUp = Vector3.Angle(_animator.transform.up, Vector3.ProjectOnPlane(v, _animator.transform.forward));
        float angleFwd = Vector3.Angle(_animator.transform.forward, Vector3.ProjectOnPlane(v, _animator.transform.up));

        bool b1 = (angleUp >= 90.0f - _tPoseThresholdAngle && angleUp <= 90.0f + _tPoseThresholdAngle);
        bool b2 = (angleFwd >= 90.0f - _tPoseThresholdAngle && angleFwd <= 90.0f + _tPoseThresholdAngle);

        return b1 && b2;
    }

    #endregion

}

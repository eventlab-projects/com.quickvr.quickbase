using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using QuickVR;

[System.Serializable]
public class InputManagerHandPosition : BaseInputManager {

    //#region PUBLIC PARAMETERS

    //public enum AxisCodes {
    //    Horizontal, 
    //    Vertical, 
    //    Forward,
    //}

    //public float _radiusDeadZone = 0.1f;

    //#endregion

    //#region PROTECTED PARAMETERS

    //protected float _upperArmLength;
    //protected float _lowerArmLength;
    //protected float _armLength;

    //#endregion

    //#region CONSTANTS

    //protected const string HAND_POSITION_ORIGIN = "__HandPositionOrigin__";

    //protected Animator _animator = null;
    //protected Transform _origin = null;

    //#endregion

    //#region CREATION AND DESTRUCTION

    //protected override void OnEnable() {
    //    base.OnEnable();
    //    if (_user) _animator = _user.GetComponent<Animator>();
    //    if (_animator) {
    //        //Compute the arm length
    //        _upperArmLength = Vector3.Distance(_animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position, _animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position);
    //        _lowerArmLength = Vector3.Distance(_animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position, _animator.GetBoneTransform(HumanBodyBones.RightHand).position);
    //        _armLength = _upperArmLength + _lowerArmLength;
    //        CreateOrigin();
    //    }
    //}

    //protected virtual void CreateOrigin() {
    //    _origin = QuickUtils.FindOrCreate(HAND_POSITION_ORIGIN).transform;
    //    Vector3 refPos = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position;
    //    _origin.position = refPos + _animator.transform.forward * _lowerArmLength - _animator.transform.up * _upperArmLength;
    //    _origin.parent = _animator.GetBoneTransform(HumanBodyBones.RightShoulder);
    //}

    //#endregion

    //#region INPUT MANAGEMENT

    //protected override float ImpGetAxis(string axis) {
    //    Vector3 handPos = _animator.GetBoneTransform(HumanBodyBones.RightHand).position - _origin.position;
    //    float d = 0.0f;
    //    if (axis == "Horizontal") d = Vector3.Dot(handPos, _animator.transform.right);
    //    else if (axis == "Vertical") d = Vector3.Dot(handPos, _animator.transform.up);
    //    else if (axis == "Forward") d = Vector3.Dot(handPos, _animator.transform.forward);

    //    //Rescale the axis value by taking into account the corresponding deadZone
    //    float value = d / (_lowerArmLength);
    //    value = Mathf.Sign(value) * Mathf.Clamp01(Mathf.Abs(value));
    //    float v = Mathf.Clamp01((Mathf.Abs(value) - _radiusDeadZone) / (1.0f - _radiusDeadZone));
    //    return Mathf.Sign(value) * v;
    //}

    protected override float ImpGetAxis(string axis)
    {
        return 0.0f;
    }

    //protected override bool ImpGetButton(string button) {
    //    return false;
    //}

    protected override bool ImpGetButton(string button)
    {
        return false;
    }

    //#endregion

    //#region DEBUG

    //protected virtual void OnDrawGizmos() {
    //    //The Origin
    //    Gizmos.color = Color.yellow;
    //    if (_origin) Gizmos.DrawWireSphere(_origin.position, _radiusDeadZone);

    //    if (_animator && _origin) {
    //        Gizmos.color = Color.black;
    //        Vector3 handPos = _animator.GetBoneTransform(HumanBodyBones.RightHand).position - _origin.position;
    //        Gizmos.DrawLine(_origin.position, _animator.GetBoneTransform(HumanBodyBones.RightHand).position);

    //        //The forward contribution
    //        Gizmos.color = Color.blue;
    //        Gizmos.DrawLine(_origin.position, _origin.position + _animator.transform.forward * Vector3.Dot(handPos, _animator.transform.forward));

    //        //The right contribution
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawLine(_origin.position, _origin.position + _animator.transform.right * Vector3.Dot(handPos, _animator.transform.right));

    //        //The up contribution
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawLine(_origin.position, _origin.position + _animator.transform.up * Vector3.Dot(handPos, _animator.transform.up));
    //    }
    //}

    //#endregion
}

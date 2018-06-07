using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using QuickVR;

[System.Serializable]
public class InputManagerHandCollider : BaseInputManager {

    //#region PUBLIC PARAMETERS

    //public enum AxisCodes {
    //    Horizontal, 
    //    Vertical, 
    //    Forward,
    //}

    //public enum ButtonCodes {
    //    ColliderForward,
    //    ColliderBackward,
    //    ColliderRight,
    //    ColliderLeft,
    //    ColliderUp,
    //    ColliderDown,
    //};

    //public enum InputMode {
    //    DISCRETE,
    //    CONTINUOUS,
    //};
    //public InputMode _inputMode = InputMode.CONTINUOUS;
    //public float _radiusColliderDirectional = 0.1f;
    //public float _radiusColliderHand = 0.05f;

    //public float _radiusDeadZone = 0.1f;

    //#endregion

    //#region PROTECTED PARAMETERS

    //protected Dictionary<ButtonCodes, Transform> _colliderDirection = new Dictionary<ButtonCodes, Transform>();
    //protected Transform _colliderHand = null;

    //protected float _upperArmLength;
    //protected float _lowerArmLength;
    //protected float _armLength;

    //#endregion

    //#region CONSTANTS

    //protected const string COLLIDER_HAND_NAME = "__ColliderHand__";
    //protected const string COLLIDER_SYSTEM_ORIGIN = "__HandColliderOrigin__";

    //protected Animator _animator = null;

    //protected Transform _origin = null;

    //#endregion

    //#region CREATION AND DESTRUCTION

    //protected override void OnEnable() {
    //    base.OnEnable();
    //    CreateButtonCodess();
    //    if (_user) _animator = _user.GetComponent<Animator>();
    //    if (_animator) {
    //        //Compute the arm length
    //        _upperArmLength = Vector3.Distance(_animator.GetBoneTransform(HumanBodyBones.RightUpperArm).position, _animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position);
    //        _lowerArmLength = Vector3.Distance(_animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position, _animator.GetBoneTransform(HumanBodyBones.RightHand).position);
    //        _armLength = _upperArmLength + _lowerArmLength;
    //        CreateUserHandCollider();
    //        SetCollidersPosition();
    //        CreateOrigin();
    //    }
    //}

    //protected virtual void CreateButtonCodess() {
    //    List<ButtonCodes> eValues = QuickUtils.GetEnumValues<ButtonCodes>();
    //    foreach (ButtonCodes dCol in eValues) {
    //        CreateButtonCodes(dCol);
    //    }
    //}

    //protected virtual Transform CreateButtonCodes(ButtonCodes cd) {
    //    string cName = "__" + cd.ToString() + "__";
    //    Transform t = QuickUtils.FindOrCreate(cName).transform;
    //    t.parent = transform;
    //    t.ResetTransformation();

    //    _colliderDirection[cd] = t;
    //    return t;
    //}

    //protected virtual void CreateUserHandCollider() {
    //    _colliderHand = QuickUtils.FindOrCreate(COLLIDER_HAND_NAME).transform;
    //    _colliderHand.parent = _animator.GetBoneTransform(HumanBodyBones.RightHand);
    //    _colliderHand.ResetTransformation();
    //}

    //protected virtual void CreateOrigin() {
    //    _origin = QuickUtils.FindOrCreate(COLLIDER_SYSTEM_ORIGIN).transform;
    //    _origin.position = Vector3.zero;
    //    foreach (var pair in _colliderDirection) {
    //        _origin.position += pair.Value.position;
    //    }
    //    _origin.position /= (float)_colliderDirection.Count;
    //    _origin.parent = _animator.GetBoneTransform(HumanBodyBones.RightShoulder);
    //}

    //#endregion

    //#region GET AND SET

    //protected virtual void SetCollidersPosition() {
    //    Transform root = _animator.GetBoneTransform(HumanBodyBones.RightShoulder);
    //    Transform origin = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

    //    Vector3 forward = _animator.transform.forward;
    //    Vector3 right = _animator.transform.right;
    //    Vector3 up = _animator.transform.up;

    //    //Compute the position of the forward collider
    //    float t = 0.0f;
    //    Vector3 dir = Vector3.Normalize(Vector3.Lerp(forward, -up, t));
    //    Transform xform = _colliderDirection[ButtonCodes.ColliderForward];
    //    xform.parent = root;
    //    xform.position = origin.position + dir * _armLength;

    //    //Compute the position of the right collider
    //    t = 0.0f;
    //    dir = Vector3.Normalize(Vector3.Lerp(right, -up, t));
    //    xform = _colliderDirection[ButtonCodes.ColliderRight];
    //    xform.parent = root;
    //    xform.position = origin.position + dir * _armLength;

    //    //Compute the position of the up collider
    //    t = 0.4f;
    //    dir = Vector3.Normalize(Vector3.Lerp(up, forward, t));
    //    xform = _colliderDirection[ButtonCodes.ColliderUp];
    //    xform.parent = root;
    //    xform.position = origin.position + dir * _armLength;

    //    //Compute the position of the down collider
    //    t = 0.4f;
    //    dir = Vector3.Normalize(Vector3.Lerp(-up, forward, t));
    //    xform = _colliderDirection[ButtonCodes.ColliderDown];
    //    xform.parent = root;
    //    xform.position = origin.position + dir * _armLength;

    //    //Compute the position of the left collider
    //    xform = _colliderDirection[ButtonCodes.ColliderLeft];
    //    xform.parent = root;
    //    xform.position = origin.position + forward * _upperArmLength -right * _lowerArmLength;

    //    //Compute the position of the backward collider
    //    t = 0.4f;
    //    dir = Vector3.Normalize(Vector3.Lerp(-up, forward, t));
    //    xform = _colliderDirection[ButtonCodes.ColliderBackward];
    //    xform.parent = root;
    //    xform.position = origin.position + dir * _upperArmLength;
    //}

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
    //    ButtonCodes cd = (ButtonCodes)System.Enum.Parse(typeof(ButtonCodes), button);
    //    Transform dCollider = _colliderDirection[cd];
    //    return (Vector3.Distance(dCollider.position, _colliderHand.position) < (_radiusColliderDirectional + _radiusColliderHand));
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

    //    if (_inputMode == InputMode.DISCRETE) {
    //        //The Directional Colliders
    //        Gizmos.color = Color.magenta;
    //        foreach (var pair in _colliderDirection) {
    //            Gizmos.DrawWireSphere(pair.Value.position, _radiusColliderDirectional);
    //        }

    //        //The Hand Collider 
    //        Gizmos.color = Color.cyan;
    //        if (_colliderHand) Gizmos.DrawWireSphere(_colliderHand.position, _radiusColliderHand);
    //    }
    //}

    //#endregion
}

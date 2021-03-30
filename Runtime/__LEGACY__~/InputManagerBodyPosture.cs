using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using QuickVR;

[System.Serializable]
public class InputManagerBodyPosture : BaseInputManager {

	#region PUBLIC PARAMETERS

	public enum AxisCodes 
	{
		Horizontal,
		Vertical,
		SupermanRotation,
		LeanForward,
		LeanBackward,
		TorsoRotation,
	}

	public enum ButtonCodes {
		SupermanPose, 
		TPose,
	}

	public float _deadAngleForward = 5.0f;	
	public float _maxAngleForward = 15.0f;

	public float _deadAngleBack = 5.0f;
	public float _maxAngleBack = 10.0f;

	public float _deadAngleStrafe = 5.0f;
	public float _maxAngleStrafe = 15.0f;

	public float _deadAngleHead = 10.0f;
	public float _maxAngleHead = 60.0f;

	public float _deadAngleArm = 5.0f;
	public float _maxAngleArm = 90.0f;

	public float _deadAngleSuperman = 5.0f;
	public float _maxAngleSuperman = 45.0f;

	public float _deadAngleTorso = 5.0f;
	public float _maxAngleTorso = 30.0f;

	#endregion

	#region PROTECTED PARAMETERS

    protected Animator _animator = null;
    
	protected Transform _head = null;
	protected Transform _hips = null;

	protected float _horizontalAxis = 0.0f;
	protected float _verticalAxis = 0.0f;

	protected float _rightArmElevationAxis = 0.0f;
	protected float _leftArmElevationAxis = 0.0f;

	protected Vector3 _leftArmDir = Vector3.zero;
	protected Vector3 _rightArmDir = Vector3.zero;

	#endregion

	#region CREATION AND DESTRUCTION

	protected virtual void Start() {
		_animator = QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorTarget();
		_head = _animator.GetBoneTransform(HumanBodyBones.Head);
		_hips = _animator.GetBoneTransform(HumanBodyBones.Hips);
	}

	#endregion

	#region GET AND SET

    public override string[] GetAxisCodes()
    {
        return GetCodes<InputManagerBodyPosture.AxisCodes>();
    }

    public override string[] GetButtonCodes()
    {
        return GetCodes<InputManagerBodyPosture.ButtonCodes>();
    }

	protected Vector3 ComputeBodyPosture() {
		return Vector3.Normalize(_head.position - _hips.position);
	}

	protected virtual float ComputeBodyAxisContribution(Vector3 newDir, Vector3 refDir, float deadAnglePositive, float maxAnglePositive, float deadAngleNegative, float maxAngleNegative) {
		float d = Vector3.Dot(newDir.normalized, refDir.normalized);
		float sign = Mathf.Sign(d);
		float angle = 90.0f - (Mathf.Acos(Mathf.Abs(d)) * Mathf.Rad2Deg);

		float maxAngle, deadAngle;
		if (sign > 0) {
			deadAngle = deadAnglePositive;
			maxAngle = maxAnglePositive;
		}
		else {
			deadAngle = deadAngleNegative;
			maxAngle = maxAngleNegative;
		}

		return (sign *  ClampAngleContribution(angle, deadAngle, maxAngle));
	}

	protected virtual float ClampAngleContribution(float angle, float deadAngle, float maxAngle) {
		return Mathf.Clamp01((angle - deadAngle) / (maxAngle - deadAngle));
	}

	protected virtual float ComputeSupermanRotationAxisContribution() {
		if (!_animator) return 0.0f;

		if (IsSupermanPose()) {
			//Both arms are in aprox 90º with respect to the up vector of the user. 
			//Check if arms are more or less parallel
			Vector3 targetForward = Vector3.Lerp(_leftArmDir, _rightArmDir, 0.5f);
			float angle = Vector3.Angle(_animator.transform.forward, targetForward);
			float t = ClampAngleContribution(angle, _deadAngleSuperman, _maxAngleSuperman);
			float sign = Mathf.Sign(Vector3.Dot(targetForward, _animator.transform.right));

//			Debug.Log("angle = " + angle.ToString("f3"));
//			Debug.Log("sign = " + sign.ToString("f3"));
			return sign * t;
		}

		return 0.0f;
	}

	protected virtual Vector3 ComputeTorsoForward() {
		if (!_animator) return Vector3.zero;

		Vector3 u = _animator.GetBoneTransform(HumanBodyBones.RightShoulder).position - _animator.GetBoneTransform(HumanBodyBones.LeftShoulder).position;
		Vector3 v = _animator.GetBoneTransform(HumanBodyBones.Hips).position - _animator.GetBoneTransform(HumanBodyBones.RightShoulder).position;
		Vector3 w = Vector3.Cross(v, u);

		return Vector3.ProjectOnPlane(w, _animator.transform.up).normalized;
	}

	protected virtual bool IsArmsPerpendicularToBody() {
		if (!_animator) return false;

		float dLeft = Vector3.Dot(_animator.transform.up, _leftArmDir);
		float dRight = Vector3.Dot(_animator.transform.up, _rightArmDir);
		
		float dThreshold = 0.3f;
		return ((Mathf.Abs(dLeft) <= dThreshold) && (Mathf.Abs(dRight) <= dThreshold));
	}

	protected virtual bool IsSupermanPose() {
		if (!IsArmsPerpendicularToBody()) return false;

		// Check if the arms are more or less parallel
		float d = Vector3.Dot(_leftArmDir, _rightArmDir);

		//Check if the arms are more or less parallel
		return (d >= 0.75f);	
	}

	protected virtual bool IsTPose() {
		if (!IsArmsPerpendicularToBody()) return false;

		float dLeft = Vector3.Dot(_leftArmDir, _animator.transform.forward);
		float dRight = Vector3.Dot(_rightArmDir, _animator.transform.forward);

		float dThreshold = 0.2f;
		return ((dLeft <= dThreshold) && (dRight <= dThreshold));
	}

	#endregion

	#region UPDATE

    protected virtual void Update() {
        if (!_animator) return; 

		_leftArmDir = (_animator.GetBoneTransform(HumanBodyBones.LeftHand).position - _animator.GetBoneTransform(HumanBodyBones.LeftShoulder).position).normalized; 
		_rightArmDir = (_animator.GetBoneTransform(HumanBodyBones.RightHand).position - _animator.GetBoneTransform(HumanBodyBones.RightShoulder).position).normalized; 

		//Compute the contribution of the vertical and horizontal axes
		Vector3 bodyPosture = ComputeBodyPosture();
		_verticalAxis = ComputeBodyAxisContribution(bodyPosture, _animator.transform.forward, _deadAngleForward, _maxAngleForward, _deadAngleBack, _maxAngleBack);
		_horizontalAxis = ComputeBodyAxisContribution(bodyPosture, _animator.transform.right, _deadAngleStrafe, _maxAngleStrafe, _deadAngleStrafe, _maxAngleStrafe);
	}

	protected virtual float ComputeTorsoRotationAxisContribution() {
		if (!_animator) return 0.0f;

		Vector3 fwd = _animator.transform.forward;
		Vector3 torsoFwd = ComputeTorsoForward();
		if (torsoFwd == Vector3.zero) return 0.0f;

		Vector3 rotAxis = Vector3.Cross(fwd, torsoFwd).normalized;
		float rotAngle = Vector3.Angle(fwd, torsoFwd);
		float sign = Mathf.Sign(Vector3.Dot(_animator.transform.up, rotAxis));
		float axisValue = sign * ClampAngleContribution(rotAngle, _deadAngleTorso, _maxAngleTorso);

		return axisValue;
	}

	#endregion

	#region INPUT MANAGEMENT

	protected override float ImpGetAxis(string axis) {
        if (axis == AxisCodes.Horizontal.ToString()) return _horizontalAxis;
        else if (axis == AxisCodes.Vertical.ToString())
        {
            //Debug.Log("_verticalAxis = " + _verticalAxis.ToString("f3"));
            return _verticalAxis;
        }
        else if (axis == AxisCodes.SupermanRotation.ToString()) return ComputeSupermanRotationAxisContribution();
        else if (axis == AxisCodes.LeanForward.ToString()) return (_verticalAxis > 0) ? _verticalAxis : 0.0f;
        else if (axis == AxisCodes.LeanBackward.ToString()) return (_verticalAxis < 0) ? _verticalAxis : 0.0f;
        else if (axis == AxisCodes.TorsoRotation.ToString())
        {
            float torsoRotationAxis = ComputeTorsoRotationAxisContribution();
            if (Mathf.Sign(torsoRotationAxis) == Mathf.Sign(_horizontalAxis))
            {
                return Mathf.Sign(torsoRotationAxis) * Mathf.Clamp01(Mathf.Abs(torsoRotationAxis) + Mathf.Abs(_horizontalAxis));
            }
            else
            {
                float torsoAbs = Mathf.Abs(torsoRotationAxis);
                float horizontalAbs = Mathf.Abs(_horizontalAxis);
                return (torsoAbs > horizontalAbs) ? torsoRotationAxis : _horizontalAxis;
            }
        }
		return 0.0f;
	}

	protected override bool ImpGetButton(string buttonName) {
		if (buttonName == ButtonCodes.SupermanPose.ToString()) return IsSupermanPose();
		else if (buttonName == ButtonCodes.TPose.ToString()) return IsTPose();
		return false;
	}

	#endregion

	#region DEBUG

	protected virtual void OnDrawGizmos() 
	{

		if (!_animator) return;

		//The hips to head vector
		Gizmos.color = Color.magenta;
		Gizmos.DrawLine(_hips.position, _head.position);

		//The user's up vector
		Gizmos.color = Color.green;
		Gizmos.DrawLine(_hips.position, _hips.position + _animator.transform.up);
	}

	#endregion
}

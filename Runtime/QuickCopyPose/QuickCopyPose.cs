//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using QuickVR;

//public class QuickCopyPose : MonoBehaviour
//{
//    #region EXTRA CLASSES

//    public enum CopyPoseLimbs
//    {
//        LeftArm,
//        RightArm,
//        LeftHand,
//        RightHand,
//        LeftLeg,
//        RightLeg,
//        Body
//    };

//    BoneInfo GetBoneInfo(HumanBodyBones id, Animator anim)
//    {
//        int boneid = (int)id.GetHashCode();
//        BoneInfo info = new BoneInfo();

//        info.boneId = boneid;
//        info.muscleId.x = HumanTrait.MuscleFromBone(boneid, 0);
//        info.muscleId.y = HumanTrait.MuscleFromBone(boneid, 1);
//        info.muscleId.z = HumanTrait.MuscleFromBone(boneid, 2);

//        info.muscleMax = HumanTrait.GetMuscleDefaultMax(boneid);
//        info.muscleMin = HumanTrait.GetMuscleDefaultMin(boneid);
//        info.transform = anim.GetBoneTransform(id);

//        return info;
//    }

//    public struct MuscleID
//    {
//        public int x;
//        public int y;
//        public int z;
//    }

//    public class BoneInfo
//    {
//        public int boneId;
//        public MuscleID muscleId;
//        public float muscleMax;
//        public float muscleMin;
//        public Transform transform;
//    }

//    #endregion

//    #region PUBLIC PARAMETERS

//    public GameObject _masterAvatar;

//    [BitMask(typeof(CopyPoseLimbs))]

//    public int _boneMask = -1;

//    #endregion

//    #region PROTECTED PARAMETERS

//    [SerializeField, HideInInspector]
//    protected List<CopyPoseLimbs> _Bones;

//    protected Animator _targetAnimator;
//    protected Animator _masterAnimator;


//    protected QuickHeadTracking _hTrackingMaster;

//    protected HumanPoseHandler _masterHumanPoseHandler;
//    protected HumanPoseHandler _targetHumanPoseHandler;

//    protected HumanPose _humanPoseMaster;
//    protected HumanPose _humanPoseTarget;
//    protected HumanPose _humanPoseFinal;

//    protected Quaternion _hipsRotationOffset;
//    protected Vector3 _hipsPositionOffset;

//    protected Dictionary<CopyPoseLimbs, List<HumanBodyBones>> _limbRelation;

//    #endregion

//    #region CREATION AND DESTRUCTION

//    public virtual void ForceStart()
//    {
//        Start();
//    }

//    protected virtual void Start()
//    {
//        _Bones = QuickUtils.GetEnumValues<CopyPoseLimbs>();
//        FillRelationDictionary();

//        _targetAnimator = GetComponentInChildren<Animator>();

//        if (_masterAvatar == null)
//        {
//            _masterAvatar = GameObject.Find("pf_MasterAvatar");
//            if (_masterAvatar == null)
//            {
//                Debug.LogError("QuickCopyPose: Can't find master avatar");
//                return;
//            }
//        }

//        _masterAnimator = _masterAvatar.GetComponent<Animator>();

//        _masterHumanPoseHandler = new HumanPoseHandler(_masterAnimator.avatar, _masterAnimator.transform);
//        _targetHumanPoseHandler = new HumanPoseHandler(_targetAnimator.avatar, _targetAnimator.transform);

//        _humanPoseMaster = new HumanPose();
//        _masterHumanPoseHandler.GetHumanPose(ref _humanPoseMaster);

//        _humanPoseTarget = new HumanPose();
//        _targetHumanPoseHandler.GetHumanPose(ref _humanPoseTarget);


//        _hipsPositionOffset = _targetAnimator.GetBoneTransform(HumanBodyBones.Hips).position - _masterAnimator.GetBoneTransform(HumanBodyBones.Hips).position;
//        _hipsRotationOffset = Quaternion.Inverse(_masterAnimator.GetBoneTransform(HumanBodyBones.Hips).rotation) * _targetAnimator.GetBoneTransform(HumanBodyBones.Hips).rotation;


//        _hTrackingMaster = _masterAvatar.GetComponentInChildren<QuickHeadTracking>(true);
//    }

//    protected virtual void OnEnable()
//    {
//        QuickVRManager.OnPostUpdateTracking += Copy;
//        QuickVRManager.OnPostUpdateTracking += UpdateWorldPosition;
//    }

//    protected virtual void OnDisable()
//    {
//        QuickVRManager.OnPostUpdateTracking -= Copy;
//        QuickVRManager.OnPostUpdateTracking -= UpdateWorldPosition;
//    }

//    protected void FillRelationDictionary()
//    {
//        List<HumanBodyBones> _leftArmBones = new List<HumanBodyBones>();
//        _leftArmBones.Add(HumanBodyBones.LeftShoulder);
//        _leftArmBones.Add(HumanBodyBones.LeftUpperArm);
//        _leftArmBones.Add(HumanBodyBones.LeftLowerArm);
//        _leftArmBones.Add(HumanBodyBones.LeftHand);

//        List<HumanBodyBones> _leftHandBones = new List<HumanBodyBones>();
//        for (int i = 24; i < 39; i++)
//            _leftHandBones.Add((HumanBodyBones)i);


//        List<HumanBodyBones> _rightArmBones = new List<HumanBodyBones>();
//        _rightArmBones.Add(HumanBodyBones.RightShoulder);
//        _rightArmBones.Add(HumanBodyBones.RightUpperArm);
//        _rightArmBones.Add(HumanBodyBones.RightLowerArm);
//        _rightArmBones.Add(HumanBodyBones.RightHand);

//        List<HumanBodyBones> _rightHandBones = new List<HumanBodyBones>();
//        for (int i = 39; i < 54; i++)
//            _rightHandBones.Add((HumanBodyBones)i);


//        List<HumanBodyBones> _leftLegBones = new List<HumanBodyBones>();
//        _leftLegBones.Add(HumanBodyBones.LeftUpperLeg);
//        _leftLegBones.Add(HumanBodyBones.LeftLowerLeg);
//        _leftLegBones.Add(HumanBodyBones.LeftFoot);
//        _leftLegBones.Add(HumanBodyBones.LeftToes);


//        List<HumanBodyBones> _rightLegBones = new List<HumanBodyBones>();
//        _rightLegBones.Add(HumanBodyBones.RightUpperLeg);
//        _rightLegBones.Add(HumanBodyBones.RightLowerLeg);
//        _rightLegBones.Add(HumanBodyBones.RightFoot);
//        _rightLegBones.Add(HumanBodyBones.RightToes);

//        List<HumanBodyBones> _bodyBones = new List<HumanBodyBones>();
//        _bodyBones.Add(HumanBodyBones.Hips);
//        _bodyBones.Add(HumanBodyBones.Spine);
//        _bodyBones.Add(HumanBodyBones.Chest);
//        _bodyBones.Add(HumanBodyBones.Neck);
//        _bodyBones.Add(HumanBodyBones.Head);
//        _bodyBones.Add(HumanBodyBones.LeftEye);
//        _bodyBones.Add(HumanBodyBones.RightEye);
//        _bodyBones.Add(HumanBodyBones.Jaw);

//        _limbRelation = new Dictionary<CopyPoseLimbs, List<HumanBodyBones>>();
//        _limbRelation.Add(CopyPoseLimbs.LeftArm, _leftArmBones);
//        _limbRelation.Add(CopyPoseLimbs.RightArm, _rightArmBones);
//        _limbRelation.Add(CopyPoseLimbs.LeftLeg, _leftLegBones);
//        _limbRelation.Add(CopyPoseLimbs.RightLeg, _rightLegBones);
//        _limbRelation.Add(CopyPoseLimbs.Body, _bodyBones);
//        _limbRelation.Add(CopyPoseLimbs.LeftHand, _leftHandBones);
//        _limbRelation.Add(CopyPoseLimbs.RightHand, _rightHandBones);
//    }

//    #endregion

//    #region GET AND SET

//    public void SetMasterAnimatorController(RuntimeAnimatorController _controller)
//    {
//        _masterAnimator.runtimeAnimatorController = _controller;
//    }

//    public GameObject GetMasterAvatar()
//    {
//        return _masterAvatar;
//    }

//    #endregion

//    #region UPDATE

//    protected virtual void Copy()
//    {
//        _masterHumanPoseHandler.GetHumanPose(ref _humanPoseMaster);
//        _targetHumanPoseHandler.GetHumanPose(ref _humanPoseTarget);
//        _targetHumanPoseHandler.GetHumanPose(ref _humanPoseFinal);

//        //let's mix it
//        for (int i = 0; i < _Bones.Count; i++)
//        {
//            CopyPoseLimbs limbID = _Bones[i];
//            if ((_boneMask & (1 << i)) != 0)
//            {//apply copied
//                ApplyCopiedLimb(limbID);
//            }
//            else
//            {//apply original
//                ApplyOriginalLimb(limbID);
//            }
//        }

//        _humanPoseFinal.bodyPosition = new Vector3(0, _humanPoseTarget.bodyPosition.y, 0);

//        _humanPoseFinal.bodyRotation = Quaternion.identity;
//        Vector3 euler = _humanPoseTarget.bodyRotation.eulerAngles;
//        euler = new Vector3(euler.x, 0, euler.z);
//        _humanPoseFinal.bodyRotation = Quaternion.Euler(euler);

//        _targetHumanPoseHandler.SetHumanPose(ref _humanPoseFinal);





//    }

//    protected virtual void UpdateWorldPosition()
//    {
//        transform.position = _masterAvatar.transform.position + _hipsPositionOffset;
//        transform.rotation = _hipsRotationOffset * _masterAvatar.transform.rotation;

//        _targetAnimator.GetBoneTransform(HumanBodyBones.Hips).position = _masterAnimator.GetBoneTransform(HumanBodyBones.Hips).position + _hipsPositionOffset;
//        _targetAnimator.GetBoneTransform(HumanBodyBones.Hips).rotation = _hipsRotationOffset * _masterAnimator.GetBoneTransform(HumanBodyBones.Hips).rotation;

//    }

//    protected virtual void ApplyLimb(CopyPoseLimbs limbID, Animator animator)
//    {
//        List<HumanBodyBones> currentAffectedLimb = _limbRelation[limbID];
//        foreach (HumanBodyBones currentBone in currentAffectedLimb)
//        {
//            BoneInfo currentBoneInfo = GetBoneInfo(currentBone, animator);
//            if (currentBoneInfo.muscleId.x != -1)
//            {
//                _humanPoseFinal.muscles[currentBoneInfo.muscleId.x] = _humanPoseMaster.muscles[currentBoneInfo.muscleId.x];
//            }
//            if (currentBoneInfo.muscleId.y != -1)
//            {
//                _humanPoseFinal.muscles[currentBoneInfo.muscleId.y] = _humanPoseMaster.muscles[currentBoneInfo.muscleId.y];
//            }
//            if (currentBoneInfo.muscleId.z != -1)
//            {
//                _humanPoseFinal.muscles[currentBoneInfo.muscleId.z] = _humanPoseMaster.muscles[currentBoneInfo.muscleId.z];
//            }
//        }
//    }

//    protected void ApplyCopiedLimb(CopyPoseLimbs limbID)
//    {
//        List<HumanBodyBones> currentAffectedLimb = _limbRelation[limbID];
//        foreach (HumanBodyBones currentBone in currentAffectedLimb)
//        {
//            BoneInfo currentBoneInfo = GetBoneInfo(currentBone, _targetAnimator);
//            if (currentBoneInfo.muscleId.x != -1)
//            {
//                _humanPoseFinal.muscles[currentBoneInfo.muscleId.x] = _humanPoseMaster.muscles[currentBoneInfo.muscleId.x];
//            }
//            if (currentBoneInfo.muscleId.y != -1)
//            {
//                _humanPoseFinal.muscles[currentBoneInfo.muscleId.y] = _humanPoseMaster.muscles[currentBoneInfo.muscleId.y];
//            }
//            if (currentBoneInfo.muscleId.z != -1)
//            {
//                _humanPoseFinal.muscles[currentBoneInfo.muscleId.z] = _humanPoseMaster.muscles[currentBoneInfo.muscleId.z];
//            }
//        }
//    }

//    protected void ApplyOriginalLimb(CopyPoseLimbs limbID)
//    {
//        List<HumanBodyBones> currentAffectedLimb = _limbRelation[limbID];
//        foreach (HumanBodyBones currentBone in currentAffectedLimb)
//        {
//            BoneInfo currentBoneInfo = GetBoneInfo(currentBone, _masterAnimator);
//            if (currentBoneInfo.muscleId.x != -1)
//            {
//                _humanPoseFinal.muscles[currentBoneInfo.muscleId.x] = _humanPoseTarget.muscles[currentBoneInfo.muscleId.x];
//            }
//            if (currentBoneInfo.muscleId.y != -1)
//            {
//                _humanPoseFinal.muscles[currentBoneInfo.muscleId.y] = _humanPoseTarget.muscles[currentBoneInfo.muscleId.y];
//            }
//            if (currentBoneInfo.muscleId.z != -1)
//            {
//                _humanPoseFinal.muscles[currentBoneInfo.muscleId.z] = _humanPoseTarget.muscles[currentBoneInfo.muscleId.z];
//            }
//        }
//    }

//    #endregion

//}
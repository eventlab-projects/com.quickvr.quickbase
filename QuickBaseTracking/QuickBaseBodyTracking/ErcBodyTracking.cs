using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using QuickVR;

using System.IO;

public enum ErcBodyJoints
{
    //Spine
    Hips = 0,
    SpineMid = 7,
    Neck = 8,
    Head = 9,
    MidEyePoint = 10,

    //Left Leg
    LeftUpperLeg = 4,
    LeftLowerLeg = 5,
    LeftFoot = 6,

    //Right Leg 
    RightUpperLeg = 1,
    RightLowerLeg = 2,
    RightFoot = 3,

    //Left Arm
    LeftUpperArm = 11, 
    LeftLowerArm = 12,
    LeftHand = 13,

    //Right Arm
    RightUpperArm = 14, 
    RightLowerArm = 15,
    RightHand = 16,
}

public class ErcBodyTracking : QuickBodyTracking<ErcBodyJoints> {

    #region PUBLIC ATTRIBUTES

    public TextAsset _dataFile = null;
    public float _samplingRate = 60.0f; //Sampling rate in FPS 

    public bool _autoPlay = true;
    public int _currentFrameID = 0;

    public enum PlayMode
    {
        Curve,
        Frame,
    }
    public PlayMode _playMode = PlayMode.Curve;

    public bool _feetOnFloor = true;
    public bool _mirror = false;

    #endregion

    #region PROTECTED ATTRIBUTES

    protected Dictionary<QuickJointType, QuickJointType> _jointLookTarget = new Dictionary<QuickJointType, QuickJointType>();

    protected List<QuickTransformAnimationCurve> _frameData = new List<QuickTransformAnimationCurve>();
    protected float _animTime = 0.0f;

    [ReadOnly]
    [SerializeField]
    protected int _numFrames = 0;
        
    protected static List<ErcBodyJoints> _bodyJoints = null;

    protected QuickIKManager _ikManager = null;

    protected Transform _footLeftBase = null;
    protected Transform _footRightBase = null;

    protected Vector3 _initialLocalPositionHips = Vector3.zero;

    #endregion

    #region CREATION AND DESTRUCTION

    protected override void Awake()
    {
        base.Awake();
        foreach (QuickJointType qJointID in QuickJoint.GetAllTypes())
        {
            InitJointLookTarget(qJointID);
        }

        LoadFrameData();

        CreateIKManager();

        _footLeftBase = CreateFootBase(HumanBodyBones.LeftFoot);
        _footRightBase = CreateFootBase(HumanBodyBones.RightFoot);
        _initialLocalPositionHips = _animator.GetBoneTransform(HumanBodyBones.Hips).localPosition;

        StartCoroutine(CoUpdate());
    }

    protected virtual void CreateIKManager()
    {
        _ikManager = gameObject.GetOrCreateComponent<QuickIKManager_v1>();
    }

    protected virtual Transform CreateFootBase(HumanBodyBones footBone)
    {
        Transform t = _animator.GetBoneTransform(footBone).CreateChild("__FootBase__");
        t.position = new Vector3(t.position.x, transform.position.y, t.position.z);

        return t;
    }

    protected override void CreateIntermediateSkeletonMap()
    {
        //Spine Mapping
        CreateJointMapQuickVR(QuickJointType.SPINE_BASE, ErcBodyJoints.Hips);
        CreateJointMapQuickVR(QuickJointType.SPINE_MID, ErcBodyJoints.SpineMid);
        CreateJointMapQuickVR(QuickJointType.NECK, ErcBodyJoints.Neck);
        CreateJointMapQuickVR(QuickJointType.HEAD, ErcBodyJoints.Head);
        CreateJointMapQuickVR(QuickJointType.EYE_LEFT, ErcBodyJoints.MidEyePoint);

        //Left Arm Mapping
        CreateJointMapQuickVR(QuickJointType.SHOULDER_LEFT, ErcBodyJoints.LeftUpperArm);
        CreateJointMapQuickVR(QuickJointType.ELBOW_LEFT, ErcBodyJoints.LeftLowerArm);
        CreateJointMapQuickVR(QuickJointType.HAND_LEFT, ErcBodyJoints.LeftHand);

        //Right Arm Mapping
        CreateJointMapQuickVR(QuickJointType.SHOULDER_RIGHT, ErcBodyJoints.RightUpperArm);
        CreateJointMapQuickVR(QuickJointType.ELBOW_RIGHT, ErcBodyJoints.RightLowerArm);
        CreateJointMapQuickVR(QuickJointType.HAND_RIGHT, ErcBodyJoints.RightHand);

        //Left Leg Mapping
        CreateJointMapQuickVR(QuickJointType.HIP_LEFT, ErcBodyJoints.LeftUpperLeg);
        CreateJointMapQuickVR(QuickJointType.KNEE_LEFT, ErcBodyJoints.LeftLowerLeg);
        CreateJointMapQuickVR(QuickJointType.ANKLE_LEFT, ErcBodyJoints.LeftFoot);

        //Right Leg Mapping
        CreateJointMapQuickVR(QuickJointType.HIP_RIGHT, ErcBodyJoints.RightUpperLeg);
        CreateJointMapQuickVR(QuickJointType.KNEE_RIGHT, ErcBodyJoints.RightLowerLeg);
        CreateJointMapQuickVR(QuickJointType.ANKLE_RIGHT, ErcBodyJoints.RightFoot);
    }

    protected virtual void InitJointLookTarget(QuickJointType qJointID)
    {
        QuickJointType qJointTargetID;
        if (qJointID == QuickJointType.SPINE_BASE) qJointTargetID = QuickJointType.SPINE_MID;
        else if (qJointID == QuickJointType.NECK) qJointTargetID = QuickJointType.HEAD;
        else if (qJointID == QuickJointType.HEAD) qJointTargetID = QuickJointType.EYE_LEFT;
        else
        {
            List<QuickJointType> childs = _skeleton.GetChilds(qJointID);
            qJointTargetID = (childs.Count == 1) ? childs[0] : qJointID;
        }
        _jointLookTarget[qJointID] = qJointTargetID;
    }

    protected virtual void LoadFrameData()
    {
        //1) Initialize the frame data
        _frameData.Clear();
        _numFrames = 0;
        for (int i = 0; i < GetNumErcBodyJoints(); i++)
        {
            QuickTransformAnimationCurve aCurve = new QuickTransformAnimationCurve();
            aCurve.SetWrapMode(WrapMode.Loop);
            _frameData.Add(aCurve);
        }

        //2) Fill the frame data with the information of the text file. 
        string[] lines = _dataFile.text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            //Let's find the first position value
            while ((i < lines.Length) && (lines[i].Split(';').Length != 3))
            {
                i++;
            }

            if (i < lines.Length)
            {
                //At this point, lines[i] is the first line that represents a position
                //of this block. Read the frame data of this block. 
                for (int j = 0; j < GetNumErcBodyJoints(); j++)
                {
                    string[] pos = lines[i + j].Split(';');
                    Vector3 v = new Vector3(-float.Parse(pos[0]), float.Parse(pos[2]), -float.Parse(pos[1]));
                    if (_mirror) v = new Vector3(-1.0f * v.x, v.y, -1.0f * v.z);
                    _frameData[j].AddPosition((float)_numFrames / _samplingRate, v);
                }

                _numFrames++;
                i += GetNumErcBodyJoints();
            }
        }
    }

    protected static void CheckErcBodyJoints()
    {
        if (_bodyJoints == null) _bodyJoints = QuickUtils.GetEnumValues<ErcBodyJoints>(); 
    }

    #endregion

    #region GET AND SET

    public static List<ErcBodyJoints> GetErcBodyJoints()
    {
        CheckErcBodyJoints();

        return _bodyJoints;
    }

    public static int GetNumErcBodyJoints()
    {
        CheckErcBodyJoints();

        return _bodyJoints.Count;
    }

    public virtual int GetNumFrames()
    {
        return _numFrames;
    }

    protected override bool IsTrackingDataWorldSpace()
    {
        return true;
    }

    protected override bool IsTrackingConnected()
    {
        return true;
    }

    protected override Vector3 GetTrackingJointPosition(ErcBodyJoints jointID)
    {
        QuickTransformAnimationCurve curve = _frameData[(int)jointID];
        float aTime = _playMode == PlayMode.Curve ? _animTime : _currentFrameID * (1.0f / _samplingRate);

        return curve.SamplePosition(aTime);
    }

    protected override Quaternion GetTrackingJointRotation(ErcBodyJoints jointID)
    {
        return Quaternion.identity;
    }

    protected virtual float GetScaleFactor()
    {
        float d1 = Vector3.Distance(_skeleton.GetJoint(QuickJointType.SPINE_BASE).GetTransform().position, _skeleton.GetJoint(QuickJointType.NECK).GetTransform().position);
        float d2 = Vector3.Distance(_animator.GetBoneTransform(HumanBodyBones.Hips).position, _animator.GetBoneTransform(HumanBodyBones.Neck).position);

        return d2 / d1;
    }

    protected override Vector3 GetRootDisplacement()
    {
        return base.GetRootDisplacement() * GetScaleFactor();
    }

    #endregion

    #region UPDATE

    public override void UpdateTrackingLate()
    {
        _animator.GetBoneTransform(HumanBodyBones.Hips).localPosition = _initialLocalPositionHips;

        base.UpdateTrackingLate();
        _animTime += Time.deltaTime;

        UpdateHeadRotation();

        _ikManager.UpdateTrackingLate();
    }

    protected override void UpdateRootPosition()
    {
        base.UpdateRootPosition();

        if (!_feetOnFloor) return;

        //Make sure the avatar has at least one foot on the floor
        Transform footBase = _footLeftBase.position.y < _footRightBase.position.y ? _footLeftBase : _footRightBase;
        RaycastHit result;
        if (Physics.Raycast(footBase.position, Vector3.down, out result, 1.0f))
        {
            _animator.GetBoneTransform(HumanBodyBones.Hips).position += Vector3.down * result.distance;
        }
    }

    protected virtual void UpdateHeadRotation()
    {
        Vector3 up = (_skeleton.GetJoint(QuickJointType.EYE_LEFT).GetTransform().position - _skeleton.GetJoint(QuickJointType.NECK).GetTransform().position);
        Vector3 fwd = (_skeleton.GetJoint(QuickJointType.HEAD).GetTransform().position - _skeleton.GetJoint(QuickJointType.NECK).GetTransform().position);
        Vector3 right = Vector3.Cross(up, fwd);

        Transform target = _ikManager.GetIKSolver(HumanBodyBones.Head)._targetLimb;
        //target.forward = transform.rotation * Vector3.Cross(right, up).normalized;
        target.forward = _initialRootRotation * Vector3.Cross(right, up).normalized;
    }
    
    protected virtual IEnumerator CoUpdate()
    {
        while (true)
        {
            if (_autoPlay)
            {
                yield return new WaitForSeconds(1.0f / _samplingRate);
                _currentFrameID = (_currentFrameID + 1) % GetNumFrames();
            }
            else yield return null;
        }
    }

    protected override void UpdateBoneRotation(QuickJointType qJointID)
    {
        QuickJointType qJointTargetID = _jointLookTarget[qJointID];

        //Compute the current bone direction
        Transform uBone = _animator.GetBoneTransform(_toUnity[qJointID]);
        Transform uTargetBone = _animator.GetBoneTransform(_toUnity[qJointTargetID]);
        uBone.localRotation = _initialJointLocalRotations[qJointID];
        Vector3 currentBoneDir = uTargetBone.position - uBone.position;

        //Compute the target bone direction
        Transform joint = _skeleton.GetJoint(qJointID).GetTransform();
        Transform targetJoint = _skeleton.GetJoint(qJointTargetID).GetTransform();
        Vector3 targetBoneDir = targetJoint.position - joint.position;

        //Rotate the start joint accordingly
        Vector3 rotAxis = Vector3.Cross(currentBoneDir, targetBoneDir).normalized;
        float rotAngle = Vector3.Angle(currentBoneDir, targetBoneDir);
        uBone.Rotate(rotAxis, rotAngle, Space.World);
    }

    #endregion
}

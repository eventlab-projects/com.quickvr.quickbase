using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QuickVR
{
    public class QuickIKRecorder : MonoBehaviour
    {

        #region PUBLIC PARAMETERS

        public float _recordingFrequency = 0.04f;

        #endregion

        #region PROTECTED PARAMETERS

        protected float _initRecordingTime = 0.0f;
        protected float _initPlayingTime = 0.0f;

        protected bool _recording = false;

        protected bool _playing = false;

        protected QuickIKManager _ikManager;

        protected enum IkTarget { LeftHand, RightHand, LeftFoot, RightFoot, Head, Hips };

        protected Transform[] _ikTargets;
        protected Transform _origin = null;

        protected QuickIKRecord _ikRecord;

        protected QuickIKAnimationCurve[] _ikAnimationCurves;

        protected bool _init = false;

        #endregion

        #region CREATION AND DESTRUCTION

        // Use this for initialization
        public virtual void Init(QuickIKManager ikManager)
        {

            if (ikManager == null)
                return;

            _ikManager = ikManager;

            _ikTargets = new Transform[6];
            _ikTargets[(int)IkTarget.LeftHand] = _ikManager.GetIKSolver(IKBone.LeftHand)._targetLimb;
            _ikTargets[(int)IkTarget.RightHand] = _ikManager.GetIKSolver(IKBone.RightHand)._targetLimb;
            _ikTargets[(int)IkTarget.LeftFoot] = _ikManager.GetIKSolver(IKBone.LeftFoot)._targetLimb;
            _ikTargets[(int)IkTarget.RightFoot] = _ikManager.GetIKSolver(IKBone.RightFoot)._targetLimb;
            _ikTargets[(int)IkTarget.Head] = _ikManager.GetIKSolver(IKBone.Head)._targetLimb;
            _ikTargets[(int)IkTarget.Hips] = _ikManager.transform;

            _ikRecord = new QuickIKRecord();

            _init = true;
        }

        protected virtual void OnEnable()
        {
            StartCoroutine(CoUpdateLoop());
        }

        protected virtual void OnDisable()
        {
            StopAllCoroutines();
        }

        #endregion

        #region GET AND SET

        public virtual void StartRecording()
        {
            if (!_init)
                return;

            _initRecordingTime = Time.realtimeSinceStartup;

            _origin = new GameObject("__IKRecorder_Origin__").transform;
            _origin.position = _ikManager.transform.position;
            _origin.rotation = _ikManager.transform.rotation;

            _ikRecord.StartRecording();

            _recording = true;
        }

        public virtual void StartPlaying()
        {
            if (!_init)
                return;

            LoadRecordFromXML();

            _initPlayingTime = Time.realtimeSinceStartup;

            _origin = new GameObject("__IKRecorder_Origin__").transform;
            _origin.position = _ikManager.transform.position;
            _origin.rotation = _ikManager.transform.rotation;

            CreateAnimationCurves();

            _playing = true;
        }

        protected virtual void CreateAnimationCurves()
        {
            _ikAnimationCurves = new QuickIKAnimationCurve[6];
            _ikAnimationCurves[(int)IkTarget.LeftHand] = new QuickIKAnimationCurve(ref _ikRecord._LeftHandPositions, ref _ikRecord._LeftHandForwards, ref _ikRecord._KeyFrameTimes);
            _ikAnimationCurves[(int)IkTarget.RightHand] = new QuickIKAnimationCurve(ref _ikRecord._RightHandPositions, ref _ikRecord._RightHandForwards, ref _ikRecord._KeyFrameTimes);
            _ikAnimationCurves[(int)IkTarget.LeftFoot] = new QuickIKAnimationCurve(ref _ikRecord._LeftFootPositions, ref _ikRecord._LeftFootForwards, ref _ikRecord._KeyFrameTimes);
            _ikAnimationCurves[(int)IkTarget.RightFoot] = new QuickIKAnimationCurve(ref _ikRecord._RightFootPositions, ref _ikRecord._RightFootForwards, ref _ikRecord._KeyFrameTimes);
            _ikAnimationCurves[(int)IkTarget.Head] = new QuickIKAnimationCurve(ref _ikRecord._HeadPositions, ref _ikRecord._HeadForwards, ref _ikRecord._KeyFrameTimes);
            _ikAnimationCurves[(int)IkTarget.Hips] = new QuickIKAnimationCurve(ref _ikRecord._HipsPositions, ref _ikRecord._HipsForwards, ref _ikRecord._KeyFrameTimes);
        }

        protected virtual void ReadIkTargetKeyFrame(int target, float time)
        {
            _ikTargets[target].position = _origin.TransformPoint(_ikAnimationCurves[target].SamplePosition(time));
            _ikTargets[target].forward = _origin.TransformVector(_ikAnimationCurves[target].SampleForward(time));
        }

        protected virtual void ReadKeyFrame()
        {
            float readTime = Time.realtimeSinceStartup - _initPlayingTime;

            for (int i = 0; i < 6; i++)
                ReadIkTargetKeyFrame(i, readTime);
        }

        protected virtual void RecordFrameTime()
        {
            float recordingTime = Time.realtimeSinceStartup - _initRecordingTime;

            _ikRecord._KeyFrameTimes.Add(recordingTime);
        }

        protected virtual void RecordIkFrame(IkTarget target, ref List<Vector3> positions, ref List<Vector3> forwards)
        {
            Transform ikTarget = _ikTargets[(int)target];

            Vector3 position = ikTarget.position;
            Vector3 forward = ikTarget.forward;

            position = _origin.InverseTransformPoint(position);
            forward = _origin.InverseTransformVector(forward);

            positions.Add(position);
            forwards.Add(forward);
        }

        protected virtual void RecordKeyFrame()
        {
            RecordFrameTime();
            RecordIkFrame(IkTarget.Head, ref _ikRecord._HeadPositions, ref _ikRecord._HeadForwards);
            RecordIkFrame(IkTarget.Hips, ref _ikRecord._HipsPositions, ref _ikRecord._HipsForwards);
            RecordIkFrame(IkTarget.LeftHand, ref _ikRecord._LeftHandPositions, ref _ikRecord._LeftHandForwards);
            RecordIkFrame(IkTarget.RightHand, ref _ikRecord._RightHandPositions, ref _ikRecord._RightHandForwards);
            RecordIkFrame(IkTarget.LeftFoot, ref _ikRecord._LeftFootPositions, ref _ikRecord._LeftFootForwards);
            RecordIkFrame(IkTarget.RightFoot, ref _ikRecord._RightFootPositions, ref _ikRecord._RightFootForwards);
        }

        public virtual void StopRecording()
        {
            if (!_init)
                return;

            _recording = false;

            SaveRecordToXML();
        }

        public virtual void StopPlaying()
        {
            if (!_init)
                return;

            _playing = false;
        }

        protected virtual void SaveRecordToXML()
        {
            _ikRecord.SaveToXml(QuickUtils.GetUserFolder() + @"\IkRecording.xml");
        }

        protected virtual void LoadRecordFromXML()
        {
            _ikRecord.LoadFromXml(QuickUtils.GetUserFolder() + @"\IkRecording.xml");
        }

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoUpdateLoop()
        {
            while (true)
            {
                if (enabled && _recording)
                {
                    RecordTrackers();
                }
                yield return new WaitForSeconds(_recordingFrequency);
            }
        }

        protected virtual void RecordTrackers()
        {

            if (!_recording)
                return;

            if (!_init)
                return;

            //QuickUnityVR quickUnityVR = _ikManager.GetComponent<QuickUnityVR>();
            //if (quickUnityVR != null)
            //    quickUnityVR.RelocateCamera();

            RecordKeyFrame();

        }

        //protected virtual void Replay()
        protected virtual void Update()
        {
            if (!_playing)
                return;

            if (!_init)
                return;

            ReadKeyFrame();
        }

        #endregion

    }
}

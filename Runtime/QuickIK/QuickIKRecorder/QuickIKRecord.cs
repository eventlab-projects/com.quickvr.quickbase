using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace QuickVR
{
    [Serializable]
    public class QuickIKRecord : ScriptableObject
    {
        #region PUBLIC PARAMETERS

        [SerializeField]
        public List<float> _KeyFrameTimes;

        [SerializeField]
        public List<Vector3> _HeadPositions;
        [SerializeField]
        public List<Vector3> _HeadForwards;

        [SerializeField]
        public List<Vector3> _HipsPositions;
        [SerializeField]
        public List<Vector3> _HipsForwards;

        [SerializeField]
        public List<Vector3> _LeftHandPositions;
        [SerializeField]
        public List<Vector3> _LeftHandForwards;

        [SerializeField]
        public List<Vector3> _RightHandPositions;
        [SerializeField]
        public List<Vector3> _RightHandForwards;

        [SerializeField]
        public List<Vector3> _LeftFootPositions;
        [SerializeField]
        public List<Vector3> _LeftFootForwards;

        [SerializeField]
        public List<Vector3> _RightFootPositions;
        [SerializeField]
        public List<Vector3> _RightFootForwards;

        #endregion

        #region CREATION AND DESTRUCTION

        public QuickIKRecord()
        {
            _KeyFrameTimes = new List<float>();

            _HeadPositions = new List<Vector3>();
            _HeadForwards = new List<Vector3>();

            _HipsPositions = new List<Vector3>();
            _HipsForwards = new List<Vector3>();

            _LeftHandPositions = new List<Vector3>();
            _LeftHandForwards = new List<Vector3>();

            _RightHandPositions = new List<Vector3>();
            _RightHandForwards = new List<Vector3>();

            _LeftFootPositions = new List<Vector3>();
            _LeftFootForwards = new List<Vector3>();

            _RightFootPositions = new List<Vector3>();
            _RightFootForwards = new List<Vector3>();
        }

        #endregion

        #region GET AND SET

        public virtual void StartRecording()
        {
            _KeyFrameTimes.Clear();

            _HeadPositions.Clear();
            _HeadForwards.Clear();

            _HipsPositions.Clear();
            _HipsForwards.Clear();

            _LeftHandPositions.Clear();
            _LeftHandForwards.Clear();

            _RightHandPositions.Clear();
            _RightHandForwards.Clear();

            _LeftFootPositions.Clear();
            _LeftFootForwards.Clear();

            _RightFootPositions.Clear();
            _RightFootForwards.Clear();
        }

        #endregion
    }
}

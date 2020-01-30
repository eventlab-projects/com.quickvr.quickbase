﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace QuickVR
{

    public class QuickAnimationFrame
    {

        #region PUBLIC ATTRIBUTES

        public Vector3 _rootPos = Vector3.zero;
        public Quaternion _rootRot = Quaternion.identity;
        public Dictionary<HumanBodyBones, Quaternion> _boneData = new Dictionary<HumanBodyBones, Quaternion>();

        #endregion

    }

    [System.Serializable]
    public class QuickAnimationRecorder : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public string _fileName = "animation";

        public bool _loop = true;
        public bool _applyRootMotion = true;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected enum State
        {
            Idle,
            Recording,
            Playback,
        }

        protected State _state = State.Idle;

        protected Animator _animator = null;
        protected List<QuickAnimationFrame> _animationFrames = new List<QuickAnimationFrame>();
        protected Dictionary<HumanBodyBones, Quaternion> _lastLocalRotations = new Dictionary<HumanBodyBones, Quaternion>();
        protected Dictionary<HumanBodyBones, Quaternion> _tPoseRotations = new Dictionary<HumanBodyBones, Quaternion>();
        protected float _fps = 0.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();

            foreach (int i in QuickHumanTrait.GetBonesHierarchy())
            {
                HumanBodyBones b = (HumanBodyBones)i;
                Transform tBone = _animator.GetBoneTransform(b);
                if (!tBone) continue;

                _tPoseRotations[b] = tBone.rotation;
            }
        }

        #endregion

        #region GET AND SET

        protected virtual void SetState(State newState)
        {
            if (_state == newState) return;

            _state = newState;
            if (newState == State.Recording) StartCoroutine(CoRecordAnimation());
            if (newState == State.Playback) StartCoroutine(CoPlaybackAnimation());
        }

        public virtual void RecordAnimation()
        {
            SetState(State.Recording);
        }

        public virtual void PlaybackAnimation()
        {
            SetState(State.Playback);
        }

        public virtual void Stop()
        {
            SetState(State.Idle);
        }

        protected virtual void WriteAttribute(XmlDocument doc, string attributeName, ref XmlNode node, object value)
        {
            XmlAttribute attribute = doc.CreateAttribute(attributeName);
            attribute.Value = value.ToString();
            node.Attributes.Append(attribute);
        }

        protected virtual void WritePCData(XmlDocument doc, string nodeName, ref XmlNode parent, object value)
        {
            XmlNode node = doc.CreateElement(nodeName);
            node.InnerText = value.ToString();
            parent.AppendChild(node);
        }

        protected virtual Quaternion GetRotationFromNode(XmlElement node)
        {
            float x = float.Parse(node.GetAttribute("rotX"));
            float y = float.Parse(node.GetAttribute("rotY"));
            float z = float.Parse(node.GetAttribute("rotZ"));
            float w = float.Parse(node.GetAttribute("rotW"));

            return new Quaternion(x, y, z, w);
        }

        protected virtual Vector3 GetPositionFromNode(XmlElement node)
        {
            float x = float.Parse(node.GetAttribute("posX"));
            float y = float.Parse(node.GetAttribute("posY"));
            float z = float.Parse(node.GetAttribute("posZ"));

            return new Vector3(x, y, z);
        }

        protected virtual Quaternion GetBoneRotationAtFrame(HumanBodyBones b, int frameID)
        {
            if (_animationFrames[frameID]._boneData.ContainsKey(b))
            {
                return Quaternion.Inverse(_animationFrames[frameID]._rootRot) * transform.rotation * _animationFrames[frameID]._boneData[b];
            }
            return _animator.GetBoneTransform(b).rotation;
        }

        protected virtual Quaternion GetLastLocalRotation(HumanBodyBones boneID)
        {
            return _lastLocalRotations.ContainsKey(boneID) ? _lastLocalRotations[boneID] : Quaternion.identity;
        }

        protected virtual string GetFilePath()
        {
            //return Path.Combine(Application.dataPath, _fileName + ".xml");
            return Path.Combine(QuickUtils.GetUserFolder(), _fileName + ".xml");
        }

        protected virtual void LoadAnimationFrames()
        {
            _animationFrames.Clear();
            XmlDocument document = new XmlDocument();
            document.Load(GetFilePath());

            _fps = float.Parse(document.DocumentElement.GetAttribute("fps"));

            foreach (XmlElement frameNode in document.DocumentElement.GetElementsByTagName("FrameData"))
            {
                QuickAnimationFrame frameData = new QuickAnimationFrame();
                XmlElement rootNode = (XmlElement)(frameNode.GetElementsByTagName("RootData")[0]);
                frameData._rootPos = GetPositionFromNode(rootNode);
                frameData._rootRot = GetRotationFromNode(rootNode);
                
                foreach (XmlElement boneNode in frameNode.GetElementsByTagName("BoneData"))
                {
                    HumanBodyBones boneID = QuickUtils.ParseEnum<HumanBodyBones>(boneNode.GetAttribute("name"));
                    frameData._boneData[boneID] = GetRotationFromNode(boneNode);
                }
                
                _animationFrames.Add(frameData);
            }
        }

        protected virtual void WriteQuaternion(Quaternion q, XmlDocument document, ref XmlNode node)
        {
            WriteAttribute(document, "rotX", ref node, q.x);
            WriteAttribute(document, "rotY", ref node, q.y);
            WriteAttribute(document, "rotZ", ref node, q.z);
            WriteAttribute(document, "rotW", ref node, q.w);
        }

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoRecordAnimation()
        {
            XmlDocument document = new XmlDocument();
            XmlNode animationDataNode = document.CreateElement("AnimationData");
            WriteAttribute(document, "fps", ref animationDataNode, 1.0f / Time.fixedDeltaTime);
            document.AppendChild(animationDataNode);

            XmlNode tPoseData = document.CreateElement("TPoseData");
            foreach (var pair in _tPoseRotations)
            {
                WriteAttribute(document, "name", ref tPoseData, pair.Key);
                WriteQuaternion(pair.Value, document, ref tPoseData);
            }

            foreach (int i in QuickHumanTrait.GetBonesHierarchy())
            {
                HumanBodyBones b = (HumanBodyBones)i;
                Transform tBone = _animator.GetBoneTransform(b);
                if (!tBone) continue;


            }

            while (_state == State.Recording)
            {
                XmlNode frameNode = document.CreateElement("FrameData");
                XmlNode rootNode = document.CreateElement("RootData");

                WriteAttribute(document, "posX", ref rootNode, transform.position.x);
                WriteAttribute(document, "posY", ref rootNode, transform.position.y);
                WriteAttribute(document, "posZ", ref rootNode, transform.position.z);

                WriteQuaternion(transform.rotation, document, ref rootNode);

                frameNode.AppendChild(rootNode);

                foreach (int i in QuickHumanTrait.GetBonesHierarchy())
                {
                    HumanBodyBones b = (HumanBodyBones)i;
                    Transform tBone = _animator.GetBoneTransform(b);
                    if (!tBone || tBone.localRotation == GetLastLocalRotation(b)) continue;

                    XmlNode boneNode = document.CreateElement("BoneData");
                    Quaternion rot = tBone.rotation;
                    WriteAttribute(document, "name", ref boneNode, b);
                    WriteQuaternion(rot, document, ref boneNode);

                    _lastLocalRotations[b] = tBone.localRotation;
                    frameNode.AppendChild(boneNode);
                }
                animationDataNode.AppendChild(frameNode);
                
                yield return new WaitForFixedUpdate();
            }

            document.Save(GetFilePath());
        }

        protected virtual IEnumerator CoPlaybackAnimation()
        {
            LoadAnimationFrames();

            int numFrames = _animationFrames.Count;
            float time = 0.0f;
            float animTime = numFrames * (1.0f / _fps);
            
            Vector3 initialPosition = transform.position;

            while (_state == State.Playback)
            {
                time += Time.deltaTime;
                if (time >= animTime)
                {
                    if (_loop)
                    {
                        time = Mathf.Repeat(time, animTime);
                        initialPosition = transform.position;
                    }
                    else time = animTime;
                }

                float value = time / (1.0f / _fps);
                int ceil = Mathf.Min(Mathf.CeilToInt(value), numFrames - 1);
                int floor = Mathf.Max(0, ceil - 1);
                float t = Mathf.Repeat(value, 1.0f);

                if (_applyRootMotion)
                {
                    Vector3 posOffsetFloor = transform.rotation * Quaternion.Inverse(_animationFrames[floor]._rootRot) * (_animationFrames[floor]._rootPos - _animationFrames[0]._rootPos);
                    Vector3 posOffsetCeil = transform.rotation * Quaternion.Inverse(_animationFrames[floor]._rootRot) * (_animationFrames[ceil]._rootPos - _animationFrames[0]._rootPos);

                    transform.position = Vector3.Lerp(initialPosition + posOffsetFloor, initialPosition + posOffsetCeil, t);
                }

                //Apply the rotation to each bone
                foreach (int i in QuickHumanTrait.GetBonesHierarchy())
                {
                    HumanBodyBones b = (HumanBodyBones)i;
                    Transform tBone = _animator.GetBoneTransform(b);
                    if (!tBone) continue;

                    Quaternion initialRotation = GetBoneRotationAtFrame(b, floor);
                    Quaternion finalRotation = GetBoneRotationAtFrame(b, ceil);
                    
                    tBone.rotation = Quaternion.Lerp(initialRotation, finalRotation, t);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        #endregion

    }
}


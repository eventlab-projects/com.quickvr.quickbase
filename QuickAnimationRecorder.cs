using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace QuickVR
{

    [System.Serializable]
    public class QuickAnimationRecorder : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public string _fileName = "animation";

        public bool _loop = false;

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
        protected List<Dictionary<HumanBodyBones, Quaternion>> _animationFrames = new List<Dictionary<HumanBodyBones, Quaternion>>();
        protected Dictionary<HumanBodyBones, Quaternion> _lastLocalRotations = new Dictionary<HumanBodyBones, Quaternion>();
        protected float _fps = 0.0f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();
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

        protected Quaternion GetLastLocalRotation(HumanBodyBones boneID)
        {
            return _lastLocalRotations.ContainsKey(boneID) ? _lastLocalRotations[boneID] : Quaternion.identity;
        }

        protected string GetFilePath()
        {
            return Path.Combine(Application.dataPath, _fileName + ".xml");
        }

        protected virtual void LoadAnimationFrames()
        {
            _animationFrames.Clear();
            XmlDocument document = new XmlDocument();
            document.Load(GetFilePath());

            _fps = float.Parse(document.DocumentElement.GetAttribute("fps"));

            foreach (XmlElement frameNode in document.DocumentElement.GetElementsByTagName("FrameData"))
            {
                Dictionary<HumanBodyBones, Quaternion> frameData = new Dictionary<HumanBodyBones, Quaternion>();
                foreach (XmlElement boneNode in frameNode.GetElementsByTagName("BoneData"))
                {
                    HumanBodyBones boneID = QuickUtils.ParseEnum<HumanBodyBones>(boneNode.GetAttribute("name"));

                    float x = float.Parse(boneNode.GetAttribute("x"));
                    float y = float.Parse(boneNode.GetAttribute("y"));
                    float z = float.Parse(boneNode.GetAttribute("z"));
                    float w = float.Parse(boneNode.GetAttribute("w"));

                    frameData[boneID] = new Quaternion(x, y, z, w);
                }
                
                _animationFrames.Add(frameData);
            }
        }

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoRecordAnimation()
        {
            XmlDocument document = new XmlDocument();
            XmlNode rootNode = document.CreateElement("AnimationData");
            WriteAttribute(document, "fps", ref rootNode, 1.0f / Time.fixedDeltaTime);
            document.AppendChild(rootNode);
            
            while (_state == State.Recording)
            {
                XmlNode frameNode = document.CreateElement("FrameData");
                
                for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
                {
                    HumanBodyBones b = (HumanBodyBones)i;
                    Transform tBone = _animator.GetBoneTransform(b);
                    if (!tBone || tBone.localRotation == GetLastLocalRotation(b)) continue;

                    XmlNode boneNode = document.CreateElement("BoneData");
                    Quaternion rot = tBone.rotation;
                    WriteAttribute(document, "name", ref boneNode, b);
                    WriteAttribute(document, "x", ref boneNode, rot.x);
                    WriteAttribute(document, "y", ref boneNode, rot.y);
                    WriteAttribute(document, "z", ref boneNode, rot.z);
                    WriteAttribute(document, "w", ref boneNode, rot.w);

                    _lastLocalRotations[b] = tBone.localRotation;
                    frameNode.AppendChild(boneNode);
                }
                rootNode.AppendChild(frameNode);
                
                yield return new WaitForFixedUpdate();
            }

            document.Save(GetFilePath());
        }

        protected virtual IEnumerator CoPlaybackAnimation()
        {
            LoadAnimationFrames();
            
            float time = 0.0f;
            int numFrames = _animationFrames.Count;
            while (_state == State.Playback)
            {
                time += Time.deltaTime;

                for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
                {
                    HumanBodyBones b = (HumanBodyBones)i;
                    Transform tBone = _animator.GetBoneTransform(b);
                    if (!tBone) continue;

                    float value = time / (1.0f / _fps);
                    int ceil = Mathf.CeilToInt(value);
                    if (_loop) ceil %= numFrames;
                    else ceil = Mathf.Min(ceil, numFrames - 1);
                    int floor = Mathf.Max(0, ceil - 1);

                    Quaternion initialRotation = _animationFrames[floor].ContainsKey(b)? _animationFrames[floor][b] : tBone.rotation;
                    Quaternion finalRotation = _animationFrames[ceil].ContainsKey(b)? _animationFrames[ceil][b] : tBone.rotation;
                    tBone.rotation = Quaternion.Lerp(initialRotation, finalRotation, Mathf.Repeat(value, 1.0f));
                }

                yield return new WaitForEndOfFrame();
            }
        }

        #endregion

    }
}



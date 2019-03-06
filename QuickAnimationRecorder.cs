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
        protected AnimationCurve _timeCurve = new AnimationCurve();

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();
            _timeCurve.postWrapMode = WrapMode.Loop;
            _timeCurve.preWrapMode = WrapMode.Loop;
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

            int frameID = 0;
            foreach (XmlElement frameNode in document.DocumentElement.GetElementsByTagName("FrameData"))
            {
                Dictionary<HumanBodyBones, Quaternion> frameData = new Dictionary<HumanBodyBones, Quaternion>();
                float frameTime = float.Parse(frameNode.GetAttribute("time"));

                foreach (XmlElement boneNode in frameNode.GetElementsByTagName("BoneData"))
                {
                    HumanBodyBones boneID = QuickUtils.ParseEnum<HumanBodyBones>(boneNode.GetAttribute("name"));

                    float x = float.Parse(boneNode.GetAttribute("x"));
                    float y = float.Parse(boneNode.GetAttribute("y"));
                    float z = float.Parse(boneNode.GetAttribute("z"));
                    float w = float.Parse(boneNode.GetAttribute("w"));

                    frameData[boneID] = new Quaternion(x, y, z, w);
                }
                _timeCurve.AddKey(frameTime, frameID);
                frameID++;

                _animationFrames.Add(frameData);
            }
        }

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) RecordAnimation();
            if (Input.GetKeyDown(KeyCode.P)) PlaybackAnimation();
            if (Input.GetKeyDown(KeyCode.S)) Stop();
        }

        protected virtual IEnumerator CoRecordAnimation()
        {
            XmlDocument document = new XmlDocument();
            XmlElement rootNode = document.CreateElement("AnimationData");
            document.AppendChild(rootNode);
            float time = 0.0f;

            while (_state == State.Recording)
            {
                XmlNode frameNode = document.CreateElement("FrameData");
                time += Time.deltaTime;
                WriteAttribute(document, "time", ref frameNode, time);

                for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
                {
                    HumanBodyBones b = (HumanBodyBones)i;
                    Transform tBone = _animator.GetBoneTransform(b);
                    if (!tBone || tBone.localRotation == GetLastLocalRotation(b)) continue;

                    XmlNode boneNode = document.CreateElement("BoneData");
                    Quaternion rot = tBone.localRotation;
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
            while (_state == State.Playback)
            {
                time += Time.deltaTime;

                for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
                {
                    HumanBodyBones b = (HumanBodyBones)i;
                    Transform tBone = _animator.GetBoneTransform(b);
                    if (!tBone) continue;

                    float value = _timeCurve.Evaluate(time);
                    int floor = Mathf.FloorToInt(value);
                    int ceil = Mathf.CeilToInt(value);
                    Quaternion initialRotation = _animationFrames[floor].ContainsKey(b)? _animationFrames[floor][b] : tBone.localRotation;
                    Quaternion finalRotation = _animationFrames[ceil].ContainsKey(b)? _animationFrames[ceil][b] : tBone.localRotation;
                    tBone.localRotation = Quaternion.Lerp(initialRotation, finalRotation, value - (float)floor);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        #endregion

    }
}



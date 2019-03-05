using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Xml;
using System.Xml.Serialization;
using System.IO;

using QuickBoneData = System.Collections.Generic.KeyValuePair<string, UnityEngine.Quaternion>;

namespace QuickVR
{

    public class QuickAnimationRecorder : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public string _fileName = "animation";

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Animator _animator = null;
        protected List<List<QuickBoneData>> _animationFrames = new List<List<QuickBoneData>>();
        protected Dictionary<HumanBodyBones, Quaternion> _lastLocalRotations = new Dictionary<HumanBodyBones, Quaternion>();

        protected bool _recording = false;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        #endregion

        #region GET AND SET

        public virtual void RecordAnimation()
        {
            StartCoroutine(CoRecordAnimation());
        }

        public virtual void PlaybackAnimation()
        {
            StartCoroutine(CoPlaybackAnimation());
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

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoRecordAnimation()
        {
            _recording = true;
            XmlDocument document = new XmlDocument();
            XmlElement rootNode = document.CreateElement("AnimationData");
            document.AppendChild(rootNode);

            while (_recording)
            {
                XmlNode frameNode = document.CreateElement("FrameData");

                for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
                {
                    HumanBodyBones b = (HumanBodyBones)i;
                    Transform tBone = _animator.GetBoneTransform(b);
                    if (!tBone || tBone.localRotation == GetLastLocalRotation(b)) continue;

                    XmlNode boneNode = document.CreateElement("BoneData");
                    Quaternion rot = tBone.rotation;
                    WriteAttribute(document, "name", ref boneNode, b.ToString());
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
            XmlDocument document = new XmlDocument();
            document.Load(GetFilePath());
            
            foreach (XmlElement frameNode in document.DocumentElement.GetElementsByTagName("FrameData"))
            {
                foreach (XmlElement boneNode in frameNode.GetElementsByTagName("BoneData"))
                {
                    HumanBodyBones boneID = QuickUtils.ParseEnum<HumanBodyBones>(boneNode.GetAttribute("name"));

                    float x = float.Parse(boneNode.GetAttribute("x"));
                    float y = float.Parse(boneNode.GetAttribute("y"));
                    float z = float.Parse(boneNode.GetAttribute("z"));
                    float w = float.Parse(boneNode.GetAttribute("w"));
                    Quaternion rot = new Quaternion(x, y, z, w);

                    Transform tBone = _animator.GetBoneTransform(boneID);
                    if (tBone) tBone.rotation = rot;
                }

                yield return new WaitForFixedUpdate();
                
            }
        }

        #endregion

    }
}



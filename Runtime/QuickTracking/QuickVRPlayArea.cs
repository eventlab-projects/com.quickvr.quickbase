using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.XR;

namespace QuickVR
{
    public class QuickVRPlayArea : MonoBehaviour
    {

        #region PROTECTED ATTRIBUTES


        protected Dictionary<QuickHumanBodyBones, QuickVRNode> _vrNodes = new Dictionary<QuickHumanBodyBones, QuickVRNode>();

        protected Transform _calibrationPoseRoot = null;
        
        protected bool _isHandsSwaped = false;

        protected Vector3 _customUserForward = Vector3.zero;  //A custom user forward provided by the application. 

        protected Dictionary<HumanBodyBones, float> _fingerLength = new Dictionary<HumanBodyBones, float>();

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            _calibrationPoseRoot = transform.CreateChild("__CalibrationPoseRoot__");
            foreach (QuickHumanBodyBones role in QuickVRNode.GetTypeList())
            {
                CreateVRNode(role);
            }

            for (HumanBodyBones boneID = HumanBodyBones.LeftThumbProximal; boneID <= HumanBodyBones.RightLittleProximal; boneID++)
            {
                _fingerLength[boneID] = 0;
            }
        }

        protected virtual QuickVRNode CreateVRNode(QuickHumanBodyBones role)
        {
            Transform tNode = transform.CreateChild("VRNode" + role.ToString());
            QuickVRNode n = null;

            if (role == QuickHumanBodyBones.Head)
            {
                n = tNode.GetOrCreateComponent<QuickVRNodeHead>();
            }
            else if (role == QuickHumanBodyBones.LeftHand)
            {
                n = tNode.GetOrCreateComponent<QuickVRNodeHand>();
                ((QuickVRNodeHand)n)._isLeft = true;
            }
            else if (role == QuickHumanBodyBones.RightHand)
            {
                n = tNode.GetOrCreateComponent<QuickVRNodeHand>();
                ((QuickVRNodeHand)n)._isLeft = false;
            }
            else if (role == QuickHumanBodyBones.LeftEye)
            {
                n = tNode.GetOrCreateComponent<QuickVRNodeEye>();
                ((QuickVRNodeEye)n)._isLeft = true;
            }
            else if (role == QuickHumanBodyBones.RightEye)
            {
                n = tNode.GetOrCreateComponent<QuickVRNodeEye>();
                ((QuickVRNodeEye)n)._isLeft = false;
            }
            else
            {
                n = tNode.GetOrCreateComponent<QuickVRNode>();
            }

            n.SetCalibrationPose(_calibrationPoseRoot.CreateChild(QuickVRNode.CALIBRATION_POSE_PREFIX, false));
            n.SetRole(role);

            _vrNodes[role] = n;

            return n;
        }

        #endregion

        #region GET AND SET

        public virtual Vector3 GetUserForward()
        {
            if (_customUserForward == Vector3.zero)
            {
                return Vector3.ProjectOnPlane(GetVRNodeMain().transform.forward, transform.up);
            }
            return _customUserForward;
        }

        public virtual void SetUserForward(Vector3 fwd)
        {
            _customUserForward = fwd;
        }

        public virtual void ResetUserForward()
        {
            _customUserForward = Vector3.zero;
        }

        public virtual Transform GetCalibrationPoseRoot()
        {
            return _calibrationPoseRoot;
        }

        public virtual QuickVRNode GetVRNodeMain()
        {
            QuickVRNode nodeHips = GetVRNode(HumanBodyBones.Hips);
            QuickVRNode nodeHead = GetVRNode(HumanBodyBones.Head);

            return nodeHips.IsTracked() ? nodeHips : nodeHead;
        }

        public virtual QuickVRNode GetVRNode(QuickHumanBodyBones role)
        {
            return _vrNodes[role];
        }

        public virtual QuickVRNode GetVRNode(HumanBodyBones role)
        {
            return GetVRNode((QuickHumanBodyBones)role);
        }

        protected virtual bool IsNodeLeftSide(QuickVRNode vrNode)
        {
            QuickVRNode nodeHead = GetVRNode(HumanBodyBones.Head);
            Vector3 fwd = Vector3.ProjectOnPlane(nodeHead.transform.forward, transform.up);
            Vector3 v = Vector3.ProjectOnPlane(vrNode.transform.position - nodeHead.transform.position, transform.up);

            return Vector3.SignedAngle(fwd, v, transform.up) < 0;
        }

        protected virtual List<InputDevice> GetBodyTrackers()
        {
            List<InputDevice> result = new List<InputDevice>();
            XRNode[] trackers = { XRNode.Head, XRNode.LeftHand, XRNode.RightHand, XRNode.HardwareTracker };
            foreach (XRNode n in trackers)
            {
                List<InputDevice> tmp = new List<InputDevice>();
                InputDevices.GetDevicesAtXRNode(n, tmp);
                result.AddRange(tmp);
            }
            
            result.Sort(CompareInputDevicesHeight);

            return result;
        }

        public virtual float GetFingerLength(QuickHumanFingers f, bool isLeft)
        {
            List<QuickHumanBodyBones> boneFingers = QuickHumanTrait.GetBonesFromFinger(f, isLeft);
            HumanBodyBones boneID = (HumanBodyBones)boneFingers[0];
            if (_fingerLength[boneID] == 0)
            {
                QuickVRNode n0 = GetVRNode(boneFingers[0]);
                QuickVRNode n1 = GetVRNode(boneFingers[1]);
                QuickVRNode n2 = GetVRNode(boneFingers[2]);

                if (n0.IsTracked() && n1.IsTracked() && n2.IsTracked())
                {
                    _fingerLength[boneID] = Vector3.Distance(n0.transform.position, n1.transform.position) + Vector3.Distance(n1.transform.position, n2.transform.position);
                }
            }

            return _fingerLength[boneID];
        } 

        public virtual void Calibrate()
        {
            //POSSIBLE TRACKER CONFIGURATIONS

            //1     ->  Head
            //3     ->  Head + Hands
            //4     ->  Head + Hands + Hips
            //6     ->  Head + Hands + Hips + Feet
            //10    ->  Head + Hands + Hips + Feet + Elbows + Knees

            _isHandsSwaped = false;
            List<InputDevice> bodyTrackers = GetBodyTrackers();
            int numTrackers = bodyTrackers.Count;
            Debug.Log("NUM BODY TRACKERS = " + numTrackers);

            //Try to assign the default nodes for Head and Hands
            QuickVRNode nodeHMD = GetVRNode(HumanBodyBones.Head);
            QuickVRNode nodeLeftHand = GetVRNode(HumanBodyBones.LeftHand);
            QuickVRNode nodeRightHand = GetVRNode(HumanBodyBones.RightHand);

            nodeHMD._inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            nodeLeftHand._inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            nodeRightHand._inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            if (numTrackers == 1 || numTrackers == 3 || numTrackers == 4 || numTrackers == 6 || numTrackers == 10)
            {
                if (!nodeHMD._inputDevice.isValid)
                {
                    //The head will always be the upper body tracker
                    nodeHMD._inputDevice = bodyTrackers[0];
                }

                if (numTrackers == 3)
                {
                    //Head + Hands
                    if (!nodeLeftHand._inputDevice.isValid)
                    {
                        nodeLeftHand._inputDevice = bodyTrackers[1];
                    }
                    if (!nodeRightHand._inputDevice.isValid)
                    {
                        nodeRightHand._inputDevice = bodyTrackers[2];
                    }
                }
                //else if (numTrackers == 4)
                //{
                //    //Head + Hands + Hips
                //    //1) Remove the head node from the list
                //    bodyTrackers.RemoveAt(0);

                //    //2) The hips is the node that is "in the middle", i.e., the hands are in opposite sides of the hips node. 
                //    InitHipsAndHands(bodyTrackers);
                //}
                //else if (numTrackers == 6)
                //{
                //    //Head + Hands + Hips + Feet
                //    //1) The Feet are the trackers with the lower y
                //    InitVRNode(HumanBodyBones.LeftFoot, bodyTrackers[5]);
                //    InitVRNode(HumanBodyBones.RightFoot, bodyTrackers[4]);

                //    //2) Remove the unnecessary nodes and proceed as in the previous case
                //    bodyTrackers.RemoveAt(5);
                //    bodyTrackers.RemoveAt(4);
                //    bodyTrackers.RemoveAt(0);

                //    InitHipsAndHands(bodyTrackers);
                //}

                //UpdateVRNodes();

                //IsVRNodesSwaped(HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot);
            }
            else
            {
                Debug.LogWarning("BAD NUMBER OF BODY TRACKERS!!!");
            }

            UpdateVRNodes();
            _isHandsSwaped = IsVRNodesSwaped(HumanBodyBones.LeftHand, HumanBodyBones.RightHand);
            Debug.Log("handsSwaped = " + _isHandsSwaped);

            foreach (HumanBodyBones t in QuickVRNode.GetTypeList())
            {
                QuickVRNode n = GetVRNode(t);
                if (n) n.Calibrate();
            }
        }

        protected virtual void InitHipsAndHands(List<XRNodeState> bodyTrackers)
        {
            if (bodyTrackers.Count != 3)
            {
                Debug.LogError("BODY TRACKERS LIST MUST CONTAIN EXACTLY 3 ELEMENTS");
                return;
            }

            Vector3 pos0;
            Vector3 pos1;
            Vector3 pos2;
            bodyTrackers[0].TryGetPosition(out pos0);
            bodyTrackers[1].TryGetPosition(out pos1);
            bodyTrackers[2].TryGetPosition(out pos2);

            Vector3 scale = new Vector3(1, 0, 1);
            pos0 = Vector3.Scale(pos0, scale);
            pos1 = Vector3.Scale(pos1, scale);
            pos2 = Vector3.Scale(pos2, scale);

            if (Vector3.Dot(pos1 - pos0, pos2 - pos0) < 0)
            {
                //0 is the hips tracker
                InitVRNode(HumanBodyBones.Hips, bodyTrackers[0]);
                InitVRNode(HumanBodyBones.LeftHand, bodyTrackers[1]);
                InitVRNode(HumanBodyBones.RightHand, bodyTrackers[2]);
            }
            else if (Vector3.Dot(pos0 - pos1, pos2 - pos1) < 0)
            {
                //1 is the hips tracker
                InitVRNode(HumanBodyBones.Hips, bodyTrackers[1]);
                InitVRNode(HumanBodyBones.LeftHand, bodyTrackers[0]);
                InitVRNode(HumanBodyBones.RightHand, bodyTrackers[2]);
            }
            else
            {
                //2 is the hips tracker
                InitVRNode(HumanBodyBones.Hips, bodyTrackers[2]);
                InitVRNode(HumanBodyBones.LeftHand, bodyTrackers[0]);
                InitVRNode(HumanBodyBones.RightHand, bodyTrackers[1]);
            }
        }

        protected virtual void InitVRNode(HumanBodyBones nodeType, XRNodeState initialState)
        {
            
        }

        public virtual bool IsHandsSwapped()
        {
            return _isHandsSwaped;
        }

        public virtual bool IsVRNodesSwaped(HumanBodyBones typeNodeLeft, HumanBodyBones typeNodeRight, bool doSwaping = true)
        {
            return IsVRNodesSwaped(GetVRNode(typeNodeLeft), GetVRNode(typeNodeRight), doSwaping);
        }

        public virtual bool IsVRNodesSwaped(QuickVRNode nodeLeft, QuickVRNode nodeRight, bool doSwaping = true)
        {
            bool result = false;

            QuickVRNode hmdNode = GetVRNode(HumanBodyBones.Head);
            if (hmdNode.IsTracked() && nodeLeft.IsTracked() && nodeRight.IsTracked())
            {
                float dLeft = Vector3.Dot(nodeLeft.transform.position - hmdNode.transform.position, hmdNode.transform.right);
                float dRight = Vector3.Dot(nodeRight.transform.position - hmdNode.transform.position, hmdNode.transform.right);

                result = dLeft > dRight;
                if (result && doSwaping)
                {
                    SwapQuickVRNode(nodeLeft, nodeRight);
                }
            }
            
            return result;
        }

        protected virtual void SwapQuickVRNode(QuickVRNode vrNodeA, QuickVRNode vrNodeB)
        {
            InputDevice deviceA = vrNodeA._inputDevice;
            vrNodeA._inputDevice = vrNodeB._inputDevice;
            vrNodeB._inputDevice = deviceA;
        }

        #endregion

        #region UPDATE

        public virtual void UpdateVRNodes()
        {
            //List<InputDevice> inputDevices = new List<InputDevice>();
            foreach (var pair in _vrNodes)
            {
                pair.Value.UpdateState();
            }

            //if (GetVRNode(HumanBodyBones.RightHand)._inputDevice.TryGetFeatureValue(CommonUsages.handData, out Hand rightHand))
            //{
            //    Debug.Log("HAND DATA!!!");
            //}
        }

        #endregion

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            foreach (var pair in _vrNodes)
            {
                QuickVRNode n = pair.Value;
                if (n.IsTracked())
                {
                    DebugExtension.DrawCoordinatesSystem(n.transform.position, n.transform.right, n.transform.up, n.transform.forward, 0.05f);

                    float s = 0.0125f;
                    Vector3 cSize = Vector3.one * s;
                    
                    Gizmos.matrix = n.transform.localToWorldMatrix;
                    Gizmos.DrawCube(Vector3.zero, cSize);
                    QuickTrackedObject tObject = n.GetTrackedObject();
                    if (tObject.transform.localPosition != Vector3.zero)
                    {
                        Gizmos.DrawSphere(tObject.transform.localPosition, s * 0.5f);
                        Gizmos.DrawLine(Vector3.zero, tObject.transform.localPosition);
                    }
                    Gizmos.matrix = Matrix4x4.identity;
                }
            }
        }

        #endregion

        #region HELPERS

        private static int CompareInputDevicesHeight(InputDevice deviceA, InputDevice deviceB)
        {
            deviceA.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 posA);
            deviceB.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 posB);

            return -posA.y.CompareTo(posB.y);
        }

        #endregion

    }
}



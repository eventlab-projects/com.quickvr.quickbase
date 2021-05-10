using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace QuickVR
{

    public class QuickOVRHand : OVRHand
    {

        #region PROTECTED ATTRIBUTES

        public enum FingerPhalange
        {
            Proximal, 
            Intermediate, 
            Distal, 
            Tip,
        }

        protected Animator _animator = null;
        protected OVRSkeleton _skeleton = null;
        protected SkinnedMeshRenderer _renderer = null;

        protected bool _physicsInitialized = false;

        protected Dictionary<OVRSkeleton.BoneId, QuickOVRHandBonePhysics> _handBonePhysics = new Dictionary<OVRSkeleton.BoneId, QuickOVRHandBonePhysics>();
        protected BoxCollider _handCollider = null;

        protected QuickUnityVR _headTracking = null;
        protected QuickVRPlayArea _playArea = null;

        protected int[] _handFingerConfidence = null;           //The confidence of the finger

        protected QuickVRNode _vrNodeHand = null;
        protected QuickVRNode[] _vrNodeFingers = null;
        protected Transform[] _tBoneFingers = null;
        protected OVRPlugin.Node _ovrNodeHand = OVRPlugin.Node.None;

        #endregion

        #region CONSTANTS

        protected const int NUM_FRAMES_CONFIDENCE = 5;
        protected const float BONE_RADIUS = 0.0075f;

        protected const int NUM_BONES_PER_FINGER = 4;

        protected static OVRSkeleton.BoneId[] _ovrFingerBones = new OVRSkeleton.BoneId[]
        {
            OVRSkeleton.BoneId.Hand_Thumb1,
            OVRSkeleton.BoneId.Hand_Thumb2,
            OVRSkeleton.BoneId.Hand_Thumb3,
            OVRSkeleton.BoneId.Hand_ThumbTip,

            OVRSkeleton.BoneId.Hand_Index1,
            OVRSkeleton.BoneId.Hand_Index2,
            OVRSkeleton.BoneId.Hand_Index3,
            OVRSkeleton.BoneId.Hand_IndexTip,

            OVRSkeleton.BoneId.Hand_Middle1,
            OVRSkeleton.BoneId.Hand_Middle2,
            OVRSkeleton.BoneId.Hand_Middle3,
            OVRSkeleton.BoneId.Hand_MiddleTip,

            OVRSkeleton.BoneId.Hand_Ring1,
            OVRSkeleton.BoneId.Hand_Ring2,
            OVRSkeleton.BoneId.Hand_Ring3,
            OVRSkeleton.BoneId.Hand_RingTip,

            OVRSkeleton.BoneId.Hand_Pinky1,
            OVRSkeleton.BoneId.Hand_Pinky2,
            OVRSkeleton.BoneId.Hand_Pinky3,
            OVRSkeleton.BoneId.Hand_PinkyTip,
        };

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Start()
        {
            _skeleton = transform.GetOrCreateComponent<OVRSkeleton>();
            transform.GetOrCreateComponent<OVRMesh>();
            transform.GetOrCreateComponent<OVRMeshRenderer>();
            _renderer = transform.GetOrCreateComponent<SkinnedMeshRenderer>();
            //_renderer.material = Resources.Load<Material>("Materials/QuickDiffuseCyan");
            _renderer.material = Resources.Load<Material>("Materials/QuickOVRHandMaterial");
            _headTracking = GetComponentInParent<QuickUnityVR>();
            _playArea = GetComponentInParent<QuickVRPlayArea>();
            _animator = _headTracking.GetComponent<Animator>();

            _vrNodeHand = _playArea.GetVRNode(IsLeft() ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
            _ovrNodeHand = IsLeft() ? OVRPlugin.Node.HandLeft : OVRPlugin.Node.HandRight;

            QuickHumanFingers[] fingers = QuickHumanTrait.GetHumanFingers();
            int numFingers = fingers.Length;
            _handFingerConfidence = new int[numFingers];
            _vrNodeFingers = new QuickVRNode[numFingers * NUM_BONES_PER_FINGER];
            _tBoneFingers = new Transform[numFingers * NUM_BONES_PER_FINGER];

            for (int i = 0; i < numFingers; i++)
            {
                _handFingerConfidence[i] = NUM_FRAMES_CONFIDENCE;
                List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(fingers[i], IsLeft());
                for (int j = 0; j < NUM_BONES_PER_FINGER; j++)
                {
                    int fingerBoneID = (i * NUM_BONES_PER_FINGER) + j;
                    Transform tBone = _animator.GetBoneTransform(fingerBones[j]);
                    
                    _vrNodeFingers[fingerBoneID] = _playArea.GetVRNode(fingerBones[j]);
                    _tBoneFingers[fingerBoneID] = tBone;
                }
            }
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPreUpdateTracking += UpdateVRNodeTracked;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPreUpdateTracking -= UpdateVRNodeTracked;
        }

        protected virtual void CreatePhysics()
        {
            foreach (OVRSkeleton.BoneId f in QuickUtils.GetEnumValues<OVRSkeleton.BoneId>())
            {
                if (f == OVRSkeleton.BoneId.Invalid || f == OVRSkeleton.BoneId.Max) continue;

                //float r = f == HandBone.HandCenter ? 0.025f : BONE_RADIUS;
                QuickOVRHandBonePhysics result = CreatePhysicsBone(f, BONE_RADIUS);
                _handBonePhysics[f] = result;
            }

            Vector3 p1 = GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Thumb1).position;
            Vector3 p2 = GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Index1).position;
            Vector3 p3 = GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Pinky1).position;
            Vector3 p4 = GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Pinky0).position;

            _handCollider = transform.CreateChild("__HandCollider__").GetOrCreateComponent<BoxCollider>();
            MeshRenderer r = _handCollider.GetOrCreateComponent<MeshRenderer>();
            r.material = Resources.Load<Material>("Materials/QuickDiffuseWhite");
            _handCollider.isTrigger = true;
            //_handCollider.GetOrCreateComponent<MeshFilter>().mesh = QuickUtils.GetUnityPrimitiveMesh(PrimitiveType.Cube);

            _handCollider.transform.position = (p1 + p2 + p3 + p4) * 0.25f;
            _handCollider.transform.LookAt(_handCollider.transform.position + (p2 - p3).normalized, transform.up);

            float sx = Vector3.Distance(p3, p4);
            float sz = Vector3.Distance(p2, p3);
            float sy = Vector3.Distance(p1, p2);

            _handCollider.transform.localScale = Vector3.Scale(new Vector3(sx, sy, sz), new Vector3(1.75f, 1.75f, 1.2f));
        }

        protected virtual QuickOVRHandBonePhysics CreatePhysicsBone(OVRSkeleton.BoneId boneID, float boneRadius)
        {
            Transform tBone = GetOVRBoneTransform(boneID);
            QuickOVRHandBonePhysics result = tBone.CreateChild("__Physics__").GetOrCreateComponent<QuickOVRHandBonePhysics>();
            result.SetRadius(boneRadius);

            return result;
        }

        #endregion

        #region GET AND SET

        public virtual bool IsInitialized()
        {
            return _skeleton.IsInitialized;
        }

        public virtual Transform GetOVRBoneTransform(OVRSkeleton.BoneId boneID)
        {
            return IsInitialized()? _skeleton.Bones[(int)boneID].Transform : null;
        }

        public virtual Transform GetOVRBoneTransform(HandFinger fingerID, FingerPhalange phalangeID)
        {
            if (fingerID == HandFinger.Index)
            {
                if (phalangeID == FingerPhalange.Proximal) return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Index1);
                if (phalangeID == FingerPhalange.Intermediate) return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Index2);
                if (phalangeID == FingerPhalange.Distal) return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Index3);
                return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_IndexTip);
            }
            if (fingerID == HandFinger.Middle)
            {
                if (phalangeID == FingerPhalange.Proximal) return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Middle1);
                if (phalangeID == FingerPhalange.Intermediate) return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Middle2);
                if (phalangeID == FingerPhalange.Distal) return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Middle3);
                return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_MiddleTip);
            }
            if (fingerID == HandFinger.Ring)
            {
                if (phalangeID == FingerPhalange.Proximal) return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Ring1);
                if (phalangeID == FingerPhalange.Intermediate) return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Ring2);
                if (phalangeID == FingerPhalange.Distal) return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Ring3);
                return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_RingTip);
            }
            if (fingerID == HandFinger.Pinky)
            {
                if (phalangeID == FingerPhalange.Proximal) return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Pinky1);
                if (phalangeID == FingerPhalange.Intermediate) return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Pinky2);
                if (phalangeID == FingerPhalange.Distal) return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Pinky3);
                return GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_PinkyTip);
            }

            return null;
        }

        protected virtual OVRSkeleton.BoneId GetHandBoneTip(HandFinger finger)
        {
            if (finger == HandFinger.Thumb) return OVRSkeleton.BoneId.Hand_Thumb3;
            if (finger == HandFinger.Index) return OVRSkeleton.BoneId.Hand_Index3;
            if (finger == HandFinger.Middle) return OVRSkeleton.BoneId.Hand_Middle3;
            if (finger == HandFinger.Ring) return OVRSkeleton.BoneId.Hand_Ring3;
            return OVRSkeleton.BoneId.Hand_Pinky3;
        }

        public virtual QuickOVRHandBonePhysics GetHandBonePhysics(OVRSkeleton.BoneId boneID)
        {
            return _handBonePhysics.ContainsKey(boneID)? _handBonePhysics[boneID] : null;
        }

        public virtual BoxCollider GetHandCollider()
        {
            return _handCollider;
        }

        public bool GetFingerIsTouchingHand(HandFinger finger)
        {
            Transform t1 = GetOVRBoneTransform(finger, FingerPhalange.Proximal);
            Transform t2 = GetOVRBoneTransform(finger, FingerPhalange.Intermediate);
            Transform t3 = GetOVRBoneTransform(finger, FingerPhalange.Distal);
            Transform t4 = GetOVRBoneTransform(finger, FingerPhalange.Tip);

            Vector3 u = t4.position - t3.position;
            Vector3 v = t2.position - t3.position;
            return Vector3.Angle(u, v) < 150;


            //QuickOVRHandBonePhysics tipBone = GetHandBonePhysics(GetHandBoneTip(finger));
            //if (tipBone)
            //{
            //    Collider[] colliders = Physics.OverlapSphere(tipBone.transform.position, tipBone.GetCollider().radius);
            //    foreach (Collider c in colliders)
            //    {
            //        if (c == _handCollider)
            //        {
            //            return true;
            //        }
            //    }
            //}

            //return false;

            //QuickOVRHandBonePhysics tipBone = GetHandBonePhysics(GetHandBoneTip(finger));
            //QuickOVRHandBonePhysics proximalBone = GetHandBonePhysics(GetHandBoneProximal(finger));

            //return Vector3.Distance(tipBone.transform.position, proximalBone.transform.position) < 0.05f;
        }

        public virtual bool IsFist()
        {
            return
                (
                GetFingerIsTouchingHand(HandFinger.Thumb) &&
                GetFingerIsTouchingHand(HandFinger.Index) &&
                GetFingerIsTouchingHand(HandFinger.Middle) &&
                GetFingerIsTouchingHand(HandFinger.Ring) &&
                GetFingerIsTouchingHand(HandFinger.Pinky)
                );
        }

        public virtual bool IsThumbUp()
        {
            return IsThumb(true);
        }

        public virtual bool IsThumbDown()
        {
            return IsThumb(false);
        }

        protected virtual bool IsThumb(bool isUp)
        {
            if (
                GetFingerIsTouchingHand(HandFinger.Index) &&
                GetFingerIsTouchingHand(HandFinger.Middle) &&
                GetFingerIsTouchingHand(HandFinger.Ring) &&
                GetFingerIsTouchingHand(HandFinger.Pinky)
                )
            {
                //Vector3 handDir = (GetOVRBoneTransform(HandBone.IndexProximal).position - GetOVRBoneTransform(HandBone.LittleProximal).position).normalized;
                //Vector3 thumbDir = (GetOVRBoneTransform(HandBone.ThumbTip).position - GetOVRBoneTransform(HandBone.ThumbIntermediate).position).normalized;
                //Vector3 globalUp = isUp ? Vector3.up : Vector3.down;

                //bool isMatchLocalUp = Vector3.Angle(handDir, thumbDir) < 45.0f;
                //bool isMatchGlobalUp = Vector3.Angle(thumbDir, globalUp) < 45.0f;
                //float d = Vector3.Dot(thumbDir, globalUp);

                //return (d > 0) && isMatchLocalUp && isMatchGlobalUp;

                Vector3 thumbDir = (GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_ThumbTip).position - GetOVRBoneTransform(OVRSkeleton.BoneId.Hand_Thumb2).position).normalized;
                Vector3 globalUp = isUp ? Vector3.up : Vector3.down;

                float d = Vector3.Dot(thumbDir, globalUp);

                return (d > 0) && (Vector3.Angle(thumbDir, globalUp) < 45.0f);
            }
            return false;
        }

        protected virtual bool IsLeft()
        {
            return _skeleton.GetSkeletonType() == OVRSkeleton.SkeletonType.HandLeft;
        }

        public virtual bool IsDataHighConfidenceFinger(QuickHumanFingers finger)
        {
            //return IsDataHighConfidence && GetFingerConfidence(_toOVRHandFinger[finger]) == TrackingConfidence.High;
            return IsDataHighConfidence && _handFingerConfidence[(int)finger] >= NUM_FRAMES_CONFIDENCE;
        }

        #endregion

        #region UPDATE

        protected virtual void UpdateVRNodeTracked()
        {
            if (_headTracking._handTrackingMode == QuickUnityVR.HandTrackingMode.Hands)
            {
                bool isTrackedPos = OVRPlugin.GetNodePositionTracked(_ovrNodeHand);
                if (isTrackedPos)
                {
                    _vrNodeHand.transform.localPosition = OVRPlugin.GetNodePose(_ovrNodeHand, OVRPlugin.Step.Render).ToOVRPose().position;
                }

                bool isTrackedRot = OVRPlugin.GetNodeOrientationTracked(_ovrNodeHand);
                if (isTrackedRot)
                {
                    _vrNodeHand.transform.localRotation = OVRPlugin.GetNodePose(_ovrNodeHand, OVRPlugin.Step.Render).ToOVRPose().orientation;
                }

                //vrNode.SetTracked(isTrackedPos || isTrackedRot);
                _vrNodeHand.SetTracked(IsDataHighConfidence);

                UpdateVRNodeFingers();
            }
        }

        protected virtual void UpdateVRNodeFingers()
        {
            if (IsInitialized()) 
            {
                if (!_physicsInitialized)
                {
                    CreatePhysics();
                    _physicsInitialized = true;
                }


                //Update the nodes of the fingers
                foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                {
                    _handFingerConfidence[(int)f] = IsDataHighConfidence ? Mathf.Min(_handFingerConfidence[(int)f] + 1, NUM_FRAMES_CONFIDENCE) : 0;
                                        
                    for (int i = 0; i < NUM_BONES_PER_FINGER; i++)
                    {
                        int boneID = ((int)f) * NUM_BONES_PER_FINGER + i;
                        QuickVRNode nFinger = _vrNodeFingers[boneID]; //_playArea.GetVRNode(fingerBones[i]);
                        
                        if (IsDataHighConfidenceFinger(f))
                        {
                            //The finger is tracked.
                            OVRSkeleton.BoneId ovrBoneID = _ovrFingerBones[boneID];
                            Transform ovrBone = GetOVRBoneTransform(ovrBoneID);
                            nFinger.transform.position = ovrBone.position;
                            nFinger.transform.rotation = ovrBone.rotation;

                            //Correct the rotation
                            if (IsLeft())
                            {
                                nFinger.transform.Rotate(Vector3.right, 180, Space.Self);
                                nFinger.transform.Rotate(Vector3.up, -90, Space.Self);
                            }
                            else
                            {
                                nFinger.transform.Rotate(Vector3.up, 90, Space.Self);
                            }
                            

                            nFinger.SetTracked(true);
                        }
                        else
                        {
                            //The finger is not tracked. Restore the last valid local rotation. 
                            nFinger.SetTracked(false);
                        }
                    }
                }
            }
        }

        #endregion

    }
}



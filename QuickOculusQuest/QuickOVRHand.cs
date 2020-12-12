﻿using System.Collections;
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

        protected static Dictionary<QuickHumanFingers, HandFinger> _toOVRHandFinger = new Dictionary<QuickHumanFingers, HandFinger>();
        protected Dictionary<QuickHumanFingers, int> _handFingerConfidence = new Dictionary<QuickHumanFingers, int>();
        protected Dictionary<QuickHumanFingers, Quaternion[]> _handFingerLastRotation = new Dictionary<QuickHumanFingers, Quaternion[]>();

        #endregion

        #region CONSTANTS

        protected const int NUM_FRAMES_CONFIDENCE = 5;
        protected const float BONE_RADIUS = 0.0075f;

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        protected static void Init()
        {
            _toOVRHandFinger[QuickHumanFingers.Thumb] = HandFinger.Thumb;
            _toOVRHandFinger[QuickHumanFingers.Index] = HandFinger.Index;
            _toOVRHandFinger[QuickHumanFingers.Middle] = HandFinger.Middle;
            _toOVRHandFinger[QuickHumanFingers.Ring] = HandFinger.Ring;
            _toOVRHandFinger[QuickHumanFingers.Little] = HandFinger.Pinky;
        }

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

            foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
            {
                _handFingerConfidence[f] = NUM_FRAMES_CONFIDENCE;
                _handFingerLastRotation[f] = null;
            }
        }

        protected virtual void OnEnable()
        {
            QuickVRManager.OnPreUpdateTrackingEarly += UpdateVRNodeTracked;
        }

        protected virtual void OnDisable()
        {
            QuickVRManager.OnPreUpdateTrackingEarly -= UpdateVRNodeTracked;
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
            return IsDataHighConfidence && _handFingerConfidence[finger] >= NUM_FRAMES_CONFIDENCE;
        }

        #endregion

        #region UPDATE

        public virtual void UpdateTracking()
        {
            if (IsInitialized()) 
            {
                if (!_physicsInitialized)
                {
                    CreatePhysics();
                    _physicsInitialized = true;
                }

                foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                {
                    _handFingerConfidence[f] = IsDataHighConfidence ? Mathf.Min(_handFingerConfidence[f] + 1, NUM_FRAMES_CONFIDENCE) : 0;
                    List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(f, IsLeft());
                    if (_handFingerLastRotation[f] == null)
                    {
                        //The last rotation of this finger has not been initialized yet. Initialize it with the current local rotation
                        //of the bone fingers. 
                        _handFingerLastRotation[f] = new Quaternion[3];
                        for (int i = 0; i < 3; i++)
                        {
                            _handFingerLastRotation[f][i] = _animator.GetBoneTransform(fingerBones[i]).localRotation;
                        }
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        QuickVRNode nFinger = _playArea.GetVRNode((HumanBodyBones)fingerBones[i]);

                        if (IsDataHighConfidenceFinger(f))
                        {
                            //The finger is tracked.
                            UpdateTracking(fingerBones[i], fingerBones[i + 1]);
                            _handFingerLastRotation[f][i] = _animator.GetBoneTransform(fingerBones[i]).localRotation;

                            nFinger.transform.position = GetOVRBoneTransform(QuickOVRHandsInitializer.ToOVR(fingerBones[i])).position;
                            nFinger.SetTracked(true);
                        }
                        else
                        {
                            //The finger is not tracked. Restore the last valid local rotation. 
                            _animator.GetBoneTransform(fingerBones[i]).localRotation = _handFingerLastRotation[f][i];
                            nFinger.SetTracked(false);
                        }
                    }
                }
            }
        }

        protected virtual void UpdateVRNodeTracked()
        {
            if (_headTracking._handTrackingMode == QuickUnityVR.HandTrackingMode.Hands)
            {
                QuickVRNode vrNode = _playArea.GetVRNode(IsLeft() ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
                OVRPlugin.Node ovrNode = IsLeft() ? OVRPlugin.Node.HandLeft : OVRPlugin.Node.HandRight;

                bool isTrackedPos = OVRPlugin.GetNodePositionTracked(ovrNode);
                if (isTrackedPos)
                {
                    vrNode.transform.localPosition = OVRPlugin.GetNodePose(ovrNode, OVRPlugin.Step.Render).ToOVRPose().position;
                }

                bool isTrackedRot = OVRPlugin.GetNodeOrientationTracked(ovrNode);
                if (isTrackedRot)
                {
                    vrNode.transform.localRotation = OVRPlugin.GetNodePose(ovrNode, OVRPlugin.Step.Render).ToOVRPose().orientation;
                }

                //vrNode.SetTracked(isTrackedPos || isTrackedRot);
                vrNode.SetTracked(IsDataHighConfidence);
            }
        }

        protected virtual void UpdateTracking(QuickHumanBodyBones boneID0, QuickHumanBodyBones boneID1)
        {
            Transform bone0 = _animator.GetBoneTransform(boneID0);
            Transform bone1 = _animator.GetBoneTransform(boneID1);
            Transform ovrBone0 = GetOVRBoneTransform(QuickOVRHandsInitializer.ToOVR(boneID0));
            Transform ovrBone1 = GetOVRBoneTransform(QuickOVRHandsInitializer.ToOVR(boneID1));

            Vector3 currentDir = bone1.position - bone0.position;
            Vector3 targetDir = ovrBone1.position - ovrBone0.position;
            float rotAngle = Vector3.Angle(currentDir, targetDir);
            Vector3 rotAxis = Vector3.Cross(currentDir, targetDir).normalized;

            bone0.Rotate(rotAxis, rotAngle, Space.World);
        }

        #endregion

    }
}



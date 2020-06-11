using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace QuickVR
{

    public class QuickOVRHand : OVRHand
    {

        #region PUBLIC ATTRIBUTES

        public bool _debug = true;

        #endregion

        #region PROTECTED ATTRIBUTES

        protected Animator _animator
        {
            get
            {
                return QuickSingletonManager.GetInstance<QuickVRManager>().GetAnimatorSource();
            }
        }

        protected OVRSkeleton _skeleton = null;
        protected SkinnedMeshRenderer _renderer = null;

        protected bool _physicsInitialized = false;

        protected Dictionary<OVRSkeleton.BoneId, QuickOVRHandBonePhysics> _handBonePhysics = new Dictionary<OVRSkeleton.BoneId, QuickOVRHandBonePhysics>();
        protected BoxCollider _handCollider = null;

        #endregion

        #region CONSTANTS

        protected const float BONE_RADIUS = 0.0075f;

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

        protected virtual Transform GetOVRBoneTransform(OVRSkeleton.BoneId boneID)
        {
            return _skeleton.Bones[(int)boneID].Transform;
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
            QuickOVRHandBonePhysics tipBone = GetHandBonePhysics(GetHandBoneTip(finger));
            if (tipBone)
            {
                Collider[] colliders = Physics.OverlapSphere(tipBone.transform.position, tipBone.GetCollider().radius);
                foreach (Collider c in colliders)
                {
                    if (c == _handCollider)
                    {
                        return true;
                    }
                }
            }
            
            return false;

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

        #endregion

        #region UPDATE

        protected virtual void Update()
        {
            _renderer.enabled = _debug;
        }

        public virtual void UpdateTracking()
        {
            if (_skeleton.IsInitialized) 
            {
                if (!_physicsInitialized)
                {
                    CreatePhysics();
                    _physicsInitialized = true;
                }

                foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                {
                    List<QuickHumanBodyBones> fingerBones = QuickHumanTrait.GetBonesFromFinger(f, IsLeft());
                    UpdateTracking(fingerBones[0], fingerBones[1]); //Proximal, Intermediate
                    UpdateTracking(fingerBones[1], fingerBones[2]); //Intermediate, Distal
                    UpdateTracking(fingerBones[2], fingerBones[3]); //Distal, Tip
                }
            }
        }

        protected virtual void UpdateTracking(QuickHumanBodyBones boneID0, QuickHumanBodyBones boneID1)
        {
            if (_animator)
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
        }

        #endregion

    }
}



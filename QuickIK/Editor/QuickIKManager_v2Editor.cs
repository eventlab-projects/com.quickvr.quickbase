//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using UnityEditor.SceneManagement;

//using UnityEngine.Animations.Rigging;

//namespace QuickVR
//{

//    [CustomEditor(typeof(QuickIKManager_v2), true)]
//    [InitializeOnLoad]
//    public class QuickIKSolverHumanoidEditor : Editor
//    {
        
//        #region PROTECTED ATTRIBUTES

//        protected QuickIKManager_v2 _ikManager = null;
//        protected TwoBoneIKConstraint _ikSolverHead = null;
//        protected QuickIKSolverHumanoid _ikSolverBody = null;

//        protected Animator _animator = null;

//        #endregion

//        #region CONSTANTS

//        protected const float HANDLE_SIZE = 0.2f;
//        protected const float PICK_SIZE = 0.25f;

//        #endregion

//        #region CREATION AND DESTRUCTION

//        protected virtual void OnEnable()
//        {
//            _ikManager = (QuickIKManager_v2)target;
//            _ikSolverHead = _ikManager.GetIKSolverHead();
//            _ikSolverBody = _ikManager.GetIKSolverBody();
//            _animator = _ikManager.GetComponent<Animator>();

//            SceneView.duringSceneGui += UpdateDebug;
//        }

//        protected virtual List<Transform> GetIKTargetsLimb()
//        {
//            List<Transform> result = new List<Transform>();
//            result.Add(_ikSolverHead.data.target);
//            result.Add(_ikSolverBody.data._ikTargetHips);
//            result.Add(_ikSolverBody.data._ikTargetLeftHand);
//            result.Add(_ikSolverBody.data._ikTargetRightHand);
//            result.Add(_ikSolverBody.data._ikTargetLeftFoot);
//            result.Add(_ikSolverBody.data._ikTargetRightFoot);

//            return result;
//        }

//        protected virtual List<Transform> GetIKTargetsMid()
//        {
//            List<Transform> result = new List<Transform>();
//            result.Add(_ikSolverHead.data.hint);
//            result.Add(_ikSolverBody.data._ikTargetLeftHandHint);
//            result.Add(_ikSolverBody.data._ikTargetRightHandHint);
//            result.Add(_ikSolverBody.data._ikTargetLeftFootHint);
//            result.Add(_ikSolverBody.data._ikTargetRightFootHint);

//            return result;
//        }

//        #endregion

//        #region UPDATE

//        public override void OnInspectorGUI()
//        {
//            DrawDefaultInspector();
//        }

//        protected virtual void UpdateDebug(SceneView sceneView)
//        {
//            DrawIKTargets(GetIKTargetsLimb(), Handles.CubeHandleCap);
//            DrawIKTargets(GetIKTargetsMid(), Handles.SphereHandleCap);
//        }

//        protected virtual void DrawIKTargets(List<Transform> ikTargets, Handles.CapFunction function)
//        {
//            foreach (Transform t in ikTargets)
//            {
//                if (!t) continue;

//                string tName = t.name.ToLower();
//                if (tName.Contains("left")) Handles.color = new Color(0, 0, 1, 0.5f);
//                else if (tName.Contains("right")) Handles.color = new Color(1, 0, 0, 0.5f);
//                else Handles.color = new Color(1, 1, 0, 0.5f);

//                float size = HandleUtility.GetHandleSize(t.position);
//                if (Handles.Button(t.position, t.rotation, size * HANDLE_SIZE, size * PICK_SIZE, function))
//                {
//                    Selection.activeTransform = t;
//                    Repaint();
//                }
//            }
//        }

//        protected virtual void DrawIKSolver(HumanBodyBones boneLimbID)
//        {
//            Transform boneUpper, boneMid, boneLimb, targetLimb, targetHint;
//            boneLimb = _animator.GetBoneTransform(boneLimbID);

//            if (boneLimbID == HumanBodyBones.Head)
//            {
//                boneUpper = _ikSolverHead.data.root;
//                boneMid = _ikSolverHead.data.mid;
//                targetLimb = _ikSolverHead.data.target;
//                targetHint = _ikSolverHead.data.hint;
//            }
//            else if (boneLimbID == HumanBodyBones.LeftHand)
//            {
//                boneUpper = _animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
//                boneMid = _animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
//                targetLimb = _ikSolverBody.data._ikTargetLeftHand;
//                targetHint = _ikSolverBody.data._ikTargetLeftHandHint;
//            }
//            else if (boneLimbID == HumanBodyBones.RightHand)
//            {
//                boneUpper = _animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
//                boneMid = _animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
//                targetLimb = _ikSolverBody.data._ikTargetRightHand;
//                targetHint = _ikSolverBody.data._ikTargetRightHandHint;
//            }
//            else if (boneLimbID == HumanBodyBones.LeftFoot)
//            {
//                boneUpper = _animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
//                boneMid = _animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
//                targetLimb = _ikSolverBody.data._ikTargetLeftFoot;
//                targetHint = _ikSolverBody.data._ikTargetLeftFootHint;
//            }

//        }

//        #endregion

//    }

//}



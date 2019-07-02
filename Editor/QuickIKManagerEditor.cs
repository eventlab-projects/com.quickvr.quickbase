using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine.Animations.Rigging;

namespace QuickVR
{

    [CustomEditor(typeof(QuickIKManager), true)]
    [InitializeOnLoad]
    public class QuickIKManagerEditor : Editor
    {

        #region PROTECTED ATTRIBUTES

        protected QuickIKManager _ikManager = null;

        [SerializeField] protected bool _showCfgBody = false;
        [SerializeField] protected bool _showCfgLeftHand = false;
        [SerializeField] protected bool _showCfgRightHand = false;

        #endregion

        #region CONSTANTS

        protected const float HANDLE_SIZE = 0.2f;
        protected const float PICK_SIZE = 0.25f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            _ikManager = (QuickIKManager)target;

            SceneView.duringSceneGui += UpdateDebug;
        }

        #endregion

        #region UPDATE

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            _showCfgBody = EditorGUILayout.Foldout(_showCfgBody, "Body IK Solvers");
            if (_showCfgBody)
            {
                EditorGUI.indentLevel++;
                foreach (string boneName in QuickUtils.GetEnumValuesToString<IKLimbBones>())
                {
                    DrawIKSolverProperties(_ikManager.GetIKSolver(boneName), boneName);
                    EditorGUILayout.Space();
                }
                EditorGUI.indentLevel--;
            }

            _showCfgLeftHand = EditorGUILayout.Foldout(_showCfgLeftHand, "Left Hand IK Solvers");
            if (_showCfgLeftHand)
            {
                EditorGUI.indentLevel++;
                foreach (string boneName in QuickUtils.GetEnumValuesToString<IKLimbBonesHand>())
                {
                    DrawIKSolverProperties(_ikManager.GetIKSolver("Left" + boneName + "Distal"), boneName);
                    EditorGUILayout.Space();
                }
                EditorGUI.indentLevel--;
            }

            _showCfgRightHand = EditorGUILayout.Foldout(_showCfgRightHand, "Right Hand IK Solvers");
            if (_showCfgRightHand)
            {
                EditorGUI.indentLevel++;
                foreach (string boneName in QuickUtils.GetEnumValuesToString<IKLimbBonesHand>())
                {
                    DrawIKSolverProperties(_ikManager.GetIKSolver("Right" + boneName + "Distal"), boneName);
                    EditorGUILayout.Space();
                }
                EditorGUI.indentLevel--;
            }

        }

        protected virtual void DrawIKSolverProperties(IQuickIKSolver ikSolver, string boneName)
        {
            if (ikSolver == null) return;

            EditorGUILayout.LabelField(boneName, EditorStyles.boldLabel);
            ikSolver._weightIKPos = EditorGUILayout.Slider(" IK Pos Weight", ikSolver._weightIKPos, 0.0f, 1.0f);
            ikSolver._weightIKRot = EditorGUILayout.Slider(" IK Rot Weight", ikSolver._weightIKRot, 0.0f, 1.0f);
            if (ikSolver._targetHint)
            {
                ikSolver._weightIKHint = EditorGUILayout.Slider(" IK Rot Weight", ikSolver._weightIKHint, 0.0f, 1.0f);
            }
        }

        protected virtual void UpdateDebug(SceneView sceneView)
        {
            //DrawIKTargets(GetIKTargetsLimb(), Handles.CubeHandleCap);
            //DrawIKTargets(GetIKTargetsMid(), Handles.SphereHandleCap);

            foreach (IQuickIKSolver s in _ikManager.GetIKSolversBody()) DrawIKSolver(s, false);
            foreach (IQuickIKSolver s in _ikManager.GetIKSolversLeftHand()) DrawIKSolver(s, true);
            foreach (IQuickIKSolver s in _ikManager.GetIKSolversRightHand()) DrawIKSolver(s, true);
        }

        protected virtual void DrawIKTarget(Transform t, Handles.CapFunction function, bool isSolverFinger)
        {
            if (!t) return;

            string tName = t.name.ToLower();
            if (tName.Contains("left")) Handles.color = new Color(0, 0, 1, 0.5f);
            else if (tName.Contains("right")) Handles.color = new Color(1, 0, 0, 0.5f);
            else Handles.color = new Color(1, 1, 0, 0.5f);

            float maxSize = isSolverFinger ? 0.1f : 0.5f;
            float size = Mathf.Min(HandleUtility.GetHandleSize(t.position), maxSize);
            if (Handles.Button(t.position, t.rotation, size * HANDLE_SIZE, size * PICK_SIZE, function))
            {
                Selection.activeTransform = t;
                Repaint();
            }
        }

        protected virtual void DrawIKSolver(IQuickIKSolver ikSolver, bool isSolverFinger)
        {
            Handles.color = Color.magenta;
            if (ikSolver._boneUpper && ikSolver._boneMid) Handles.DrawLine(ikSolver._boneUpper.position, ikSolver._boneMid.position);
            if (ikSolver._boneMid && ikSolver._boneLimb) Handles.DrawLine(ikSolver._boneMid.position, ikSolver._boneLimb.position);

            Handles.color = Color.yellow;
            if (ikSolver._boneMid && ikSolver._targetHint) Handles.DrawLine(ikSolver._boneMid.position, ikSolver._targetHint.position);

            Handles.color = Color.cyan;
            if (ikSolver._boneUpper && ikSolver._targetLimb)
            {
                float chainLength = Vector3.Distance(ikSolver._boneUpper.position, ikSolver._boneMid.position) + Vector3.Distance(ikSolver._boneMid.position, ikSolver._boneLimb.position);
                Vector3 v = ikSolver._targetLimb.position - ikSolver._boneUpper.position;
                Vector3 p = ikSolver._boneUpper.position + (v.normalized * Mathf.Min(v.magnitude, chainLength));
                Handles.DrawLine(ikSolver._boneUpper.position, p);
            }

            DrawIKTarget(ikSolver._targetLimb, Handles.CubeHandleCap, isSolverFinger);
            DrawIKTarget(ikSolver._targetHint, Handles.SphereHandleCap, isSolverFinger);
        }

        #endregion

    }

}



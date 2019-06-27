using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine.Animations.Rigging;

namespace QuickVR
{

    [CustomEditor(typeof(QuickIKManager_v2), true)]
    [InitializeOnLoad]
    public class QuickIKSolverHumanoidEditor : Editor
    {

        #region PROTECTED ATTRIBUTES

        protected QuickIKManager_v2 _ikManager = null;
        
        protected Animator _animator = null;

        #endregion

        #region CONSTANTS

        protected const float HANDLE_SIZE = 0.2f;
        protected const float PICK_SIZE = 0.25f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            _ikManager = (QuickIKManager_v2)target;
            _animator = _ikManager.GetComponent<Animator>();

            SceneView.duringSceneGui += UpdateDebug;
        }

        #endregion

        #region UPDATE

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
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



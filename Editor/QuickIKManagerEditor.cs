﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

//using UnityEngine.Animations.Rigging;

namespace QuickVR
{

    [CustomEditor(typeof(QuickIKManager), true)]
    [InitializeOnLoad]
    public class QuickIKManagerEditor : QuickBaseEditor
    {

        #region PROTECTED ATTRIBUTES

        protected QuickIKManager _ikManager = null;
        
        #endregion

        #region CONSTANTS

        protected const float HANDLE_SIZE = 0.2f;
        protected const float PICK_SIZE = 0.25f;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            _ikManager = (QuickIKManager)target;

            //SceneView.duringSceneGui += UpdateDebug;
        }

        protected virtual void OnDisable()
        {
            //SceneView.duringSceneGui -= UpdateDebug;
        }

        #endregion

        #region UPDATE

        protected override void DrawGUI()
        {
            base.DrawGUI();

            EditorGUI.BeginChangeCheck();

            _ikManager._showControlsBody = FoldoutBolt(_ikManager._showControlsBody, "Body Controls");
            if (_ikManager._showControlsBody)
            {
                EditorGUI.indentLevel++;
                foreach (HumanBodyBones boneID in QuickIKManager.GetIKLimbBones())
                {
                    EditorGUILayout.BeginVertical("box");
                    DrawIKSolverProperties(boneID);
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }

            _ikManager._showControlsFingersLeftHand = FoldoutBolt(_ikManager._showControlsFingersLeftHand, "Left Hand Fingers Controlsa");
            if (_ikManager._showControlsFingersLeftHand)
            {
                EditorGUI.indentLevel++;
                foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                {
                    EditorGUILayout.BeginVertical("box");
                    DrawIKSolverProperties(f, true);
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }

            _ikManager._showControlsFingersRightHand = FoldoutBolt(_ikManager._showControlsFingersRightHand, "Right Hand Fingers Controls");
            if (_ikManager._showControlsFingersRightHand)
            {
                EditorGUI.indentLevel++;
                foreach (QuickHumanFingers f in QuickHumanTrait.GetHumanFingers())
                {
                    EditorGUILayout.BeginVertical("box");
                    DrawIKSolverProperties(f, false);
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                //serializedObject.ApplyModifiedProperties();
                QuickUtilsEditor.MarkSceneDirty();
            }

            //UpdateDebug();
        }

        protected virtual void DrawIKSolverProperties(HumanBodyBones boneID)
        {
            DrawIKSolverProperties(_ikManager.GetIKSolver(boneID), boneID.ToString());
        }

        protected virtual void DrawIKSolverProperties(QuickHumanFingers f, bool isLeft)
        {
            DrawIKSolverProperties(_ikManager.GetIKSolver(f, isLeft), f.ToString());
        }

        protected virtual void DrawIKSolverProperties(QuickIKSolver ikSolver, string name)
        {
            ikSolver._enableIK = EditorGUILayout.Toggle(name, ikSolver._enableIK);
            if (ikSolver._enableIK)
            {
                DrawIKSolverPropertiesBase(ikSolver);
            }
        }

        protected virtual void DrawIKSolverPropertiesBase(QuickIKSolver ikSolver)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = false;
            EditorGUILayout.ObjectField("IKTarget", ikSolver._targetLimb, typeof(Transform), true);
            GUI.enabled = true;
            if (DrawButton("Reset", GUILayout.Width(52)))
            {
                ikSolver.LoadCurrentPose();
                //_target.ResetIKTarget(boneID);
            }
            EditorGUILayout.EndHorizontal();

            ikSolver._weightIKPos = EditorGUILayout.Slider("IKPosWeight", ikSolver._weightIKPos, 0, 1);
            ikSolver._weightIKRot = EditorGUILayout.Slider("IKRotWeight", ikSolver._weightIKRot, 0, 1);
        }

        protected virtual void UpdateDebug()
        {
            if (!_ikManager.gameObject.activeInHierarchy) return;
            //DrawIKTargets(GetIKTargetsLimb(), Handles.CubeHandleCap);
            //DrawIKTargets(GetIKTargetsMid(), Handles.SphereHandleCap);

            foreach (QuickIKSolver s in _ikManager.GetIKSolversBody()) DrawIKSolver(s, false);
            foreach (QuickIKSolver s in _ikManager.GetIKSolversHand(true)) DrawIKSolver(s, true);
            foreach (QuickIKSolver s in _ikManager.GetIKSolversHand(false)) DrawIKSolver(s, true);
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

        protected virtual void DrawIKSolver(QuickIKSolver ikSolver, bool isSolverFinger)
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



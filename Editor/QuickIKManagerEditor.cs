using System.Collections;
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

            DrawIKControls();

            if (EditorGUI.EndChangeCheck())
            {
                //serializedObject.ApplyModifiedProperties();
                if (_ikManager.gameObject.activeInHierarchy)
                {
                    _ikManager.UpdateTracking();
                }
                QuickUtilsEditor.MarkSceneDirty();
            }

            //UpdateDebug();
        }

        protected virtual void DrawIKControls()
        {
            _ikManager._showControlsBody = FoldoutBolt(_ikManager._showControlsBody, "Body Controls");
            if (_ikManager._showControlsBody)
            {
                EditorGUI.indentLevel++;
                for (IKBone ikBone = IKBone.Hips; ikBone <= IKBone.RightFoot; ikBone++)
                {
                    EditorGUILayout.BeginVertical("box");
                    DrawIKSolverProperties(ikBone);
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }

            _ikManager._showControlsFingersLeftHand = FoldoutBolt(_ikManager._showControlsFingersLeftHand, "Left Hand Fingers Controls");
            if (_ikManager._showControlsFingersLeftHand)
            {
                EditorGUI.indentLevel++;
                for (IKBone ikBone = IKBone.LeftThumbDistal; ikBone <= IKBone.LeftLittleDistal; ikBone++)
                {
                    EditorGUILayout.BeginVertical("box");
                    DrawIKSolverProperties(ikBone);
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }

            _ikManager._showControlsFingersRightHand = FoldoutBolt(_ikManager._showControlsFingersRightHand, "Right Hand Fingers Controls");
            if (_ikManager._showControlsFingersRightHand)
            {
                EditorGUI.indentLevel++;
                for (IKBone ikBone = IKBone.RightThumbDistal; ikBone <= IKBone.RightLittleDistal; ikBone++)
                {
                    EditorGUILayout.BeginVertical("box");
                    DrawIKSolverProperties(ikBone);
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }

            _ikManager._showControlsFace = FoldoutBolt(_ikManager._showControlsFace, "Face Controls");
            if (_ikManager._showControlsFace)
            {
                EditorGUI.indentLevel++;
                for (IKBone ikBone = IKBone.LeftEye; ikBone <= IKBone.RightEye; ikBone++)
                {
                    EditorGUILayout.BeginVertical("box");
                    DrawIKSolverProperties(ikBone);
                    EditorGUILayout.EndVertical();
                }
                EditorGUI.indentLevel--;
            }

            DrawPoseButtons();
        }

        protected virtual void DrawIKSolverProperties(IKBone ikBone)
        {
            DrawIKSolverProperties(_ikManager.GetIKSolver(ikBone), ikBone.ToString());
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
                ikSolver.LoadPose();
                //_target.ResetIKTarget(boneID);
            }
            EditorGUILayout.EndHorizontal();

            ikSolver._weightIKPos = EditorGUILayout.Slider("IKPosWeight", ikSolver._weightIKPos, 0, 1);
            ikSolver._weightIKRot = EditorGUILayout.Slider("IKRotWeight", ikSolver._weightIKRot, 0, 1);

            if (ikSolver.GetType() == typeof(QuickIKSolverEye))
            {
                QuickIKSolverEye ikSolverEye = (QuickIKSolverEye)ikSolver;
                if (ikSolverEye._showAngleLimits = FoldoutBolt(ikSolverEye._showAngleLimits, "Angle Limits"))
                {
                    EditorGUI.indentLevel++;
                    ikSolverEye._angleLimitLeft = EditorGUILayout.FloatField("Left", ikSolverEye._angleLimitLeft);
                    ikSolverEye._angleLimitRight = EditorGUILayout.FloatField("Right", ikSolverEye._angleLimitRight);
                    ikSolverEye._angleLimitDown = EditorGUILayout.FloatField("Down", ikSolverEye._angleLimitDown);
                    ikSolverEye._angleLimitUp = EditorGUILayout.FloatField("Up", ikSolverEye._angleLimitUp);
                    EditorGUI.indentLevel--;
                }
                ikSolverEye._leftRight = EditorGUILayout.Slider("Left - Right", ikSolverEye._leftRight, ikSolverEye._angleLimitLeft, ikSolverEye._angleLimitRight);
                ikSolverEye._downUp = EditorGUILayout.Slider("Down - Up", ikSolverEye._downUp, ikSolverEye._angleLimitDown, ikSolverEye._angleLimitUp);

                if (ikSolverEye._showBlinking = FoldoutBolt(ikSolverEye._showBlinking, "Blinking"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space();
                    if (DrawButton("Add"))
                    {
                        ikSolverEye._blinking.Add(new QuickIKSolverEye.BlinkData());
                    }
                    if (DrawButton("Remove Last"))
                    {
                        if (ikSolverEye._blinking.Count > 0)
                        {
                            ikSolverEye._blinking.RemoveAt(ikSolverEye._blinking.Count - 1);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.Space();
                    for (int i = 0; i < ikSolverEye._blinking.Count; i++)
                    {
                        QuickIKSolverEye.BlinkData bData = ikSolverEye._blinking[i];

                        if (bData._showInInspector = FoldoutBolt(bData._showInInspector, "Element " + i.ToString()))
                        {
                            EditorGUI.indentLevel++;

                            bData._renderer = (SkinnedMeshRenderer)EditorGUILayout.ObjectField("Mesh", bData._renderer, typeof(SkinnedMeshRenderer), true);
                            if (bData._renderer && bData._renderer.sharedMesh)
                            {
                                Mesh mesh = bData._renderer.sharedMesh;
                                List<string> bNames = new List<string>();
                                bNames.Add("None");
                                for (int j = 0; j < mesh.blendShapeCount; j++)
                                {
                                    bNames.Add(mesh.GetBlendShapeName(j));
                                }

                                bData._blendshapeID = EditorGUILayout.Popup("Blendshape ID", bData._blendshapeID + 1, bNames.ToArray()) - 1;
                            }

                            EditorGUI.indentLevel--;
                        }
                    }

                    EditorGUILayout.Space();
                    
                    EditorGUI.indentLevel--;
                }

                ikSolverEye._weightBlink = EditorGUILayout.Slider("BlinkWeight", ikSolverEye._weightBlink, 0, 1);

            }
        }

        protected virtual void UpdateDebug()
        {
            if (!_ikManager.gameObject.activeInHierarchy) return;
            //DrawIKTargets(GetIKTargetsLimb(), Handles.CubeHandleCap);
            //DrawIKTargets(GetIKTargetsMid(), Handles.SphereHandleCap);

            for (IKBone ikBone = IKBone.Hips; ikBone <= IKBone.RightFoot; ikBone++)
            {
                DrawIKSolver(_ikManager.GetIKSolver(ikBone), false);
            }

            for (IKBone ikBone = IKBone.LeftThumbDistal; ikBone <= IKBone.LeftLittleDistal; ikBone++)
            {
                DrawIKSolver(_ikManager.GetIKSolver(ikBone), true);
            }

            for (IKBone ikBone = IKBone.RightThumbDistal; ikBone <= IKBone.RightLittleDistal; ikBone++)
            {
                DrawIKSolver(_ikManager.GetIKSolver(ikBone), true);
            }
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

        protected virtual void DrawPoseButtons()
        {
            EditorGUILayout.BeginVertical("box");

            if (DrawButton(new GUIContent("Save Pose", "Save the current pose")))
            {
                _ikManager.SavePose();
            }

            EditorGUILayout.BeginHorizontal();
            if (DrawButton(new GUIContent("Copy LH > RH", "Copy the Left Hand Pose to the Right Hand")))
            {
                _ikManager.CopyLeftHandPoseToRightHand();
            }
            if (DrawButton(new GUIContent("Copy RH > LH", "Copy the Right Hand Pose to the Left Hand")))
            {
                _ikManager.CopyRightHandPoseToLeftHand();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (DrawButton(new GUIContent("Copy LF > RF", "Copy the Left Foot Pose to the Right Foot")))
            {
                _ikManager.CopyLeftFootPoseToRightFoot();
            }
            if (DrawButton(new GUIContent("Copy RF > LF", "Copy the Right Foot Pose to the Left Foot")))
            {
                _ikManager.CopyRightFootPoseToLeftFoot();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");

            if (DrawButton(new GUIContent("Load Pose", "Load the last saved pose")))
            {
                _ikManager.LoadPose();
            }
            if (DrawButton(new GUIContent("Load T-Pose", "Load the T-Pose")))
            {
                _ikManager.LoadTPose();
            }
            if (DrawButton(new GUIContent("Load Anim Pose", "Load the pose determined by the first frame of the Animation")))
            {
                _ikManager.LoadAnimPose();
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

    }

}



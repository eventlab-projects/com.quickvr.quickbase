using UnityEditor;
using UnityEngine;

using System.Collections.Generic;

namespace QuickVR
{

    [InitializeOnLoad]
    static public class QuickIKTargetsRenderer
    {

        private class IKTargetData
        {
            public Transform _transform = null;
            public QuickIKSolver _ikSolver = null;
            public IKBone _ikBone = IKBone.LastBone;
        }

        private static List<QuickIKManagerExecuteInEditMode> _ikManagers = new List<QuickIKManagerExecuteInEditMode>();
        private static IKTargetData _selectedIKTarget = new IKTargetData();

        static QuickIKTargetsRenderer()
        {
            QuickIKManagerExecuteInEditMode.OnIKManagerAdded += OnQuickIKManagerAdded;
            QuickIKManagerExecuteInEditMode.OnIKManagerRemoved += OnQuickIKManagerRemoved;

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnQuickIKManagerAdded(QuickIKManagerExecuteInEditMode ikManager)
        {
            if (!_ikManagers.Contains(ikManager))
            {
                _ikManagers.Add(ikManager);
            }
        }

        private static void OnQuickIKManagerRemoved(QuickIKManagerExecuteInEditMode ikManager)
        {
            _ikManagers.Remove(ikManager);
        }

        private static void OnSceneGUI(SceneView sView)
        {
            if (Selection.activeTransform != _selectedIKTarget._transform)
            {
                _selectedIKTarget._transform = null;
            }

            DrawIKTargets();
        }

        private static void DrawIKTargets()
        {
            foreach (QuickIKManagerExecuteInEditMode ikManagerEditor in _ikManagers)
            {
                QuickIKManager ikManager = ikManagerEditor._ikManager;
                if (ikManager && ikManager.gameObject.activeInHierarchy)
                {
                    for (IKBone ikBone = 0; ikBone < IKBone.LastBone; ikBone++)
                    {
                        QuickIKSolver ikSolver = ikManager.GetIKSolver(ikBone);
                        float size;
                        if (ikBone >= IKBone.Hips && ikBone <= IKBone.RightFoot)
                        {
                            size = 0.05f;
                        }
                        else
                        {
                            size = 0.01f;
                        }

                        Handles.color = new Color(1, 0, 0, 0.5f);
                        if (Handles.Button(ikSolver._targetLimb.position, ikSolver._targetLimb.rotation, size, size, Handles.CubeHandleCap))
                        {
                            SelectIKTarget(ikSolver._targetLimb, ikSolver, ikBone);
                        }

                        if (ikSolver._targetHint)
                        {
                            Handles.color = new Color(0, 1, 0, 0.5f);
                            if (Handles.Button(ikSolver._targetHint.position, ikSolver._targetHint.rotation, size, size, Handles.SphereHandleCap))
                            {
                                SelectIKTarget(ikSolver._targetHint, ikSolver, ikBone);
                            }
                        }
                    }
                }
            }
        }

        private static void SelectIKTarget(Transform ikTarget, QuickIKSolver ikSolver, IKBone ikBone)
        {
            _selectedIKTarget._transform = ikTarget;
            _selectedIKTarget._ikSolver = ikSolver;
            _selectedIKTarget._ikBone = ikBone;

            Selection.activeTransform = ikTarget;
        }

    }

}


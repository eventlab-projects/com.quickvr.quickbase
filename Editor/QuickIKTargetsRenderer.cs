using UnityEditor;
using UnityEngine;

using System.Collections.Generic;

namespace QuickVR
{

    [InitializeOnLoad]
    static public class QuickIKTargetsRenderer
    {

        private static List<QuickIKManager> _ikManagers = new List<QuickIKManager>();
        private static Transform _selectedIKTarget = null;
        private static QuickIKSolver _selectedIKSolver = null;

        static QuickIKTargetsRenderer()
        {
            QuickIKManager.OnAdd += OnQuickIKManagerAdded;
            QuickIKManager.OnRemove += OnQuickIKManagerRemoved;

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnQuickIKManagerAdded(QuickIKManager ikManager)
        {
            if (!_ikManagers.Contains(ikManager))
            {
                _ikManagers.Add(ikManager);
            }
        }

        private static void OnQuickIKManagerRemoved(QuickIKManager ikManager)
        {
            _ikManagers.Remove(ikManager);
        }

        private static void OnSceneGUI(SceneView sView)
        {
            DrawIKTargets(ref _selectedIKTarget, ref _selectedIKSolver);

            if (_selectedIKTarget)
            {
                if (_selectedIKTarget == _selectedIKSolver._targetLimb)
                {
                    //The targer cannot be farther away than the chain length
                    Vector3 v = _selectedIKTarget.position - _selectedIKSolver._boneUpper.position;
                    _selectedIKTarget.position = _selectedIKSolver._boneUpper.position + v.normalized * Mathf.Min(v.magnitude, _selectedIKSolver.GetChainLength());
                }
            }
        }

        private static void DrawIKTargets(ref Transform selectedIKTarget, ref QuickIKSolver selectedIKSolver)
        {
            foreach (QuickIKManager ikManager in _ikManagers)
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
                        selectedIKTarget = Selection.activeTransform = ikSolver._targetLimb;
                        selectedIKSolver = ikSolver;
                    }

                    if (ikSolver._targetHint)
                    {
                        Handles.color = new Color(0, 1, 0, 0.5f);
                        if (Handles.Button(ikSolver._targetHint.position, ikSolver._targetHint.rotation, size, size, Handles.SphereHandleCap))
                        {
                            selectedIKTarget = Selection.activeTransform = ikSolver._targetHint;
                            selectedIKSolver = ikSolver;
                        }
                    }
                }
            }
        }
    }

}


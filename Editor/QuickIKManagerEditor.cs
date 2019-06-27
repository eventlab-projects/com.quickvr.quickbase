using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QuickVR
{

    [CustomEditor(typeof(QuickIKManager), true)]
    public class QuickIKManagerEditor : QuickBaseEditor
    {

        protected override void DrawGUI()
        {
            base.DrawGUI();
            
            QuickIKManager ikManager = (QuickIKManager)target;
            foreach (IKLimbBones boneLimbID in QuickIKManager.GetIKLimbBones())
            {
                EditorGUILayout.Space();
                IQuickIKSolver ikSolver = ikManager.GetIKSolver(boneLimbID);
                ikSolver._weightIKPos = EditorGUILayout.Slider(boneLimbID.ToString() + " IK Pos Weight", ikSolver._weightIKPos, 0.0f, 1.0f);
                ikSolver._weightIKRot = EditorGUILayout.Slider(boneLimbID.ToString() + " IK Rot Weight", ikSolver._weightIKRot, 0.0f, 1.0f);
            }
            
        }

    }

}



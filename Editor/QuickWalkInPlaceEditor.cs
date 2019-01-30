using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QuickVR
{
    [System.Serializable]
    [CustomEditor(typeof(QuickWalkInPlace_v2), true)]
    [CanEditMultipleObjects]
    public class QuickWalkInPlaceEditor : QuickBaseEditor
    {
        protected override void DrawGUI()
        {
            DrawPropertyField("m_Script", "Script");

            DrawPropertyField("_move", "Move");
            DrawPropertyField("_hasPriority", "Has Priority");
            DrawPropertyField("_radius", "Radius");
            DrawPropertyField("_height", "Height");
            DrawPropertyField("_speedMin", "Speed Min");
            DrawPropertyField("_speedMax", "Speed Max");
            DrawPropertyField("_linearAcceleration", "Linear Acceleration");
            DrawPropertyField("_maxStepHeight", "Max Step Height");

            DrawPropertyField("_desiredSpeed", "Desired Speed");
        }
    }
}


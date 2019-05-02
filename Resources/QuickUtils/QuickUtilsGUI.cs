﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{
    public static class QuickUtilsGUI
    {

        public static bool Button(string text, bool newLine)
        {
            bool pressed = GUILayout.Button(text);
            return pressed;
        }

        public static void DrawSelectionGrid(string label, ref int index, string[] options)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(label);
            index = GUILayout.SelectionGrid(index, options, 1, "toggle");
            GUILayout.EndVertical();
        }

        public static bool DrawToggle(string label, bool value)
        {
            GUILayout.BeginVertical();
            bool result = GUILayout.Toggle(value, label);
            GUILayout.EndVertical();

            return result;
        }

        public static bool DrawToggleRight(string label, bool value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label);
            bool result = GUILayout.Toggle(value, "");
            GUILayout.EndHorizontal();

            return result;
        }

        public static string DrawTextArea(string label, string value)
        {
            GUILayout.Label(label);
            value = GUILayout.TextArea(value, 200);

            return value;
        }

        public static void DrawLabel(string label)
        {
            GUILayout.Label(label);
        }
    }
}



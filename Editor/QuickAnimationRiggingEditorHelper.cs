using System;

using UnityEngine;
using UnityEditor;

static class QuickAnimationRiggingEditorHelper
{
    private const string EditorFolder = "Packages/com.unity.animation.rigging/Editor/";
    private const string ShadersFolder = EditorFolder + "Shaders/";
    private const string ShapesFolder = EditorFolder + "Shapes/";

    public static Shader LoadShader(string filename)
    {
        return AssetDatabase.LoadAssetAtPath<Shader>(ShadersFolder + filename);
    }

    public static Mesh LoadShape(string filename)
    {
        return AssetDatabase.LoadAssetAtPath<Mesh>(ShapesFolder + filename);
    }

    public static T GetClosestComponent<T>(Transform transform, Transform root = null)
    {
        if (transform == null)
            return default(T);

        var top = (root != null) ? root : transform.root;

        while (true)
        {
            if (transform.GetComponent<T>() != null) return transform.GetComponent<T>();
            if (transform == top) break;
            transform = transform.parent;
        }

        return default(T);
    }

    public static void HandleClickSelection(GameObject gameObject, Event evt)
    {
        if (evt.shift || EditorGUI.actionKey)
        {
            UnityEngine.Object[] existingSelection = Selection.objects;

            // For shift, we check if EXACTLY the active GO is hovered by mouse and then subtract. Otherwise additive.
            // For control/cmd, we check if ANY of the selected GO is hovered by mouse and then subtract. Otherwise additive.
            // Control/cmd takes priority over shift.
            bool subtractFromSelection = EditorGUI.actionKey ? Selection.Contains(gameObject) : Selection.activeGameObject == gameObject;
            if (subtractFromSelection)
            {
                // subtract from selection
                var newSelection = new UnityEngine.Object[existingSelection.Length - 1];

                int index = Array.IndexOf(existingSelection, gameObject);

                System.Array.Copy(existingSelection, newSelection, index);
                System.Array.Copy(existingSelection, index + 1, newSelection, index, newSelection.Length - index);

                Selection.objects = newSelection;
            }
            else
            {
                // add to selection
                var newSelection = new UnityEngine.Object[existingSelection.Length + 1];
                System.Array.Copy(existingSelection, newSelection, existingSelection.Length);
                newSelection[existingSelection.Length] = gameObject;

                Selection.objects = newSelection;
            }
        }
        else
            Selection.activeObject = gameObject;
    }
}

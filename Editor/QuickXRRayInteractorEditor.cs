using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor.XR.Interaction.Toolkit;

using UnityEditor;


namespace QuickVR
{
    
    [CustomEditor(typeof(QuickXRRayInteractor), true)]
    public class QuickXRRayInteractorEditor : XRRayInteractorEditor
    {

        #region PROTECTED ATTRIBUTES

        protected SerializedProperty _interactionType;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void OnEnable()
        {
            base.OnEnable();

            _interactionType = serializedObject.FindProperty("_interactionType");
        }

        #endregion

        #region UPDATE

        protected override void DrawProperties()
        {
            EditorGUILayout.PropertyField(_interactionType);

            base.DrawProperties();
        }

        #endregion

    }

}


using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
//
namespace QuickVR
{

    [System.Serializable]
    [CustomEditor(typeof(QuickBaseVibratorManager), true)]
    [CanEditMultipleObjects]
    public class QuickBaseVibratorManagerEditor : QuickBaseEditor
    {

        #region PROTECTED PARAMETERS

        protected QuickVibratorManager _vibratorManager = null;
        protected QuickBaseVibratorManager _baseVibratorManager = null;

        protected string[] _vibratorCodes;
        
        [SerializeField]
        protected bool _showMapping = true;
        
        #endregion

        #region CONSTANTS

        protected const string WARNING_TITLE = "Reset All Mapping";
        protected const string WARNING_MESSAGE = "Are you sure you want to reset all mapped vibrators of ";

        #endregion

        #region GUI DRAW

        protected override void DrawGUI()
        {
            _vibratorManager = QuickSingletonManager.GetInstance<QuickVibratorManager>();
            _baseVibratorManager = target as QuickBaseVibratorManager;

            _vibratorCodes = _baseVibratorManager.GetVibratorCodes();

            DrawVibratorsMapping();
            EditorGUILayout.Separator();
            if (DrawButton(WARNING_TITLE))
            {
                if (EditorUtility.DisplayDialog(WARNING_TITLE + "?", WARNING_MESSAGE + target.name + "?", "Confirm", "Cancel"))
                {
                    _baseVibratorManager.ResetAllMapping();
                }
            }

            base.DrawGUI();
        }

        protected virtual void DrawVibratorsMapping()
        {
            if (_baseVibratorManager.GetNumVibratorsMapped() != _vibratorManager.GetNumVibrators()) _baseVibratorManager.ResetAllMapping();

            _showMapping = EditorGUILayout.Foldout(_showMapping, "Vibrators Mapping");
            if (!_showMapping) return;

            int numVibrators = _vibratorManager.GetNumVibrators();
            for (int i = 0; i < numVibrators; i++)
            {
                DrawVibratorMapping(i);
            }
        }

        protected virtual void DrawVibratorMapping(int vibratorID)
        {
            SerializedProperty p = serializedObject.FindProperty("_vibratorMapping");
            EditorGUI.indentLevel++;

            string mapping = _baseVibratorManager.GetVibratorMapping(vibratorID);
            int index = EditorGUILayout.Popup(_vibratorManager.GetVirtualVibrator(vibratorID), GetSelectedIndex(_vibratorCodes, mapping), _vibratorCodes);
            p.GetArrayElementAtIndex(vibratorID).stringValue = _vibratorCodes[index];

            serializedObject.ApplyModifiedProperties();
            
            EditorGUI.indentLevel--;
        }

        #endregion

        #region AUX FUNCTIONS

        protected virtual int GetSelectedIndex(string[] list, string value)
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] == value) return i;
            }
            return 0;
        }

        #endregion
    }

}

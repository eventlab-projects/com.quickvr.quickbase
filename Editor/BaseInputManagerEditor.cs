using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace QuickVR
{

    [System.Serializable]
    [CustomEditor(typeof(BaseInputManager), true)]
    [CanEditMultipleObjects]
    public class BaseInputManagerEditor : QuickBaseEditor
    {

        #region PROTECTED PARAMETERS

        protected InputManager _inputManager = null;
        protected BaseInputManager _baseInputManager = null;

        protected string[] _axisCodes;
        protected string[] _buttonCodes;

        [SerializeField] protected bool _showAxes = true;
        [SerializeField] protected bool _showButtons = true;

        #endregion

        #region CONSTANTS

        protected const string AXIS = "Axis";
        protected const string POSITIVE_KEY = "Positive Key";
        protected const string NEGATIVE_KEY = "Negative Key";
        protected const string ALT_POSITIVE_KEY = "Alt. Positive Key";
        protected const string ALT_NEGATIVE_KEY = "Alt. Negative Key";
        protected const string KEY = "Key";
        protected const string ALT_KEY = "Alt. Key";

        protected const string WARNING_TITLE = "Reset All Mapping";
        protected const string WARNING_MESSAGE = "Are you sure you want to reset all mapped keys of ";

        #endregion

        #region GUI DRAW

        protected override void DrawGUI()
        {
            base.DrawGUI();

            _inputManager = QuickSingletonManager.GetInstance<InputManager>();
            _baseInputManager = target as BaseInputManager;

            _axisCodes = _baseInputManager.GetAxisCodes();
            _buttonCodes = _baseInputManager.GetButtonCodes();

            EditorGUI.BeginChangeCheck();
            DrawAxesMapping();
            EditorGUILayout.Separator();
            DrawButtonsMapping();
            if (EditorGUI.EndChangeCheck())
            {
                QuickUtilsEditor.MarkSceneDirty();
            }
        }

        protected virtual void DrawAxesMapping()
        {
            _baseInputManager.CheckAxesMapping();

            _showAxes = EditorGUILayout.Foldout(_showAxes, "Axes Mapping");
            if (!_showAxes) return;

            int numAxes = _inputManager.GetNumAxes();
            for (int i = 0; i < numAxes; i++)
            {
                DrawAxisMapping(i);
            }
        }

        protected virtual void DrawButtonsMapping()
        {
            _baseInputManager.CheckButtonsMapping();

            _showButtons = EditorGUILayout.Foldout(_showButtons, "Buttons Mapping");
            if (!_showButtons) return;

            int numButtons = _inputManager.GetNumButtons();
            for (int i = 0; i < numButtons; i++)
            {
                DrawButtonMapping(i);
            }
        }

        protected virtual void DrawAxisMapping(int axisID)
        {
            AxisMapping mapping = _baseInputManager.GetAxisMapping(axisID);
            EditorGUI.indentLevel++;

            bool showInInspector = EditorGUILayout.Foldout(mapping._showInInspector, _inputManager.GetVirtualAxis(axisID));
            if (showInInspector)
            {

                EditorGUI.indentLevel++;

                int index = EditorGUILayout.Popup(AXIS, GetSelectedIndex(_axisCodes, mapping._axisCode), _axisCodes);
                mapping._axisCode = _axisCodes[index];

                ButtonMapping positiveButton = mapping._positiveButton;
                index = EditorGUILayout.Popup(POSITIVE_KEY, GetSelectedIndex(_buttonCodes, positiveButton._keyCode), _buttonCodes);
                positiveButton._keyCode = _buttonCodes[index];

                index = EditorGUILayout.Popup(ALT_POSITIVE_KEY, GetSelectedIndex(_buttonCodes, positiveButton._altKeyCode), _buttonCodes);
                positiveButton._altKeyCode = _buttonCodes[index];

                ButtonMapping negativeButton = mapping._negativeButton;
                index = EditorGUILayout.Popup(NEGATIVE_KEY, GetSelectedIndex(_buttonCodes, negativeButton._keyCode), _buttonCodes);
                negativeButton._keyCode = _buttonCodes[index];
                index = EditorGUILayout.Popup(ALT_NEGATIVE_KEY, GetSelectedIndex(_buttonCodes, negativeButton._altKeyCode), _buttonCodes);
                negativeButton._altKeyCode = _buttonCodes[index];

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;

            mapping._showInInspector = showInInspector;
            //mapping.ApplyModifiedProperties();
        }

        protected virtual void DrawButtonMapping(int buttonID)
        {
            ButtonMapping mapping = _baseInputManager.GetButtonMapping(buttonID);
            EditorGUI.indentLevel++;

            bool showInInspector = EditorGUILayout.Foldout(mapping._showInInspector, _inputManager.GetVirtualButton(buttonID));
            if (showInInspector)
            {

                EditorGUI.indentLevel++;

                int index = EditorGUILayout.Popup(KEY, GetSelectedIndex(_buttonCodes, mapping._keyCode), _buttonCodes);
                mapping._keyCode = _buttonCodes[index];

                index = EditorGUILayout.Popup(ALT_KEY, GetSelectedIndex(_buttonCodes, mapping._altKeyCode), _buttonCodes);
                mapping._altKeyCode = _buttonCodes[index];

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;

            mapping._showInInspector = showInInspector;
            //mapping.ApplyModifiedProperties();
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

        protected virtual string GetButtonCode(int key)
        {
            if (key < _buttonCodes.Length) return _buttonCodes[key];
            return _buttonCodes[0];
        }

        #endregion
    }

}
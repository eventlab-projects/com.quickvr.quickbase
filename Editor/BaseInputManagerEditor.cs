using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace QuickVR {

	[System.Serializable]
	[CustomEditor(typeof(BaseInputManager), true)]	
	[CanEditMultipleObjects]
	public class BaseInputManagerEditor : QuickBaseEditor {

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

		protected const string WARNING_TITLE = 	"Reset All Mapping";
		protected const string WARNING_MESSAGE = "Are you sure you want to reset all mapped keys of ";

		#endregion

		#region GUI DRAW

		protected override void DrawGUI() {
			_inputManager = QuickSingletonManager.GetInstance<InputManager>();
			_baseInputManager = target as BaseInputManager;

			_axisCodes = _baseInputManager.GetAxisCodes();
            _buttonCodes = _baseInputManager.GetButtonCodes();

			DrawAxesMapping();
			EditorGUILayout.Separator();
			DrawButtonsMapping();

			EditorGUILayout.Separator();
			if (DrawButton(WARNING_TITLE)) {
				if (EditorUtility.DisplayDialog(WARNING_TITLE + "?", WARNING_MESSAGE + target.name + "?", "Confirm", "Cancel")) {
					BaseInputManager bManager = target as BaseInputManager;
					bManager.ResetAllMapping();
				}
			}

            base.DrawGUI();
		}

		protected virtual void DrawAxesMapping() {
			if (_baseInputManager.GetNumAxesMapped() != _inputManager.GetNumAxes()) _baseInputManager.ResetAxesMapping();

			_showAxes = EditorGUILayout.Foldout(_showAxes, "Axes Mapping");
			if (!_showAxes) return;
			
			int numAxes = _inputManager.GetNumAxes();
			for (int i = 0; i < numAxes; i++) {
				DrawAxisMapping(i);
			}
		}
		
		protected virtual void DrawButtonsMapping() {
			if (_baseInputManager.GetNumButtonsMapped() != _inputManager.GetNumButtons()) _baseInputManager.ResetButtonMapping();

			_showButtons = EditorGUILayout.Foldout(_showButtons, "Buttons Mapping");
			if (!_showButtons) return;
			
			int numButtons = _inputManager.GetNumButtons();
			for (int i = 0; i < numButtons; i++) {
				DrawButtonMapping(i);
			}
		}

		protected virtual void DrawAxisMapping(int axisID) {
			SerializedObject mapping = new SerializedObject(_baseInputManager.GetAxisMapping(axisID));
			mapping.Update();

            EditorGUI.indentLevel++;
			
            bool showInInspector = EditorGUILayout.Foldout(mapping.FindProperty("_showInInspector").boolValue, _inputManager.GetVirtualAxis(axisID));
			if (showInInspector) {

                EditorGUI.indentLevel++;
				
                int index = EditorGUILayout.Popup(AXIS, GetSelectedIndex(_axisCodes, mapping.FindProperty("_axisCode").stringValue), _axisCodes);
				mapping.FindProperty("_axisCode").stringValue = _axisCodes[index];

				SerializedObject positiveButton = new SerializedObject(((AxisMapping)mapping.targetObject).GetPositiveButton());
				positiveButton.Update();
				index = EditorGUILayout.Popup(POSITIVE_KEY, GetSelectedIndex(_buttonCodes, positiveButton.FindProperty("_keyCode").stringValue), _buttonCodes);
                positiveButton.FindProperty("_keyCode").stringValue = _buttonCodes[index];
				index = EditorGUILayout.Popup(ALT_POSITIVE_KEY, GetSelectedIndex(_buttonCodes, positiveButton.FindProperty("_altKeyCode").stringValue), _buttonCodes);
                positiveButton.FindProperty("_altKeyCode").stringValue = _buttonCodes[index];
				positiveButton.ApplyModifiedProperties();

                SerializedObject negativeButton = new SerializedObject(((AxisMapping)mapping.targetObject).GetNegativeButton());
                negativeButton.Update();
                index = EditorGUILayout.Popup(NEGATIVE_KEY, GetSelectedIndex(_buttonCodes, negativeButton.FindProperty("_keyCode").stringValue), _buttonCodes);
                negativeButton.FindProperty("_keyCode").stringValue = _buttonCodes[index];
                index = EditorGUILayout.Popup(ALT_NEGATIVE_KEY, GetSelectedIndex(_buttonCodes, negativeButton.FindProperty("_altKeyCode").stringValue), _buttonCodes);
                negativeButton.FindProperty("_altKeyCode").stringValue = _buttonCodes[index];
                negativeButton.ApplyModifiedProperties();
                
                EditorGUI.indentLevel--;
			}

            EditorGUI.indentLevel--;

            mapping.FindProperty("_showInInspector").boolValue = showInInspector;
			mapping.ApplyModifiedProperties();
		}

		protected virtual void DrawButtonMapping(int buttonID) {
			SerializedObject mapping = new SerializedObject(_baseInputManager.GetButtonMapping(buttonID));
			mapping.Update();

            EditorGUI.indentLevel++;

			bool showInInspector = EditorGUILayout.Foldout(mapping.FindProperty("_showInInspector").boolValue, _inputManager.GetVirtualButton(buttonID));
			if (showInInspector) {

                EditorGUI.indentLevel++;

				int index = EditorGUILayout.Popup(KEY, GetSelectedIndex(_buttonCodes, mapping.FindProperty("_keyCode").stringValue), _buttonCodes);
                mapping.FindProperty("_keyCode").stringValue = _buttonCodes[index];

				index = EditorGUILayout.Popup(ALT_KEY, GetSelectedIndex(_buttonCodes, mapping.FindProperty("_altKeyCode").stringValue), _buttonCodes);
                mapping.FindProperty("_altKeyCode").stringValue = _buttonCodes[index];

                EditorGUI.indentLevel--;
			}

            EditorGUI.indentLevel--;

            mapping.FindProperty("_showInInspector").boolValue = showInInspector;
			mapping.ApplyModifiedProperties();
		}

		#endregion

		#region AUX FUNCTIONS

		protected virtual int GetSelectedIndex(string[] list, string value) {
			for (int i = 0; i < list.Length; i++) {
				if (list[i] == value) return i;
			}
			return 0;
		}

		protected virtual string GetButtonCode(int key) {
			if (key < _buttonCodes.Length) return _buttonCodes[key];
			return _buttonCodes[0];
		}

		#endregion
	}

}
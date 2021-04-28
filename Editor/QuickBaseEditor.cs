using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace QuickVR {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour),true)]
	public class QuickBaseEditor : Editor {

		#region CONSTANTS
        
		public static Color DEFAULT_BUTTON_COLOR = new Color(0.0f, 162.0f / 255.0f, 232.0f / 255.0f);

		#endregion

        #region GET AND SET

        public static string GetScriptPath(System.Type t)
        {
            return GetScriptPath(t.Name);
        }

        public static string GetScriptPath(string tName)
        {
            //Returns the path of the script file defining Type t, if any
            string path = "";
            string[] guids = AssetDatabase.FindAssets("t:script " + tName);
            foreach (string id in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(id);
                MonoScript script = AssetDatabase.LoadAssetAtPath(p, typeof(MonoScript)) as MonoScript;
                if ((script != null) && (script.GetClass().Name == tName))
                {
                    path = p;
                    break;
                }
            }

            return path;
        }

        #endregion

        #region UPDATE

        public override void OnInspectorGUI()
        {
            DrawGUI();
            DrawMethodButtons();

            if (serializedObject.targetObject != null) serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawGUI()
        {
            DrawDefaultInspector();
        }

        protected virtual void DrawMethodButtons()
        {
            MethodInfo[] methods = target.GetType().GetMethods();
            if (methods.Length > 0)
            {
                EditorGUILayout.Space();
                foreach (MethodInfo m in methods)
                {
                    ButtonMethodAttribute attribute = QuickUtils.GetCustomAttribute<ButtonMethodAttribute>(m);
                    if ((attribute != null) && DrawButton(m.Name))
                    {
                        QuickUtils.Invoke(target, m.Name);
                        OnButtonMethod(m.Name);
                    }
                }
            }
        }

        protected virtual void OnButtonMethod(string methodName)
        {

        }

        #endregion

		#region HELPER FUNCTIONS

        public static void DrawHorizontalLine()
        {
            EditorGUILayout.TextArea("", GUI.skin.horizontalSlider);
        }

        public static T EnumPopup<T>(string label, T value) where T : struct
        {
            string[] values = QuickUtils.GetEnumValuesToString<T>().ToArray();
            int selectedIndex = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == value.ToString()) {
                    selectedIndex = i;
                    break;
                }
            }

            return QuickUtils.ParseEnum<T>(values[EditorGUILayout.Popup(label, selectedIndex, values)]);
        }

        public virtual bool DrawPropertyField(string pName, string label) {
			return DrawPropertyField(serializedObject, pName, label);
		}

        public virtual bool DrawPropertyField(SerializedObject sObject, string pName, string label) 
        {
            SerializedProperty sProperty = sObject.FindProperty(pName);
            if (sProperty == null) Debug.Log("PROPERTY NOT FOUND!!! = " + pName);

            return DrawPropertyField(sProperty, label);
		}

        public virtual bool DrawPropertyField(SerializedProperty sProperty, string label)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(sProperty, new GUIContent(label), true);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                return true;
            }

            return false;
        }

        public static bool DrawButton(string label, params GUILayoutOption[] options)
        {
            Color originalColor = GUI.backgroundColor;
			GUI.backgroundColor = DEFAULT_BUTTON_COLOR;

            bool result = GUILayout.Button(label, options);

            GUI.backgroundColor = originalColor;

            return result;
        }

        public static bool DrawButton(GUIContent content, params GUILayoutOption[] options)
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = DEFAULT_BUTTON_COLOR;

            bool result = GUILayout.Button(content, options);

            GUI.backgroundColor = originalColor;

            return result;
        }

        public static bool FoldoutBolt(bool expand, string label)
        {
            GUIStyle style = EditorStyles.foldout;
            FontStyle previousStyle = style.fontStyle;
            style.fontStyle = FontStyle.Bold;
            bool result = EditorGUILayout.Foldout(expand, label);
            style.fontStyle = previousStyle;

            return result;
        }

		#endregion
	}

    [CanEditMultipleObjects]
    [CustomEditor(typeof(ScriptableObject), true)]
    public class QuickScriptableObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            QuickBaseEditor.DrawHorizontalLine();

            ScriptableObject obj = (ScriptableObject)target;
            if (QuickBaseEditor.DrawButton("Save To XML"))
            {
                obj.SaveToXml(EditorUtility.SaveFilePanel("Save Asset to XML", "", target.name + ".xml", "xml"));
            }
            if (QuickBaseEditor.DrawButton("Load From XML"))
            {
                obj.LoadFromXml(EditorUtility.OpenFilePanel("Load Asset from XML", "", "xml"));
            }
        }
    }

}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;

namespace QuickVR {

    [CanEditMultipleObjects]
    [CustomEditor(typeof(MonoBehaviour),true)]
	public class QuickBaseEditor : UnityEditor.Editor {

		#region CONSTANTS

		public static Color DEFAULT_BUTTON_COLOR = new Color(0.0f, 162.0f / 255.0f, 232.0f / 255.0f);

		#endregion

        #region CREATION AND DESTRUCTION

        protected virtual void CheckDefaultConfigurationFolder(ConfigurableAttribute attribute) {
			string sFolderName = "Assets/QuickVRCfg";
            if (attribute._configurationFolder.Length > 0)
            {
                sFolderName += "/" + attribute._configurationFolder;
            }
            if (!AssetDatabase.IsValidFolder(sFolderName))
            {
                string[] folders = sFolderName.Split('/');
                for (int i = 1; i < folders.Length; i++)
                {
                    string parentFolder = "";
                    for (int j = 0; j < i; j++)
                    {
                        if (parentFolder.Length > 0) parentFolder += "/";
                        parentFolder += folders[j];
                    }
                    AssetDatabase.CreateFolder(parentFolder, folders[i]);
                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();
                }
			}
		}

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

        public static void CreateConfigurationScript(System.Type t)
        {
            System.Type tAsset = GetAssetType(t);
            string tAssetName = t.Name + "Asset";

            string copyPath = tAsset == null ? GetScriptPath(t).Replace(t.Name + ".", tAssetName + ".") : GetScriptPath(tAsset); 

            Debug.Log("Creating Classfile: " + copyPath);
            IndentedTextWriter outFile = new IndentedTextWriter(new StreamWriter(copyPath));
            outFile.WriteLine("using UnityEngine;");
            outFile.WriteLine("using QuickVR;");
            outFile.WriteLine("");
            outFile.WriteLine("public class " + tAssetName + " : ScriptableObject");
            outFile.WriteLine("{");
            outFile.WriteLine(" ");

            //Draw all the serializable fields
            outFile.Indent++;
            List<FieldInfo> fields = QuickUtils.GetAllFieldsConfigurable(t);
            foreach (FieldInfo f in fields)
            {
                //Draw the attributes of the field. 
                BitMaskAttribute bmaskAtt = QuickUtils.GetCustomAttribute<BitMaskAttribute>(f);
                if (bmaskAtt != null)
                {
                    outFile.WriteLine("[BitMask(" + "typeof("+QuickUtils.GetTypeFullName(bmaskAtt._type)+")" + ", " + bmaskAtt._startID + ", " + bmaskAtt._endID + ")]");
                }

                string fTypeName = QuickUtils.GetTypeFullName(f.FieldType);
                //Debug.Log("fTypeName = " + fTypeName);
                outFile.WriteLine("public " + fTypeName + " " + f.Name + ";");
            }
            outFile.Indent--;
            
            outFile.WriteLine(" ");
            outFile.WriteLine("}");
            outFile.Close();

            AssetDatabase.ImportAsset(copyPath);
            AssetDatabase.Refresh();
        }

		#endregion

		#region GET AND SET

        public static System.Type GetAssetType(System.Type t)
        {
            return t.Assembly.GetType(t.Name + "Asset");
        }

        protected virtual void SaveConfiguration()
        {
            string title = "Save a " + target.GetType().Name + " configuration asset";
            string defaultName = target.GetType().Name + "Configuration";
            string path = QuickUtils.GetRelativeAssetsPath(EditorUtility.SaveFilePanel(title, "Assets" + GetConfigurationFolderName(), defaultName, "asset"));
            if (path != "")
            {
                ScriptableObject cfg = ScriptableObject.CreateInstance(target.GetType().Name + "Asset");
                QuickUtils.CopyFields(target, cfg);
                AssetDatabase.CreateAsset(cfg, path);
            }
        }

        protected virtual void LoadConfiguration()
        {
            string title = "Load a " + target.GetType().Name + " configuration asset";
            string path = QuickUtils.GetRelativeAssetsPath(EditorUtility.OpenFilePanel(title, "Assets" + GetConfigurationFolderName(), "asset"));
            if (path != "")
            {
                System.Type t = GetAssetType(target.GetType());
                if (t != null)
                {
                    QuickUtils.CopyFields(AssetDatabase.LoadAssetAtPath(path, t), target);
                    serializedObject.Update();
                    MarkSceneDirty();
                }
            }
        }

		public virtual string GetConfigurationFolderName() {
			return "/QuickVRCfg";
		}

		protected virtual void MarkSceneDirty() {
			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		}

		#endregion

        #region UPDATE

        public override void OnInspectorGUI()
        {
            DrawGUI();
            DrawMethodButtons();
            DrawCfgButtons();

            if (serializedObject.targetObject != null) serializedObject.ApplyModifiedProperties();

            //if (DrawButton("TEST_FIELDS"))
            //{
            //    List<FieldInfo> fields = QuickUtils.GetAllFieldsConfigurable(target.GetType());
            //    foreach (FieldInfo f in fields)
            //    {
            //        Debug.Log("=========================");
            //        Debug.Log(f.Name);
            //        Debug.Log(QuickUtils.GetTypeFullName(f.FieldType));
            //    }
            //}
        }

        protected virtual void DrawGUI()
        {
            SerializedProperty p = serializedObject.GetIterator();
            bool showChildren = p.hasVisibleChildren;
            while (p.NextVisible(showChildren))
            {
                showChildren = EditorGUILayout.PropertyField(p, true);
            }
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
                    if ((attribute != null) && DrawButton(m.Name)) QuickUtils.Invoke(target, m.Name);
                }
            }
        }

        protected virtual void DrawCfgButtons()
        {
            //Check if the class has been marked as [Configurable], and therefore, we can save/load its state.
            ConfigurableAttribute attribute = QuickUtils.GetCustomAttribute<ConfigurableAttribute>(target.GetType());
            if (attribute != null)
            {
                CheckDefaultConfigurationFolder(attribute);
                if (!EditorApplication.isCompiling && GetAssetType(target.GetType()) == null)
                {
                    CreateConfigurationScript(target.GetType());
                }

                EditorGUILayout.Space();
                GUI.enabled = !EditorApplication.isCompiling;
                if (DrawButton("Save Configuration")) SaveConfiguration();
                if (DrawButton("Load Configuration")) LoadConfiguration();
                GUI.enabled = true;
            }
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

    class QuickAssetPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {
                MonoScript s = AssetDatabase.LoadAssetAtPath(str, typeof(MonoScript)) as MonoScript;
                if (s == null) continue;

                System.Type t = s.GetClass();
                if (t != null && QuickUtils.GetCustomAttribute<ConfigurableAttribute>(t) != null)
                {
                    QuickBaseEditor.CreateConfigurationScript(t);
                }
            }
        }
    }

}
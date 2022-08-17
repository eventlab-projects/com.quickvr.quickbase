using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.CodeDom.Compiler;

using UnityEngine;
using UnityEditor;

namespace QuickVR
{

    [System.Serializable]
    public class QuickPlayerPrefsWindowEditor : EditorWindow
    {
        #region PROTECTED PARAMETERS

        [SerializeField]
        protected bool _showNewSetting = true;

        [SerializeField]
        protected bool _showBaseSettings = true;

        [SerializeField]
        protected bool _showCustomSettings = true;

        protected string _newSettingKey = "";
        protected PrefType _newSettingType = PrefType.String;
        protected string _newSettingTypeEnum = "";

        [SerializeField]
        protected string _customSettingsScriptName = "SettingsCustom";

        protected Vector2 _scrollPos;

        protected enum PrefType
        {
            String,
            Int,
            Float,
            Bool,
            Enum,
        }

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual bool CreateNewSetting(out object value)
        {
            //Create a new playerpref with the specified key and type
            if (_newSettingType == PrefType.String) value = "";
            else if (_newSettingType == PrefType.Int) value = 0;
            else if (_newSettingType == PrefType.Float) value = 0.0f;
            else if (_newSettingType == PrefType.Bool) value = false;
            else
            {
                //Check if the enum type specified by the user exists in any of the current assemblies. 
                Type t = null;
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly a in assemblies)
                {
                    t = a.GetType(_newSettingTypeEnum);
                    if (t != null) break;
                }

                if (t == null)
                {
                    EditorUtility.DisplayDialog("New Setting Creation Error", "The Type " + _newSettingTypeEnum + " does not define an Enum Type. Please, specify a valid Enum Type using its FullName", "Ok");
                    value = null;
                    return false;
                }
                else
                {
                    value = Activator.CreateInstance(t);
                }
            }

            return true;
        }

        public virtual void CreateSettingsScript(string scriptName, List<string> keys)
        {
            string copyPath = QuickBaseEditor.GetScriptPath(scriptName);
            if (copyPath.Length == 0) copyPath = "Assets/" + scriptName + ".cs";

            Debug.Log("Creating Custom Settings Script: " + copyPath);
            IndentedTextWriter outFile = new IndentedTextWriter(new StreamWriter(copyPath));
            outFile.WriteLine("using UnityEngine;");
            outFile.WriteLine("using QuickVR;");
            outFile.WriteLine("");
            outFile.WriteLine("public static class " + scriptName);
            outFile.WriteLine("{");
            outFile.WriteLine();

            WriteNestedTypes(scriptName, outFile);
            outFile.WriteLine();
            WriteGettersAndSetters(keys, outFile);

            outFile.WriteLine();
            outFile.WriteLine("}");
            outFile.Close();

            AssetDatabase.ImportAsset(copyPath);
            AssetDatabase.Refresh();
        }

        protected virtual void WriteNestedTypes(string scriptName, IndentedTextWriter outFile)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type type = null;
            foreach (Assembly a in assemblies)
            {
                type = a.GetType(scriptName);
                if (type != null) break;
            }
            if (type == null) return;

            Type[] nTypes = type.GetNestedTypes();
            if (nTypes.Length == 0) return;

            outFile.Indent++;
            outFile.WriteLine("#region PUBLIC PARAMETERS");
            outFile.WriteLine();

            foreach (Type t in nTypes)
            {
                if (!t.IsEnum) continue;

                outFile.WriteLine("public enum " + t.Name);
                outFile.WriteLine("{");
                outFile.Indent++;

                string[] enumNames = System.Enum.GetNames(t);
                int[] values = System.Enum.GetValues(t) as int[];
                for (int i = 0; i < enumNames.Length; i++)
                {
                    outFile.WriteLine(enumNames[i] + " = " + values[i].ToString() + ",");
                }

                outFile.Indent--;
                outFile.WriteLine("};");
                outFile.WriteLine();
            }

            outFile.WriteLine("#endregion");
            outFile.Indent--;

        }

        protected virtual void WriteGettersAndSetters(List<string> keys, IndentedTextWriter outFile)
        {
            outFile.Indent++;
            outFile.WriteLine("#region GET AND SET");
            outFile.WriteLine();

            foreach (string k in keys)
            {
                System.Type type = System.Type.GetType(QuickPlayerPrefs.GetSetting(k).GetTypeName());
                if (type == null) type = typeof(string);

                string typeName;
                if (type == typeof(string)) typeName = "string";
                else if (type == typeof(int)) typeName = "int";
                else if (type == typeof(float)) typeName = "float";
                else if (type == typeof(bool)) typeName = "bool";
                else typeName = QuickUtils.GetTypeFullName(type);

                //The getter
                outFile.WriteLine("public static " + typeName + " Get" + k + "()");
                outFile.WriteLine("{");
                outFile.Indent++;
                if (type == typeof(string)) outFile.WriteLine("return QuickPlayerPrefs.GetString(\"" + k + "\");");
                else if (type == typeof(int)) outFile.WriteLine("return QuickPlayerPrefs.GetInt(\"" + k + "\");");
                else if (type == typeof(float)) outFile.WriteLine("return QuickPlayerPrefs.GetFloat(\"" + k + "\");");
                else if (type == typeof(bool)) outFile.WriteLine("return QuickPlayerPrefs.GetBool(\"" + k + "\");");
                else if (type.IsEnum)
                {
                    outFile.WriteLine("return QuickPlayerPrefs.GetEnum<" + typeName + ">(\"" + k + "\");");
                }
                outFile.Indent--;
                outFile.WriteLine("}");

                outFile.WriteLine();

                //The setter
                outFile.WriteLine("public static void Set" + k + "(" + typeName + " value)");
                outFile.WriteLine("{");
                outFile.Indent++;
                outFile.WriteLine("QuickPlayerPrefs.SetValue(\"" + k + "\", value);");
                outFile.Indent--;
                outFile.WriteLine("}");

                //The reset
                outFile.WriteLine("public static void Reset" + k + "()");
                outFile.WriteLine("{");
                outFile.Indent++;
                outFile.WriteLine("QuickPlayerPrefs.ResetSetting(\"" + k + "\");");
                outFile.Indent--;
                outFile.WriteLine("}");

                outFile.WriteLine();
            }

            outFile.WriteLine("#endregion");
            outFile.Indent--;
        }

        #endregion

        #region UPDATE

        protected virtual void OnGUI()
        {
            if (!QuickPlayerPrefs.GetSettingsAsset())
            {
                QuickPlayerPrefs.Init();
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            titleContent.text = "PlayerPrefs";

            GUILayoutOption[] options = { GUILayout.Width(256) };

            EditorGUILayout.BeginVertical("box");
            DrawNewSettingsArea(options);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            _showBaseSettings = EditorGUILayout.Foldout(_showBaseSettings, "Base Settings");
            if (_showBaseSettings) DrawSettings(true);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
            _showCustomSettings = EditorGUILayout.Foldout(_showCustomSettings, "Custom Settings");
            if (_showCustomSettings) DrawSettings(false);
            EditorGUILayout.EndVertical();

            //if (QuickBaseEditor.DrawButton("Clear Custom Settings", options))
            //{
            //    if (EditorUtility.DisplayDialog("Clear Custom Settings", "This will remove all the defined Custom Settings. Are you sure?", "Yes", "No"))
            //    {
            //        QuickPlayerPrefs.ClearSettingsCustom();
            //    }
            //}

            //QuickBaseEditor.DrawHorizontalLine();

            EditorGUILayout.BeginVertical("box");
            _customSettingsScriptName = EditorGUILayout.TextField("Custom Settings Script: ", _customSettingsScriptName, options);
            List<QuickSetting> customSettings = QuickPlayerPrefs.GetSettingsCustom();
            List<string> keys = new List<string>();
            foreach (QuickSetting s in customSettings) keys.Add(s.GetKey());

            if (QuickBaseEditor.DrawButton("Create Custom Settings Script", options))
            {
                CreateSettingsScript(_customSettingsScriptName, keys);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

        }

        protected virtual void DrawNewSettingsArea(GUILayoutOption[] options)
        {
            _showNewSetting = EditorGUILayout.Foldout(_showNewSetting, "New Setting");
            if (_showNewSetting)
            {
                EditorGUI.indentLevel++;

                _newSettingKey = EditorGUILayout.TextField("Key: ", _newSettingKey, options);
                _newSettingType = (PrefType)EditorGUILayout.EnumPopup("Type: ", _newSettingType, options);
                if (_newSettingType == PrefType.Enum) _newSettingTypeEnum = EditorGUILayout.TextField("Enum Type: ", _newSettingTypeEnum, options);

                EditorGUI.indentLevel--;

                if (QuickBaseEditor.DrawButton("Create New Setting", options))
                {
                    object value = null;
                    if (_newSettingKey.Length == 0)
                    {
                        EditorUtility.DisplayDialog("New Setting Creation Error", "The Key string cannot be empty", "Ok");
                    }
                    else if (QuickPlayerPrefs.HasKey(_newSettingKey))
                    {
                        EditorUtility.DisplayDialog("New Setting Creation Error", "The Key " + _newSettingKey + " already exists", "Ok");
                    }
                    else if (CreateNewSetting(out value))
                    {
                        QuickPlayerPrefs.SetValue(_newSettingKey, value);
                        _newSettingKey = _newSettingTypeEnum = "";
                        _newSettingType = PrefType.String;
                    }
                }
            }
        }

        protected virtual void DrawSettings(bool isBase)
        {
            List<QuickSetting> settings = isBase ? QuickPlayerPrefs.GetSettingsBase() : QuickPlayerPrefs.GetSettingsCustom();
            if (settings.Count == 0) return;

            EditorGUI.indentLevel++;

            //Draw the "Name Value" column
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField("Name", "Value");
            foreach (QuickSetting s in settings)
            {
                GUILayout.BeginHorizontal();
                string key = s.GetKey();
                string typeName = s.GetTypeName();
                QuickPlayerPrefs.SetValue(key, DrawPlayerPref(key, typeName));
                if (!isBase)
                {
                    if (QuickBaseEditor.DrawButton("-", GUILayout.Width(32)))
                    {
                        QuickPlayerPrefs.DeleteSetting(key);
                        break;
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            EditorGUI.indentLevel--;
        }

        protected virtual object DrawPlayerPref(string key, string typeName)
        {
            System.Type t = System.Type.GetType(typeName);
            GUILayoutOption[] options = { GUILayout.Width(256) };

            if (t == typeof(int)) return EditorGUILayout.IntField(key, QuickPlayerPrefs.GetInt(key), options);
            if (t == typeof(float)) return EditorGUILayout.FloatField(key, QuickPlayerPrefs.GetFloat(key), options);
            if (t == typeof(bool)) return EditorGUILayout.Toggle(key, QuickPlayerPrefs.GetBool(key), options);
            if (t != null && t.IsEnum)
            {
                string[] enumValues = System.Enum.GetNames(t);
                string value = QuickPlayerPrefs.GetString(key);

                int i = 0;
                for (; i < enumValues.Length; i++)
                {
                    if (enumValues[i] == value) break;
                }

                if (i == enumValues.Length) i = 0;

                return System.Enum.Parse(t, enumValues[EditorGUILayout.Popup(key, i, enumValues, options)]);
            }

            return EditorGUILayout.TextField(key, QuickPlayerPrefs.GetString(key), options);
        }

        #endregion

    }

}

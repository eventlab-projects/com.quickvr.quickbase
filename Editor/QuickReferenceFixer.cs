using System.Collections; 
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using UnityEngine;
using UnityEditor;

using AltProg.CleanEmptyDir;

/**
 * Fixes the missing references for the QuickVR library, when we move from source to binaries. 
 * 
 * References:
 * https://www.gamasutra.com/blogs/VegardMyklebust/20150417/241398/Resolving_Missing_Script_References_in_Unity.php
 * https://github.com/BitPuffin/Unity-Fix-Script-References/blob/master/fix-script-references.rkt
 * https://forum.unity.com/threads/yaml-fileid-hash-function-for-dll-scripts.252075/#post-1695479
 * 
**/

namespace QuickVR
{

    public class QuickReferenceFixerAssetCreator
    {

        public static QuickReferenceFixerAsset CreateAsset(string path)
        {
            QuickReferenceFixerAsset asset = ScriptableObject.CreateInstance<QuickReferenceFixerAsset>();

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;

            return asset;
        }
    }

    public class QuickReferenceFixer : EditorWindow
    {

        #region PROTECTED ATTRIBUTES

        protected QuickReferenceFixerAsset _asset = null;
        protected Dictionary<string, QuickReferenceFixerAsset.ClassData> _guidToClass = new Dictionary<string, QuickReferenceFixerAsset.ClassData>();

        #endregion

        #region GET AND SET

        protected virtual string GetPathRoot()
        {
            return "Assets/QuickVR";
        }

        protected virtual string GetDefaultAssetPath()
        {
            return "Assets/" + GetType().ToString() + "Asset" + ".asset";
        }

        protected virtual string GetModuleName(string path)
        {
            return path.Split('/')[2];
        }

        protected virtual string GetNameDll(string moduleName)
        {
            return moduleName;
        }

        protected virtual bool IsSourceScript(string path)
        {
            //1) Check if the script is an editor script. We ignore those kind of scripts, as they are not 
            //referenced on the serialzed objects. 
            string[] sPath = path.Split('/');
            for (int i = 0; (i < sPath.Length); i++)
            {
                if (sPath[i] == "Editor") return false;
            }

            //2) Check if it is a source file, i.e., it is not included in a dll. 
            return path.EndsWith(".cs");
        }

        protected virtual string GetFullAssetPathByGUID(string guid)
        {
            string dataPath = Application.dataPath;
            dataPath = dataPath.Remove(dataPath.LastIndexOf("/"));

            return (dataPath + @"/" + AssetDatabase.GUIDToAssetPath(guid)).Replace(@"/", @"\");
        }

        protected virtual string GetFileIDBinary(string srcGUID)
        {
            string result = "";
            if (_guidToClass.ContainsKey(srcGUID))
            {
                QuickReferenceFixerAsset.ClassData cData = _guidToClass[srcGUID];
                result = FileIDUtil.Compute(cData._tNamespace, cData._tName).ToString();
            }

            return result;
        }

        protected virtual string GetGUIDBinary(string srcGUID)
        {
            string result = "";
            if (_guidToClass.ContainsKey(srcGUID))
            {
                QuickReferenceFixerAsset.ClassData cData = _guidToClass[srcGUID];
                string path = GetPathRoot() + "/" + cData._quickvrModule + "/" + GetNameDll(cData._quickvrModule) + ".dll";
                Debug.Log("path = " + path);
                result = AssetDatabase.AssetPathToGUID(path);
            }

            return result;
        }

        #endregion

        #region UPDATE

        protected virtual void OnGUI()
        {
            _asset = EditorGUILayout.ObjectField("ReferenceFixerAsset", _asset, typeof(QuickReferenceFixerAsset), true) as QuickReferenceFixerAsset;

            if (GUILayout.Button("Save References"))
            {
                SaveReferences();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Fix References Scenes"))
            {
                FixReferences("t:scene");
            }
            if (GUILayout.Button("Fix References Prefabs"))
            {
                FixReferences("t:prefab");
            }
            if (GUILayout.Button("Fix References Assets"))
            {
                FixReferences("t:scriptableobject");
            }
            if (GUILayout.Button("Fix References All"))
            {
                FixReferences("t:scene t:prefab t:scriptableobject");
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Delete Source Files"))
            {
                DeleteSourceFiles();
            }
        }

        protected virtual void SaveReferences()
        {
            if (!_asset) _asset = QuickReferenceFixerAssetCreator.CreateAsset(GetDefaultAssetPath());
            _asset.Clear();

            string[] scripts = AssetDatabase.FindAssets("t:script", new string[] { GetPathRoot() });
            foreach (string s in scripts)
            {
                string path = AssetDatabase.GUIDToAssetPath(s);
                MonoScript mScript = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (mScript)
                {
                    System.Type t = mScript.GetClass();
                    if (t != null && IsSourceScript(path))
                    {
                        Debug.Log(path);
                        string quickvrModule = GetModuleName(path);
                        _asset.Add(s, new QuickReferenceFixerAsset.ClassData(t.Namespace, t.Name, quickvrModule));
                    }
                }
            }
        }

        protected virtual void FixReferences(string assetType)
        {
            if (!_asset)
            {
                Debug.LogError("[QuickReferenceFixer] Assign a QuickReferenceFixerAsset to fix the references");
                return;
            }

            //Create the map for faster access. 
            _guidToClass.Clear();
            for (int i = 0; i < _asset._keys.Count; i++)
            {
                _guidToClass[_asset._keys[i]] = _asset._values[i];
            }

            string[] assets = AssetDatabase.FindAssets(assetType);
            foreach (string s in assets)
            {
                FixReferencesAsset(s);
            }

            Debug.Log("ALL FIXED!!!");
        }

        protected virtual void FixReferencesAsset(string assetGUID)
        {
            string path = GetFullAssetPathByGUID(assetGUID);
            //Debug.Log("FIXING ASSET = " + path);

            string[] lines = File.ReadAllLines(path);
            for (int i = 0; i < lines.Length; i++)
            {
                Match m = Regex.Match(lines[i], "m_Script:\\s*{fileID:\\s*(-?\\w*),\\s*guid:\\s*(-?\\w*)");
                if (m.Success)
                {
                    string srcFileID = m.Groups[1].ToString();
                    string srcGUID = m.Groups[2].ToString();

                    if (_guidToClass.ContainsKey(srcGUID))
                    {
                        string newFileID = GetFileIDBinary(srcGUID);
                        string newGUID = GetGUIDBinary(srcGUID);

                        if ((newFileID.Length > 0) && (newGUID.Length > 0))
                        {
                            lines[i] = lines[i].Replace(srcFileID, newFileID).Replace(srcGUID, newGUID);
                            //Debug.Log("srcFile = " + AssetDatabase.GUIDToAssetPath(srcGUID));
                            //Debug.Log("newFileID = " + GetFileIDBinary(srcGUID));
                            //Debug.Log("newGUID = " + GetGUIDBinary(srcGUID));
                        }
                    }
                }
            }

            File.WriteAllLines(path, lines);

            //Debug.Log("ASSET FIXED!!!");
        }

        protected virtual void DeleteSourceFiles()
        {
            if (!_asset)
            {
                Debug.LogError("[QuickReferenceFixer] Assign a QuickReferenceFixerAsset to fix the references");
                return;
            }

            foreach (string k in _asset._keys)
            {
                string path = AssetDatabase.GUIDToAssetPath(k);
                Debug.Log("Removing source file: " + path);
                AssetDatabase.DeleteAsset(path);
            }

            //Remove all the empty folders
            List<System.IO.DirectoryInfo> emptyDirs;
            Core.FillEmptyDirList(out emptyDirs);
            Core.DeleteAllEmptyDirAndMeta(ref emptyDirs);

            Debug.Log("All Source Files Removed!!!");

        }

        #endregion

    }

}

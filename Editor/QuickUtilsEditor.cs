using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

using System.IO;

namespace QuickVR
{

    [InitializeOnLoad]
    public static class QuickUtilsEditor
    {

        static QuickUtilsEditor()
        {
            QuickPlayerPrefs.OnSetValue += SaveSettingsAsset;
            EditorApplication.playModeStateChanged += PlayModeChanged;
        }

        public static void MarkSceneDirty()
        {
            if (!Application.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }
        }

        private static void SaveSettingsAsset()
        {
            UnityEditor.EditorUtility.SetDirty(QuickPlayerPrefs.GetSettingsAsset());
        }

        private static void PlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                QuickUtils.OnCloseApplication += StopEditor;
            }
        }

        private static void StopEditor()
        {
            EditorApplication.isPlaying = false;
        }

        public static void CreateDataFolder(string relativePath)
        {
            string path = Application.dataPath + "/" + relativePath;
            if (!Directory.Exists(path))
            {
                Debug.Log("CREATING PATH = " + path);
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.Log("DIRECTORY EXISTS!!!" + path);
            }
        }

        public static string CreateAssetFolder(string path)
        {
            string[] folders = path.Split('/');
            string assetFolderPath;

            if (folders.Length > 1)
            {
                string parentFolder = folders[0];

                //Create all the intermediate parent folders if necessary
                for (int i = 1; i < folders.Length - 1; i++)
                {
                    string s = folders[i];
                    string tmp = parentFolder + '/' + s;
                    if (!AssetDatabase.IsValidFolder(parentFolder + '/' + s))
                    {
                        AssetDatabase.CreateFolder(parentFolder, s);
                    }
                    parentFolder = tmp;
                }

                //Create the last folder in the path
                assetFolderPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(parentFolder, folders[folders.Length - 1]));
            }
            else
            {
                assetFolderPath = path;
            }


            return assetFolderPath;
        }
    }

}

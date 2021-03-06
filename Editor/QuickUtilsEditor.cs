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
    public class QuickUtilsEditor
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
    }

}

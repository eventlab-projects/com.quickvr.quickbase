using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using AltProg.CleanEmptyDir;

namespace QuickVR
{

    public static class QuickVRMenu
    {
        
        private static void AddUniqueComponent<T>() where T : Component
        {
            if (Selection.activeTransform == null) return;

            if (!Selection.activeGameObject.GetComponent<T>())
            {
                Selection.activeGameObject.AddComponent<T>();
            }
        }

        #region TRACKING COMPONENTS

        private const string MENU_QUICK_UNITY_VR = "QuickVR/Tracking/QuickUnityVR";

        [MenuItem(MENU_QUICK_UNITY_VR)]
        private static void AddQuickUnityVR()
        {
            AddUniqueComponent<QuickUnityVR>();
        }

        [MenuItem(MENU_QUICK_UNITY_VR, true)]
        private static bool ValidateAddQuickUnityVR()
        {
            GameObject go = Selection.activeGameObject;
            return go ? go.GetComponent<Animator>() && !go.GetComponent<QuickUnityVR>() : false;
        }

        #endregion

        #region TOOLS

        private const string MENU_ENFORCE_TPOSE = "QuickVR/EnforceTPose";

        [MenuItem(MENU_ENFORCE_TPOSE)]
        private static void EnforceTPose()
        {
            Selection.activeGameObject.GetComponent<Animator>().EnforceTPose();
        }

        [MenuItem(MENU_ENFORCE_TPOSE, true)]
        private static bool ValidateEnforceTPose()
        {
            GameObject go = Selection.activeGameObject;
            return go ? go.GetComponent<Animator>() : false;
        }

        [MenuItem("QuickVR/PlayerPrefs")]
        private static void GetWindowPlayerPrefs()
        {
            EditorWindow.GetWindow<QuickPlayerPrefsWindowEditor>();

            string path = "Assets/QuickVRCfg/Resources/QuickSettingsCustom.asset";
            QuickSettingsAsset settings = AssetDatabase.LoadAssetAtPath<QuickSettingsAsset>(path);
            if (!settings)
            {
                settings = ScriptableObject.CreateInstance<QuickSettingsAsset>();
                QuickUtilsEditor.CreateDataFolder("QuickVRCfg/Resources");
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
            }

            QuickPlayerPrefs.Init();

            //Check if the base settings are defined
            SettingsBase.SetSubjectID(SettingsBase.GetSubjectID());
            SettingsBase.SetGender(SettingsBase.GetGender());
            SettingsBase.SetLanguage(SettingsBase.GetLanguage());
        }

        [MenuItem("QuickVR/MetallicMapFixer")]
        private static void GetWindowMetallicMapFixer()
        {
            EditorWindow.GetWindow<QuickMetallicMapFixer>();
        }

        [MenuItem("QuickVR/ReferenceFixer")]
        private static void GetWindowReferenceFixer()
        {
            EditorWindow.GetWindow<QuickReferenceFixer>();
        }

        [MenuItem("QuickVR/SkeletonFixer")]
        private static void GetWindowSkeletonFixer()
        {
            EditorWindow.GetWindow<QuickSkeletonFixer>();
        }

        [MenuItem("QuickVR/CleanEmptyDir")]
        private static void ShowWindow()
        {
            MainWindow w = EditorWindow.GetWindow<MainWindow>();
            w.titleContent.text = "Clean";
        }

        #endregion

        //#region BODY TRACKING COMPONENTS

        //[MenuItem(MENU_QUICKVR_ROOT + "/" + MENU_QUICKVR_BODYTRACKING + "/" + "QuickMotive")]
        //static void AddQuickMotive()
        //{
        //    AddUniqueComponent<QuickMotiveBodyTracking>();
        //}

        //[MenuItem(MENU_QUICKVR_ROOT + "/" + MENU_QUICKVR_BODYTRACKING + "/" + "QuickKinect")]
        //static void AddQuickKinnect()
        //{
        //    AddUniqueComponent<QuickKinectBodyTracking>();
        //}

        //[MenuItem(MENU_QUICKVR_ROOT + "/" + MENU_QUICKVR_BODYTRACKING + "/" + "QuickNeuron")]
        //static void AddQuickNeuron()
        //{
        //    AddUniqueComponent<QuickNeuronBodyTracking>();
        //}

        //[MenuItem(MENU_QUICKVR_ROOT + "/" + MENU_QUICKVR_BODYTRACKING + "/" + "QuickDummy")]
        //static void AddQuickDummy()
        //{
        //    AddUniqueComponent<QuickDummyBodyTracking>();
        //}

        //#endregion

    }

}

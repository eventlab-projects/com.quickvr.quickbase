using UnityEngine;
using UnityEditor;
using System.Collections;

namespace QuickVR
{

    [CustomEditor(typeof(QuickUIMenuPage), true)]
    [CanEditMultipleObjects]
    public class QuickUIMenuPageEditor : QuickBaseEditor
    {

        #region GET AND SET

        protected override void SaveConfiguration()
        {
            string path = QuickUtils.GetRelativeAssetsPath(EditorUtility.SaveFilePanel("Save a Menu page", "Assets" + GetConfigurationFolderName(), "MenuPage", "prefab"));
            if (path != "")
            {
                GameObject go = ((QuickUIMenuPage)target).gameObject;
                GameObject goPrefab = PrefabUtility.CreatePrefab(path, go, ReplacePrefabOptions.ConnectToPrefab);
                goPrefab.name = go.name;
            }
        }

        protected override void LoadConfiguration()
        {
            string path = QuickUtils.GetRelativeAssetsPath(EditorUtility.OpenFilePanel("Load a Menu page", "Assets" + GetConfigurationFolderName(), "prefab"));
            if (path != "")
            {
                GameObject go = ((QuickUIMenuPage)target).gameObject;
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                PrefabUtility.ConnectGameObjectToPrefab(go, prefab);
            }
        }

        #endregion

        #region UPDATE

        protected override void DrawGUI()
        {
            QuickUIMenuPage page = (QuickUIMenuPage)target;
            if (DrawButton("Add Child Page"))
            {
                page.AddChildPage();
                MarkSceneDirty();
            }
            if (DrawButton("Remove Child Page"))
            {
                page.RemoveChildPage();
                MarkSceneDirty();
            }

            EditorGUILayout.Space();

            if (DrawButton("Save Configuration")) SaveConfiguration();
            if (DrawButton("Load Configuration")) LoadConfiguration();
        }

        #endregion

    }

}

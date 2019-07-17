using UnityEngine;
using UnityEditor;
using System.Collections;

namespace QuickVR
{

    [CustomEditor(typeof(QuickUIMenuPage), true)]
    [CanEditMultipleObjects]
    public class QuickUIMenuPageEditor : QuickBaseEditor
    {

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

        }

        #endregion

    }

}

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace QuickVR
{

    [CustomEditor(typeof(QuickUIMenuPageOptions), true)]
    public class QuickUIMenuPageOptionsEditor : QuickUIMenuPageEditor
    {

        protected override void DrawGUI()
        {
            QuickUIMenuPageOptions page = (QuickUIMenuPageOptions)target;
            if (DrawPropertyField("_options", "Options: "))
            {
                page.UpdateOptions();
            }
            
            EditorGUILayout.Space();

            base.DrawGUI();
        }
        
    }

}


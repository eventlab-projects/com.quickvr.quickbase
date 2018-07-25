using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;


namespace QuickVR {

	[CustomEditor(typeof(QuickUIMenu), true)]
    [CanEditMultipleObjects]
	public class QuickUIMenuEditor : QuickBaseEditor {

		#region GET AND SET

		protected virtual int GetMinNumPages() {
			return 1;
		}

		#endregion

		#region GUI DRAW

		protected virtual void DrawProperties() {
            int numPagesBefore = ((QuickUIMenu)target).GetNumPages();
			int numPagesAfter = Mathf.Max(GetMinNumPages(), EditorGUILayout.IntField("Num Pages: ", numPagesBefore));
			if (numPagesBefore != numPagesAfter) {
                Debug.Log("numPagesBefore = " + numPagesBefore);
                Debug.Log("numPagesAfter = " + numPagesAfter);
				QuickUIMenu menu = (QuickUIMenu)target;
				menu.CreatePages(numPagesAfter);
				//MarkSceneDirty();
			}

			DrawPropertyField("_resolutionX", "Resolution X: ");
			DrawPropertyField("_resolutionY", "Resolution Y: ");

			DrawPropertyField("_size", "Size: ");
		}

		protected override void DrawGUI() {
			DrawProperties();

			QuickUIMenu menu = (QuickUIMenu)target;
			menu.UpdateDimensions();
		}

		#endregion

	}

}

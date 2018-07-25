using UnityEngine;
using UnityEditor;
using System.Collections;


namespace QuickVR {

	[CustomEditor(typeof(QuickUIMenuPolygon), true)]
	public class QuickUIMenuPolygonEditor : QuickUIMenuEditor {

		#region GET AND SET

		protected override int GetMinNumPages() {
			return 3;
		}

		#endregion

		#region GUI DRAW

		protected override void DrawProperties() {
			base.DrawProperties();
			DrawPropertyField("_rotationSpeed", "Rotation Speed: ");
		}

		#endregion

	}

}

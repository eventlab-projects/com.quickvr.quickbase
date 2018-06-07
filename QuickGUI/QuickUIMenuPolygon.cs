using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	[System.Serializable]
	public class QuickUIMenuPolygon : QuickUIMenu {

		#region PUBLIC PARAMETERS

		public float _rotationSpeed = 180.0f;

		#endregion

		#region CREATION AND DESTRUCTION

		public override void Init() {
			CreatePages(4);
		}

		public override void CreatePages(int numPages) {
			base.CreatePages(numPages);

			for (int i = 0; i < transform.childCount; i++) {
				transform.GetChild(i).Rotate(Vector3.up, 180.0f + (float)i * -GetAngleStep(), Space.Self);
			}
		}

		#endregion

		#region GET AND SET

		protected virtual float GetAngleStep() {
			return 360.0f / (float)GetNumPages();
		}

		protected virtual float GetTriangleBase() {
			return _resolutionX * 0.5f;
		}

		protected virtual float GetTriangleHeight() {
			float alpha = GetAngleStep() * 0.5f;
			float gamma = 90.0f - alpha;
			return GetTriangleBase() * Mathf.Sin(gamma * Mathf.Deg2Rad) / Mathf.Sin(alpha * Mathf.Deg2Rad);
		}

		protected virtual float GetBoundingRadius() {
			//The bounding radius is equal to the hypotenuse of the triangle. 
			return Mathf.Sqrt(GetTriangleBase() * GetTriangleBase() + GetTriangleHeight() * GetTriangleHeight());
		}

		protected override void SetVisiblePage(int pageID, bool v) {
			base.SetVisiblePage(pageID, v);
			QuickUIMenuPage page = GetPage(pageID);
			page.SetActiveElement(QuickUIMenuPage.UIElement.Background, _isVisible);
		}

		#endregion

		#region UPDATE

		public override void UpdateDimensions() {
			float s = _size / (2.0f * GetBoundingRadius());
			transform.localScale = new Vector3(s, s, s);
//			base.UpdateDimensions();

			float h = GetTriangleHeight();
			for (int i = 0; i < GetNumPages(); i++) {
				Vector3 fwd = Quaternion.Inverse(transform.rotation) * Quaternion.AngleAxis(-GetAngleStep() * i, transform.up) * transform.forward;
				GetPage(i).transform.localPosition = transform.position + fwd * h;
			}
		}

		protected override IEnumerator CoChangePage(float sign) {

			//Compute the target rotation
			float angle = sign * GetAngleStep();
			Quaternion rotInitial = transform.rotation;
			transform.Rotate(transform.up, angle, Space.World);
			Quaternion rotTarget = transform.rotation;
			transform.rotation = rotInitial;

			float rotTime = Mathf.Abs(angle) / _rotationSpeed;	//The total amount of time required to acquire the targetRotation
			float elapsedTime = 0.0f;
			while (elapsedTime < rotTime) {
				elapsedTime += Time.deltaTime;
				transform.rotation = Quaternion.Slerp(rotInitial, rotTarget, elapsedTime / rotTime);
				yield return null;
			}
			transform.rotation = rotTarget;

			yield return StartCoroutine(base.CoChangePage(sign));
		}

		#endregion

	}

}

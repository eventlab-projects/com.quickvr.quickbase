using UnityEngine;
using System.Collections;

namespace QuickVR {

	public class QuickUIMenuAnchor : MonoBehaviour {

		#region PROTECTED PARAMETERS

		protected QuickHeadTracking _hTracking = null;
		protected QuickUIMenu _menu = null;

		#endregion

		#region CREATION AND DESTRUCTION

		protected virtual void Awake() {
			CheckRequirements();
		}

		protected virtual void OnEnable() {
			QuickVRManager.OnPostUpdateTracking += UpdateMenuPosition;
			QuickBaseGameManager.OnRunning += CheckRequirements;
			_menu.OnOpen += ResetMenuOrientation;
		}

		protected virtual void OnDisable() {
			QuickVRManager.OnPostUpdateTracking -= UpdateMenuPosition;
			QuickBaseGameManager.OnRunning += CheckRequirements;
			_menu.OnOpen -= ResetMenuOrientation;
		}

		protected virtual void CheckRequirements() {
			_hTracking = FindObjectOfType<QuickHeadTracking>();
			_menu = GetComponent<QuickUIMenu>();
		}

		#endregion

		#region UPDATE

		protected virtual void ResetMenuOrientation() {
			if (_menu) _menu.transform.LookAt(_hTracking.GetEyeCenterPosition());
		}

		protected virtual void UpdateMenuPosition() {
			if (_hTracking && _menu) {
				Vector3 ePos = _hTracking.GetEyeCenterPosition();
				_menu.transform.position = ePos + _hTracking.transform.forward * 0.5f;
			}
		}

		#endregion

	}

}

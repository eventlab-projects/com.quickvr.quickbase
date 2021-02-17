using UnityEngine;
using UnityEngine.EventSystems;

namespace QuickVR {

	public class QuickTeleportWalkableObject : QuickUIInteractiveItem {

		#region PUBLIC PARAMETERS

	    public GameObject HighLightPrefab;

		#endregion

		#region PROTECTED PARAMETERS

		protected GameObject _HighLightPrefabInstance;

		#endregion

		#region CREATION AND DESTRUCTION

		protected virtual void Awake() {
	        if (HighLightPrefab != null)
	        {
	            _HighLightPrefabInstance = (GameObject)Instantiate(HighLightPrefab, transform);
	            _HighLightPrefabInstance.transform.localPosition = Vector3.zero;

	            _HighLightPrefabInstance.SetActive(false);
	        }
	    }

		#endregion

		#region UPDATE

		public override void Over() {
			if (_HighLightPrefabInstance) _HighLightPrefabInstance.SetActive(true);
			base.Over();
		}

		public override void Out() {
			if (_HighLightPrefabInstance) _HighLightPrefabInstance.SetActive(false);
			base.Out();
		}

		#endregion

	}

}
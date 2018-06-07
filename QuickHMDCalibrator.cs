using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

namespace QuickVR {

	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class QuickHMDCalibrator : MonoBehaviour {

		#region CONSTANTS

		protected const string CALIBRATION_MATERIAL = "QuickUtils/CalibrationScreen";

        #endregion

        #region PROTECTED PARAMETERS

        protected Material _material = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake() {
			//Create the mesh filter
			MeshFilter mFilter = gameObject.GetComponent<MeshFilter>();
			if (!mFilter) mFilter = gameObject.AddComponent<MeshFilter>();
			mFilter.mesh = QuickUtils.CreateFullScreenQuad();
			mFilter.mesh.bounds = new Bounds(Vector3.zero, new Vector3(1000000, 1000000, 1000000));
			
			//Create the mesh renderer
			MeshRenderer r = gameObject.GetComponent<MeshRenderer>();
			if (!r) r = gameObject.AddComponent<MeshRenderer>();
			r.shadowCastingMode = ShadowCastingMode.Off;
			r.receiveShadows = false;
			r.material = new Material(Shader.Find("QuickVR/CalibrationScreen"));
            _material = r.material;

			gameObject.layer = LayerMask.NameToLayer("UI");
		}

		#endregion

		#region GET AND SET

        public virtual Texture GetCalibrationTexture()
        {
            return _material.mainTexture;
        }

		public virtual void SetCalibrationTexture(Texture2D tex) {
			_material.mainTexture = tex;
		}

        public virtual void SetCalibrationColor(Color color)
        {
            _material.color = color;
        }

		#endregion

	}

}
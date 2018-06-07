using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

namespace QuickVR {

	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]

	public class BillboardManager : MonoBehaviour {

		#region PUBLIC PARAMETERS

		public Texture2D _texture = null;
		public Color _color = new Color(0,0,0,0);

		public Vector2 _position = Vector2.zero;
		public Vector2 _scale = new Vector2(1,1);

		#endregion

		#region PROTECTED PARAMETERS

		protected Mesh _billboard = null;
		protected Material _material = null;
		protected bool _isVisible = true;

		#endregion

		#region CONSTANTS

		protected const string BILLBOARD_SHADER_NAME = "Custom/Billboard";
		protected const float BILLBOARD_BOUNDS_SIZE = 100000.0f;

		#endregion

		#region CREATION AND DESTRUCTION

		protected virtual void Awake() {
			transform.position = Vector3.zero;

			//Create the billboard
			MeshFilter mFilter = GetComponent<MeshFilter>();
			_billboard = CreateQuad();
			mFilter.mesh = _billboard;

			//Create the material with the custom shader for the billboard
			_material = new Material(Shader.Find(BILLBOARD_SHADER_NAME));
			MeshRenderer mRenderer = GetComponent<MeshRenderer>();
			mRenderer.material = _material;
			mRenderer.shadowCastingMode = ShadowCastingMode.Off;
			mRenderer.receiveShadows = false;

			//Force the GameObject to be in the Water Layer by default, so it won't
			//be rendered by the mirror cameras
			gameObject.layer = LayerMask.NameToLayer("Water");
		}

		protected virtual void OnEnable() {
			Show(true);
		}

		protected virtual void OnDisable() {
			Show(false);
		}

		protected virtual void OnDestroy() {
			if (_billboard) Destroy(_billboard);
			if (_material) Destroy(_material);
		}

		protected virtual Mesh CreateQuad() {
			//Creates a special quad where the vertices are already in NDC
			Mesh m = new Mesh();
			m.name = "_QuadNDC_";

			//Create the vertices
			m.vertices = new Vector3[] {
				new Vector3(-1, -1, 0),
				new Vector3(1, -1, 0),
				new Vector3(1, 1, 0),
				new Vector3(-1, 1, 0)
			};

			//Create the UVs
			m.uv = new Vector2[] {
				new Vector2 (0, 0),
				new Vector2 (1, 0),
				new Vector2(1, 1),
				new Vector2 (0, 1)
			};

			//Create the triangle list
			m.triangles = new int[] { 0, 1, 2, 2, 3, 0};

			//Other stuff
			m.RecalculateNormals();
			m.bounds = new Bounds(Vector3.zero, new Vector3(BILLBOARD_BOUNDS_SIZE, BILLBOARD_BOUNDS_SIZE, BILLBOARD_BOUNDS_SIZE));
			
			return m;
		}

		#endregion

		#region GET AND SET

		public virtual bool IsVisible() {
			return _isVisible;
		}

		public virtual void Show(bool b) {
			_isVisible = b;
		}
		
		public virtual void Toggle() {
			_isVisible = !_isVisible;
		}

		#endregion

		#region UPDATE

		protected virtual void Update() {
			if (_material) {
				Shader.SetGlobalMatrix("BILLBOARD_MATRIX", ComputeBillboardMatrix());
				_material.SetTexture("_MainTex", _texture);
				if (_isVisible) _material.SetColor("_Color", _color);
				else _material.SetColor("_Color", new Color(0,0,0,0));
			}
		}

		protected virtual Matrix4x4 ComputeBillboardMatrix() {
			return Matrix4x4.TRS(new Vector3(_position.x, -_position.y, 0.0f), Quaternion.identity, new Vector3(_scale.x, _scale.y, 0.0f));
		}
				
		#endregion

	}

}
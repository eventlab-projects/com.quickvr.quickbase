using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	/// <summary>
	/// Mirror Reflection Script for planar surfaces with dynamic shadows support. 
	/// \author Ramon Oliva
	/// </summary>
	 
	[ExecuteInEditMode] // Make mirror live-update even when not in play mode
	public class QuickMirrorReflection : MonoBehaviour {
		
		#region PUBLIC PARAMETERS

        public enum ReflectionQuality
        {
            HIGH = 1,
            GOOD = 2,
            FAIR = 4,
            POOR = 8,
        };

        public enum Direction
        {
            X,Y,Z,
        };

		public Material _reflectionMat = null;

        public bool _ignoreSceneCamera = false;
        public bool _disablePixelLights = false;							//Disable per pixel lighting on the reflection. Use this for performance boost. 

        public LightShadows _reflectionShadowType = LightShadows.Soft;	//Determines the most expensive type of shadows that will be used on the reflected image. 
        public RenderingPath _reflectedRenderingPath = RenderingPath.Forward;
		public ReflectionQuality _reflectionQuality = ReflectionQuality.HIGH;
        public float _reflectionDistance = 100.0f;

        public LayerMask _reflectLayers = -1;							//The layers that will be reflected on the mirror.

        public Direction _direction = Direction.Z;
		
		#endregion
		
		#region PROTECTED PARAMETERS
		
		protected RenderTexture _reflectionTextureLeft = null;
        protected RenderTexture _reflectionTextureRight = null;

        protected Vector2 _oldTextureSize = Vector2.zero;

        protected Camera _reflectionCamera = null;
		protected static bool _insideRendering = false;

		protected MeshFilter _mFilter;
        protected Renderer _renderer;
        protected BoxCollider _collider;

        protected Material _rendererMaterial = null;
        protected Material _rendererSharedMaterial = null;

        protected enum Corner {
			TOP_LEFT,
			TOP_RIGHT,
			BOTTOM_LEFT,
			BOTTOM_RIGHT,
		};

        protected Light[] _lights = null;
        protected LightShadows[] _shadowTypes = null;
		
		#endregion
		
		#region INITIALIZATION AND DESTRUCTION
        
		protected virtual void OnEnable() {
            //Create the mesh filter
			_mFilter = gameObject.GetOrCreateComponent<MeshFilter>();
            if (!_mFilter.sharedMesh) _mFilter.sharedMesh = QuickUtils.CreateFullScreenQuad();
            
            //Create the mesh renderer
            _renderer = gameObject.GetOrCreateComponent<MeshRenderer>();
            _renderer.receiveShadows = false;
                        
			_collider = gameObject.GetOrCreateComponent<BoxCollider>();
            if (!_reflectionMat) _reflectionMat = Resources.Load<Material>("QuickMirrorReflection");

            //Ensure that the renderer has the reflection material. 
            if (Application.isPlaying)
            {
                _renderer.material = _reflectionMat;
                _rendererMaterial = _renderer.material;
            }
            else {
                _renderer.sharedMaterial = _reflectionMat;
                _rendererSharedMaterial = _renderer.sharedMaterial;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
		
		protected virtual void OnDisable() {
            // Cleanup all the objects we possibly have created
            if (_reflectionCamera) DestroyImmediate(_reflectionCamera.gameObject);

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
		
		protected virtual void CreateReflectionTexture() {
			Vector2 texSize = GetTextureSize(_reflectionQuality);

            //Check the reflection texture for the left eye
            if (!_reflectionTextureLeft || _oldTextureSize != texSize)
            {
                if (_reflectionTextureLeft) DestroyImmediate(_reflectionTextureLeft);
                _reflectionTextureLeft = CreateRenderTexture("__ReflectionTextureLeft__", texSize);
            }

            //Check the reflection texture for the right eye
            if (!_reflectionTextureRight || _oldTextureSize != texSize)
            {
                if (_reflectionTextureRight) DestroyImmediate(_reflectionTextureRight);
                _reflectionTextureRight = CreateRenderTexture("__ReflectionTextureRight__", texSize);
            }

            _oldTextureSize = texSize;
        }

        //protected virtual RenderTexture CreateRenderTexture(string name, Vector2 size, RenderTextureFormat format, FilterMode filter) {
        //	RenderTexture rTex = new RenderTexture((int)size.x, (int)size.y, 24);
        //	rTex.name = name;
        //	rTex.isPowerOfTwo = true;
        //	rTex.hideFlags = HideFlags.DontSave;
        //	rTex.anisoLevel = 0;
        //	rTex.format = format;
        //	rTex.filterMode = filter;
        //	return rTex;
        //}

        protected virtual RenderTexture CreateRenderTexture(string name, Vector2 size, int aaLevel = 4)
        {
            RenderTexture rTex = new RenderTexture((int)size.x, (int)size.y, 24);
            rTex.name = name;
            rTex.hideFlags = HideFlags.DontSave;
            rTex.antiAliasing = aaLevel;

            return rTex;
        }

        protected virtual void ClearRenderTexture(RenderTexture rTex, Color bgColor)
        {
            RenderTexture oldRenderTex = RenderTexture.active;
            RenderTexture.active = rTex;
            GL.Clear(true, true, bgColor);
            RenderTexture.active = oldRenderTex;
        }

        protected virtual void CreateReflectionCamera() {
            //Create the camera
            if (_reflectionCamera) return;

            _reflectionCamera = new GameObject("__MirrorReflectionCamera__").AddComponent<Camera>();
            _reflectionCamera.gameObject.layer = LayerMask.NameToLayer("Water");
            _reflectionCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
            _reflectionCamera.gameObject.AddComponent<Skybox>();

            _reflectionCamera.enabled = false;
            
            _reflectionCamera.cullingMask = ~(1 << LayerMask.NameToLayer("Water")) & ~(1 << LayerMask.NameToLayer("UI")) & _reflectLayers.value; // never render water layer
            _reflectionCamera.renderingPath = _reflectedRenderingPath;
            _reflectionCamera.allowMSAA = false;

   //         if (cam.clearFlags == CameraClearFlags.Skybox) {
			//	Skybox sky = cam.GetComponent<Skybox>();
			//	Skybox mysky = reflectionCamera.GetComponent<Skybox>();
			//	if (!sky || !sky.material) {
			//		mysky.enabled = true;
			//	}
			//	else {
			//		mysky.enabled = true;
			//		mysky.material = sky.material;
			//	}
			//}
			
			//return reflectionCamera;
		}

        protected virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _lights = FindObjectsOfType<Light>();
            _shadowTypes = new LightShadows[_lights.Length];
            for (int i = 0; i < _lights.Length; i++)
            {
                _shadowTypes[i] = _lights[i].shadows;
            }
        }
		
		#endregion
		
		#region GET AND SET
		
		protected Vector2 GetTextureSize(ReflectionQuality quality) {
			float tSize = ((int)quality == 0)? 2048.0f : (float)(2048 / (int)quality);
			return new Vector2(tSize, tSize);
		}

        protected virtual Vector3 GetCenter()
        {
            return transform.TransformPoint(_mFilter.sharedMesh.bounds.center);
        }

        protected virtual Vector3 GetScreenAxis(Direction dir)
        {
            Vector3 pa = GetCornerPosition(Corner.BOTTOM_LEFT);
            if (dir == Direction.X) return (GetCornerPosition(Corner.BOTTOM_RIGHT) - pa).normalized;
            else if (dir == Direction.Y) return (GetCornerPosition(Corner.TOP_LEFT) - pa).normalized;
            return -GetNormal();
        }

        protected virtual Vector3 GetNormal() {
            Vector3 center = transform.position;
            Vector3 tr = GetCornerPosition(Corner.TOP_RIGHT);
            Vector3 tl = GetCornerPosition(Corner.TOP_LEFT);

            return Vector3.Cross(tr - center, tl - center).normalized;
		}

		protected virtual Vector3 GetCornerPosition(Corner corner) {
            Bounds bounds = _mFilter.sharedMesh.bounds;

            if (_direction == Direction.Y)
            {
                if (corner == Corner.BOTTOM_LEFT) return transform.TransformPoint(new Vector3(bounds.min.x, bounds.center.y, -bounds.min.z));
                if (corner == Corner.TOP_LEFT) return transform.TransformPoint(new Vector3(bounds.min.x, bounds.center.y, -bounds.max.z));
                if (corner == Corner.BOTTOM_RIGHT) return transform.TransformPoint(new Vector3(bounds.max.x, bounds.center.y, -bounds.min.z));
                return transform.TransformPoint(new Vector3(bounds.max.x, bounds.center.y, -bounds.max.z));
            }

            //if (_direction == Direction.Z)
            if (corner == Corner.BOTTOM_LEFT) return transform.TransformPoint(new Vector3(bounds.min.x, bounds.min.y, bounds.center.z));
            if (corner == Corner.TOP_LEFT) return transform.TransformPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.center.z));
            if (corner == Corner.BOTTOM_RIGHT) return transform.TransformPoint(new Vector3(bounds.max.x, bounds.min.y, bounds.center.z));
            return transform.TransformPoint(new Vector3(bounds.max.x, bounds.max.y, bounds.center.z));
        }

        protected virtual Dictionary<Corner, Vector3> GetCameraCornerRays(Camera cam, float d)
        {
            Vector3[] corners = new Vector3[4];
            cam.CalculateFrustumCorners(cam.rect, d, Camera.MonoOrStereoscopicEye.Mono, corners);
            Dictionary<Corner, Vector3> result = new Dictionary<Corner, Vector3>();

            result[Corner.BOTTOM_LEFT] = cam.transform.TransformVector(corners[0]);
            result[Corner.TOP_LEFT] = cam.transform.TransformVector(corners[1]);
            result[Corner.TOP_RIGHT] = cam.transform.TransformVector(corners[2]);
            result[Corner.BOTTOM_RIGHT] = cam.transform.TransformVector(corners[3]);

            return result;
        }

        #endregion

        #region MIRROR RENDER

        protected virtual bool AllowRender() {
            //return ((!_ignoreSceneCamera || (_ignoreSceneCamera && cam.name != "SceneCamera")) && !_insideRendering && Vector3.Distance(cam.transform.position, transform.position) < _reflectionDistance);
            return (!_insideRendering && Vector3.Distance(Camera.current.transform.position, transform.position) < _reflectionDistance);
        }

        // This is called when it's known that the object will be rendered by some
		// camera. We render reflections and do other updates here.
		// Because the script executes in edit mode, reflections for the scene view
		// camera will just work!
		public virtual void OnWillRenderObject() {
            //Compute the UVs
            Vector3[] vertices = _mFilter.sharedMesh.vertices;
            Vector2[] uv = _mFilter.sharedMesh.uv;
            Vector3 origin = GetCornerPosition(Corner.BOTTOM_LEFT);
            Vector3 offset = GetCornerPosition(Corner.TOP_RIGHT) - origin;
            Vector3 vr = GetScreenAxis(Direction.X);
            Vector3 vu = GetScreenAxis(Direction.Y);
            float w = Vector3.Dot(offset, vr);
            float h = Vector3.Dot(offset, vu);

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = transform.TransformPoint(vertices[i]) - origin;
                uv[i] = new Vector2(1.0f - (Vector3.Dot(v, vr) / w), 1.0f - (Vector3.Dot(v, vu) / h));
            }
            _mFilter.sharedMesh.uv = uv;
            
			//Force the mirror to be in the Water layer, so it will avoid to be rendered by the reflection cameras
			gameObject.layer = LayerMask.NameToLayer("Water");

            if (AllowRender())
            {

                _insideRendering = true;    // Safeguard from recursive reflections.        

                // Optionally disable pixel lights for reflection
                int oldPixelLightCount = QualitySettings.pixelLightCount;
                if (_disablePixelLights) QualitySettings.pixelLightCount = 0;

                CreateReflectionTexture();
                CreateReflectionCamera();
                RenderReflection();

                _insideRendering = false;

                // Restore pixel light count
                if (_disablePixelLights) QualitySettings.pixelLightCount = oldPixelLightCount;
            }
            else
            {
                ClearRenderTexture(_reflectionTextureLeft, Color.black);
                ClearRenderTexture(_reflectionTextureRight, Color.black);
            }
		}
		
		protected virtual void RenderReflection() {
            OnPreRenderVirtualImage();
			
			ReflectCamera();

            RenderVirtualImageStereo(_reflectionTextureLeft, _reflectionTextureRight);

            OnPostRenderVirtualImage();
        }
		
		protected virtual void OnPreRenderVirtualImage() {
            if (_lights != null) {
                for (int i = 0; i < _lights.Length; i++)
                {
                    _lights[i].shadows = LightShadows.None;
                }
            }
        }
		
		protected virtual void OnPostRenderVirtualImage() {
			Material mat = (Application.isPlaying) ? _rendererMaterial : _rendererSharedMaterial;
            if (mat) ConfigureMaterial(mat);

            if (_lights != null)
            {
                for (int i = 0; i < _lights.Length; i++)
                {
                    _lights[i].shadows = _shadowTypes[i];
                }
            }
        }

        protected virtual void ConfigureMaterial(Material mat)
        {
            mat.SetTexture("_LeftEyeTexture", _reflectionTextureLeft);
            mat.SetTexture("_RightEyeTexture", _reflectionTextureRight);
        }
		
		protected virtual void ReflectCamera() {
			//Reflect camera around reflection plane
			Vector3 normal = GetNormal();							
			Vector3 camToPlane = Camera.current.transform.position - transform.position; 
			Vector3 reflectionCamToPlane = Vector3.Reflect(camToPlane, normal);
			Vector3 camPosRS = transform.position + reflectionCamToPlane;
			_reflectionCamera.transform.position = camPosRS;
		}

        protected virtual void RenderVirtualImageStereo(RenderTexture rtLeft, RenderTexture rtRight, bool mirrorStereo = true)
        {
            if (Camera.current.stereoEnabled)
            {
                float stereoSign = mirrorStereo ? -1.0f : 1.0f;
                float stereoSeparation = Vector3.Distance(InputTracking.GetLocalPosition(XRNode.LeftEye), InputTracking.GetLocalPosition(XRNode.RightEye));
                if (Camera.current.stereoTargetEye == StereoTargetEyeMask.Both || Camera.current.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    RenderVirtualImage(rtLeft, stereoSign * stereoSeparation * 0.5f);
                }

                if (Camera.current.stereoTargetEye == StereoTargetEyeMask.Both || Camera.current.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    RenderVirtualImage(rtRight, -stereoSign * stereoSeparation * 0.5f);
                }
            }
            else
            {
                RenderVirtualImage(rtLeft);
            }
        }
		
		protected virtual void RenderVirtualImage(RenderTexture targetTexture, float stereoSeparation = 0.0f) {
			//Setup the projection and worldView matrices as explained in:
            //http://csc.lsu.edu/~kooima/pdfs/gen-perspective.pdf 

			Vector3 pa = GetCornerPosition(Corner.BOTTOM_LEFT);
			Vector3 pb = GetCornerPosition(Corner.BOTTOM_RIGHT);
			Vector3 pc = GetCornerPosition(Corner.TOP_LEFT);
			
			Vector3 pe = _reflectionCamera.transform.position + Camera.current.transform.right * stereoSeparation; // eye position

			Vector3 va = pa - pe;
			Vector3 vb = pb - pe;
			Vector3 vc = pc - pe;
			Vector3 vr = GetScreenAxis(Direction.X);		// right axis of screen
			Vector3 vu = GetScreenAxis(Direction.Y);		// up axis of screen
			Vector3 vn = GetScreenAxis(Direction.Z);        // normal vector of screen

            //Adjust the near and far clipping planes of the reflection camera. 
            Vector3 v = pe - GetCenter();
            Vector3 projectedPoint = pe - Vector3.Project(v, vn);
            float n = Mathf.Max(Camera.current.nearClipPlane, Vector3.Distance(pe, projectedPoint));
            float f = Mathf.Max(n, Camera.current.farClipPlane);

            float d = -Vector3.Dot(va, vn);			// distance from eye to screen 
            float l = Vector3.Dot(vr, va) * n / d;	// distance to left screen edge
			float r = Vector3.Dot(vr, vb) * n / d;	// distance to right screen edge
			float b = Vector3.Dot(vu, va) * n / d;	// distance to bottom screen edge
			float t = Vector3.Dot(vu, vc) * n / d;  // distance to top screen edge

            //Projection matrix
			Matrix4x4 p = new Matrix4x4();
			p.SetRow(0, new Vector4(2.0f * n / (r-l), 	0.0f, 				(r+l) / (r-l), 	0.0f));
			p.SetRow(1, new Vector4(0.0f, 				2.0f * n / (t-b), 	(t+b) / (t-b), 	0.0f));
			p.SetRow(2, new Vector4(0.0f, 				0.0f, 				(f+n) / (n-f), 	2.0f * f * n / (n-f)));
			p.SetRow(3, new Vector4(0.0f, 				0.0f, 				-1.0f, 			0.0f));

			//Rotation matrix
			Matrix4x4 rm = Matrix4x4.identity; 
			rm.SetRow(0, new Vector4(vr.x, vr.y, vr.z, 0.0f));
			rm.SetRow(1, new Vector4(vu.x, vu.y, vu.z, 0.0f));
			rm.SetRow(2, new Vector4(vn.x, vn.y, vn.z, 0.0f));

			//Translation matrix
			Matrix4x4 tm = Matrix4x4.identity;
			tm.SetColumn(3, new Vector4(-pe.x, -pe.y, -pe.z, 1.0f));

            // set matrices
            _reflectionCamera.projectionMatrix = p;
            _reflectionCamera.worldToCameraMatrix = rm * tm;

            // The original paper puts everything into the projection 
            // matrix (i.e. sets it to p * rm * tm and the other 
            // matrix to the identity), but this doesn't appear to 
            // work with Unity's shadow maps.
            _reflectionCamera.targetTexture = targetTexture;
            _reflectionCamera.Render();
		}

        #endregion

        #region DEBUG

        protected virtual void OnDrawGizmos()
        {
            float r = 0.05f;

            Gizmos.color = Color.blue;
            Vector3 pa = GetCornerPosition(Corner.BOTTOM_LEFT);
            Gizmos.DrawSphere(pa, r);

            Gizmos.color = Color.yellow;
            Vector3 pb = GetCornerPosition(Corner.BOTTOM_RIGHT);
            Gizmos.DrawSphere(pb, r);

            Gizmos.color = Color.red;
            Vector3 pc = GetCornerPosition(Corner.TOP_LEFT);
            Gizmos.DrawSphere(pc, r);

            Gizmos.color = Color.green;
            Vector3 pd = GetCornerPosition(Corner.TOP_RIGHT);
            Gizmos.DrawSphere(pd, r);

            Vector3 vr = GetScreenAxis(Direction.X);        // right axis of screen
            Vector3 vu = GetScreenAxis(Direction.Y);        // up axis of screen
            Vector3 vn = GetScreenAxis(Direction.Z);        // normal vector of screen

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, vr);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, vu);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, -vn);
        }

        protected virtual void DrawReflectionCamera(Camera reflectionCamera)
        {
            reflectionCamera.ResetProjectionMatrix();
            float s = 0.05f;
            Vector3 pe = reflectionCamera.transform.position; // eye position

            Gizmos.color = Color.grey;
            Gizmos.DrawCube(reflectionCamera.transform.position, new Vector3(s, s, s));

            Vector3 pa = GetCornerPosition(Corner.BOTTOM_LEFT);
            Vector3 pb = GetCornerPosition(Corner.BOTTOM_RIGHT);
            Vector3 pc = GetCornerPosition(Corner.TOP_LEFT);

            Vector3 va = pa - pe;
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(pe, va);

            Vector3 vb = pb - pe;
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(pe, vb);

            Vector3 vc = pc - pe;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(pe, vc);

            Dictionary<Corner, Vector3> camCornerRays = GetCameraCornerRays(reflectionCamera, reflectionCamera.farClipPlane);
            Plane plane = new Plane(pc, pb, pa);
            float rayDistance;
            Vector3 tl = Vector3.zero;
            Vector3 tr = Vector3.zero;
            Vector3 bl = Vector3.zero;
            Vector3 br = Vector3.zero;
            
            if (plane.Raycast(new Ray(pe, camCornerRays[Corner.TOP_LEFT]), out rayDistance))
            {
                tl = pe + camCornerRays[Corner.TOP_LEFT].normalized * rayDistance;
            }
            if (plane.Raycast(new Ray(pe, camCornerRays[Corner.TOP_RIGHT]), out rayDistance))
            {
                tr = pe + camCornerRays[Corner.TOP_RIGHT].normalized * rayDistance;
            }
            if (plane.Raycast(new Ray(pe, camCornerRays[Corner.BOTTOM_RIGHT]), out rayDistance))
            {
                br = pe + camCornerRays[Corner.BOTTOM_RIGHT].normalized * rayDistance;
            }
            if (plane.Raycast(new Ray(pe, camCornerRays[Corner.BOTTOM_LEFT]), out rayDistance))
            {
                bl = pe + camCornerRays[Corner.BOTTOM_LEFT].normalized * rayDistance;
            }

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(tl, tr);
            Gizmos.DrawLine(tr, br);
            Gizmos.DrawLine(br, bl);
            Gizmos.DrawLine(bl, tl);
        }

        #endregion
    }

}
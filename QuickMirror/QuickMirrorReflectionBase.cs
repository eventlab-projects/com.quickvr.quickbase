using UnityEngine;
using UnityEngine.XR;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

	/// <summary>
	/// Mirror Reflection Script for planar surfaces with dynamic shadows support. 
	/// \author Ramon Oliva
	/// </summary>
	 
	[ExecuteInEditMode] // Make mirror live-update even when not in play mode
	public abstract class QuickMirrorReflectionBase : MonoBehaviour {
		
		#region PUBLIC PARAMETERS

        public enum ReflectionQuality
        {
            HIGH = 1,
            GOOD = 2,
            FAIR = 4,
            POOR = 8,
        };

        public bool _disablePixelLights = false;							//Disable per pixel lighting on the reflection. Use this for performance boost. 

        public ShadowQuality _reflectionShadowType = ShadowQuality.All;	//Determines the most expensive type of shadows that will be used on the reflected image. 
        public RenderingPath _reflectedRenderingPath = RenderingPath.UsePlayerSettings;
		public ReflectionQuality _reflectionQuality = ReflectionQuality.HIGH;
        public float _reflectionDistance = 100.0f;

        public LayerMask _reflectLayers = -1;							//The layers that will be reflected on the mirror.

        public enum NormalDirection
        {
            Forward,
            Up,
            Right,
        }
        public NormalDirection _normalDirection = NormalDirection.Forward;

		#endregion
		
		#region PROTECTED PARAMETERS
		
		protected RenderTexture _reflectionTextureLeft = null;
        protected RenderTexture _reflectionTextureRight = null;

        protected ReflectionQuality _oldReflectionQuality = ReflectionQuality.HIGH;

        protected Camera _reflectionCamera = null;
		protected static bool _insideRendering = false;

		protected MeshFilter _mFilter;
        protected Renderer _renderer;

        protected static Queue<QuickMirrorReflectionBase> _mirrorQueue = new Queue<QuickMirrorReflectionBase>();

        protected bool _interleavedRendering = false;

        #endregion
		
		#region CREATION AND DESTRUCTION
        
		protected virtual void OnEnable() {
            //Create the mesh filter
			_mFilter = gameObject.GetOrCreateComponent<MeshFilter>();
            if (!_mFilter.sharedMesh) _mFilter.sharedMesh = QuickUtils.CreateFullScreenQuad();
            
            //Create the mesh renderer
            _renderer = gameObject.GetOrCreateComponent<MeshRenderer>();
            _renderer.receiveShadows = false;
                        
			gameObject.GetOrCreateComponent<BoxCollider>();
            
            //Ensure that the renderer has the reflection material. 
            string shaderName = GetShaderName();
            if (!_renderer.sharedMaterial || _renderer.sharedMaterial.shader.name != shaderName)
            {
                _renderer.sharedMaterial = new Material(Shader.Find(shaderName));
            }
        }

        protected virtual void OnDisable()
        {
            DestroyImmediate(_reflectionTextureLeft);
            DestroyImmediate(_reflectionTextureRight);
        }
		
		protected virtual void CreateReflectionTexture() {
			int texSize = GetTextureSize(_reflectionQuality);

            //Check the reflection texture for the left eye
            if (!_reflectionTextureLeft || _reflectionQuality != _oldReflectionQuality)
            {
                if (_reflectionTextureLeft) DestroyImmediate(_reflectionTextureLeft);
                _reflectionTextureLeft = CreateRenderTexture("__ReflectionTextureLeft__", texSize);
            }

            //Check the reflection texture for the right eye
            if (!_reflectionTextureRight || _reflectionQuality != _oldReflectionQuality)
            {
                if (_reflectionTextureRight) DestroyImmediate(_reflectionTextureRight);
                _reflectionTextureRight = CreateRenderTexture("__ReflectionTextureRight__", texSize);
            }

            _oldReflectionQuality = _reflectionQuality;
        }

        protected virtual RenderTexture CreateRenderTexture(string name, int size, RenderTextureFormat format = RenderTextureFormat.Default)
        {
            RenderTexture rTex = new RenderTexture(size, size, 24, format);
            rTex.name = name;
            rTex.isPowerOfTwo = true;
            rTex.hideFlags = HideFlags.DontSave;
            rTex.antiAliasing = 1;

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

            _reflectionCamera = transform.CreateChild("__MirrorReflectionCamera__").gameObject.GetOrCreateComponent<Camera>();
            //_reflectionCamera = new GameObject("__MirrorReflectionCamera__").AddComponent<Camera>();
            _reflectionCamera.gameObject.layer = LayerMask.NameToLayer("Water");
            //_reflectionCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
            _reflectionCamera.gameObject.GetOrCreateComponent<Skybox>();

            _reflectionCamera.enabled = false;
            _reflectionCamera.allowMSAA = false;

            _reflectionCamera.renderingPath = _reflectedRenderingPath;
        }

        #endregion

        #region GET AND SET

        protected abstract string GetShaderName();

        protected virtual Material GetMaterial()
        {
            return (Application.isPlaying) ? _renderer.material : _renderer.sharedMaterial;
        }

        public virtual Vector3 GetNormal()
        {
            if (_normalDirection == NormalDirection.Forward) return transform.forward;
            if (_normalDirection == NormalDirection.Right) return transform.right;
            return transform.up;
        }
		
		protected int GetTextureSize(ReflectionQuality quality)
        {
            return 2048 / (int)quality;
		}

        protected Vector3 GetReflectedPosition(Vector3 pos)
        {
            //Reflect camera around reflection plane
            Vector3 normal = transform.forward;
            Vector3 posToPlane = pos - transform.position;
            Vector3 reflectionPosToPlane = Vector3.Reflect(posToPlane, normal);

            return transform.position + reflectionPosToPlane;
        }

        protected bool IsEditorCamera(Camera cam)
        {
            return !Application.isPlaying || (Application.isPlaying && Application.isEditor && cam.name == "SceneCamera");
        }

        #endregion

        #region MIRROR RENDER

        protected virtual bool AllowRender() {
            return (Vector3.Distance(Camera.current.transform.position, transform.position) < _reflectionDistance);
        }

        protected virtual void AddToMirrorQueue()
        {
            _mirrorQueue.Enqueue(this);
        }

        // This is called when it's known that the object will be rendered by some
		// camera. We render reflections and do other updates here.
		// Because the script executes in edit mode, reflections for the scene view
		// camera will just work!
		protected virtual void OnWillRenderObject() {
            //Force the mirror to be in the Water layer, so it will avoid to be rendered by the reflection cameras
			gameObject.layer = LayerMask.NameToLayer("Water");

            if (AllowRender())
            {
                if (!_insideRendering && (_mirrorQueue.Count == 0 || _mirrorQueue.Peek() == this))
                {
                    if (_mirrorQueue.Count > 0) _mirrorQueue.Dequeue();

                    _insideRendering = true;
                    //StartCoroutine(CoUpdateInsideRendering());
                    
                    //Tune the quality settings for the reflected image
                    int oldPixelLightCount = QualitySettings.pixelLightCount;
                    if (_disablePixelLights) QualitySettings.pixelLightCount = 0;
                    ShadowQuality oldShadowQuality = QualitySettings.shadows;
                    QualitySettings.shadows = (ShadowQuality)(Mathf.Min((int)_reflectionShadowType, (int)oldShadowQuality));

                    CreateReflectionTexture();
                    CreateReflectionCamera();
                    RenderReflection();

                    _insideRendering = false;

                    // Restore the quality settings
                    QualitySettings.pixelLightCount = oldPixelLightCount;
                    QualitySettings.shadows = oldShadowQuality;
                }
                else if (!_mirrorQueue.Contains(this))
                {
                    //AddToMirrorQueue();
                }
            }
            else
            {
                ClearRenderTexture(_reflectionTextureLeft, Color.black);
                ClearRenderTexture(_reflectionTextureRight, Color.black);
            }
		}

        protected virtual IEnumerator CoUpdateInsideRendering()
        {
            _insideRendering = true;    // Safeguard from recursive reflections.        

            yield return null;
            //yield return new WaitForFixedUpdate();

            _insideRendering = false;
        }
		
		protected virtual void RenderReflection() {
            UpdateCameraModes();

            RenderVirtualImageStereo(_reflectionTextureLeft, _reflectionTextureRight);

            Material mat = GetMaterial();
            mat.SetTexture("_LeftEyeTexture", _reflectionTextureLeft);
            mat.SetTexture("_RightEyeTexture", _reflectionTextureRight);
        }
		
		protected virtual void RenderVirtualImageStereo(RenderTexture rtLeft, RenderTexture rtRight, bool mirrorStereo = true)
        {
            if (Camera.current.stereoEnabled)
            {
                float stereoSign = mirrorStereo ? -1.0f : 1.0f;
                float stereoSeparation = Vector3.Distance(InputTracking.GetLocalPosition(XRNode.LeftEye), InputTracking.GetLocalPosition(XRNode.RightEye));
                if (Camera.current.stereoTargetEye == StereoTargetEyeMask.Both || Camera.current.stereoTargetEye == StereoTargetEyeMask.Left)
                {
                    RenderVirtualImage(rtLeft, Camera.StereoscopicEye.Left, stereoSign * stereoSeparation * 0.5f);
                }

                if (Camera.current.stereoTargetEye == StereoTargetEyeMask.Both || Camera.current.stereoTargetEye == StereoTargetEyeMask.Right)
                {
                    RenderVirtualImage(rtRight, Camera.StereoscopicEye.Right, -stereoSign * stereoSeparation * 0.5f);
                }
            }
            else
            {
                RenderVirtualImage(rtLeft, Camera.StereoscopicEye.Left);
            }
        }

        protected virtual void UpdateCameraModes()
        {
            // set camera to clear the same way as current camera
            _reflectionCamera.cullingMask = ~(1 << LayerMask.NameToLayer("Water")) & ~(1 << LayerMask.NameToLayer("UI")) & _reflectLayers.value; // never render water layer
            _reflectionCamera.clearFlags = Camera.current.clearFlags;
            _reflectionCamera.backgroundColor = Camera.current.backgroundColor;
            if (Camera.current.clearFlags == CameraClearFlags.Skybox)
            {
                Skybox sky = Camera.current.GetComponent<Skybox>();
                Skybox mysky = _reflectionCamera.GetComponent<Skybox>();
                if (!sky || !sky.material)
                {
                    mysky.enabled = false;
                }
                else
                {
                    mysky.enabled = true;
                    mysky.material = sky.material;
                }
            }

            // update other values to match current camera.
            // even if we are supplying custom camera&projection matrices,
            // some of values are used elsewhere (e.g. skybox uses far plane)
            _reflectionCamera.nearClipPlane = Mathf.Max(Camera.current.nearClipPlane, 0.1f);
            _reflectionCamera.farClipPlane = Mathf.Min(Camera.current.farClipPlane, 1000.0f);
            _reflectionCamera.orthographic = Camera.current.orthographic;
            //_reflectionCamera.fieldOfView = Camera.current.fieldOfView;
            _reflectionCamera.aspect = Camera.current.aspect;
            _reflectionCamera.orthographicSize = Camera.current.orthographicSize;
        }

        protected abstract void RenderVirtualImage(RenderTexture targetTexture, Camera.StereoscopicEye eye, float stereoSeparation = 0.0f);

        #endregion

    }

}
using UnityEngine;
using UnityEngine.XR;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    /// <summary>
    /// Mirror Reflection Script for planar surfaces with dynamic shadows support. 
    /// \author Ramon Oliva
    /// </summary>

    [ExecuteInEditMode] // Make mirror live-update even when not in play mode
    public abstract class QuickMirrorReflectionBase : MonoBehaviour
    {

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

        public enum UpdateMode
        {
            Automatic,
            ByScript, 
        }
        public UpdateMode _updateMode = UpdateMode.Automatic;
        
        [Range(1.0f, 2.0f)]
        public float _reflectionScale = 1.0f;

        #endregion

        #region PROTECTED PARAMETERS

        protected RenderTexture _reflectionTextureLeft = null;
        protected RenderTexture _reflectionTextureRight = null;
        protected RenderTexture _bakedReflectionTextureLeft = null;
        protected RenderTexture _bakedReflectionTextureRight = null;

        protected ReflectionQuality _oldReflectionQuality = ReflectionQuality.HIGH;

        protected Camera _currentCamera = null;
        protected Vector3 _currentCameraPosBaked = Vector3.zero;
        protected Camera _reflectionCamera = null;
        protected static bool _insideRendering = false;

        protected MeshFilter _mFilter;
        protected Renderer _renderer;

        protected bool _requestRenderGeometryBaked = false;
        protected bool _requestRenderGeometryDefault = false;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            CreateReflectionTexture();
            CreateReflectionCamera();
            StartCoroutine(CoResetRenderRequest());
        }

        protected virtual void OnEnable()
        {
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

            QuickMirrorReflectionManager.AddMirror(this);
        }

        protected virtual void OnDisable()
        {
            DestroyImmediate(_reflectionTextureLeft);
            DestroyImmediate(_reflectionTextureRight);
            DestroyImmediate(_bakedReflectionTextureLeft);
            DestroyImmediate(_bakedReflectionTextureRight);

            QuickMirrorReflectionManager.RemoveMirror(this);
        }

        protected virtual void CreateReflectionTexture()
        {
            int texSize = GetTextureSize(_reflectionQuality);

            //Check the reflection texture for the left eye
            CreateRenderTexture(ref _reflectionTextureLeft, "__ReflectionTextureLeft__", texSize);
            CreateRenderTexture(ref _reflectionTextureRight, "__ReflectionTextureRight__", texSize);
            CreateRenderTexture(ref _bakedReflectionTextureLeft, "__BakedReflectionTextureLeft__", texSize);
            CreateRenderTexture(ref _bakedReflectionTextureRight, "__BakedReflectionTextureRight__", texSize);

            _oldReflectionQuality = _reflectionQuality;
        }

        protected virtual void CreateRenderTexture(ref RenderTexture result, string name, int size, RenderTextureFormat format = RenderTextureFormat.Default)
        {
            if (!result || _reflectionQuality != _oldReflectionQuality)
            {
                if (result) DestroyImmediate(result);

                result = new RenderTexture(size, size, 16, format);
                result.name = name;
                result.isPowerOfTwo = true;
                result.hideFlags = HideFlags.DontSave;
                result.antiAliasing = 2;
            }
        }

        protected virtual void ClearRenderTexture(RenderTexture rTex, Color bgColor)
        {
            RenderTexture oldRenderTex = RenderTexture.active;
            RenderTexture.active = rTex;
            GL.Clear(true, true, bgColor);
            RenderTexture.active = oldRenderTex;
        }

        protected virtual void CreateReflectionCamera()
        {
            //Create the camera
            if (!_reflectionCamera)
            {
                _reflectionCamera = transform.CreateChild(QuickMirrorReflectionManager.MIRROR_CAMERA_NAME).gameObject.GetOrCreateComponent<Camera>();
                //_reflectionCamera = new GameObject("__MirrorReflectionCamera__").AddComponent<Camera>();
                _reflectionCamera.gameObject.layer = LayerMask.NameToLayer("Water");
                //_reflectionCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
                _reflectionCamera.gameObject.GetOrCreateComponent<Skybox>();

                _reflectionCamera.allowMSAA = true;
            }

            _reflectionCamera.name = QuickMirrorReflectionManager.MIRROR_CAMERA_NAME;
            _reflectionCamera.enabled = false;
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

        protected virtual bool AllowRender()
        {
            return (Vector3.Distance(_currentCamera.transform.position, transform.position) < _reflectionDistance);
        }

        //protected virtual void OnWillRenderObject()
        //{
        //    BeginCameraRendering(Camera.current);
        //}

        public virtual void BeginCameraRendering(Camera cam)
        {
            if (cam == _reflectionCamera) return;

            _currentCamera = cam;
            //Force the mirror to be in the Water layer, so it will avoid to be rendered by the reflection cameras
            gameObject.layer = LayerMask.NameToLayer("Water");

            if (AllowRender())
            {
                if (!_insideRendering)
                {
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
            }
            else
            {
                ClearRenderTexture(_reflectionTextureLeft, Color.red);
                ClearRenderTexture(_reflectionTextureRight, Color.red);
            }
        }

        protected virtual IEnumerator CoUpdateInsideRendering()
        {
            _insideRendering = true;    // Safeguard from recursive reflections.        

            yield return null;
            //yield return new WaitForFixedUpdate();

            _insideRendering = false;
        }

        public virtual void RenderGeometryBaked()
        {
            _requestRenderGeometryBaked = true;
        }

        public virtual void RenderGeometryDefault()
        {
            _requestRenderGeometryDefault = true;
        }

        protected virtual void RenderReflection()
        {
            UpdateCameraModes();

            Material mat = GetMaterial();

            if (_updateMode == UpdateMode.Automatic || _requestRenderGeometryBaked)
            {
                //Render the baked geometry
                _currentCameraPosBaked = _currentCamera.transform.position;
                _reflectionCamera.cullingMask = (1 << LayerMask.NameToLayer("Water"));
                _reflectionCamera.clearFlags = _currentCamera.clearFlags;
                _reflectionCamera.backgroundColor = _currentCamera.backgroundColor;
                RenderVirtualImageStereo(_bakedReflectionTextureLeft, _bakedReflectionTextureRight);
                mat.SetTexture("_LeftEyeBakedTexture", _bakedReflectionTextureLeft);
                mat.SetTexture("_RightEyeBakedTexture", _bakedReflectionTextureRight);
                UpdateReflectionUV();
            }

            if (_updateMode == UpdateMode.Automatic || _requestRenderGeometryDefault)
            {
                //Render the dynamic geometry
                _reflectionCamera.cullingMask = ~(1 << LayerMask.NameToLayer("Water")) & ~(1 << LayerMask.NameToLayer("UI")) & _reflectLayers.value; // never render water layer
                _reflectionCamera.clearFlags = CameraClearFlags.SolidColor;
                _reflectionCamera.backgroundColor = Color.clear;
                RenderVirtualImageStereo(_reflectionTextureLeft, _reflectionTextureRight);

                mat.SetTexture("_LeftEyeTexture", _reflectionTextureLeft);
                mat.SetTexture("_RightEyeTexture", _reflectionTextureRight);
            }
        }

        protected virtual void UpdateReflectionUV()
        {
            Vector3 realSize = _mFilter.sharedMesh.bounds.size;
            Vector3 totalSize = _mFilter.sharedMesh.bounds.size * _reflectionScale;
            Vector3 maxOffset = (totalSize - realSize) * 0.5f;

            Vector3 c0 = transform.position + Vector3.ProjectOnPlane(_currentCameraPosBaked - transform.position, transform.forward);
            Vector3 c1 = transform.position + Vector3.ProjectOnPlane(_currentCamera.transform.position - transform.position, transform.forward);
            Vector3 cOffset = c1 - c0;
            cOffset = new Vector3
                (
                Mathf.Sign(cOffset.x) * Mathf.Min(Mathf.Abs(cOffset.x), maxOffset.x),
                Mathf.Sign(cOffset.y) * Mathf.Min(Mathf.Abs(cOffset.y), maxOffset.y),
                0
                );

            float t = 1.0f / _reflectionScale;
            Vector2 uvOffset = new Vector2(cOffset.x / totalSize.x, cOffset.y / totalSize.y);
            Vector2 uvCenter = new Vector2(0.5f, 0.5f) + uvOffset;
            _mFilter.sharedMesh.uv = new Vector2[]
            {
                Vector2.Lerp(uvCenter, new Vector2(0, 0), t),
                Vector2.Lerp(uvCenter, new Vector2(1, 0), t),
                Vector2.Lerp(uvCenter, new Vector2(1, 1), t),
                Vector2.Lerp(uvCenter, new Vector2(0, 1), t),
            };
        }

        protected virtual void RenderVirtualImageStereo(RenderTexture rtLeft, RenderTexture rtRight, bool mirrorStereo = true)
        {
            float stereoSign = mirrorStereo ? -1.0f : 1.0f;
            float stereoSeparation = stereoSign * _currentCamera.stereoSeparation * 0.5f;
            if (_currentCamera.stereoTargetEye == StereoTargetEyeMask.Both || _currentCamera.stereoTargetEye == StereoTargetEyeMask.Left)
            {
                RenderVirtualImage(rtLeft, Camera.StereoscopicEye.Left, stereoSeparation);
            }
            if (_currentCamera.stereoTargetEye == StereoTargetEyeMask.Both || _currentCamera.stereoTargetEye == StereoTargetEyeMask.Right)
            {
                RenderVirtualImage(rtRight, Camera.StereoscopicEye.Right, -stereoSeparation);
            }
        }

        protected virtual void UpdateCameraModes()
        {
            // set camera to clear the same way as current camera
            if (_currentCamera.clearFlags == CameraClearFlags.Skybox)
            {
                Skybox sky = _currentCamera.GetComponent<Skybox>();
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
            _reflectionCamera.nearClipPlane = Mathf.Max(_currentCamera.nearClipPlane, 0.1f);
            _reflectionCamera.farClipPlane = Mathf.Min(_currentCamera.farClipPlane, 1000.0f);
            _reflectionCamera.orthographic = _currentCamera.orthographic;
            //_reflectionCamera.fieldOfView = _currentCamera.fieldOfView;
            _reflectionCamera.aspect = _currentCamera.aspect;
            _reflectionCamera.orthographicSize = _currentCamera.orthographicSize;
        }

        protected abstract void RenderVirtualImage(RenderTexture targetTexture, Camera.StereoscopicEye eye, float stereoSeparation = 0.0f);

        protected virtual void OnDrawGizmos()
        {
            if (_currentCamera && _reflectionCamera)
            {
                Gizmos.color = Color.red;
                Gizmos.matrix = Matrix4x4.TRS(_currentCamera.transform.position, _currentCamera.transform.rotation, Vector3.one);
                Gizmos.DrawFrustum(Vector3.zero, _currentCamera.fieldOfView, _currentCamera.farClipPlane, _currentCamera.nearClipPlane, _currentCamera.aspect);

                Gizmos.color = Color.blue;
                Gizmos.matrix = Matrix4x4.TRS(_reflectionCamera.transform.position, _reflectionCamera.transform.rotation, Vector3.one);
                Gizmos.DrawFrustum(Vector3.zero, _reflectionCamera.fieldOfView, _reflectionCamera.farClipPlane, _reflectionCamera.nearClipPlane, _reflectionCamera.aspect);
            }

        }

        protected virtual IEnumerator CoResetRenderRequest()
        {
            yield return new WaitForEndOfFrame();

            _requestRenderGeometryBaked = false;
            _requestRenderGeometryDefault = false;
        }

        #endregion

    }

}
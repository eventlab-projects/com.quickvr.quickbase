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

        protected ReflectionQuality _oldReflectionQuality = ReflectionQuality.HIGH;

        protected Camera _currentCamera = null;
        protected Vector3 _currentCameraPosBaked = Vector3.zero;
        protected Camera _reflectionCamera = null;
        protected static bool _insideRendering = false;

        protected MeshFilter _mFilter;
        protected Renderer _renderer;

        protected bool _requestRender = false;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
            CreateReflectionTexture();
            _reflectionCamera = CreateReflectionCamera(QuickMirrorReflectionManager.MIRROR_CAMERA_NAME);
        }

        protected virtual void OnEnable()
        {
            //Create the mesh filter
            _mFilter = gameObject.GetOrCreateComponent<MeshFilter>();
            if (!_mFilter.sharedMesh) _mFilter.sharedMesh = QuickUtils.CreateFullScreenQuad();

            //Create the mesh renderer
            _renderer = gameObject.GetOrCreateComponent<MeshRenderer>();
            _renderer.receiveShadows = false;

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
            
            QuickMirrorReflectionManager.RemoveMirror(this);
        }

        protected virtual void CreateReflectionTexture()
        {
            //Check the reflection texture for the left eye
            CreateRenderTexture(ref _reflectionTextureLeft, "__ReflectionTextureLeft__");
            CreateRenderTexture(ref _reflectionTextureRight, "__ReflectionTextureRight__");
            
            _oldReflectionQuality = _reflectionQuality;
        }

        protected virtual void CreateRenderTexture(ref RenderTexture result, string name, RenderTextureFormat format = RenderTextureFormat.Default)
        {
            if (!result || _reflectionQuality != _oldReflectionQuality)
            {
                if (result) DestroyImmediate(result);

                int size = GetTextureSize(_reflectionQuality);
                result = new RenderTexture(size, size, 16, format);
                result.name = name;
                result.isPowerOfTwo = true;
                result.hideFlags = HideFlags.DontSave;
#if UNITY_WEBGL
                result.antiAliasing = 1;
#else
                result.antiAliasing = 2;
#endif
            }
        }

        protected virtual void ClearRenderTexture(RenderTexture rTex, Color bgColor)
        {
            RenderTexture oldRenderTex = RenderTexture.active;
            RenderTexture.active = rTex;
            GL.Clear(true, true, bgColor);
            RenderTexture.active = oldRenderTex;
        }

        protected virtual Camera CreateReflectionCamera(string cameraName)
        {
            //Create the camera
            Camera result = transform.CreateChild(cameraName).gameObject.GetOrCreateComponent<Camera>();
            result.gameObject.layer = LayerMask.NameToLayer("Water");
            //_reflectionCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;

            result.enabled = false;
            result.renderingPath = _reflectedRenderingPath;

#if UNITY_WEBGL
            result.allowMSAA = false;
            result.allowHDR = false;
#else
            result.allowMSAA = true;
            result.allowHDR = true;
#endif

            return result;
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
            return ((_currentCamera.transform.position - transform.position).sqrMagnitude < _reflectionDistance * _reflectionDistance);
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
                    //Tune the quality settings for the reflected image
                    int oldPixelLightCount = QualitySettings.pixelLightCount;
                    if (_disablePixelLights) QualitySettings.pixelLightCount = 0;
                    ShadowQuality oldShadowQuality = QualitySettings.shadows;
                    QualitySettings.shadows = (ShadowQuality)(Mathf.Min((int)_reflectionShadowType, (int)oldShadowQuality));

                    CreateReflectionTexture();
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

        public virtual void RequestRender()
        {
            _requestRender = true;
        }

        protected virtual void RenderReflection()
        {
            UpdateCameraModes();

            Material mat = GetMaterial();

            if (_updateMode == UpdateMode.Automatic || _requestRender)
            {
                _reflectionCamera.cullingMask = ~(1 << LayerMask.NameToLayer("Water")) & ~(1 << LayerMask.NameToLayer("UI")) & _reflectLayers.value; // never render water layer
                RenderVirtualImageStereo();

                mat.SetTexture("_LeftEyeTexture", _reflectionTextureLeft);
                mat.SetTexture("_RightEyeTexture", _reflectionTextureRight);

                _requestRender = false;
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

        protected virtual void RenderVirtualImageStereo(bool mirrorStereo = true)
        {
            float stereoSign = mirrorStereo ? -1.0f : 1.0f;
            float stereoSeparation = stereoSign * _currentCamera.stereoSeparation * 0.5f;
            if (_currentCamera.stereoTargetEye == StereoTargetEyeMask.Both || _currentCamera.stereoTargetEye == StereoTargetEyeMask.Left)
            {
                //_reflectionCamera.SetTargetBuffers(_reflectionTextureLeft.colorBuffer, _reflectionTextureLeft.depthBuffer);
                _reflectionCamera.targetTexture = _reflectionTextureLeft;
                RenderVirtualImage(Camera.StereoscopicEye.Left, stereoSeparation);
            }
            if (_currentCamera.stereoTargetEye == StereoTargetEyeMask.Both || _currentCamera.stereoTargetEye == StereoTargetEyeMask.Right)
            {
                //_reflectionCamera.SetTargetBuffers(_reflectionTextureRight.colorBuffer, _reflectionTextureRight.depthBuffer);
                _reflectionCamera.targetTexture = _reflectionTextureRight;
                RenderVirtualImage(Camera.StereoscopicEye.Right, -stereoSeparation);
            }
        }

        protected virtual void UpdateCameraModes()
        {
            _reflectionCamera.clearFlags = _currentCamera.clearFlags;
            _reflectionCamera.backgroundColor = _currentCamera.backgroundColor;
            // update other values to match current camera.
            // even if we are supplying custom camera&projection matrices,
            // some of values are used elsewhere (e.g. skybox uses far plane)
            _reflectionCamera.nearClipPlane = Mathf.Max(_currentCamera.nearClipPlane, 0.1f);
            _reflectionCamera.farClipPlane = Mathf.Min(_currentCamera.farClipPlane, _reflectionDistance);
            _reflectionCamera.orthographic = _currentCamera.orthographic;
            //_reflectionCamera.fieldOfView = _currentCamera.fieldOfView;
            _reflectionCamera.aspect = _currentCamera.aspect;
            _reflectionCamera.orthographicSize = _currentCamera.orthographicSize;
        }

        protected abstract void RenderVirtualImage(Camera.StereoscopicEye eye, float stereoSeparation = 0.0f);

        //protected virtual void OnDrawGizmos()
        //{
        //    if (_currentCamera && _reflectionCamera)
        //    {
        //        Gizmos.color = Color.red;
        //        Gizmos.matrix = Matrix4x4.TRS(_currentCamera.transform.position, _currentCamera.transform.rotation, Vector3.one);
        //        Gizmos.DrawFrustum(Vector3.zero, _currentCamera.fieldOfView, _currentCamera.farClipPlane, _currentCamera.nearClipPlane, _currentCamera.aspect);

        //        Gizmos.color = Color.blue;
        //        Gizmos.matrix = Matrix4x4.TRS(_reflectionCamera.transform.position, _reflectionCamera.transform.rotation, Vector3.one);
        //        Gizmos.DrawFrustum(Vector3.zero, _reflectionCamera.fieldOfView, _reflectionCamera.farClipPlane, _reflectionCamera.nearClipPlane, _reflectionCamera.aspect);
        //    }

        //}

#endregion

    }

}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class QuickIEGlow : MonoBehaviour {
	
	#region PUBLIC PARAMETERS
	
	public Shader _glowingMaskShader = null;
	public Shader _blurShader = null;
	public Shader _expandShader = null;
	public Shader _compositeShader = null;

    #endregion

    #region PROTECTED PARAMETERS 

    protected Camera _camera = null;
	[SerializeField] protected Camera _shaderCamera = null;
	
	protected RenderTexture _glowingMask = null;		//Contains the glowing objects visible from the camera
	protected RenderTexture _blurX = null;
	protected RenderTexture _blurY = null;
	
	protected Material _blurMaterial = null;
	protected Material _expandMaterial = null;
	protected Material _compositeMaterial = null;
	
	#endregion
	
	#region CONSTANTS
	
	protected const string GLOWING_MASK_CAMERA_NAME = "__GlowingMaskCamera__";
	
	#endregion
	
	#region CREATION AND DESTRUCTION
	
	protected virtual void Start() {
        _camera = GetComponent<Camera>();
        if (!_camera) Debug.LogWarning("Image Effect Script requires a camera attached to this GameObject");

        CreateShaderObjects();
	}
	
	protected virtual void CreateShaderObjects() {
		if (!_glowingMaskShader) _glowingMaskShader = Shader.Find("Hidden/GlowingMask");
		if (!_blurShader) _blurShader = Shader.Find("Hidden/Blur_9");
		if (!_expandShader) _expandShader = Shader.Find("Hidden/Expand");
		if (!_compositeShader) _compositeShader = Shader.Find("Hidden/GlowingThingsComposer");

		_blurMaterial = new Material(_blurShader);
		_expandMaterial = new Material(_expandShader);
		_compositeMaterial = new Material(_compositeShader);
		
		_shaderCamera = new GameObject(GLOWING_MASK_CAMERA_NAME).AddComponent<Camera>(); 
		_shaderCamera.enabled = false;
		_shaderCamera.gameObject.hideFlags = HideFlags.DontSave;
	}
	
	protected virtual void OnDestroy() {
		if (_blurMaterial) DestroyImmediate(_blurMaterial);
		if (_expandMaterial) DestroyImmediate(_expandMaterial);
		if (_compositeMaterial) DestroyImmediate(_compositeMaterial);
		
		if (_shaderCamera) DestroyImmediate(_shaderCamera.gameObject);
		ReleaseRenderTextures();
	}
	
	protected virtual void CreateRenderTextures() {
		//The glowing mask is set to a 1/4 of the camera's viewport resolution
        Vector2 size = new Vector2(_camera.pixelWidth, _camera.pixelHeight) * 0.25f;
		if (!_glowingMask) _glowingMask = RenderTexture.GetTemporary((int)size.x, (int)size.y, 16);	
		if (!_blurX) _blurX = RenderTexture.GetTemporary((int)size.x, (int)size.y, 0, RenderTextureFormat.ARGBHalf);
		if (!_blurY) _blurY = RenderTexture.GetTemporary((int)size.x, (int)size.y, 0, RenderTextureFormat.ARGBHalf);
	}

    protected virtual void ReleaseRenderTextures() {
		if (_glowingMask) {
			RenderTexture.ReleaseTemporary(_glowingMask);
			_glowingMask = null;
		}
		if (_blurX) {
			RenderTexture.ReleaseTemporary(_blurX);
			_blurX = null;
		}
		if (_blurY) {
			RenderTexture.ReleaseTemporary(_blurY);
			_blurY = null;
		}
	}

	#endregion
	
	#region GET AND SET
	
	protected virtual void ClearRenderTexture(RenderTexture rTex, Color bgColor) {
		RenderTexture current = RenderTexture.active;
		RenderTexture.active = rTex;
		GL.Clear(true, true, bgColor);
		RenderTexture.active = current;
	}
	
	protected virtual void ResetShaderCamera() {
		_shaderCamera.CopyFrom(GetComponent<Camera>());
		_shaderCamera.cullingMask = _shaderCamera.cullingMask & ~(1 << LayerMask.NameToLayer("Water")) & ~(1 << LayerMask.NameToLayer("Terrain"));
		_shaderCamera.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
		_shaderCamera.clearFlags = CameraClearFlags.SolidColor;
		_shaderCamera.depthTextureMode = DepthTextureMode.None;
		//Force the rendering path of this camera to be forward, as it does not propperly works on 
		//deferred rendering due to some Unity mistery...
		_shaderCamera.renderingPath = RenderingPath.Forward;
	}
	
	#endregion
	
	#region ON RENDER IMAGE
	
	protected virtual void OnRenderImage (RenderTexture source, RenderTexture destination) {
		if (!_shaderCamera) return;

        //Source contains the image rendered by this camera, i.e., the result of rendering the whole scene. 
		//Destination will contain the final result with the glowing post process effect applied. 
		CreateRenderTextures();
		
		ResetShaderCamera();
		RenderGlowingMask();
        BlurGlowingMask();
        ComposeScene(source, destination);
		
		ReleaseRenderTextures();
	}
	
	protected virtual void RenderGlowingMask() {
		_shaderCamera.targetTexture = _glowingMask;
		_shaderCamera.RenderWithShader(_glowingMaskShader, "RenderType");
	}
	
	protected virtual void BlurGlowingMask() {
		//Blur the _glowingMask horizontally
		_blurMaterial.SetInt("_BlurDirection", 0);
		Graphics.Blit(_glowingMask, _blurX, _blurMaterial);
		
		//Blur vertically
		_blurMaterial.SetInt("_BlurDirection", 1);
		Graphics.Blit(_blurX, _blurY, _blurMaterial);
	}
	
	protected virtual void ComposeScene(RenderTexture source, RenderTexture dest) {
        //Compose the source (the image containing the render of the whole scene) with the glowing objects
        Graphics.Blit(_blurY, source, _compositeMaterial);
        Graphics.Blit(source, dest);

        //Graphics.Blit(_glowingMask, dest);
    }

    #endregion
}
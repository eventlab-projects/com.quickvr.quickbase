using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GlowingThing : MonoBehaviour {

	#region PUBLIC PARAMETERS

	public Color _glowColor = Color.white;

	public float _minGlowIntensity = 0.25f; //1.0f;
	public float _maxGlowIntensity = 3.0f; //1.0f;
	public float _glowSpeed = 5.0f; //1.0f;

	#endregion

	#region PROTECTED PARAMETERS

	protected float _currentGlowIntensity = 0.0f;
	protected Material _glowingMaterial = null;

	protected Color _oldColor;
	protected float _oldMinGlowIntensity;
	protected float _oldMaxGlowIntensity;
	protected float _oldGlowSpeed;

	#endregion

	#region CREATION AND DESTRUCTION

	protected virtual void Start () {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
		
		bool glow = false;
		foreach(Renderer r in renderers)
		{
			foreach (Material mat in r.materials) {
				if (mat.GetTag("RenderType", true) == "Glowing") {
					glow = true;
					_glowingMaterial = mat;
					break;
				}
			}
		}

		if (!glow) {
            Debug.LogWarning("NO GLOWING MATERIAL FOUND!!!");
            enabled = false;
		}
		SaveParameters();
	}

	protected virtual void OnDestroy() {
		if (_glowingMaterial) DestroyImmediate(_glowingMaterial);
	}

	protected virtual void OnDisable() {
		if (_glowingMaterial) {
			_glowingMaterial.SetFloat("_GlowIntensity", 0.0f);
		}
	}

	#endregion

	#region GET AND SET

	public virtual void SaveParameters() {
		_oldColor = _glowColor;
		_oldMinGlowIntensity = _minGlowIntensity;
		_oldMaxGlowIntensity = _maxGlowIntensity;
		_oldGlowSpeed = _glowSpeed;
	}

	public virtual void LoadParameters() {
		_glowColor = _oldColor;
		_minGlowIntensity = _oldMinGlowIntensity;
		_maxGlowIntensity = _oldMaxGlowIntensity;
		_glowSpeed = _oldGlowSpeed;
	}

	#endregion

	#region UPDATE

	protected virtual void LateUpdate() {
		float sin = Mathf.Sin(Time.time * _glowSpeed);

		//-1..1 => 0..1
		float t = (sin + 1.0f) * 0.5f;
		_currentGlowIntensity = Mathf.Lerp(_minGlowIntensity, _maxGlowIntensity, t);
		_glowingMaterial.SetColor("_GlowColor", _glowColor);
		_glowingMaterial.SetFloat("_GlowIntensity", _currentGlowIntensity);
	}

	#endregion
	
}

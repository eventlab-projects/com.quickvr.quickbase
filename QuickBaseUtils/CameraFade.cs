using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace QuickVR {

    /// <summary>
    /// Camera fade. Simple fading script. A texture is stretched over the entire screen. The color of the pixel is set each
    /// frame until it reaches its target color.
    /// </summary>
    public class CameraFade : MonoBehaviour
    {

	    #region PROTECTED PARAMETERS

	    protected bool _isFading = false;
        protected Material _material = null;
	    
	    #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void Awake()
        {
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

            SetColor(Color.clear);
        }

        protected virtual void Start()
        {
            transform.parent = Camera.main.transform;
            transform.localPosition = Vector3.forward * 0.75f;
            transform.localRotation = Quaternion.identity;
            transform.Rotate(transform.up, 180.0f, Space.World);
        }

        #endregion

        #region GET AND SET

        public virtual bool IsFading() {
		    return _isFading;
	    }

	    public virtual void SetColor(Color color) {
            if (_isFading) StopAllCoroutines();
            _material.color = color;
	    }

        public virtual void SetTexture(Texture tex)
        {
            _material.mainTexture = tex;
        }

        public virtual Texture GetTexture()
        {
            return _material.mainTexture;
        }

	    public virtual void StartFade(Color toColor, float fadeTime) {
            StartFade(_material.color, toColor, fadeTime);
	    }

	    public virtual void StartFade(Color fromColor, Color toColor, float fadeTime) {
		    if (_isFading) StopAllCoroutines();
            StartCoroutine(CoFade(fromColor, toColor, fadeTime));
	    }

        public virtual void FadeOut(float fadeTime)
        {
            StartFade(Color.black, fadeTime);
        }

        public virtual void FadeIn(float fadeTime)
        {
            StartFade(Color.clear, fadeTime);
        }

	    #endregion
    
	    #region UPDATE

        protected virtual IEnumerator CoFade(Color fromColor, Color toColor, float fadeTime)
        {
            _isFading = true;
            float elapsedTime = 0.0f;

            _material.color = fromColor;
            while (elapsedTime < fadeTime)
            {
                float t = elapsedTime / fadeTime;
                _material.color = Color.Lerp(fromColor, toColor, t);
                elapsedTime += Time.deltaTime;

                yield return null;
            }
            _material.color = toColor;

            _isFading = false;
        }

	    #endregion

    }

}

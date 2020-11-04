using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuickVR
{

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class QuickMirrorReflectionManager
    {

        public const string MIRROR_CAMERA_NAME = "__MirrorReflectionCamera__";
        public const string REFLECTION_INVERT_Y = "REFLECTION_INVERT_Y";

        #region PRIVATE ATTRIBUTES

        private static HashSet<QuickMirrorReflectionBase> _mirrors = new HashSet<QuickMirrorReflectionBase>();
        //private static float _timeLastRender = 0;

        #endregion

        #region CREATION AND DESTRUCTION

        static QuickMirrorReflectionManager()
        {
#if UNITY_EDITOR
            UnityEditor.SceneView.beforeSceneGui += UpdateMirrorsSceneView;
#endif
            //QuickVRManager.OnPostCameraUpdate += UpdateMirrorsMainCamera;
            Camera.onPreCull += UpdateMirrors;
            //Application.onBeforeRender += AllowRenderReflection;
        }

        #endregion

        #region GET AND SET

        public static void AddMirror(QuickMirrorReflectionBase mirror)
        {
            _mirrors.Add(mirror);
        }

        public static void RemoveMirror(QuickMirrorReflectionBase mirror)
        {
            _mirrors.Remove(mirror);
        }

        #endregion

        #region UPDATE

#if UNITY_EDITOR
        static void UpdateMirrorsSceneView(UnityEditor.SceneView sView)
        {
            if (Application.isPlaying) return;

            UpdateMirrors(sView.camera);
        }
#endif

        static void UpdateMirrors(Camera cam)
        {
            if (cam.enabled)
            {
                foreach (QuickMirrorReflectionBase mirror in _mirrors)
                {
                    mirror.BeginCameraRendering(cam);
                }
            }
        }

        static void UpdateMirrorsMainCamera()
        {
            foreach (Camera cam in Camera.allCameras)
            {
                UpdateMirrors(cam);
            }
        }

        static void AllowRenderReflection()
        {
            //if (_renderReflection)
            //{
            //    _renderReflection = false;
            //}

            //if (Time.time - _timeLastRender > (1.0f / 30.0f))
            //{
            //    _renderReflection = true;
            //    _timeLastRender = Time.time;
            //}
            //_renderReflection = true;

            //if (_renderStaticGeometry)
            //{
            //    _renderStaticGeometry = false;
            //}

            //if (Time.time - _timeLastRender > 10)
            //{
            //    Debug.Log("RENDER STATIC GEOMETRY!!!");
            //    _renderStaticGeometry = true;
            //    _timeLastRender = Time.time;
            //}
        }

        #endregion
    }

}


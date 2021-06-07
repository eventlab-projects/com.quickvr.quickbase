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
            Camera.onPreCull += UpdateMirrors;
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

        private static bool IsValidCamera(Camera cam)
        {

#if UNITY_EDITOR
            UnityEditor.SceneView sView = UnityEditor.SceneView.currentDrawingSceneView;
            if (sView && sView.camera == cam)
            {
                return true;
            }
#endif

            return cam.enabled;
        }

        #endregion

        #region UPDATE

        static void UpdateMirrors(Camera cam)
        {
            if (IsValidCamera(cam))
            {
                foreach (QuickMirrorReflectionBase mirror in _mirrors)
                {
                    mirror.BeginCameraRendering(cam);
                }
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


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

        #endregion

        #region CREATION AND DESTRUCTION

        static QuickMirrorReflectionManager()
        {
#if UNITY_EDITOR
            UnityEditor.SceneView.beforeSceneGui += UpdateMirrorsSceneView;
#endif
            QuickVRManager.OnPostCameraUpdate += UpdateMirrorsMainCamera;
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
            foreach (QuickMirrorReflectionBase mirror in _mirrors)
            {
                mirror.BeginCameraRendering(cam);
            }
        }

        static void UpdateMirrorsMainCamera()
        {
            foreach (Camera cam in Camera.allCameras)
            {
                UpdateMirrors(cam);
            }
        }

        #endregion
    }

}


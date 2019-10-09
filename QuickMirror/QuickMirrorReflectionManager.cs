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

        #region PRIVATE ATTRIBUTES

        private static HashSet<QuickMirrorReflectionBase> _mirrors = new HashSet<QuickMirrorReflectionBase>();

        #endregion

        #region CREATION AND DESTRUCTION

        static QuickMirrorReflectionManager()
        {
            QuickSingletonManager.GetInstance<QuickVRManager>();
#if UNITY_EDITOR
            UnityEditor.SceneView.beforeSceneGui += UpdateMirrorsSceneView;
#endif
            QuickVRManager.OnPostUpdateTracking += UpdateMirrors;
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

            foreach (QuickMirrorReflectionBase mirror in _mirrors)
            {
                mirror.BeginCameraRendering(sView.camera);
            }
        }
#endif

        static void UpdateMirrors()
        {
            foreach (QuickMirrorReflectionBase mirror in _mirrors)
            {
                foreach (Camera cam in Camera.allCameras)
                {
                    mirror.BeginCameraRendering(cam);
                }
            }
        }

        #endregion
    }

}


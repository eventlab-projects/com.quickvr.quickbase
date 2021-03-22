using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    public class QuickSceneManager : MonoBehaviour
    {

        #region PROTECTED ATTRIBUTES

        protected static HashSet<string> _loadedScenes = new HashSet<string>();

        #endregion

        #region GET AND SET

        public virtual bool IsValidScene(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName).IsValid();
        }

        public virtual void LoadScenesAdditive(string[] sceneNames)
        {
            foreach (string s in sceneNames)
            {
                LoadSceneAdditive(s);
            }
        }

        public virtual void LoadSceneAdditive(string sceneName)
        {
            if (!_loadedScenes.Contains(sceneName))
            {
                SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
                _loadedScenes.Add(sceneName);
            }
        }

        public virtual void LoadScenesAdditiveAsync(string[] sceneNames)
        {
            foreach (string s in sceneNames)
            {
                LoadSceneAdditiveAsync(s, false);
            }
        }

        public virtual void LoadSceneAdditiveAsync(string sceneName, bool allowSceneActivation)
        {
            if (!_loadedScenes.Contains(sceneName))
            {
                StartCoroutine(CoLoadSceneAdditiveAsync(sceneName, allowSceneActivation));
                _loadedScenes.Add(sceneName);
            }
        }

        public virtual void UnloadScenesAsync(string[] sceneNames)
        {
            foreach (string s in sceneNames)
            {
                UnloadSceneAsync(s);
            }
        }

        public virtual void UnloadSceneAsync(string sceneName)
        {
            if (IsValidScene(sceneName))
            {
                StartCoroutine(CoUnloadSceneAsync(sceneName));
            }
        }

        public virtual void ActivateScene(string sceneName, bool disableCameras = true)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid())
            {
                StartCoroutine(CoActivateScene(scene, disableCameras));
            }
        }

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoLoadSceneAdditiveAsync(string sceneName, bool allowSceneActivation)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (allowSceneActivation)
            {
                ActivateScene(sceneName);
            }
        }

        protected virtual IEnumerator CoUnloadSceneAsync(string sceneName)
        {
            yield return SceneManager.UnloadSceneAsync(sceneName);

            _loadedScenes.Remove(sceneName);
        }

        protected virtual IEnumerator CoActivateScene(Scene scene, bool disableCameras)
        {
            //Wait for the scene to be loaded, in case we are trying to activate a scene 
            //that is not loaded yet. 
            while (!scene.isLoaded)
            {
                yield return null;
            }

            //At this point, the scene is loaded. Activate it. 
            SceneManager.SetActiveScene(scene);
            LightProbes.Tetrahedralize();

            if (disableCameras)
            {
                //Deactivate any present camera in the geometry scene 
                foreach (Camera cam in Camera.allCameras)
                {
                    if (cam.tag != "MainCamera")
                    {
                        cam.gameObject.SetActive(false);
                    }
                }
            }
        }

        #endregion

    }
}

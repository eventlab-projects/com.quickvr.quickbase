using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    public class QuickSceneManager : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public enum SceneState
        {
            Loading,    //The scene is loading
            Preloaded,  //The scene is loaded, but remains in background, meaning that the root gameobjects are not active
            Loaded,     //The scene is loaded and the root gameobjects are active
            Unloading,  //The scene is unloading
        }

        #endregion

        #region PROTECTED ATTRIBUTES

        protected static Dictionary<string, SceneData> _loadedScenes = new Dictionary<string, SceneData>();

        protected class SceneData
        {

            public SceneState _state;
            public List<bool> _rootGOActive = new List<bool>();
        }

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        protected static void Init()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                SceneData sData = new SceneData();
                sData._state = SceneState.Loaded;
                _loadedScenes[SceneManager.GetSceneAt(i).name] = sData;
            }
        }

        #endregion

        #region GET AND SET

        public virtual SceneState? GetSceneState(string sceneName)
        {
            if (_loadedScenes.TryGetValue(sceneName, out SceneData sceneData))
            {
                return sceneData._state;
            }

            return null;
        }

        protected virtual void SetSceneState(string sceneName, SceneState newState)
        {
            if (newState == SceneState.Loading)
            {
                _loadedScenes[sceneName] = new SceneData();
            }
            else if (newState == SceneState.Preloaded)
            {
                Scene scene = SceneManager.GetSceneByName(sceneName);
                foreach (GameObject go in scene.GetRootGameObjects())
                {
                    _loadedScenes[sceneName]._rootGOActive.Add(go.activeSelf);
                    go.SetActive(false);
                }
            }
            else if (newState == SceneState.Loaded)
            {
                Scene scene = SceneManager.GetSceneByName(sceneName);
                GameObject[] rootObjects = scene.GetRootGameObjects();
                List<bool> preloadObjectsState = _loadedScenes[sceneName]._rootGOActive;

                for (int i = 0; i < preloadObjectsState.Count; i++)
                {
                    rootObjects[i].SetActive(preloadObjectsState[i]);
                }
            }

            _loadedScenes[sceneName]._state = newState;
        }

        public virtual bool IsValidScene(string sceneName)
        {
            return SceneManager.GetSceneByName(sceneName).IsValid();
        }

        public virtual void PreLoadSceneAdditive(string sceneName)
        {
            LoadSceneAdditive(sceneName, false, false);
        }

        public virtual void PreLoadSceneAdditiveAsync(string sceneName)
        {
            LoadSceneAdditive(sceneName, false, true);
        }

        public virtual void LoadSceneAdditive(string sceneName)
        {
            LoadSceneAdditive(sceneName, true, false);
        }

        public virtual void LoadSceneAdditiveAsync(string sceneName)
        {
            LoadSceneAdditive(sceneName, true, true);
        }

        protected virtual void LoadSceneAdditive(string sceneName, bool allowSceneActivation, bool isAsync)
        {
            //We can only Load a scene if it has not yet been loaded, or it is is preloaded
            if (
                !_loadedScenes.TryGetValue(sceneName, out SceneData sceneData) || 
                allowSceneActivation && (sceneData._state == SceneState.Loading || sceneData._state == SceneState.Preloaded)
                )
            {
                StartCoroutine(CoLoadSceneAdditive(sceneName, allowSceneActivation, isAsync));
            }
            else
            {
                Debug.LogWarning(sceneName + " is already " + SceneState.Loaded + ". Please, unload it first");
            }
        }

        public virtual void ActivateScene(string sceneName, bool disableCameras = true)
        {
            if (!_loadedScenes.TryGetValue(sceneName, out SceneData sceneData) || sceneData._state != SceneState.Unloading)
            {
                StartCoroutine(CoActivateScene(sceneName, disableCameras));
            }
            else
            {
                Debug.LogWarning(sceneName + " cannot be the Active scene because it is " + SceneState.Unloading);
            }
        }

        public virtual void UnloadScenes(string[] sceneNames)
        {
            foreach (string s in sceneNames)
            {
                UnloadScene(s);
            }
        }

        public virtual void UnloadScene(string sceneName)
        {
            //The scene is send back to background
            if (_loadedScenes.TryGetValue(sceneName, out SceneData sceneData) && sceneData._state != SceneState.Unloading)
            {
                StartCoroutine(CoUnloadScene(sceneName));
            }
        }

        #endregion

        #region UPDATE

        protected virtual IEnumerator CoLoadSceneAdditive(string sceneName, bool allowSceneActivation, bool isAsync)
        {
            if (_loadedScenes.TryGetValue(sceneName, out SceneData sceneData))
            {
                while (sceneData._state == SceneState.Loading)
                {
                    yield return null;
                }
            }
            else
            {
                SetSceneState(sceneName, SceneState.Loading);
                if (isAsync)
                {
                    yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                }
                else
                {
                    SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
                }
            }

            SetSceneState(sceneName, allowSceneActivation ? SceneState.Loaded : SceneState.Preloaded);
        }

        protected virtual IEnumerator CoActivateScene(string sceneName, bool disableCameras)
        {
            //Ensure that we are activating a loaded scene
            if (!_loadedScenes.ContainsKey(sceneName))
            {
                LoadSceneAdditiveAsync(sceneName);
            }

            //Wait for the scene to be loaded, in case we are trying to activate a scene 
            //that is not loaded yet. 
            while (_loadedScenes[sceneName]._state == SceneState.Loading)
            {
                yield return null;
            }
            SetSceneState(sceneName, SceneState.Loaded);

            //At this point, the scene is loaded. Activate it. 
            Scene scene = SceneManager.GetSceneByName(sceneName);
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

        protected virtual IEnumerator CoUnloadScene(string sceneName)
        {
            SetSceneState(sceneName, SceneState.Unloading);

            yield return SceneManager.UnloadSceneAsync(sceneName);

            _loadedScenes.Remove(sceneName);
        }

        #endregion

    }
}

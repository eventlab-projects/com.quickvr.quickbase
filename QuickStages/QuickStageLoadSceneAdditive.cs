using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

namespace QuickVR
{
    public class QuickStageLoadSceneAdditive : QuickStageBase
    {

        #region PUBLIC PARAMETERS

        public string[] _scenesToLoad;

        public string[] _scenesToUnload;

        public string _activeScene = "";

        #endregion

        #region PROTECTED PARAMETERS

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _avoidable = false;
        }

        protected override IEnumerator CoUpdate()
        {
            //Unload undesired scenes
            yield return StartCoroutine(UnloadScenes());

            //Load the new scenes
            yield return StartCoroutine(LoadScenes());

            ActivateScene();
        }

        #endregion

        #region GET AND SET

        protected virtual IEnumerator UnloadScenes()
        {
            foreach (string sceneToUnload in _scenesToUnload)
            {
                Scene gScene = SceneManager.GetSceneByName(sceneToUnload);
                if (gScene.isLoaded)
                {
                    SceneManager.UnloadSceneAsync(sceneToUnload);
                    while (gScene.isLoaded) yield return null;
                }
            }
        }

        protected virtual IEnumerator LoadScenes()
        {
            foreach (string sceneToLoad in _scenesToLoad)
            {
                Scene gScene = SceneManager.GetSceneByName(sceneToLoad);
                if (!gScene.isLoaded)
                {
                    SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Additive);
                    while (!gScene.isLoaded) yield return null;
                }
            }
        }

        protected virtual void ActivateScene()
        {
            //Mark as current the desired one or the first one to load
            if (_activeScene == "" && _scenesToLoad.Length > 0)
                _activeScene = _scenesToLoad[0];
            
            if (_activeScene != "")
            {
                Scene activeScene = SceneManager.GetSceneByName(_activeScene);
                SceneManager.SetActiveScene(activeScene);
            }

            //Deactivate any present camera in the geometry scene 
            foreach (Camera cam in Camera.allCameras)
            {
                if (cam.tag != "MainCamera") cam.gameObject.SetActive(false);
            }
        }

        #endregion

    }
}

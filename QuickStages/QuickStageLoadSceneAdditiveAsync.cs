using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

namespace QuickVR
{
    public class QuickStageLoadSceneAdditiveAsync : QuickStageLoadSceneAdditive
    {

        #region PUBLIC PARAMETERS

        public enum LoadMode { PRELOAD, ACTIVATE, ASAP };
        public LoadMode _loadMode = LoadMode.PRELOAD;

        public bool _dontLoadInEditor = false;

        #endregion

        #region PROTECTED PARAMETERS

        protected static Dictionary<string, AsyncOperation> _loadingScenes = null;

        #endregion

        #region CREATION AND DESTRUCTION

        public override void Init()
        {
            if (_loadingScenes == null)
                _loadingScenes = new Dictionary<string, AsyncOperation>();

            base.Init();
        }

        #endregion

        #region GET AND SET

        protected override IEnumerator LoadScenes()
        {
            if (!_dontLoadInEditor || !Application.isEditor)
            {
                switch (_loadMode)
                {
                    case LoadMode.PRELOAD:
                        foreach (string sceneToLoad in _scenesToLoad)
                        {
                            Scene gScene = SceneManager.GetSceneByName(sceneToLoad);
                            if (!gScene.isLoaded && !_loadingScenes.ContainsKey(sceneToLoad))
                            {
                                AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
                                loadOperation.allowSceneActivation = false;
                                _loadingScenes[sceneToLoad] = loadOperation;
                            }
                        }
                        break;

                    case LoadMode.ACTIVATE:
                        foreach (string sceneToLoad in _scenesToLoad)
                        {
                            if (_loadingScenes.ContainsKey(sceneToLoad))
                            {
                                AsyncOperation loadOperation = _loadingScenes[sceneToLoad];
                                if (loadOperation != null)
                                {
                                    loadOperation.allowSceneActivation = true;
                                    while (!loadOperation.isDone) yield return null;
                                    _loadingScenes.Remove(sceneToLoad);
                                    LightProbes.Tetrahedralize();
                                }
                            }
                        }
                        break;

                    case LoadMode.ASAP:
                        foreach (string sceneToLoad in _scenesToLoad)
                        {
                            Scene gScene = SceneManager.GetSceneByName(sceneToLoad);
                            if (!gScene.isLoaded && !_loadingScenes.ContainsKey(sceneToLoad))
                            {
                                AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
                                loadOperation.allowSceneActivation = true;
                                while (!loadOperation.isDone) yield return null;
                                LightProbes.Tetrahedralize();
                            }
                        }
                        break;
                }
            }
        }

        #endregion

    }
}

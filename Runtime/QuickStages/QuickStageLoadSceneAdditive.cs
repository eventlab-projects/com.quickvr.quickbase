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

        public bool _asyncLoad = true;
        public bool _autoActivateScene = true;
        public string _activeScene = "";

        #endregion

        #region PROTECTED PARAMETERS

        protected QuickSceneManager _sceneManager = null;

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _sceneManager = QuickSingletonManager.GetInstance<QuickSceneManager>();
            _avoidable = false;
        }

        public override void Init()
        {
            base.Init();

            //Unload undesired scenes
            _sceneManager.UnloadScenesAsync(_scenesToUnload);

            //Load the new scenes
            if (_asyncLoad)
            {
                _sceneManager.LoadScenesAsyncAdditive(_scenesToLoad);
            }
            else
            {
                _sceneManager.LoadScenes(_scenesToUnload);
            }

            //Mark as current the desired one or the first one to load
            if (_autoActivateScene && _activeScene == "" && _scenesToLoad.Length > 0)
            {
                _activeScene = _scenesToLoad[0];
            }

            _sceneManager.ActivateScene(_activeScene);
        }

        #endregion

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

namespace QuickVR
{
    public class QuickStageLoadSceneAdditive1 : QuickStageLoadSceneAdditive
    {
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

            foreach (SceneAction s in _sceneActions)
            {
                if (s._type == SceneAction.Type.Preload)
                {
                    _sceneManager.PreLoadSceneAdditive(s._sceneName);
                }
                else if (s._type == SceneAction.Type.PreLoadAsync)
                {
                    _sceneManager.PreLoadSceneAdditiveAsync(s._sceneName);
                }
                else if (s._type == SceneAction.Type.Load)
                {
                    _sceneManager.LoadSceneAdditive(s._sceneName);
                }
                else if (s._type == SceneAction.Type.LoadAsync)
                {
                    _sceneManager.LoadSceneAdditiveAsync(s._sceneName);
                }
                else if (s._type == SceneAction.Type.Unload)
                {
                    _sceneManager.UnloadScene(s._sceneName);
                }
            }

            ////Mark as current the desired one or the first one to load
            //if (_autoActivateScene && _activeScene == "" && _scenesToLoad.Length > 0)
            //{
            //    _activeScene = _scenesToLoad[0];
            //}

            if (_activeScene.Length != 0)
            {
                _sceneManager.ActivateScene(_activeScene);
            }
        }

        #endregion

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

namespace QuickVR
{
    public class QuickStageLoadSceneAdditive : QuickStageBase
    {
        #region PUBLIC PARAMETERS

        [System.Serializable]
        public struct SceneAction
        {
            public enum Type
            {
                Preload,        //The scene is loaded, but remains in background with the root game objects deactivated
                PreLoadAsync,   //...async
                Load,           //The scene is loaded and activated. 
                LoadAsync,      //...async
                Unload,         //The scene is unloaded. 
            }

            public string _sceneName;
            public Type _type;

            public SceneAction(string sceneName, Type t)
            {
                _sceneName = sceneName;
                _type = t;
            }
        }

        public List<SceneAction> _sceneActions = new List<SceneAction>();

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

            //_sceneManager.ActivateScene(_activeScene);
            Debug.Log("ACTIVE SCENE = " + SceneManager.GetActiveScene().name);
        }

        #endregion

    }
}

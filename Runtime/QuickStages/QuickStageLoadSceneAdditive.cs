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
        public string _activeScene = "";    //The new ActiveScene, if any
        public string _logicScene = "";     //The new LogicScene, if any

        #endregion

        #region CREATION AND DESTRUCTION

        protected override void Awake()
        {
            base.Awake();

            _avoidable = false;
        }

        public override void Init()
        {
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

            base.Init();
        }

        public override void Finish()
        {
            if (_logicScene.Length > 0)
            {
                StartCoroutine(CoWaitForLogicScene());
            }
            else
            {
                base.Finish();
            }
        }

        protected virtual IEnumerator CoWaitForLogicScene()
        {
            Debug.Log("Waiting for new logic scene: " + _logicScene);
            while (_sceneManager.GetSceneState(_logicScene) != QuickSceneManager.SceneState.Loaded)
            {
                yield return null;
            }
            Debug.Log("Logic scene: " + _logicScene + " is now loaded!!!");

            _sceneManager.SetLogicScene(_logicScene);

            if (_gameManager == QuickBaseGameManager._instance)
            {
                //The GameManager has not changed. This means that it is an old scene or a scene that does not follow
                //the QuickVR logic workflow. So just fadein to the new scene. 
                CameraFade cFade = QuickSingletonManager.GetInstance<CameraFade>();
                cFade.StartFade(Color.clear, 5);
                while (true)
                {
                    yield return null;
                }
            }
            else
            {
                //Wait till the logic flow control has been given back to the GameManager of this scene. 
                while (_gameManager != QuickBaseGameManager._instance)
                {
                    yield return null;
                }
            }
            
            //CameraFade cFade = QuickSingletonManager.GetInstance<CameraFade>();
            //cFade.StartFade(Color.clear, 5);

            //int thisSceneHandle = gameObject.scene.handle;
            //while (thisSceneHandle != _sceneManager.GetLogicScene().handle)
            //{
            //    yield return null;
            //}

            //Finish the stage as usual. 
            base.Finish();
        }

        #endregion

    }
}

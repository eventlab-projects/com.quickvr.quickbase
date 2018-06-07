using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;

namespace QuickVR
{

    public class QuickSceneManager : MonoBehaviour
    {

        #region PROTECTED PARAMETERS

        protected Dictionary<string, AsyncOperation> _asyncOperations = new Dictionary<string, AsyncOperation>();

        #endregion

        #region GET AND SET

        public virtual bool IsSceneLoaded(string sceneName)
        {
            return _asyncOperations.ContainsKey(sceneName) && _asyncOperations[sceneName].isDone;
        }

        #endregion

        #region UPDATE

        public virtual void LoadSceneAsync(string sceneName, bool allowSceneActivation = false)
        {
            StartCoroutine(CoLoadSceneAsync(sceneName, allowSceneActivation));
        }

        protected virtual IEnumerator CoLoadSceneAsync(string sceneName, bool allowSceneActivation)
        {
            _asyncOperations[sceneName] = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            _asyncOperations[sceneName].allowSceneActivation = allowSceneActivation;
            yield return _asyncOperations[sceneName];
        }

        public virtual void ActivateScene(string sceneName)
        {
            if (_asyncOperations.ContainsKey(sceneName)) _asyncOperations[sceneName].allowSceneActivation = true;
            else SceneManager.LoadScene(sceneName);
        }

        #endregion

    }
}

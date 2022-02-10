using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace QuickVR
{

    public class AddressablesManager : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        public static string URL = "";
        public List<QuickCatalogSettings> _catalogs = new List<QuickCatalogSettings>();

        #endregion

        #region PROTECTED ATTRIBUTES

        private string[] _keys = null;

        protected Dictionary<string, GameObject> _loadedCharacters = new Dictionary<string, GameObject>();
        protected float _progressInitialize = 0;
        protected float _progressAvatarKeys = 0;
        protected float _progressAvatars = 0;

        protected bool _isCharactersLoaded = false;

        #endregion

        #region CREATION AND DESTRUCTION

        //[ButtonMethod]
        //public virtual void Test()
        //{
        //    m_UseLocalAssets = false;
        //    Debug.Log(GetAvatarsCatalogPath());
        //    m_UseLocalAssets = true;
        //    Debug.Log(GetAvatarsCatalogPath());
        //}

        protected virtual IEnumerator Start()
        {
            AsyncOperationHandle<IResourceLocator> op = Addressables.InitializeAsync();
            while (!op.IsDone)
            {
                _progressInitialize = op.PercentComplete / 100.0f;
                yield return null;
            }
            _progressInitialize = 1;
            Debug.Log("Adressables: Initialize Async COMPLETED!!!");

            //Load the Avatars catalog
            op = Addressables.LoadContentCatalogAsync(GetCatalogPathAvatars());
            while (!op.IsDone)
            {
                _progressInitialize = op.PercentComplete / 100.0f;
                yield return null;
            }
            _progressInitialize = 1;
            Debug.Log("Avatars catalog loaded!!!");

            op = Addressables.LoadContentCatalogAsync(GetCatalogPathModernOffice());
            while (!op.IsDone)
            {
                _progressInitialize = op.PercentComplete / 100.0f;
                yield return null;
            }
            _progressInitialize = 1;
            Debug.Log("ModernOffice catalog loaded!!!");

            op = Addressables.LoadContentCatalogAsync(GetCatalogPathRestaurant());
            while (!op.IsDone)
            {
                _progressInitialize = op.PercentComplete / 100.0f;
                yield return null;
            }
            _progressInitialize = 1;
            Debug.Log("Restaurant catalog loaded!!!");

            StartCoroutine(CoLoadCharacters());
        }

        protected virtual IEnumerator CoLoadCharacters()
        {
            //Load the characters keys
            //foreach (IResourceLocator rLocator in Addressables.ResourceLocators)
            //{
            //    Debug.Log(rLocator.);
            //    rLocator.
            //}

            AsyncOperationHandle<TextAsset> opKeys = Addressables.LoadAssetAsync<TextAsset>("__KEYS__");
            while (!opKeys.IsDone)
            {
                _progressAvatarKeys = (float)(opKeys.GetDownloadStatus().DownloadedBytes / (double)opKeys.GetDownloadStatus().TotalBytes);
                yield return null;
            }
            _progressAvatarKeys = 1;

            Debug.Log("CHARACTER KEYS LOADED!!!");
            string[] tmp = opKeys.Result.text.Split('\n');
            _keys = new string[tmp.Length];

            int numAvatars = tmp.Length;
            for (int i = 0; i < tmp.Length; i++)
            {
                _progressAvatars = i / (float)numAvatars;
                string s = tmp[i];
                for (int j = 0; j < s.Length; j++)
                {
                    if (s[j] >= '0' && s[j] <= '9')
                    {
                        _keys[i] += s[j];
                    }
                }

                //Debug.Log(_keys[i]);
                string key = _keys[i];
                AsyncOperationHandle<GameObject> op = Addressables.LoadAssetAsync<GameObject>(key);
                if (op.IsValid())
                {
                    while (!op.IsDone)
                    {
                        yield return null;
                    }

                    _loadedCharacters[key] = op.Result;
                    QuickAddress address = op.Result.GetOrCreateComponent<QuickAddress>();
                    address._address = key;

                    //yield return StartCoroutine(CoInstantiateCharacter(op, _keys[i]));
                }
            }
            _progressAvatars = 1;

            _isCharactersLoaded = true;
        }

        #endregion

        #region GET AND SET

        protected virtual string GetCatalogPathAvatars()
        {
            return _catalogs[0].GetCatalogPath();
        }

        protected virtual string GetCatalogPathModernOffice()
        {
            URL = _catalogs[1].GetServerDataPath();
            return _catalogs[1].GetCatalogPath();
        }

        protected virtual string GetCatalogPathRestaurant()
        {
            URL = _catalogs[2].GetServerDataPath();
            return _catalogs[2].GetCatalogPath();
        }

        public virtual GameObject GetCharacter(string address)
        {
            _loadedCharacters.TryGetValue(address, out GameObject result);

            return result;
        }

        public virtual bool IsCharactersLoaded()
        {
            return _isCharactersLoaded;
        }

        public virtual float GetProgressInitialize()
        {
            return _progressInitialize;
        }

        public virtual float GetProgressAvatarKeys()
        {
            return _progressAvatarKeys;
        }

        public virtual float GetProgressAvatars()
        {
            return _progressAvatars;
        }

        #endregion

        [ButtonMethod]
        public virtual void GenerateCharacterCode()
        {
            string code = "";
            for (int i = 0; i < 6; i++)
            {
                code += Random.Range(0, 9).ToString();
            }

            Debug.Log(code);
        }
    }

}



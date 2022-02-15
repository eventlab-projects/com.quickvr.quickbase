using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.Initialization;

namespace QuickVR
{

    public class QuickAddressablesManager : MonoBehaviour
    {

        #region PUBLIC ATTRIBUTES

        [System.Serializable]
        public class QuickCatalogSettings
        {
            public string _name = "Addressables Catalog";
            public List<string> _paths = new List<string>();
            public bool _lookSubfolders = true;

            public bool _ignore = false;
        }

        public List<QuickCatalogSettings> _catalogs = new List<QuickCatalogSettings>();

        public static bool _isInitialized
        {
            get
            {
                return m_IsInitialized;
            }
        }
        protected static bool m_IsInitialized;

        public static string URL = "";

        #endregion

        #region PROTECTED ATTRIBUTES

        private string[] _keys = null;

        protected Dictionary<string, GameObject> _loadedCharacters = new Dictionary<string, GameObject>();
        protected float _progressInitialize = 0;
        protected float _progressAvatarKeys = 0;
        protected float _progressAvatars = 0;

        protected bool _isCharactersLoaded = false;

        protected enum BuildPlatform
        {
            Undefined = -1,

            StandaloneWindows64,
            Android,
        }

        protected static BuildPlatform _buildPlatform
        {
            get
            {
                if (m_BuildPlatform == BuildPlatform.Undefined)
                {
                    m_BuildPlatform = BuildPlatform.Undefined;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                    m_BuildPlatform = BuildPlatform.StandaloneWindows64;
#elif UNITY_ANDROID
                    m_BuildPlatform = BuildPlatform.Android;
#endif
                }

                return m_BuildPlatform;
            }
        }

        protected static BuildPlatform m_BuildPlatform = BuildPlatform.Undefined;

        #endregion

        #region CREATION AND DESTRUCTION

        public string _testWeb = "http://www.google.com";

        [ButtonMethod]
        public virtual void TestWeb()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead(_testWeb))
                {
                    Debug.Log("WEB OK!!!");
                }
            }
            catch
            {
                Debug.Log("WEB FAIL!!!");
            }
        }

        [ButtonMethod]
        public virtual void Test()
        {
            string pathScenes = "../VRUnited_Scenes";
            Debug.Log(Path.GetFullPath(pathScenes));
            if (Directory.Exists(pathScenes))
            {
                foreach (string s in Directory.GetDirectories(pathScenes))
                {
                    string dir = s + "/ServerData/" + _buildPlatform.ToString();

                    if (Directory.Exists(dir))
                    {
                        List<string> files = new List<string>(Directory.EnumerateFiles(dir, "*.json"));

                        string catalogPath = "";
                        DateTime dTimeLast = new DateTime();
                        foreach (string f in files)
                        {
                            DateTime dTime = File.GetLastWriteTime(f);
                            if (dTime > dTimeLast)
                            {
                                catalogPath = f;
                                dTimeLast = dTime;
                            }
                        }

                        if (catalogPath.Length > 0)
                        {
                            Debug.Log(catalogPath);
                        }
                    }
                    
                }
            }
        }

        protected virtual bool IsLocalPath(string path)
        {
            return !path.Contains("http://") && !path.Contains("www.");
        }

        protected virtual bool PathExists(string path)
        {
            if (IsLocalPath(path)) return Directory.Exists(path);

            return true;
        }

        protected virtual IEnumerator CoLoadContentCatalog(QuickCatalogSettings catalogSettings)
        {
            int i = 0; 
            for (; i < catalogSettings._paths.Count && !PathExists(catalogSettings._paths[i]); i++);
            
            if (i < catalogSettings._paths.Count)
            {
                string catalogPath = catalogSettings._paths[i];
                string sufix = (IsLocalPath(catalogPath) ? "/ServerData/" : "/") + _buildPlatform.ToString();

                if (catalogSettings._lookSubfolders)
                {
                    foreach (string s in Directory.GetDirectories(catalogPath))
                    {
                        string dir = s + sufix;

                        if (PathExists(dir))
                        {
                            yield return StartCoroutine(CoLoadContentCatalog(dir));
                        }
                    }
                }
                else
                {
                    string dir = catalogPath + sufix;
                    if (PathExists(dir))
                    {
                        yield return StartCoroutine(CoLoadContentCatalog(dir));
                    }
                }
            }
        }

        protected virtual IEnumerator CoLoadContentCatalog(string serverDataPath)
        {
            Debug.Log("SERVER DATA PATH = " + serverDataPath);
            string catalogPath = GetCatalogPath(serverDataPath);
            Debug.Log("CATALOG PATH = " + catalogPath);
            if (catalogPath.Length > 0)
            {
                //Debug.Log("Loading catalog " + catalogPath);
                
                AddressablesRuntimeProperties.ClearCachedPropertyValues();
                URL = serverDataPath;
                AsyncOperationHandle<IResourceLocator> op = Addressables.LoadContentCatalogAsync(catalogPath);
                while (!op.IsDone)
                {
                    _progressInitialize = op.PercentComplete / 100.0f;
                    yield return null;
                }

                //Debug.Log(catalogPath + " loaded!");
            }
            
            _progressInitialize = 1;
        }

        protected virtual string GetCatalogPath(string serverDataPath)
        {
            if (!IsLocalPath(serverDataPath))
            {
                return serverDataPath + "/catalog.json";
            }

            string catalogPath = "";

            if (Directory.Exists(serverDataPath))
            {
                List<string> files = new List<string>(Directory.EnumerateFiles(serverDataPath, "*.json"));
                DateTime dTimeLast = new DateTime();

                foreach (string f in files)
                {
                    DateTime dTime = File.GetLastWriteTime(f);
                    if (dTime > dTimeLast)
                    {
                        catalogPath = f;
                        dTimeLast = dTime;
                    }
                }
            }

            return catalogPath;
        }

        protected virtual IEnumerator Start()
        {
            AsyncOperationHandle<IResourceLocator> op = Addressables.InitializeAsync();
            while (!op.IsDone)
            {
                _progressInitialize = op.PercentComplete / 100.0f;
                yield return null;
            }
            _progressInitialize = 1;
            m_IsInitialized = true;
            Debug.Log("Adressables: Initialize Async COMPLETED!!!");

            foreach (QuickCatalogSettings c in _catalogs)
            {
                if (!c._ignore)
                {
                    yield return StartCoroutine(CoLoadContentCatalog(c));
                }
            }

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
                code += UnityEngine.Random.Range(0, 9).ToString();
            }

            Debug.Log(code);
        }
    }

}



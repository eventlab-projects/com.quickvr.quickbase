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
using UnityEngine.ResourceManagement.ResourceLocations;

using UnityEngine.Networking;

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

        #region CONSTANTS

        protected const string PHP_DISCOVER_CATALOGS = "discovercatalogs.php";

        #endregion

        #region CREATION AND DESTRUCTION

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

        public string _testCatalog = "";
        [ButtonMethod]
        public virtual void TestDiscoverCatalogs()
        {
            List<string> result = new List<string>();
            DiscoverCatalogsLocal(_testCatalog, result);
            foreach (string r in result)
            {
                Debug.Log(r);
            }
        }

        protected virtual IEnumerator CoLoadContentCatalog(QuickCatalogSettings catalogSettings)
        {
            int i = 0;
            for (; i < catalogSettings._paths.Count && !PathExists(catalogSettings._paths[i]); i++) ;

            if (i < catalogSettings._paths.Count)
            {
                string serverPath = catalogSettings._paths[i];
                List<string> catalogPaths = new List<string>();
                if (IsLocalPath(serverPath))
                {
                    DiscoverCatalogsLocal(serverPath, catalogPaths);
                }
                else
                {
                    yield return StartCoroutine(CoDiscoverCatalogsRemote(serverPath, catalogPaths));
                }

                foreach (string cPath in catalogPaths)
                {
                    yield return CoLoadContentCatalog(cPath);
                }
            }
        }

        protected virtual void DiscoverCatalogsLocal(string dir, List<string> result)
        {
            string[] subDirs = Directory.GetDirectories(dir);
            int i = 0;
            for (; i < subDirs.Length && !subDirs[i].Contains("ServerData"); i++) ;

            if (i < subDirs.Length)
            {
                //We have found the ServerData subfolder, which by convention, it contains the catalogs. 
                //Look for the catalog file on that folder
                string serverDataPath = subDirs[i] + "/" + _buildPlatform.ToString();
                if (Directory.Exists(serverDataPath))
                {
                    List<string> files = new List<string>(Directory.EnumerateFiles(serverDataPath, "*.json"));
                    DateTime dTimeLast = new DateTime();
                    string catalogPath = "";

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
                        result.Add(catalogPath);
                    }
                }
            }
            else
            {
                foreach (string s in subDirs)
                {
                    DiscoverCatalogsLocal(s, result);
                }
            }
        }

        protected virtual IEnumerator CoDiscoverCatalogsRemote(string serverPath, List<string> catalogPaths)
        {
            WWWForm form = new WWWForm();
            form.AddField("BuildPlatform", _buildPlatform.ToString());
            UnityWebRequest web = UnityWebRequest.Post(serverPath + "/" + PHP_DISCOVER_CATALOGS, form);
            yield return web.SendWebRequest();

            string[] catalogFiles = web.downloadHandler.text.Split(';');
            foreach (string c in catalogFiles)
            {
                catalogPaths.Add(serverPath + c.Substring(1));
            }
        }

        protected virtual IEnumerator CoLoadContentCatalog(string catalogPath)
        {
            if (catalogPath.Length > 0)
            {
                Debug.Log("Loading catalog " + catalogPath);

                AddressablesRuntimeProperties.ClearCachedPropertyValues();
                URL = GetCatalogDirectory(catalogPath);
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

        protected virtual string GetCatalogDirectory(string catalogPath)
        {
            int i = catalogPath.Length - 1;
            for (; i >= 0 && catalogPath[i] != '/' && catalogPath[i] != '\\'; i--) ;

            return catalogPath.Substring(0, i);
        }

        protected virtual IEnumerator CoLoadCharacters()
        {
            //Look for the IResourceLocations of all the objects tagged as VRUAvatar
            AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync("VRUAvatar");
            while (!handle.IsDone)
            {
                _progressAvatarKeys = handle.GetDownloadStatus().DownloadedBytes / handle.GetDownloadStatus().TotalBytes;
                yield return null;
            }
            _progressAvatarKeys = 1;
            
            int numAvatars = handle.Result.Count;
            for (int i = 0; i < numAvatars; i++)
            {
                _progressAvatars = i / (float)numAvatars;
                IResourceLocation rLocation = handle.Result[i];
                string key = rLocation.PrimaryKey;
                AsyncOperationHandle<GameObject> op = Addressables.LoadAssetAsync<GameObject>(rLocation);
                yield return op;
                
                _loadedCharacters[key] = op.Result;
                QuickAddress address = op.Result.GetOrCreateComponent<QuickAddress>();
                address._address = key;
            }

            _progressAvatars = 1;

            _isCharactersLoaded = true;

            Addressables.Release(handle);
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

        protected virtual bool IsLocalPath(string path)
        {
            return !path.Contains("http://") && !path.Contains("www.");
        }

        protected virtual bool PathExists(string path)
        {
            bool exists = false;

            if (IsLocalPath(path))
            {
                exists = Directory.Exists(path);
            }
            else
            {
                try
                {
                    using (var client = new WebClient())
                    using (var stream = client.OpenRead(path + "/" + PHP_DISCOVER_CATALOGS))
                    {
                        //Debug.Log("WEB OK!!!");
                        exists = true;
                    }
                }
                catch
                {
                    //Debug.Log("WEB FAIL!!!");
                    exists = false;
                }
            }

            return exists;
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



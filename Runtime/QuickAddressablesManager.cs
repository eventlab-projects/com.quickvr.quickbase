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

        public List<string> _addressableServerPaths = new List<string>();

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

        protected virtual void Awake()
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            List<string> tmp = new List<string>();
            tmp.Add(Application.persistentDataPath + "/AddressableProjects");
            tmp.AddRange(_addressableServerPaths);
            _addressableServerPaths = tmp;
#endif
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

            int i = 0;
            for (; i < _addressableServerPaths.Count && !PathExists(_addressableServerPaths[i]); i++) ;

            if (i < _addressableServerPaths.Count)
            {
                string serverPath = _addressableServerPaths[i];
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

            StartCoroutine(CoLoadCharacters());
        }

        //public string _testCatalog = "";
        //[ButtonMethod]
        //public virtual void TestDiscoverCatalogs()
        //{
        //    List<string> result = new List<string>();
        //    DiscoverCatalogsLocal(_testCatalog, result);
        //    foreach (string r in result)
        //    {
        //        Debug.Log(r);
        //    }
        //}

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
                yield return Addressables.LoadContentCatalogAsync(catalogPath);

                Debug.Log(catalogPath + " loaded!");
            }
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
            yield return handle;

            if (handle.Result.Count > 0)
            {
                //Load the first avatar. This will produce to download the whole avatars catalog if it has not been downloaded yet. 
                AsyncOperationHandle<GameObject> op = Addressables.LoadAssetAsync<GameObject>(handle.Result[0]);
                while (!op.IsDone)
                {
                    _progressInitialize = op.GetDownloadStatus().Percent; 
                    yield return null;
                }

                AddLoadedAvatar(handle.Result[0].PrimaryKey, op.Result);

                //Load the other avatars
                int numAvatars = handle.Result.Count;
                for (int i = 1; i < numAvatars; i++)
                {
                    _progressInitialize = i / (float)numAvatars;
                    IResourceLocation rLocation = handle.Result[i];
                    op = Addressables.LoadAssetAsync<GameObject>(rLocation);

                    while (!op.IsDone)
                    {
                        yield return null;
                    }

                    AddLoadedAvatar(rLocation.PrimaryKey, op.Result);
                }
            }

            _progressInitialize = 1;

            _isCharactersLoaded = true;

            Addressables.Release(handle);
        }

        #endregion

        #region GET AND SET

        protected virtual void AddLoadedAvatar(string address, GameObject go)
        {
            _loadedCharacters[address] = go;
            go.GetOrCreateComponent<QuickAddress>()._address = address;
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

        //[ButtonMethod]
        //public virtual void GenerateCharacterCode()
        //{
        //    string code = "";
        //    for (int i = 0; i < 6; i++)
        //    {
        //        code += UnityEngine.Random.Range(0, 9).ToString();
        //    }

        //    Debug.Log(code);
        //}
    }

}



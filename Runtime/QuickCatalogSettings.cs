using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace QuickVR
{

    [CreateAssetMenu(fileName = "CatalogSettings", menuName = "QuickVR/QuickCatalogSettings")]
    public class QuickCatalogSettings : ScriptableObject
    {

        public static string URL = "";

        #region PUBLIC ATTRIBUTES

        public string _catalogAndroid = "";
        public string _catalogWindows = "";

        public List<string> _paths = new List<string>();

        //public bool _useLocalAssets
        //{
        //    get
        //    {
        //        if (m_UseLocalAssets && !Application.isEditor)
        //        {
        //            m_UseLocalAssets = false;
        //        }

        //        return m_UseLocalAssets;
        //    }
        //}
        //public bool m_UseLocalAssets = true;

        #endregion

        #region PROTECTED ATTRIBUTES

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

        #region GET AND SET

        public virtual string GetServerDataPath()
        {
            string path = _paths[0];
            if (path[0] == '.')
            {
                //This is a local path. Convert it to global path. 
                path = Path.GetFullPath(path);
            }

            char lastChar = path[path.Length - 1];
            if (lastChar != '/' && lastChar != '\\')
            {
                path += "/";
            }

            path += _buildPlatform.ToString() + "/";

            return path;

            //string path = "";

            //string path = "";

            //if (_useLocalAssets)
            //{
            //    string[] tmp = Application.dataPath.Split('/');
            //    for (int i = 0; i < tmp.Length - 2; i++)
            //    {
            //        path += tmp[i] + '/';
            //    }
            //    path += "VRUnited_Avatars/ServerData/";
            //}
            //else
            //{
            //    path = "https://ramonoliva.com/vrunited/avatars/";
            //}

            //path += _buildPlatform.ToString() + "/";

            //return path;
        }

        public virtual string GetCatalogPath()
        {
            string path = GetServerDataPath();
            if (_buildPlatform == BuildPlatform.StandaloneWindows64)
            {
                path += _catalogWindows;
            }
            else if (_buildPlatform == BuildPlatform.Android)
            {
                path += _catalogAndroid;
            }

            return path;
        }

        #endregion

    }

}



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

        public string _address = "";
        public string _displayName = "";
        public Texture2D _thumbnail = null;

    }

}



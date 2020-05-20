using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_WEBGL
using WebXR;
#endif

namespace QuickVR
{

    [InitializeOnLoad]
    public class QuickVRManagerInitializerEditor
    {

        #region CREATION AND DESTRUCTION

        static QuickVRManagerInitializerEditor()
        {
            QuickVRManager vrManager = QuickSingletonManager.GetInstance<QuickVRManager>();
#if UNITY_WEBGL
            vrManager.GetOrCreateComponent<WebXRManager>();
#endif
        }

        #endregion

    }

}


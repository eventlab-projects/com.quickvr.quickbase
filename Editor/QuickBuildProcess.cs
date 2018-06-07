using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace QuickVR
{
    class QuickPreprocessBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder { get { return 0; } }

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("COMPUTING BUILD SCENES!!!");
            QuickPlayerPrefs.ComputeBuildScenes();
        }
    }
}
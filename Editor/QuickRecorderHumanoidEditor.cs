using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

namespace QuickVR
{

    [CustomEditor(typeof(QuickRecorderHumanoid), true)]
    public class QuickRecorderHumanoidEditor : QuickBaseEditor
    {

        #region PROTECTED ATTRIBUTES

        protected string _path = "Anim";

        protected QuickRecorderHumanoid _recorder = null;
        
        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            _recorder = (QuickRecorderHumanoid)target;
        }

        #endregion

        #region UPDATE

        protected override void DrawGUI()
        {
            base.DrawGUI();

            _path = EditorGUILayout.TextField("Path:", _path);

            if (!_recorder.IsRecording())
            {
                if (DrawButton("Record"))
                {
                    _recorder.Record();
                }
            }
            else
            {
                if (DrawButton("Stop"))
                {
                    SaveRecordedAnimation();
                }
            }
        }
        
        protected virtual void SaveRecordedAnimation()
        {
            AssetDatabase.CreateAsset(_recorder.StopRecording(), "Assets/" + _path + ".anim");
            AssetDatabase.SaveAssets();
        }

        #endregion
    }

}

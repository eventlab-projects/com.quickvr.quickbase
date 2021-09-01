using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QuickVR 
{

    public class QuickSkeletonFixer : EditorWindow
    {

        #region PROTECTED ATTRIBUTES

        protected Transform _hipsSrc = null;
        protected Transform _hipsDst = null;

        #endregion

        #region UPDATE

        protected virtual void OnGUI()
        {
            _hipsSrc = EditorGUILayout.ObjectField("Hips Source", _hipsSrc, typeof(Transform), true) as Transform;
            _hipsDst = EditorGUILayout.ObjectField("Hips Dest", _hipsDst, typeof(Transform), true) as Transform;

            if (_hipsSrc && _hipsDst && GUILayout.Button("Fix"))
            {
                Fix(_hipsSrc, _hipsDst);
                Debug.Log("SKELETON FIXED!!!");
            }
        }

        protected virtual void Fix(Transform tRootSrc, Transform tRootDst)
        {
            foreach (Transform tSrc in tRootSrc)
            {
                Transform tDst = tRootDst.Find(tSrc.name);
                if (!tDst)
                {
                    tDst = tRootDst.CreateChild(tSrc.name, false);
                    tDst.localPosition = tSrc.localPosition;
                    tDst.localRotation = tSrc.localRotation;
                }

                Fix(tSrc, tDst);
            }
        }

        #endregion

    }

}


using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace QuickVR
{

    public static class QuickSingletonManager
    {

        #region PROTECTED PARAMETERS

        private static Dictionary<Type, Component> _instances = new Dictionary<Type, Component>();
        private static bool _isQuitting = false;
        
        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            _isQuitting = false;
            Application.quitting += ApplicationQuitting;
        }

        #endregion

        #region GET AND SET

        public static bool IsQuitting()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                _isQuitting = false;
            }

            return _isQuitting;
        }

        public static T GetInstance<T>() where T : Component
        {
            if (IsQuitting())
            {
                return null;
            }

            Type type = typeof(T);
            if (!_instances.ContainsKey(type) || _instances[type] == null)
            {
                //Check if the singleton object has been already created in the scene
                T instance = GameObject.FindObjectOfType<T>();
                if (!instance)
                {
                    //There is no singleton created in the scene nor a prefab
                    instance = new GameObject("__" + type.Name + "__").AddComponent<T>();
                }
                _instances[type] = instance;
            }

            return (T)_instances[type];
        }

        public static bool IsInstantiated<T>() where T : Component
        {
            return (_instances.ContainsKey(typeof(T)) && _instances[typeof(T)] != null) || (GameObject.FindObjectOfType<T>() != null);
        }

        private static void ApplicationQuitting()
        {
            _isQuitting = true;
        }

        #endregion

    }

    //public class QuickSingletonManager<T> : MonoBehaviour where T : Component {

    //    #region PROTECTED PARAMETERS

    //    protected static T _instance = null;

    //    #endregion

    //    #region CREATION AND DESTRUCTION

    //    public QuickSingletonManager()
    //    {
    //        Init();
    //    }

    //    protected virtual void Init()
    //    {

    //    }

    //    #endregion

    //    #region GET AND SET

    //    public static T GetInstance() {
    //        if (!_instance) {
    //            //Check if the singleton object has been already created in the scene
    //            _instance = FindObjectOfType<T>();
    //            if (!_instance) {
    //                //There is no singleton created in the scene nor a prefab
    //                GameObject go = new GameObject("__" + typeof(T).Name + "__");
    //                go.transform.ResetTransformation();
    //                _instance = go.AddComponent<T>();
    //            }
    //        }
    //        return _instance;
    //    }

    //    public static bool IsInstantiated()
    //    {
    //        return _instance != null;
    //    }

    //    #endregion

    //}

}
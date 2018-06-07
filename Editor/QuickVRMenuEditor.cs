using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QuickVR
{

    public static class QuickVRMenu
    {
        #region CONSTANTS

        const string MENU_QUICKVR_ROOT = "QuickVR";
        const string MENU_QUICKVR_HEADTRACKING = "HeadTracking";
        const string MENU_QUICKVR_BODYTRACKING = "BodyTracking";

        #endregion

        static void AddUniqueComponent<T>() where T : Component
        {
            if (Selection.activeTransform == null) return;

            if (!Selection.activeGameObject.GetComponent<T>())
            {
                Selection.activeGameObject.AddComponent<T>();
            }
        }

        #region HEAD TRACKING COMPONENTS

        [MenuItem(MENU_QUICKVR_ROOT + "/" + MENU_QUICKVR_HEADTRACKING + "/" + "QuickUnityVR")]
        static void AddQuickUnityVR()
        {
            AddUniqueComponent<QuickUnityVR>();
        }

        [MenuItem(MENU_QUICKVR_ROOT + "/" + MENU_QUICKVR_HEADTRACKING + "/" + "QuickUnityVRHands")]
        static void AddQuickUnityVRHands()
        {
            AddUniqueComponent<QuickUnityVRHands>();
        }

        #endregion

        //#region BODY TRACKING COMPONENTS

        //[MenuItem(MENU_QUICKVR_ROOT + "/" + MENU_QUICKVR_BODYTRACKING + "/" + "QuickMotive")]
        //static void AddQuickMotive()
        //{
        //    AddUniqueComponent<QuickMotiveBodyTracking>();
        //}

        //[MenuItem(MENU_QUICKVR_ROOT + "/" + MENU_QUICKVR_BODYTRACKING + "/" + "QuickKinect")]
        //static void AddQuickKinnect()
        //{
        //    AddUniqueComponent<QuickKinectBodyTracking>();
        //}

        //[MenuItem(MENU_QUICKVR_ROOT + "/" + MENU_QUICKVR_BODYTRACKING + "/" + "QuickNeuron")]
        //static void AddQuickNeuron()
        //{
        //    AddUniqueComponent<QuickNeuronBodyTracking>();
        //}

        //[MenuItem(MENU_QUICKVR_ROOT + "/" + MENU_QUICKVR_BODYTRACKING + "/" + "QuickDummy")]
        //static void AddQuickDummy()
        //{
        //    AddUniqueComponent<QuickDummyBodyTracking>();
        //}

        //#endregion

    }

}

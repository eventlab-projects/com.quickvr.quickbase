using UnityEngine;
using System.Collections;

using QuickVR;

public class DemoQuickGUIGameManager : QuickBaseGameManager {

    protected virtual void OnEnable() {
		OnRunning += InitCursor;
	}

	protected virtual void OnDisable() {
		OnRunning -= InitCursor;
	}

	protected virtual void InitCursor() {
        QuickHeadTracking hTracking = FindObjectOfType<QuickHeadTracking>();
        if (hTracking) hTracking.SetVRCursorActive(VRCursorType.HEAD, true);
	}
}

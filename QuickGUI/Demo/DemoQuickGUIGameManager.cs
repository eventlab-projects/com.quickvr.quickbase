using UnityEngine;
using System.Collections;

using QuickVR;

public class DemoQuickGUIGameManager : QuickBaseGameManager {

    protected override void OnEnable() {
        base.OnEnable();

		OnRunning += InitCursor;
	}

	protected override void OnDisable() {
        base.OnDisable();

		OnRunning -= InitCursor;
	}

	protected virtual void InitCursor() {
        QuickHeadTracking hTracking = FindObjectOfType<QuickHeadTracking>();
        if (hTracking) hTracking.SetVRCursorActive(VRCursorType.HEAD, true);
	}
}

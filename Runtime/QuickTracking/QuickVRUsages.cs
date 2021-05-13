using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR;

namespace QuickVR
{
    public static class QuickVRUsages
    {
        public static InputFeatureUsage<Vector3> combineEyePoint = new InputFeatureUsage<Vector3>("CombinedEyeGazePoint");
        public static InputFeatureUsage<Vector3> combineEyeVector = new InputFeatureUsage<Vector3>("CombinedEyeGazeVector");
        public static InputFeatureUsage<Vector3> leftEyePoint = new InputFeatureUsage<Vector3>("LeftEyeGazePoint");
        public static InputFeatureUsage<Vector3> leftEyeVector = new InputFeatureUsage<Vector3>("LeftEyeGazeVector");
        public static InputFeatureUsage<Vector3> rightEyePoint = new InputFeatureUsage<Vector3>("RightEyeGazePoint");
        public static InputFeatureUsage<Vector3> rightEyeVector = new InputFeatureUsage<Vector3>("RightEyeGazeVector");
        public static InputFeatureUsage<float> leftEyeOpenness = new InputFeatureUsage<float>("LeftEyeOpenness");
        public static InputFeatureUsage<float> rightEyeOpenness = new InputFeatureUsage<float>("RightEyeOpenness");
        public static InputFeatureUsage<uint> leftEyePoseStatus = new InputFeatureUsage<uint>("LeftEyePoseStatus");
        public static InputFeatureUsage<uint> rightEyePoseStatus = new InputFeatureUsage<uint>("RightEyePoseStatus");
        public static InputFeatureUsage<uint> combinedEyePoseStatus = new InputFeatureUsage<uint>("CombinedEyePoseStatus");
        public static InputFeatureUsage<float> leftEyePupilDilation = new InputFeatureUsage<float>("LeftEyePupilDilation");
        public static InputFeatureUsage<float> rightEyePupilDilation = new InputFeatureUsage<float>("RightEyePupilDilation");
        public static InputFeatureUsage<Vector3> leftEyePositionGuide = new InputFeatureUsage<Vector3>("LeftEyePositionGuide");
        public static InputFeatureUsage<Vector3> rightEyePositionGuide = new InputFeatureUsage<Vector3>("RightEyePositionGuide");
        public static InputFeatureUsage<Vector3> foveatedGazeDirection = new InputFeatureUsage<Vector3>("FoveatedGazeDirection");
        public static InputFeatureUsage<uint> foveatedGazeTrackingState = new InputFeatureUsage<uint>("FoveatedGazeTrackingState");
    }
}

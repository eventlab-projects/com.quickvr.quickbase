using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace QuickVR
{

    public static class QuickAnimationUtils
    {

        /// <summary>
        /// An intermediate class used for json serialization. 
        /// </summary>
        [System.Serializable]
        protected class QuickAnimationParser
        {

            #region PUBLIC ATTRIBUTES

            public List<string> _curveNames = new List<string>();
            public List<QuickAnimationKeyframeParser<float>> _curveKeyFramesFloat = new List<QuickAnimationKeyframeParser<float>>();
            public List<QuickAnimationKeyframeParser<byte>> _curveKeyFramesByte = new List<QuickAnimationKeyframeParser<byte>>();

            #endregion

            /// <summary>
            /// 
            /// </summary>
            /// <param name="curveName"></param>
            /// <param name="curve"></param>
            /// <param name="saveKeys"></param>
            public virtual void ParseFloat(string curveName, AnimationCurve curve, bool saveKeys = true)
            {
                _curveNames.Add(curveName);

                QuickAnimationKeyframeParser<float> kFrames = new QuickAnimationKeyframeParser<float>();
                
                foreach (Keyframe k in curve.keys)
                {
                    if (saveKeys)
                    {
                        kFrames._keys.Add(k.time);
                    }
                    kFrames._values.Add(k.value);
                }

                _curveKeyFramesFloat.Add(kFrames);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="muscleID"></param>
            /// <param name="curve"></param>
            public virtual void ParseMuscle(int muscleID, AnimationCurve curve)
            {
                string curveName = QuickHumanTrait.GetMuscleName(muscleID);
                _curveNames.Add(curveName);

                QuickAnimationKeyframeParser<byte> kFrames = new QuickAnimationKeyframeParser<byte>();

                foreach (Keyframe k in curve.keys)
                {
                    kFrames._keys.Add(k.time);

                    float muscleMin = QuickHumanTrait.GetMuscleDefaultMin(muscleID);
                    float muscleMax = QuickHumanTrait.GetMuscleDefaultMax(muscleID);
                    float fValue = Mathf.Clamp(k.value, muscleMin, muscleMax);

                    byte value = (byte)(((fValue - muscleMin) / (muscleMax - muscleMin)) * 255.0f);

                    //Debug.Log("kValue = " + k.value.ToString("f3"));
                    //Debug.Log("muscleMin = " + muscleMin.ToString("f3"));
                    //Debug.Log("muscleMax = " + muscleMax.ToString("f3"));
                    //Debug.Log("value = " + value);

                    kFrames._values.Add(value);
                }

                _curveKeyFramesByte.Add(kFrames);
            }

        }

        [System.Serializable]
        protected class QuickAnimationKeyframeParser<T>
        {
            public List<float> _keys = new List<float>();
            public List<T> _values = new List<T>();
        }

        public static AnimationClip ToAnimationClip(QuickAnimation animation)
        {
            AnimationClip clip = new AnimationClip();

            QuickAnimationCurve curvePos = animation.GetAnimationCurve(QuickAnimation.CURVE_BODY_POSITION);
            clip.SetCurve("", typeof(Animator), "RootT.x", curvePos[0]);
            clip.SetCurve("", typeof(Animator), "RootT.y", curvePos[1]);
            clip.SetCurve("", typeof(Animator), "RootT.z", curvePos[2]);

            QuickAnimationCurve curveRot = animation.GetAnimationCurve(QuickAnimation.CURVE_BODY_ROTATION);
            clip.SetCurve("", typeof(Animator), "RootQ.x", curveRot[0]);
            clip.SetCurve("", typeof(Animator), "RootQ.y", curveRot[1]);
            clip.SetCurve("", typeof(Animator), "RootQ.z", curveRot[2]);
            clip.SetCurve("", typeof(Animator), "RootQ.w", curveRot[3]);

            curvePos = animation.GetAnimationCurve(QuickAnimation.CURVE_LEFT_FOOT_IK_GOAL_POSITION);
            clip.SetCurve("", typeof(Animator), "LeftFootT.x", curvePos[0]);
            clip.SetCurve("", typeof(Animator), "LeftFootT.y", curvePos[1]);
            clip.SetCurve("", typeof(Animator), "LeftFootT.z", curvePos[2]);

            curveRot = animation.GetAnimationCurve(QuickAnimation.CURVE_LEFT_FOOT_IK_GOAL_ROTATION);
            clip.SetCurve("", typeof(Animator), "LeftFootQ.x", curveRot[0]);
            clip.SetCurve("", typeof(Animator), "LeftFootQ.y", curveRot[1]);
            clip.SetCurve("", typeof(Animator), "LeftFootQ.z", curveRot[2]);
            clip.SetCurve("", typeof(Animator), "LeftFootQ.w", curveRot[3]);

            curvePos = animation.GetAnimationCurve(QuickAnimation.CURVE_RIGHT_FOOT_IK_GOAL_POSITION);
            clip.SetCurve("", typeof(Animator), "RightFootT.x", curvePos[0]);
            clip.SetCurve("", typeof(Animator), "RightFootT.y", curvePos[1]);
            clip.SetCurve("", typeof(Animator), "RightFootT.z", curvePos[2]);

            curveRot = animation.GetAnimationCurve(QuickAnimation.CURVE_RIGHT_FOOT_IK_GOAL_ROTATION);
            clip.SetCurve("", typeof(Animator), "RightFootQ.x", curveRot[0]);
            clip.SetCurve("", typeof(Animator), "RightFootQ.y", curveRot[1]);
            clip.SetCurve("", typeof(Animator), "RightFootQ.z", curveRot[2]);
            clip.SetCurve("", typeof(Animator), "RightFootQ.w", curveRot[3]);

            for (int i = 0; i < QuickHumanTrait.GetNumMuscles(); i++)
            {
                string muscleName = QuickHumanTrait.GetMuscleName(i);
                clip.SetCurve("", typeof(Animator), muscleName, animation.GetAnimationCurve(muscleName)[0]);
            }

            return clip;
        }

        private static QuickAnimationParser ToAnimationParser(QuickAnimation animation)
        {
            QuickAnimationParser parser = new QuickAnimationParser();
            string[] dimensions = { ".x", ".y", ".z", ".w" };

            for (int i = 0; i < 3; i++)
            {
                parser.ParseFloat("Position" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_TRANSFORM_POSITION)[i], i == 0);
            }

            for (int i = 0; i < 4; i++)
            {
                parser.ParseFloat("Rotation" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_TRANSFORM_ROTATION)[i], i == 0);
            }

            for (int i = 0; i < 3; i++)
            {
                parser.ParseFloat("RootT" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_BODY_POSITION)[i], i == 0);
            }
            
            for (int i = 0; i < 4; i++)
            {
                parser.ParseFloat("RootQ" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_BODY_ROTATION)[i], i == 0);
            }

            for (int i = 0; i < 3; i++)
            {
                parser.ParseFloat("LeftFootT" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_LEFT_FOOT_IK_GOAL_POSITION)[i], i == 0);
            }

            for (int i = 0; i < 4; i++)
            {
                parser.ParseFloat("LeftFootQ" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_LEFT_FOOT_IK_GOAL_ROTATION)[i], i == 0);
            }

            for (int i = 0; i < 3; i++)
            {
                parser.ParseFloat("RightFootT" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_RIGHT_FOOT_IK_GOAL_POSITION)[i], i == 0);
            }

            for (int i = 0; i < 4; i++)
            {
                parser.ParseFloat("RightFootQ" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_RIGHT_FOOT_IK_GOAL_ROTATION)[i], i == 0);
            }

            for (int i = 0; i < QuickHumanTrait.GetNumMuscles(); i++)
            {
                string muscleName = QuickHumanTrait.GetMuscleName(i);
                parser.ParseFloat(muscleName, animation.GetAnimationCurve(muscleName)[0]);
                //parser.ParseMuscle(i, animation.GetAnimationCurve(muscleName)[0]);
            }

            return parser;
        }

        private static QuickAnimation ToQuickAnimation(QuickAnimationParser parser, Animator animator)
        {

            QuickAnimation result = new QuickAnimation(animator);

            result.SetAnimationCurve(QuickAnimation.CURVE_TRANSFORM_POSITION, ParseCurveFloat(parser, 0, 3));
            result.SetAnimationCurve(QuickAnimation.CURVE_TRANSFORM_ROTATION, ParseCurveFloat(parser, 3, 4));
            result.SetAnimationCurve(QuickAnimation.CURVE_BODY_POSITION, ParseCurveFloat(parser, 7, 3));
            result.SetAnimationCurve(QuickAnimation.CURVE_BODY_ROTATION, ParseCurveFloat(parser, 10, 4));
            result.SetAnimationCurve(QuickAnimation.CURVE_LEFT_FOOT_IK_GOAL_POSITION, ParseCurveFloat(parser, 14, 3));
            result.SetAnimationCurve(QuickAnimation.CURVE_LEFT_FOOT_IK_GOAL_ROTATION, ParseCurveFloat(parser, 17, 4));
            result.SetAnimationCurve(QuickAnimation.CURVE_RIGHT_FOOT_IK_GOAL_POSITION, ParseCurveFloat(parser, 21, 3));
            result.SetAnimationCurve(QuickAnimation.CURVE_RIGHT_FOOT_IK_GOAL_ROTATION, ParseCurveFloat(parser, 24, 4));

            for (int i = 0; i < QuickHumanTrait.GetNumMuscles(); i++)
            {
                result.SetAnimationCurve(QuickHumanTrait.GetMuscleName(i), ParseCurveFloat(parser, 28 + i, 1));
                //result.SetAnimationCurve(QuickHumanTrait.GetMuscleName(i), ParseCurveMuscle(parser, i));
            }

            int kNew = 0;
            int kFound = 0;
            HashSet<float> test = new HashSet<float>();
            foreach (QuickAnimationCurve c in result.GetAnimationCurves())
            {
                foreach (QuickAnimationCurveBase b in c.GetAnimationCurves())
                {
                    foreach (Keyframe k in b.keys)
                    {
                        if (!test.Contains(k.time))
                        {
                            test.Add(k.time);
                            kNew++;
                        }
                        else
                        {
                            kFound++;
                        }
                    }
                }
            }

            Debug.Log("kNew = " + kNew);
            Debug.Log("kFound = " + kFound);

            return result;
        }

        private static QuickAnimationCurve ParseCurveFloat(QuickAnimationParser parser, int curveIndex, int dimensions)
        {
            QuickAnimationCurve result = new QuickAnimationCurve(parser._curveNames[curveIndex]);
            int numKeys = parser._curveKeyFramesFloat[curveIndex]._keys.Count;

            for (int i = 0; i < numKeys; i++)
            {
                float time = parser._curveKeyFramesFloat[curveIndex + 0]._keys[i];
                if (dimensions == 1)
                {
                    float value = parser._curveKeyFramesFloat[curveIndex + 0]._values[i];

                    result.AddKey(time, value);
                }
                else if (dimensions == 3)
                {
                    Vector3 value = new Vector3
                        (
                        parser._curveKeyFramesFloat[curveIndex + 0]._values[i],
                        parser._curveKeyFramesFloat[curveIndex + 1]._values[i],
                        parser._curveKeyFramesFloat[curveIndex + 2]._values[i]
                        );

                    result.AddKey(time, value);
                }
                else if (dimensions == 4)
                {
                    Quaternion value = new Quaternion
                        (
                        parser._curveKeyFramesFloat[curveIndex + 0]._values[i],
                        parser._curveKeyFramesFloat[curveIndex + 1]._values[i],
                        parser._curveKeyFramesFloat[curveIndex + 2]._values[i],
                        parser._curveKeyFramesFloat[curveIndex + 3]._values[i]
                        );

                    result.AddKey(time, value);
                }
                
            }

            return result;
        }

        private static QuickAnimationCurve ParseCurveMuscle(QuickAnimationParser parser, int muscleID)
        {
            QuickAnimationCurve result = new QuickAnimationCurve(QuickHumanTrait.GetMuscleName(muscleID));

            int numKeys = parser._curveKeyFramesByte[muscleID]._keys.Count;

            for (int i = 0; i < numKeys; i++)
            {
                float time = parser._curveKeyFramesByte[muscleID]._keys[i];
                byte bValue = parser._curveKeyFramesByte[muscleID]._values[i];

                float muscleMin = QuickHumanTrait.GetMuscleDefaultMin(muscleID);
                float muscleMax = QuickHumanTrait.GetMuscleDefaultMax(muscleID);
                float value = (bValue / 255.0f) * (muscleMax - muscleMin) + muscleMin;

                //Debug.Log("bValue = " + bValue);
                //Debug.Log("muscleMin = " + muscleMin.ToString("f3"));
                //Debug.Log("muscleMax = " + muscleMax.ToString("f3"));
                //Debug.Log("value = " + value.ToString("f3"));

                result.AddKey(time, Mathf.Clamp(value, muscleMin, muscleMax));
            }

            return result;
        }

        public static string ToJson(QuickAnimation animation)
        {
            return JsonUtility.ToJson(ToAnimationParser(animation));
        }

        public static void SaveToJson(string path, QuickAnimation animation)
        {
            TextWriter writer = new StreamWriter(path);
            writer.Write(ToJson(animation));
            writer.Close();
        }

        public static QuickAnimation LoadFromJson(string path, Animator animator)
        {
            string s = File.ReadAllText(path);

            return ToQuickAnimation(JsonUtility.FromJson<QuickAnimationParser>(s), animator);
        }

    }

}


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
            public List<QuickAnimationKeyframeParser> _curveKeyFrames = new List<QuickAnimationKeyframeParser>();

            #endregion

            /// <summary>
            /// 
            /// </summary>
            /// <param name="curveName"></param>
            /// <param name="curve"></param>
            /// <param name="saveKeys"></param>
            public virtual void Parse(string curveName, AnimationCurve curve, bool saveKeys = true)
            {
                _curveNames.Add(curveName);

                QuickAnimationKeyframeParser kFrames = new QuickAnimationKeyframeParser();
                
                foreach (Keyframe k in curve.keys)
                {
                    if (saveKeys)
                    {
                        kFrames._keys.Add(k.time);
                    }
                    kFrames._values.Add(k.value);
                }

                _curveKeyFrames.Add(kFrames);
            }

        }

        [System.Serializable]
        protected class QuickAnimationKeyframeParser
        {
            public List<float> _keys = new List<float>();
            public List<float> _values = new List<float>();
        }

        [System.Serializable]
        protected class QuickAnimationKeyframeParserByte
        {

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
                parser.Parse("Position" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_TRANSFORM_POSITION)[i], i == 0);
            }

            for (int i = 0; i < 4; i++)
            {
                parser.Parse("Rotation" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_TRANSFORM_ROTATION)[i], i == 0);
            }

            for (int i = 0; i < 3; i++)
            {
                parser.Parse("RootT" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_BODY_POSITION)[i], i == 0);
            }
            
            for (int i = 0; i < 4; i++)
            {
                parser.Parse("RootQ" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_BODY_ROTATION)[i], i == 0);
            }

            for (int i = 0; i < 3; i++)
            {
                parser.Parse("LeftFootT" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_LEFT_FOOT_IK_GOAL_POSITION)[i], i == 0);
            }

            for (int i = 0; i < 4; i++)
            {
                parser.Parse("LeftFootQ" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_LEFT_FOOT_IK_GOAL_ROTATION)[i], i == 0);
            }

            for (int i = 0; i < 3; i++)
            {
                parser.Parse("RightFootT" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_RIGHT_FOOT_IK_GOAL_POSITION)[i], i == 0);
            }

            for (int i = 0; i < 4; i++)
            {
                parser.Parse("RightFootQ" + dimensions[i], animation.GetAnimationCurve(QuickAnimation.CURVE_RIGHT_FOOT_IK_GOAL_ROTATION)[i], i == 0);
            }

            for (int i = 0; i < QuickHumanTrait.GetNumMuscles(); i++)
            {
                string muscleName = QuickHumanTrait.GetMuscleName(i);
                parser.Parse(muscleName, animation.GetAnimationCurve(muscleName)[0]);
            }

            return parser;
        }

        private static QuickAnimation ToQuickAnimation(QuickAnimationParser parser, Animator animator)
        {
            QuickAnimation result = new QuickAnimation(animator);

            result.SetAnimationCurve(QuickAnimation.CURVE_TRANSFORM_POSITION, ParseCurve(parser, 0, 3));
            result.SetAnimationCurve(QuickAnimation.CURVE_TRANSFORM_ROTATION, ParseCurve(parser, 3, 4));
            result.SetAnimationCurve(QuickAnimation.CURVE_BODY_POSITION, ParseCurve(parser, 7, 3));
            result.SetAnimationCurve(QuickAnimation.CURVE_BODY_ROTATION, ParseCurve(parser, 10, 4));
            result.SetAnimationCurve(QuickAnimation.CURVE_LEFT_FOOT_IK_GOAL_POSITION, ParseCurve(parser, 14, 3));
            result.SetAnimationCurve(QuickAnimation.CURVE_LEFT_FOOT_IK_GOAL_ROTATION, ParseCurve(parser, 17, 4));
            result.SetAnimationCurve(QuickAnimation.CURVE_RIGHT_FOOT_IK_GOAL_POSITION, ParseCurve(parser, 21, 3));
            result.SetAnimationCurve(QuickAnimation.CURVE_RIGHT_FOOT_IK_GOAL_ROTATION, ParseCurve(parser, 24, 4));

            for (int i = 0; i < QuickHumanTrait.GetNumMuscles(); i++)
            {
                result.SetAnimationCurve(QuickHumanTrait.GetMuscleName(i), ParseCurve(parser, 28 + i, 1));
            }

            return result;
        }

        private static QuickAnimationCurve ParseCurve(QuickAnimationParser parser, int curveIndex, int dimensions)
        {
            QuickAnimationCurve result = new QuickAnimationCurve();
            int numKeys = parser._curveKeyFrames[curveIndex]._keys.Count;

            for (int i = 0; i < numKeys; i++)
            {
                float time = parser._curveKeyFrames[curveIndex + 0]._keys[i];
                if (dimensions == 1)
                {
                    float value = parser._curveKeyFrames[curveIndex + 0]._values[i];

                    result.AddKey(time, value);
                }
                else if (dimensions == 3)
                {
                    Vector3 value = new Vector3
                        (
                        parser._curveKeyFrames[curveIndex + 0]._values[i],
                        parser._curveKeyFrames[curveIndex + 1]._values[i],
                        parser._curveKeyFrames[curveIndex + 2]._values[i]
                        );

                    result.AddKey(time, value);
                }
                else if (dimensions == 4)
                {
                    Quaternion value = new Quaternion
                        (
                        parser._curveKeyFrames[curveIndex + 0]._values[i],
                        parser._curveKeyFrames[curveIndex + 1]._values[i],
                        parser._curveKeyFrames[curveIndex + 2]._values[i],
                        parser._curveKeyFrames[curveIndex + 3]._values[i]
                        );

                    result.AddKey(time, value);
                }
                
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


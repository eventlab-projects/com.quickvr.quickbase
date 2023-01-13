using UnityEngine;

using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace QuickVR
{

    public class QuickAnimWriter
    {

        #region PROTECTED ATTRIBUTES

        protected StreamWriter _sWriter = null;
        protected float _animTime = 0;

        protected string _indent = "";

        protected int _indentLevel
        {
            get
            {
                return m_IndentLevel;
            }
            set
            {
                m_IndentLevel = value;

                _indent = "";
                for (int i = 0; i < m_IndentLevel; i++)
                {
                    _indent += "  ";
                }
            }
        }
        protected int m_IndentLevel = 0;

        #endregion

        #region GET AND SET

        public virtual void WriteAnimation(string filePath, QuickAnimation animation)
        {
            WriteBegin(filePath);

            WriteLine("%YAML 1.1");
            WriteLine("%TAG !u! tag:unity3d.com,2011:");
            WriteLine("--- !u!74 &7400000");
            WriteLine("AnimationClip:");

            _indentLevel++;
            WriteLine("m_ObjectHideFlags: 0");
            WriteLine("m_CorrespondingSourceObject: {fileID: 0}");
            WriteLine("m_PrefabInstance: {fileID: 0}");
            WriteLine("m_PrefabAsset: {fileID: 0}");
            WriteLine("m_Name: " + "testAnimConverted");
            WriteLine("serializedVersion: 6");
            WriteLine("m_Legacy: 0");
            WriteLine("m_Compressed: 0");
            WriteLine("m_UseHighQualityCurve: 1");
            WriteLine("m_RotationCurves: []");
            WriteLine("m_CompressedRotationCurves: []");
            WriteLine("m_EulerCurves: []");
            WriteLine("m_PositionCurves: []");
            WriteLine("m_ScaleCurves: []");

            WriteLine("m_FloatCurves:");

            WriteCurves(animation);

            WriteLine("m_PPtrCurves: []");
            WriteLine("m_SampleRate: 60");
            WriteLine("m_WrapMode: 0");

            WriteLine("m_Bounds:");
            _indentLevel++;
            WriteLine("m_Center: {x: 0, y: 0, z: 0}");
            WriteLine("m_Extent: {x: 0, y: 0, z: 0}");
            _indentLevel--;

            WriteLine("m_ClipBindingConstant:");
            _indentLevel++;
            WriteLine("genericBindings: []");
            WriteLine("pptrCurveMapping: []");
            _indentLevel--;

            WriteLine("m_AnimationClipSettings:");
            _indentLevel++;
            WriteLine("serializedVersion: 2");
            WriteLine("m_AdditiveReferencePoseClip: {fileID: 0}");
            WriteLine("m_AdditiveReferencePoseTime: 0");
            WriteLine("m_StartTime: 0");
            WriteLine("m_StopTime: " + _animTime.ToString().Replace(",", "."));
            WriteLine("m_OrientationOffsetY: 0");
            WriteLine("m_Level: 0");
            WriteLine("m_CycleOffset: 0");
            WriteLine("m_HasAdditiveReferencePose: 0");
            WriteLine("m_LoopTime: 0");
            WriteLine("m_LoopBlend: 0");
            WriteLine("m_LoopBlendOrientation: 1");
            WriteLine("m_LoopBlendPositionY: 1");
            WriteLine("m_LoopBlendPositionXZ: 1");
            WriteLine("m_KeepOriginalOrientation: 0");
            WriteLine("m_KeepOriginalPositionY: 1");
            WriteLine("m_KeepOriginalPositionXZ: 0");
            WriteLine("m_HeightFromFeet: 0");
            WriteLine("m_Mirror: 0");
            _indentLevel--;

            WriteLine("m_EditorCurves: []");
            WriteLine("m_EulerEditorCurves: []");
            WriteLine("m_HasGenericRootTransform: 0");
            WriteLine("m_HasMotionFloatCurves: 0");
            WriteLine("m_Events: []");

            WriteEnd();
        }

        protected virtual void WriteCurves(QuickAnimation animation)
        {
            //Save the main animation curves
            WriteCurve("RootT", animation.GetAnimationCurve(QuickAnimation.CURVE_BODY_POSITION) + animation.GetAnimationCurve(QuickAnimation.CURVE_TRANSFORM_POSITION));

            QuickAnimationCurve curveRot = animation.GetAnimationCurve(QuickAnimation.CURVE_TRANSFORM_ROTATION);
            WriteCurve("RootQ", animation.GetAnimationCurve(QuickAnimation.CURVE_BODY_ROTATION));

            WriteCurve("LeftFootT", animation.GetAnimationCurve(QuickAnimation.CURVE_LEFT_FOOT_IK_GOAL_POSITION));

            WriteCurve("LeftFootQ", animation.GetAnimationCurve(QuickAnimation.CURVE_LEFT_FOOT_IK_GOAL_ROTATION));

            WriteCurve("RightFootT", animation.GetAnimationCurve(QuickAnimation.CURVE_RIGHT_FOOT_IK_GOAL_POSITION));

            WriteCurve("RightFootQ", animation.GetAnimationCurve(QuickAnimation.CURVE_RIGHT_FOOT_IK_GOAL_ROTATION));

            //Save the curves for the muscles
            for (int i = 0; i < QuickHumanTrait.GetNumMuscles(); i++)
            {
                string muscleName = QuickHumanTrait.GetMuscleName(i);
                WriteCurve(muscleName, animation.GetAnimationCurve(muscleName));
            }
        }

        protected virtual AnimationCurve AddCurves(AnimationCurve c1, AnimationCurve c2)
        {
            AnimationCurve result = new AnimationCurve();

            Keyframe[] kFrames = new Keyframe[c1.keys.Length];
            for (int i = 0; i < kFrames.Length; i++)
            {
                float t = c1.keys[i].time;
                kFrames[i].time = t;
                kFrames[i].value = c1.keys[i].value + c2.Evaluate(t);
            }
            result.keys = kFrames;

            return result;
        }

        protected virtual void WriteBegin(string filePath)
        {
            _indentLevel = 0;
            _animTime = 0;
            _sWriter = new StreamWriter(filePath);
        }

        protected virtual void WriteLine(string line)
        {
            _sWriter.WriteLine(_indent + line);
        }

        protected virtual void WriteCurve(string curveName, QuickAnimationCurve curve)
        {
            int dim = curve._numDimensions;
            if (dim == 1)
            {
                WriteCurve(curveName, curve[0]);
            }
            else
            {
                string[] suffix = { ".x", ".y", ".z", ".w" };
                for (int i = 0; i < dim; i++)
                {
                    WriteCurve(curveName + suffix[i], curve[i]);
                }
            }
        }

        protected virtual void WriteCurve(string curveName, AnimationCurve aCurve)
        {
            WriteLine("- curve:");
            _indentLevel += 2;
            WriteLine("serializedVersion: 2");
            WriteLine("m_Curve:");

            foreach (Keyframe k in aCurve.keys)
            {
                WriteLine("- serializedVersion: 3");
                _indentLevel++;

                WriteLine("time: " + k.time.ToString().Replace(",", "."));
                WriteLine("value: " + k.value.ToString().Replace(",", "."));
                WriteLine("inSlope: " + k.inTangent.ToString().Replace(",", "."));
                WriteLine("outSlope: " + k.outTangent.ToString().Replace(",", "."));
                WriteLine("tangentMode: 0");
                WriteLine("weightedMode: " + (int)k.weightedMode);
                WriteLine("inWeight: " + k.inWeight.ToString().Replace(",", "."));
                WriteLine("outWeight: " + k.outWeight.ToString().Replace(",", "."));
                _indentLevel--;

                //Update the animTime if necessary
                _animTime = Mathf.Max(_animTime, k.time);
            }

            WriteLine("m_PreInfinity: 2");
            WriteLine("m_PostInfinity: 2");
            WriteLine("m_RotationOrder: 4");

            _indentLevel--;
            WriteLine("attribute: " + curveName);
            WriteLine("path: ");
            WriteLine("classID: 95");
            WriteLine("script: {fileID: 0}");
            _indentLevel--;
        }

        protected virtual void WriteEnd()
        {
            _sWriter.Close();
        }

        #endregion

    }

    public class QuickAsyncOperation<T> : QuickAsyncOperation
    {

        #region PUBLIC ATTRIBUTES

        public T _result
        {
            get
            {
                return m_Result;
            }

            set
            {
                m_Result = value;
                _isDone = true;
            }
        }
        protected T m_Result;

        #endregion

    }

    public class QuickAsyncOperation : CustomYieldInstruction
    {

        #region PUBLIC ATTRIBUTES

        public bool _isDone
        {
            get; set;
        }

        public override bool keepWaiting
        {
            get
            {
                return !_isDone;
            }
        }

        #endregion

    }

    public static class QuickAnimationUtils
    {

        #region CONSTANTS

        public static string FLOAT_PRECISION = "f6";

        #endregion

        /// <summary>
        /// An intermediate class used for json serialization. 
        /// </summary>
        [System.Serializable]
        protected class QuickAnimationParser
        {

            #region PUBLIC ATTRIBUTES

            public List<string> _curveNames = new List<string>();
            public List<QuickAnimationKeyframeParser> _keyFrames = new List<QuickAnimationKeyframeParser>();

            #endregion

            ///// <summary>
            ///// 
            ///// </summary>
            ///// <param name="curveName"></param>
            ///// <param name="curve"></param>
            ///// <param name="saveKeys"></param>
            //public virtual void ParseFloat(string curveName, AnimationCurve curve, bool saveKeys = true)
            //{
            //    _curveNames.Add(curveName);

            //    QuickAnimationKeyframeParser<float> kFrames = new QuickAnimationKeyframeParser<float>();

            //    foreach (Keyframe k in curve.keys)
            //    {
            //        if (saveKeys)
            //        {
            //            kFrames._keys.Add(k.time);
            //        }
            //        kFrames._values.Add(k.value);
            //    }

            //    _curveKeyFramesFloat.Add(kFrames);
            //}

            ///// <summary>
            ///// 
            ///// </summary>
            ///// <param name="muscleID"></param>
            ///// <param name="curve"></param>
            //public virtual void ParseMuscle(int muscleID, AnimationCurve curve)
            //{
            //    string curveName = QuickHumanTrait.GetMuscleName(muscleID);
            //    _curveNames.Add(curveName);

            //    QuickAnimationKeyframeParser<byte> kFrames = new QuickAnimationKeyframeParser<byte>();

            //    foreach (Keyframe k in curve.keys)
            //    {
            //        kFrames._keys.Add(k.time);

            //        float muscleMin = QuickHumanTrait.GetMuscleDefaultMin(muscleID);
            //        float muscleMax = QuickHumanTrait.GetMuscleDefaultMax(muscleID);
            //        float fValue = Mathf.Clamp(k.value, muscleMin, muscleMax);

            //        byte value = (byte)(((fValue - muscleMin) / (muscleMax - muscleMin)) * 255.0f);

            //        //Debug.Log("kValue = " + k.value.ToString("f3"));
            //        //Debug.Log("muscleMin = " + muscleMin.ToString("f3"));
            //        //Debug.Log("muscleMax = " + muscleMax.ToString("f3"));
            //        //Debug.Log("value = " + value);

            //        kFrames._values.Add(value);
            //    }

            //    _curveKeyFramesByte.Add(kFrames);
            //}

        }

        [System.Serializable]
        protected class QuickAnimationKeyframeParser
        {
            /// <summary>
            /// The time of the keyframe
            /// </summary>
            public float t; 

            /// <summary>
            /// The curves that has any value on this frame
            /// </summary>
            public List<QuickAnimationKeyframeValueParser> cv = new List<QuickAnimationKeyframeValueParser>();
        }

        [System.Serializable]
        protected class QuickAnimationKeyframeValueParser 
        {
            /// <summary>
            /// The curve ID
            /// </summary>
            public int id = 0;

            /// <summary>
            /// The curve values
            /// </summary>
            public List<string> v = new List<string>();
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

            //1) Save the curveNames
            int numCurves = animation.GetNumCurves();
            for (int i = 0; i <  numCurves; i++)
            {
                parser._curveNames.Add(animation.GetCurveName(i));
            }

            //2) Save the Keyframes
            List<QuickAnimationKeyframe> kFrames = animation.GetKeyframes();
            int numKFrames = kFrames.Count;
            parser._keyFrames = new List<QuickAnimationKeyframeParser>(numKFrames);

            for (int i = 0; i < numKFrames; i++)
            {
                QuickAnimationKeyframeParser kFrameParser = new QuickAnimationKeyframeParser();

                QuickAnimationKeyframe kFrame = kFrames[i];
                kFrameParser.t = kFrame._time;

                //Debug.Log("mask = " + kFrame.ToString());

                for (int j = 0; j < numCurves; j++)
                {
                    if (kFrame.HasCurve(j))
                    {
                        //i is a KeyFrame of the curve j
                        QuickAnimationCurve aCurve = animation.GetAnimationCurve(animation.GetCurveName(j));
                        QuickAnimationKeyframeValueParser kValueParser = new QuickAnimationKeyframeValueParser();
                        kValueParser.id = j;

                        for (int d = 0; d < aCurve._numDimensions; d++)
                        {
                            string[] tmp = aCurve[d].Evaluate(kFrame._time).ToString(FLOAT_PRECISION).Split('.');
                            string sNumber = tmp[0];
                            string sFraction = tmp[1];

                            while (sFraction.Length > 0 && sFraction[sFraction.Length - 1] == '0')
                            {
                                sFraction = sFraction.Remove(sFraction.Length - 1);
                            }

                            string v = sNumber;
                            if (sFraction.Length > 0)
                            {
                                v += "." + sFraction; 
                            }

                            kValueParser.v.Add(v);
                        }

                        kFrameParser.cv.Add(kValueParser);
                    }
                    //else
                    //{
                    //    Debug.Log("MISS!!!");
                    //}
                }

                parser._keyFrames.Add(kFrameParser);
            }

            return parser;
        }

        private static QuickAnimation ToQuickAnimation(QuickAnimationParser parser, Animator animator)
        {

            QuickAnimation result = new QuickAnimation(animator);

            //1) Create the curves
            foreach (string cName in parser._curveNames)
            {
                result.GetAnimationCurve(cName);
            }

            foreach (QuickAnimationKeyframeParser kParser in parser._keyFrames)
            {
                float time = kParser.t;
                foreach (QuickAnimationKeyframeValueParser kValueParser in kParser.cv)
                {
                    QuickAnimationCurve aCurve = result.GetAnimationCurve(kValueParser.id);
                    int dimensions = kValueParser.v.Count;
                    float[] values = new float[dimensions];
                    for (int i = 0; i < dimensions; i++)
                    {
                        values[i] = float.Parse(kValueParser.v[i]);
                    }
                    
                    if (dimensions == 1)
                    {
                        aCurve.AddKey(time, values[0], true);
                    }
                    else if (dimensions == 3)
                    {
                        aCurve.AddKey(time, new Vector3(values[0], values[1], values[2]));
                    }
                    else if (dimensions == 4)
                    {
                        aCurve.AddKey(time, new Quaternion(values[0], values[1], values[2], values[3]));
                    }
                    else
                    {
                        Debug.LogError("Num Dimensions not supported!!! " + dimensions);
                    }
                }
            }

            result.ComputeAnimationTime();

            return result;
        }

        //private static QuickAnimationCurve ParseCurveFloat(QuickAnimationParser parser, int curveIndex, int dimensions)
        //{
        //    QuickAnimationCurve result = new QuickAnimationCurve(parser._curveNames[curveIndex]);
        //    int numKeys = parser._curveKeyFramesFloat[curveIndex]._keys.Count;

        //    for (int i = 0; i < numKeys; i++)
        //    {
        //        float time = parser._curveKeyFramesFloat[curveIndex + 0]._keys[i];
        //        if (dimensions == 1)
        //        {
        //            float value = parser._curveKeyFramesFloat[curveIndex + 0]._values[i];

        //            result.AddKey(time, value);
        //        }
        //        else if (dimensions == 3)
        //        {
        //            Vector3 value = new Vector3
        //                (
        //                parser._curveKeyFramesFloat[curveIndex + 0]._values[i],
        //                parser._curveKeyFramesFloat[curveIndex + 1]._values[i],
        //                parser._curveKeyFramesFloat[curveIndex + 2]._values[i]
        //                );

        //            result.AddKey(time, value);
        //        }
        //        else if (dimensions == 4)
        //        {
        //            Quaternion value = new Quaternion
        //                (
        //                parser._curveKeyFramesFloat[curveIndex + 0]._values[i],
        //                parser._curveKeyFramesFloat[curveIndex + 1]._values[i],
        //                parser._curveKeyFramesFloat[curveIndex + 2]._values[i],
        //                parser._curveKeyFramesFloat[curveIndex + 3]._values[i]
        //                );

        //            result.AddKey(time, value);
        //        }
                
        //    }

        //    return result;
        //}

        //private static QuickAnimationCurve ParseCurveMuscle(QuickAnimationParser parser, int muscleID)
        //{
        //    QuickAnimationCurve result = new QuickAnimationCurve(QuickHumanTrait.GetMuscleName(muscleID));

        //    int numKeys = parser._curveKeyFramesByte[muscleID]._keys.Count;

        //    for (int i = 0; i < numKeys; i++)
        //    {
        //        float time = parser._curveKeyFramesByte[muscleID]._keys[i];
        //        byte bValue = parser._curveKeyFramesByte[muscleID]._values[i];

        //        float muscleMin = QuickHumanTrait.GetMuscleDefaultMin(muscleID);
        //        float muscleMax = QuickHumanTrait.GetMuscleDefaultMax(muscleID);
        //        float value = (bValue / 255.0f) * (muscleMax - muscleMin) + muscleMin;

        //        //Debug.Log("bValue = " + bValue);
        //        //Debug.Log("muscleMin = " + muscleMin.ToString("f3"));
        //        //Debug.Log("muscleMax = " + muscleMax.ToString("f3"));
        //        //Debug.Log("value = " + value.ToString("f3"));

        //        result.AddKey(time, Mathf.Clamp(value, muscleMin, muscleMax));
        //    }

        //    return result;
        //}

        public static string ToJson(QuickAnimation animation)
        {
            return JsonUtility.ToJson(ToAnimationParser(animation));
        }

        public static QuickAsyncOperation<string> ToJsonAsync(QuickAnimation animation)
        {
            QuickAsyncOperation<string> op = new QuickAsyncOperation<string>();

            Thread thread = new Thread
            (
                () => 
                {
                    op._result = ToJson(animation);
                }
            );
            thread.Start();
            
            return op;
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

        public static void SaveToAnim(string path, QuickAnimation animation)
        {
            QuickAnimWriter aWriter = new QuickAnimWriter();
            aWriter.WriteAnimation(path, animation);
        }

    }

}


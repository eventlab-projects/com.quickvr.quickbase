using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Xml;
using System.Reflection;

namespace QuickVR
{

    public static class QuickScriptableObjectExtensions
    {

        #region SAVE METHODS

        public static void SaveToXml(this ScriptableObject obj, string path)
        {
            if (path.Length == 0) return;

            XmlDocument doc = new XmlDocument();
            XmlElement nodeFields = doc.AddElement("ClassFields");

            List<FieldInfo> fields = QuickUtils.GetAllFieldsConfigurable(obj.GetType());
            foreach (FieldInfo f in fields)
            {
                XmlElement e = nodeFields.AddElement("Field");
                e.SetAttribute("name", f.Name);
                SaveField(f.GetValue(obj), e);
            }

            doc.Save(path);
        }

        private static void SaveField(object fieldObj, XmlElement node)
        {
            if (fieldObj == null)
            {
                node.AddElement("value").SetValue("");
                return;
            }

            Type fieldType = fieldObj.GetType();
            if (IsPrimitiveType(fieldType)) node.AddElement("value").SetValue(fieldObj);
            else if (fieldType == typeof(Vector2)) SaveField(ToFloatArray((Vector2)fieldObj), node);
            else if (fieldType == typeof(Vector3)) SaveField(ToFloatArray((Vector3)fieldObj), node);
            else if (fieldType == typeof(Vector4)) SaveField(ToFloatArray((Vector4)fieldObj), node);
            else if (fieldType == typeof(Quaternion)) SaveField(ToFloatArray((Quaternion)fieldObj), node);
            else if (fieldType == typeof(Color)) SaveField(ToFloatArray((Color)fieldObj), node);
            else if (fieldType.IsArray || (fieldObj is IList && fieldType.IsGenericType))
            {
                IList arrayObjects = (IList)fieldObj;
                foreach (var o in arrayObjects)
                {
                    SaveField(o, node.AddElement("ArrayElement"));
                }
            }
            else if (fieldType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
#if UNITY_EDITOR
                string assetPath = UnityEditor.AssetDatabase.GetAssetPath(((UnityEngine.Object)fieldObj).GetInstanceID());
                node.AddElement("value").SetValue(UnityEditor.AssetDatabase.AssetPathToGUID(assetPath));
#endif
            }
            else
            {
                List<FieldInfo> fields = QuickUtils.GetAllFieldsConfigurable(fieldObj.GetType());
                foreach (FieldInfo f in fields)
                {
                    XmlElement e = node.AddElement("Field");
                    e.SetAttribute("name", f.Name);
                    SaveField(f.GetValue(fieldObj), e);
                }
            }
        }

        private static bool IsPrimitiveType(Type t)
        {
            return (t.IsPrimitive || t == typeof(string) || t.IsEnum);
        }

        private static bool IsUnityFloatContainer(Type t)
        {
            return 
                (
                    t == typeof(Vector2) ||
                    t == typeof(Vector3) ||
                    t == typeof(Vector4) ||
                    t == typeof(Quaternion) ||
                    t == typeof(Color)
                );
        }

        private static float[] ToFloatArray(Vector2 v)
        {
            return new float[] { v.x, v.y };
        }

        private static float[] ToFloatArray(Vector3 v)
        {
            return new float[] { v.x, v.y, v.z };
        }

        private static float[] ToFloatArray(Vector4 v)
        {
            return new float[] { v.x, v.y, v.z, v.w };
        }

        private static float[] ToFloatArray(Quaternion q)
        {
            return new float[] { q.x, q.y, q.z, q.w };
        }

        private static float[] ToFloatArray(Color c)
        {
            return new float[] { c.r, c.g, c.b, c.a };
        }

        #endregion

        #region LOAD METHODS

        public static void LoadFromXml(this ScriptableObject obj, string path)
        {
            if (path.Length == 0) return;

            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            QuickVRManager.Log("path = " + path);

            LoadClass(obj, (XmlElement)doc.FirstChild.FirstChild);
        }

        private static void LoadClass(object obj, XmlElement nodeClass)
        {
            foreach (XmlNode n in nodeClass.ChildNodes)
            {
                XmlElement nodeField = (XmlElement)n;
                FieldInfo f = obj.GetType().GetField(nodeField.GetAttribute("name"));
                if (f == null) continue;

                object tmp = LoadNodeValue(f.FieldType, nodeField);
                if (tmp != null) f.SetValue(obj, tmp);
                else
                {
                    if (typeof(IList).IsAssignableFrom(f.FieldType))    //is an array?
                    {
                        Type elementType = f.FieldType.IsArray ? f.FieldType.GetElementType() : f.FieldType.GetGenericArguments()[0];
                        IList array = Array.CreateInstance(elementType, nodeField.ChildNodes.Count);
                        for (int i = 0; i < nodeField.ChildNodes.Count; i++)
                        {
                            array[i] = LoadNodeValue(elementType, (XmlElement)nodeField.ChildNodes[i]);
                        }
                        if (f.FieldType.IsArray) f.SetValue(obj, array);
                        else f.SetValue(obj, Activator.CreateInstance(f.FieldType, array));
                    }
                    else if (f.FieldType.IsClass) LoadClass(f.GetValue(obj), nodeField);
                }
            }
        }

        private static object LoadNodeValue(Type valueType, XmlElement nodeField)
        {
            if (valueType == typeof(int)) return QuickUtils.ParseInt(nodeField.FirstChild.InnerText);
            else if (valueType == typeof(float)) return QuickUtils.ParseFloat(nodeField.FirstChild.InnerText);
            else if (valueType == typeof(string)) return nodeField.FirstChild.InnerText;
            else if (valueType == typeof(bool)) return QuickUtils.ParseBool(nodeField.FirstChild.InnerText);
            else if (valueType.IsEnum) return QuickUtils.ParseEnum(valueType, nodeField.FirstChild.InnerText);
            else if (IsUnityFloatContainer(valueType)) return LoadUnityFloatContainer(valueType, nodeField);
            else if (valueType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
#if UNITY_EDITOR
                string guid = nodeField.FirstChild.InnerText;
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                return UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, valueType);
#endif
            }
            //else if (valueType.IsClass) return Activator.CreateInstance(valueType);

            return null;
        }

        private static object LoadUnityFloatContainer(Type t, XmlElement nodeField)
        {
            float[] tmp = LoadArray<float>(nodeField);
            if (t == typeof(Vector2)) return Activator.CreateInstance(t, tmp[0], tmp[1]);
            if (t == typeof(Vector3)) return Activator.CreateInstance(t, tmp[0], tmp[1], tmp[2]);
            if (t == typeof(Vector4)) return Activator.CreateInstance(t, tmp[0], tmp[1], tmp[2]);
            return Activator.CreateInstance(t, tmp[0], tmp[1], tmp[2], tmp[3]);
        }

        private static T[] LoadArray<T>(XmlElement nodeField)
        {
            T[] result = new T[nodeField.ChildNodes.Count];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = (T)LoadNodeValue(typeof(T), (XmlElement)nodeField.ChildNodes[i]);
            }

            return result;
        }

        #endregion

    }

}

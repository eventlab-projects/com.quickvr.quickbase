using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.IO;

namespace QuickVR
{

    [System.Serializable]
    public class Pair<T, U>
    {
        [SerializeField]
        public T First;

        [SerializeField]
        public U Second;

        public Pair()
            : base()
        {

        }

        public Pair(T first, U second)
            : base()
        {
            this.First = first;
            this.Second = second;
        }
    };

    public enum GameMode
    {
        SINGLE_PLAYER,
        MULTI_PLAYER,
    };

    public static class QuickUtils
    {

        public delegate void CloseApplicationAction();
        public static event CloseApplicationAction OnCloseApplication;

        public static GameMode _gameMode = GameMode.SINGLE_PLAYER;

        public static Mesh CreateFullScreenQuad()
        {
            Mesh m = new Mesh();
            m.name = "_FullScreenQuad_";

            //Create the vertices
            m.vertices = new Vector3[] {
				new Vector3(1f, 1f, 0),
				new Vector3(-1f, 1f, 0),
				new Vector3(-1f, -1f, 0),
				new Vector3(1f, -1f, 0)
			};

            //Create the UVs
            m.uv = new Vector2[] {
				new Vector2(0, 0),
				new Vector2(1, 0),
				new Vector2(1, 1),
				new Vector2(0, 1)
			};

            //Create the triangle list
            m.triangles = new int[] { 0, 1, 2, 2, 3, 0 };

            //Other stuff
            m.RecalculateNormals();
            m.RecalculateBounds();
            
            return m;
        }

        public static Mesh CreateFullScreenQuadY()
        {
            Mesh m = new Mesh();
            m.name = "_FullScreenQuad_";

            //Create the vertices
            m.vertices = new Vector3[] {
                new Vector3(1f, 0, 1f),
                new Vector3(-1f, 0, 1f),
                new Vector3(-1f, 0, -1f),
                new Vector3(1f, 0, -1f)
            };

            //Create the UVs
            m.uv = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };

            //Create the triangle list
            m.triangles = new int[] { 2, 1, 0, 0, 3, 2 };

            //Other stuff
            m.RecalculateNormals();
            m.RecalculateBounds();

            return m;
        }

        public static List<T> GetEnumValues<T>()
        {
            return new List<T>((IEnumerable<T>)(Enum.GetValues(typeof(T))));
        }

        public static List<string> GetEnumValuesToString<T>()
        {
            List<T> eValues = GetEnumValues<T>();
            List<string> names = new List<string>();
            foreach (T val in eValues) names.Add(val.ToString());
            return names;
        }

        public static List<int> GetEnumValuesToInt<T>() where T : IConvertible
        {
            return new List<int>((IEnumerable<int>)(Enum.GetValues(typeof(T))));
        }

        public static int ParseInt(string value)
        {
            int result;
            int.TryParse(value, out result);
            return result;
        }

        public static float ParseFloat(string value)
        {
            float result;
            float.TryParse(value, out result);
            return result;
        }

        public static bool ParseBool(string value)
        {
            bool result;
            bool.TryParse(value, out result);
            return result;
        }

        public static bool IsEnumValue<T>(string value)
        {
            object obj = ParseEnum(typeof(T), value);
            return obj != null;
        }

        public static T ParseEnum<T>(string value)
        {
            object obj = ParseEnum(typeof(T), value);
            return (obj != null)? (T)obj : default(T);
        }

        public static object ParseEnum(System.Type t, string value)
        {
            return (System.Enum.IsDefined(t, value)) ? System.Enum.Parse(t, value) : null;
        }

        public static string[] ToStringArray<T>(T[] array)
        {
            string[] result = new string[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                result[i] = array[i].ToString();
            }

            return result;
        }

        public static string GetDataPath()
        {
            string dataPath = Application.dataPath;
            dataPath = dataPath.Remove(dataPath.LastIndexOf("/"));
            dataPath = dataPath.Replace(@"/", @"\");
            return dataPath;
        }

        public static string GetUsersFolder()
        {
#if !UNITY_ANDROID
            //Check if the global Users directory exists
            string usersFolder = GetDataPath() + @"\Users";
            if (!Directory.Exists(usersFolder))
            {
                Debug.Log("Creating Users Folder = " + usersFolder);
                Directory.CreateDirectory(usersFolder);
            }
            return usersFolder;
#else
            return "";
#endif
        }
        
        public static string GetUserFolder()
        {
            return GetUserFolder(SettingsBase.GetSubjectID().ToString());
        }

        public static string GetUserFolder(string userID)
        {
            //Check if the concrete User's folder exists
#if !UNITY_ANDROID
            string userFolder = GetUsersFolder() + @"\" + userID;
            if (!Directory.Exists(userFolder))
            {
                Debug.Log("Creating User Folder = " + userFolder);
                Directory.CreateDirectory(userFolder);
            }

            return userFolder;
#else
            return "";
#endif
        }

        public static string GetRelativeAssetsPath(string path)
        {
            return (path.StartsWith(Application.dataPath)) ? "Assets" + path.Substring(Application.dataPath.Length) : "";
        }

        public static void CloseApplication()
        {
            if (OnCloseApplication != null) OnCloseApplication();

		    Application.Quit();
        }

        public static void SaveCameraRenderToFile(string file, Camera cam, int width, int height)
        {

            RenderTexture currentCameraRT = cam.targetTexture;
            RenderTexture tmpCameraRT = RenderTexture.GetTemporary(width, height, 24);
            cam.targetTexture = tmpCameraRT;
            cam.Render();

            Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = tmpCameraRT;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

            cam.targetTexture = currentCameraRT;
            RenderTexture.active = currentActiveRT;

            byte[] bytes = screenShot.EncodeToJPG();
            System.IO.File.WriteAllBytes(file, bytes);

            Texture2D.Destroy(screenShot);
            RenderTexture.ReleaseTemporary(tmpCameraRT);
        }

        public static List<string> GetLayerNames()
        {
            List<string> layerNames = new List<string>();
            for (int i = 0; i < 32; i++)
            {
                string lName = LayerMask.LayerToName(i);
                if (lName.Length != 0) layerNames.Add(lName);
            }
            return layerNames;
        }

        public static bool Invoke(object obj, string methodName, object[] parameters = null)
        {
            System.Reflection.MethodInfo m = obj.GetType().GetMethod(methodName);
            if (m != null) m.Invoke(obj, parameters);
            else Debug.LogError("Invoke failed!!! The method " + methodName + " does not exist on class " + obj.GetType().FullName);

            return m != null;
        }

        public static void CopyFields(object src, object dst)
        {
            Dictionary<string, object> srcFields = new Dictionary<string, object>();
            List<FieldInfo> fields = GetAllFieldsConfigurable(src.GetType()); //src.GetType().GetFields();
            foreach (FieldInfo f in fields)
            {
                srcFields[f.Name] = f.GetValue(src);
            }

            fields = GetAllFieldsConfigurable(dst.GetType());
            foreach (FieldInfo f in fields)
            {
                if (srcFields.ContainsKey(f.Name))
                {
                    f.SetValue(dst, srcFields[f.Name]);
                }
            }
        }

        public static List<FieldInfo> GetAllFields(Type t)
        {
            if ((t == typeof(MonoBehaviour)) || (t == typeof(ScriptableObject)) || (t == null)) return new List<FieldInfo>();

            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            List<FieldInfo> result = GetAllFields(t.BaseType);
            result.AddRange(t.GetFields(flags));

            return result;
        }

        public static List<FieldInfo> GetAllFieldsConfigurable(Type t)
        {
            List<FieldInfo> tmp = GetAllFields(t);
            List<FieldInfo> result = new List<FieldInfo>();
            foreach (FieldInfo f in tmp)
            {
                if (IsFieldConfigurable(f)) result.Add(f);
            }

            return result;
        }

        public static bool IsFieldConfigurable(FieldInfo f)
        {
            return (GetCustomAttribute<HideInInspector>(f) == null && (f.IsPublic || System.Attribute.GetCustomAttribute(f, typeof(SerializeField)) != null));
        }

        public static T GetCustomAttribute<T>(MemberInfo f) where T : Attribute
        {
            return Attribute.GetCustomAttribute(f, typeof(T)) as T;
        }

        public static T GetCustomAttribute<T>(Type t) where T : Attribute
        {
            return Attribute.GetCustomAttribute(t, typeof(T)) as T;
        }

        public static string GetTypeFullName(System.Type t)
        {
            string result = t.AssemblyQualifiedName.Split(',')[0].Replace('+', '.');
            if (result.Contains("`"))
            {
                //The type contains generic arguments
                result = result.Split('`')[0];

                result += "<";
                Type[] generics = t.GetGenericArguments();
                for (int i = 0; i < generics.Length; i++)
                {
                    result += GetTypeFullName(generics[i]);
                    if (i < (generics.Length - 1)) result += ", ";
                }
                result += ">";
            }

            return result;
        }

        public static bool IsSameClassOrDerived(Type Derived, Type Base)
        {
            return Derived.IsSubclassOf(Base) || Derived == Base;
        }

        public static Mesh GetUnityPrimitiveMesh(PrimitiveType primitiveType)
        {
            Debug.Log("Getting Unity Primitive Mesh: " + primitiveType);
            Mesh primMesh = Resources.GetBuiltinResource<Mesh>(GetPrimitiveMeshPath(primitiveType));

            if (primMesh == null)
            {
                Debug.LogError("Couldn't load Unity Primitive Mesh: " + primitiveType);
            }

            return primMesh;
        }

        private static string GetPrimitiveMeshPath(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.Sphere:
                    return "New-Sphere.fbx";
                case PrimitiveType.Capsule:
                    return "New-Capsule.fbx";
                case PrimitiveType.Cylinder:
                    return "New-Cylinder.fbx";
                case PrimitiveType.Cube:
                    return "Cube.fbx";
                case PrimitiveType.Plane:
                    return "New-Plane.fbx";
                case PrimitiveType.Quad:
                    return "Quad.fbx";
                default:
                    throw new ArgumentOutOfRangeException(nameof(primitiveType), primitiveType, null);
            }
        }

        public static bool IsMobileTarget()
        {
#if UNITY_ANDROID
            return true;
#else
            return false;
#endif
        }

        public static bool IsOculusQuest()
        {
            return UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.Head).name.ToLower().Contains("quest");
            //return UnityEngine.XR.XRDevice.model.ToLower().Contains("quest");
        }

        public static bool IsHandTrackingSupported()
        {
            return IsOculusQuest();
        }

        #region EXTENSION METHODS

        public static int GetNumSiblings(this Component c)
        {
            return c.transform.parent ? c.transform.parent.childCount : c.gameObject.scene.rootCount;
        }

        public static T GetNextSibling<T>(this T t) where T : Component
        {
            int sIndex = t.transform.GetSiblingIndex() + 1;
            while (sIndex < GetNumSiblings(t))
            {
                Transform tChild = t.transform.parent ? t.transform.parent.GetChild(sIndex) : t.gameObject.scene.GetRootGameObjects()[sIndex].transform;
                if (tChild.gameObject.activeInHierarchy) return tChild.GetComponent<T>();
                sIndex++;
            }
            
            return null;
        } 

        public static T GetOrCreateComponent<T>(this Component c) where T : Component
        {
            return c.gameObject.GetOrCreateComponent<T>();
        }

        public static T GetOrCreateComponent<T>(this GameObject go) where T : Component
        {
            if (!go.GetComponent<T>()) go.AddComponent<T>();
            return go.GetComponent<T>();
        }

        public static bool InViewportBounds(this Camera cam, Vector3 position)
        {
            if (!cam) return false;

            Vector3 vPos = cam.WorldToViewportPoint(position);
            return
            (
                (vPos.x >= 0) && (vPos.x <= 1) &&
                (vPos.y >= 0) && (vPos.y <= 1)
            );
        }

        public static Vector3 ClampPositionByViewport(this Camera cam, Vector3 position)
        {
            Vector3 vPos = cam.WorldToViewportPoint(position);
            vPos = new Vector3(Mathf.Clamp01(vPos.x), Mathf.Clamp01(vPos.y), vPos.z);
            return cam.ViewportToWorldPoint(vPos);
        }

        public static string ExecuteCMDCommand(string command, bool hidden, bool waitForAnswer)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            if (hidden)
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            else
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Minimized;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = command;
            process.StartInfo = startInfo;
            process.StartInfo.CreateNoWindow = hidden;
            process.Start();

            if (waitForAnswer) return process.StandardOutput.ReadToEnd().Trim();
            return "";
        }

        public static Dictionary<HumanBodyBones, Vector3> GetLocalPositions(this Animator animator)
        {
            List<HumanBodyBones> bones = QuickUtils.GetEnumValues<HumanBodyBones>();
            Dictionary<HumanBodyBones, Vector3> result = new Dictionary<HumanBodyBones, Vector3>();
            foreach (HumanBodyBones boneID in bones)
            {
                Transform b = animator.GetBoneTransform(boneID);
                result[boneID] = b ? b.localPosition : Vector3.zero;
            }
            return result;
        }

        public static void ResetTransformation(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        public static void GetProperties(this Transform transform, out Vector3 pos, out Quaternion rot)
        {
            pos = transform.position;
            rot = transform.rotation;
        }

        public static void SetProperties(this Transform transform, Vector3 pos, Quaternion rot)
        {
            transform.position = pos;
            transform.rotation = rot;
        }

        public static Transform CreateChild(this Transform transform, string name, bool checkName = true)
        {
            Transform t = transform.Find(name);
            if (!t || !checkName)
            {
                t = new GameObject(name).transform;
                t.parent = transform;
                t.ResetTransformation();
            }

            return t;
        }

        public static Transform Find(this Transform transform, string name, bool recursive = false)
        {
            if (!recursive) return transform.Find(name);

            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(transform);
            while (queue.Count > 0)
            {
                Transform c = queue.Dequeue();
                if (c.name.CompareTo(name) == 0) return c;
                foreach (Transform t in c) queue.Enqueue(t);
            }
            return null;
        }

        public static void DestroyChild(this Transform transform, int childID)
        {
            if ((transform.childCount == 0) || (childID >= transform.childCount)) return;

            UnityEngine.Object.Destroy(transform.GetChild(childID).gameObject);
        }

        public static void DestroyChildImmediate(this Transform transform, int childID)
        {
            if ((transform.childCount == 0) || (childID >= transform.childCount)) return;

            UnityEngine.Object.DestroyImmediate(transform.GetChild(childID).gameObject);
        }

        public static void DestroyChilds(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        public static void DestroyChildsImmediate(this Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }

        public static Vector3 GetEyeCenterPosition(this Animator animator)
        {
            Transform lEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            Transform rEye = animator.GetBoneTransform(HumanBodyBones.RightEye);
            if (lEye && rEye) return Vector3.Lerp(lEye.position, rEye.position, 0.5f);
            if (lEye) return lEye.position;
            if (rEye) return rEye.position;

            return animator.GetBoneTransform(HumanBodyBones.Head).position;
        }

        public static Transform GetLookAtBone(this Animator animator, HumanBodyBones boneID)
        {
            HumanBodyBones? lookAtBone = QuickHumanTrait.GetLookAtBone(boneID);
            if (!lookAtBone.HasValue) return null;

            Transform tLookAt = animator.GetBoneTransform(lookAtBone.Value);
            if (tLookAt) return tLookAt;

            return animator.GetLookAtBone(lookAtBone.Value);
        }

        public static void Shuffle<T>(this IList<T> ts)
        {
            var count = ts.Count;
            var last = count - 1;
            for (var i = 0; i < last; ++i)
            {
                var r = UnityEngine.Random.Range(i, count);
                var tmp = ts[i];
                ts[i] = ts[r];
                ts[r] = tmp;
            }
        }

        public static float GetElapsedSeconds(this System.Diagnostics.Stopwatch watch)
        {
            return watch.ElapsedMilliseconds / 1000.0f;
        }

#endregion

    }
}
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

using UnityEngine;
using UnityEngine.SceneManagement;

using System.Linq;

namespace QuickVR
{

    public class QuickPlayerPrefs
    {

        #region PRIVATE PARAMETERS

        private static QuickSettingsAsset _settings = null;

        #endregion

        #region EVENTS

        public delegate void SetValueAction();
        public static event SetValueAction OnSetValue;

        #endregion

        #region CREATION AND DESTRUCTION

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            //Check if the asset exists
            _settings = Resources.Load<QuickSettingsAsset>("QuickSettingsCustom");
            if (_settings == null)
            {
                _settings = ScriptableObject.CreateInstance<QuickSettingsAsset>();
            }
        }

        #endregion

        #region GET AND SET

        public static QuickSettingsAsset GetSettingsAsset()
        {
            return _settings;
        }

        public static List<string> GetBuildScenes()
        {
            List<string> result = new List<string>();
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                if (path.Length > 0) result.Add(path);
            }

            return result;
        }

        public static bool HasKey(string key)
        {
            return _settings.IsSettingDefined(key);
        }

        public static string GetString(string key, string defaultValue = "")
        {
            QuickSetting s = _settings.GetSetting(key);
            if (s == null) s = _settings.CreateSetting(key);

            return s.GetValue();
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            int result;
            if (!int.TryParse(GetString(key, defaultValue.ToString()), out result)) result = defaultValue;

            return result;
        }

        public static float GetFloat(string key, float defaultValue = 0)
        {
            float result;
            string s = GetString(key, defaultValue.ToString());
            s = s.Replace(',', '.');
            NumberStyles numStyle = NumberStyles.Float | NumberStyles.AllowThousands;
            CultureInfo cInfo = CultureInfo.InvariantCulture;

            if (!float.TryParse(s, numStyle, cInfo, out result))
            {
                result = defaultValue;
            }

            return result;
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            bool result;
            if (!bool.TryParse(GetString(key, defaultValue.ToString()), out result)) result = defaultValue;

            return result;
        }

        public static T GetEnum<T>(string key, T defaultValue = default(T)) where T : struct
        {
            return QuickUtils.ParseEnum<T>(GetString(key, defaultValue.ToString()));
        }

        public static void SetValue(string key, object value)
        {
            QuickSetting s = _settings.GetSetting(key);
            if (s == null) s = _settings.CreateSetting(key);

            s.SetValue(value);

            if (OnSetValue != null) OnSetValue();
        }

        public static QuickSetting GetSetting(string key)
        {
            return _settings.GetSetting(key);
        }

        public static List<QuickSetting> GetSettingsBase()
        {
            return _settings._settingsBase.OrderBy(o => o.GetOrder()).ToList();
        }

        public static List<QuickSetting> GetSettingsCustom()
        {
            return _settings._settingsCustom.OrderBy(o => o.GetOrder()).ToList();
        }

        public static void DeleteSetting(string key)
        {
            _settings.RemoveSetting(key);
        }

        public static void ResetAllSettings()
        {
            foreach (QuickSetting s in _settings._settingsBase)
            {
                s.ResetValue();
            }

            foreach (QuickSetting s in _settings._settingsCustom)
            {
                s.ResetValue();
            }
        }

        public static void ResetSetting(string key)
        {
            QuickSetting s =_settings.GetSetting(key);
            if (s != null)
            {
                s.ResetValue();
            }
        }

        #endregion

    }

}


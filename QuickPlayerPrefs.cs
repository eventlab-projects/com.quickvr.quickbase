using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

namespace QuickVR
{

    [System.Serializable]
    public class QuickPlayerPrefs
    {

        #region PRIVATE PARAMETERS

        private static QuickSettingsAsset _settings = null;

        #endregion

        #region CREATION AND DESTRUCTION

        private static void CheckSettings()
        {
            if (_settings != null) return;

            //Check if the asset exists
            _settings = Resources.Load<QuickSettingsAsset>("QuickSettingsCustom");
            if (_settings == null)
            {
                _settings = ScriptableObject.CreateInstance<QuickSettingsAsset>();
            }

            if (Application.isPlaying)
            {
                _settings.LoadPlayerPrefs();
            }
        }

        #endregion

        #region GET AND SET

        public static void ComputeBuildScenes()
        {
            CheckSettings();

            _settings.ComputeBuildScenes();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_settings);
#endif
        }

        public static List<string> GetBuildScenes()
        {
            ComputeBuildScenes();
            
            return _settings.GetBuildScenes();
        }

        public static void ClearSettingsCustom()
        {
            CheckSettings();

            _settings.ClearSettingsCustom();
        }

        public static bool HasKey(string key)
        {
            CheckSettings();

            return _settings.IsSettingDefined(key);
        }

        public static string GetString(string key, string defaultValue = "")
        {
            CheckSettings();
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
            if (!float.TryParse(GetString(key, defaultValue.ToString()), out result)) result = defaultValue;

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
            CheckSettings();
            QuickSetting s = _settings.GetSetting(key);
            if (s == null) s = _settings.CreateSetting(key);

            s.SetValue(value);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_settings);
#endif
        }

        public static QuickSetting GetSetting(string key)
        {
            CheckSettings();
            return _settings.GetSetting(key);
        }

        public static List<QuickSetting> GetSettingsBase()
        {
            CheckSettings();
            return _settings._settingsBase.OrderBy(o => o.GetOrder()).ToList();
        }

        public static List<QuickSetting> GetSettingsCustom()
        {
            CheckSettings();
            return _settings._settingsCustom.OrderBy(o => o.GetOrder()).ToList();
        }

        public static void DeleteSetting(string key)
        {
            CheckSettings();
            _settings.RemoveSetting(key);
        }

        #endregion

    }

}


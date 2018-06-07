﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace QuickVR
{

    [System.Serializable]
    public class QuickSetting
    {
        #region PROTECTED PARAMETERS

        [SerializeField]
        protected string _key = "";

        [SerializeField]
        protected string _value = "";

        [SerializeField]
        protected string _type = "";

        [SerializeField]
        protected bool _visibleInMenu = true;

        [SerializeField]
        protected int _order = 0;

        #endregion

        #region CREATION AND DESTRUCTION

        public QuickSetting(string key)
        {
            _key = key;
            _type = "";
            _visibleInMenu = true;
        }

        #endregion

        #region GET AND SET

        public virtual string GetKey()
        {
            return _key;
        }

        public virtual string GetValue()
        {
            return _value;
        }

        public virtual void SetValue(object value)
        {
            _value = value.ToString();

            if (_type == "")
            {
                _type = value.GetType().AssemblyQualifiedName;
            }

            PlayerPrefs.SetString(_key, _value);
            PlayerPrefs.Save();
        }

        public virtual int GetOrder()
        {
            return _order;
        }

        public virtual void SetOrder(int order)
        {
            _order = order;
        }

        public virtual string GetTypeName()
        {
            return _type;
        }

        public virtual void SetVisibleInMenu(bool visible)
        {
            _visibleInMenu = visible;
        }

        public virtual bool IsVisibleInMenu()
        {
            return _visibleInMenu;
        }

        #endregion

    }

    [System.Serializable]
    public class QuickSettingsAsset : ScriptableObject
    {

        #region PUBLIC PARAMETERS

        [SerializeField]
        public List<QuickSetting> _settingsBase = new List<QuickSetting>();

        [SerializeField]
        public List<QuickSetting> _settingsCustom = new List<QuickSetting>();

        #endregion

        #region PROTECTED PARAMETERS

        [SerializeField]
        protected List<string> _buildScenes = new List<string>();

        #endregion

        #region CREATION AND DESTRUCTION

        public virtual QuickSetting CreateSetting(string key)
        {
            QuickSetting s = GetSetting(key);
            if (s == null)
            {
                s = new QuickSetting(key);
                List<QuickSetting> settings = IsSettingBase(key) ? _settingsBase : _settingsCustom;
                settings.Add(s);
            }

            return s;
        }

        public virtual void RemoveSetting(string key)
        {
            int idSetting = -1;
            for (int i = 0; (i < _settingsCustom.Count) && (idSetting == -1); i++)
            {
                if (_settingsCustom[i].GetKey() == key) idSetting = i;
            }

            if (idSetting != -1)
            {
                _settingsCustom.RemoveAt(idSetting);
            }
        }

        #endregion

        #region GET AND SET

        public virtual void ComputeBuildScenes()
        {
#if UNITY_EDITOR
            //Find the enabled scenes in the editor window. 
            _buildScenes.Clear();
            UnityEditor.EditorBuildSettingsScene[] allScenes = UnityEditor.EditorBuildSettings.scenes;
            foreach (UnityEditor.EditorBuildSettingsScene s in allScenes)
            {
                if (!s.enabled) continue;
                _buildScenes.Add(s.path);
            }
#endif
        }

        public virtual List<string> GetBuildScenes()
        {
            ComputeBuildScenes();

            return _buildScenes;
        }

        public virtual void LoadPlayerPrefs()
        {
            foreach (QuickSetting s in _settingsBase)
            {
                string key = s.GetKey();
                s.SetValue(PlayerPrefs.GetString(key));
            }

            foreach (QuickSetting s in _settingsCustom)
            {
                string key = s.GetKey();
                s.SetValue(PlayerPrefs.GetString(key));
            }
        }

        public virtual void ClearSettingsCustom()
        {
            _settingsCustom.Clear();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public virtual QuickSetting GetSetting(string key)
        {
            QuickSetting result = null;
            List<QuickSetting> allSettings = new List<QuickSetting>(_settingsBase);
            allSettings.AddRange(_settingsCustom);
            foreach (QuickSetting s in allSettings)
            {
                if (s.GetKey() == key)
                {
                    result = s;
                    break;
                }
            }

            return result;
        }

        public virtual bool IsSettingDefined(string key)
        {
            return GetSetting(key) != null;
        }

        protected virtual bool IsSettingBase(string key)
        {
            bool isBase = false;
            List<string> values = QuickUtils.GetEnumValuesToString<SettingsBase.Key>();
            foreach (string s in values)
            {
                if (s == key)
                {
                    isBase = true;
                    break;
                }
            }

            return isBase;
        }

        #endregion

    }
}

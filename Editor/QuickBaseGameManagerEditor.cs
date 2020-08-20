using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace QuickVR
{

    [CustomEditor(typeof(QuickBaseGameManager), true)]
    public class QuickBaseGameManagerEditor : QuickBaseEditor
    {

        #region PROTECTED ATTRIBUTES

        protected QuickBaseGameManager _target = null;

        protected string[] _days = null;
        protected string[] _months = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        protected string[] _years = null;

        protected int _selectedDayID = 0;
        protected int _selectedMonthID = 0;
        protected int _selectedYearID = 0;

        #endregion

        #region CONSTANTS

        protected const int YEAR_MIN = 2010;
        protected const int YEAR_MAX = 2030;

        #endregion

        #region CREATION AND DESTRUCTION

        protected virtual void OnEnable()
        {
            int day, month, year;
            _target = (QuickBaseGameManager)target;
            _target.GetExpirationDate(out day, out month, out year);
            _selectedDayID = Mathf.Max(0, Mathf.Min(day - 1, 30));
            _selectedMonthID = Mathf.Clamp(month - 1, 0, 11);
            _selectedYearID = Mathf.Clamp(year, YEAR_MIN, YEAR_MAX) - YEAR_MIN;
        }

        protected virtual void InitDays()
        {
            List<string> tmp = new List<string>();
            for (int i = 1; i <= 31; i++) tmp.Add(i.ToString());

            _days = tmp.ToArray();
        }

        protected virtual void InitYears()
        {
            List<string> tmp = new List<string>();
            for (int i = YEAR_MIN; i <= YEAR_MAX; i++) tmp.Add(i.ToString());

            _years = tmp.ToArray();
        }

        #endregion

        protected override void DrawGUI()
        {
            base.DrawGUI();

            DrawPropertyField("_useExpirationDate", "Use Expiration Date");
            if (_target._useExpirationDate)
            {
                if (_days == null) InitDays();
                if (_years == null) InitYears();

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                _selectedDayID = EditorGUILayout.Popup(_selectedDayID, _days);
                _selectedMonthID = EditorGUILayout.Popup(_selectedMonthID, _months);
                _selectedYearID = EditorGUILayout.Popup(_selectedYearID, _years);
                
                if (EditorGUI.EndChangeCheck())
                {
                    _target.SetExpirationDate(_selectedDayID + 1, _selectedMonthID + 1, int.Parse(_years[_selectedYearID]));
                    QuickUtilsEditor.MarkSceneDirty();
                }

                EditorGUILayout.EndHorizontal();
            }
        }

    }

}



using UnityEngine;
using QuickVR;

public static class SettingsBase
{

    #region PUBLIC PARAMETERS
    
    public enum Key
    {
        SubjectID,
        Gender,
        Language,
    };

    //public enum Key
    //{
    //    SubjectID,
    //    Gender,
    //    HeightMode,
    //    SubjectHeight,
    //    TimeOutMinutes,
    //    Language,
    //    Environment,
    //    COMPort,
    //};

    public enum Genders
    {
        Male = 0,
        Female = 1,
    };
    
    public enum Languages
    {
        English = 0,
        Spanish = 1,
    };
    
    //public enum Environment
    //{
    //    LOCAL = 0,
    //    LAB1 = 1,
    //    LAB2 = 2,
    //};
    
    #endregion

    #region GET AND SET
    
    public static string GetSubjectID()
    {
        return QuickPlayerPrefs.GetString("SubjectID");
    }
    
    public static void SetSubjectID(string value)
    {
        QuickPlayerPrefs.SetValue("SubjectID", value);
    }

    public static SettingsBase.Genders GetGender()
    {
        return QuickPlayerPrefs.GetEnum<SettingsBase.Genders>("Gender");
    }
    
    public static void SetGender(SettingsBase.Genders value)
    {
        QuickPlayerPrefs.SetValue("Gender", value);
    }
    
    public static SettingsBase.Languages GetLanguage()
    {
        return QuickPlayerPrefs.GetEnum<SettingsBase.Languages>("Language");
    }
    
    public static void SetLanguage(SettingsBase.Languages value)
    {
        QuickPlayerPrefs.SetValue("Language", value);
    }
    
    #endregion

}

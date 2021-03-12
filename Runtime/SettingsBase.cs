using UnityEngine;
using QuickVR;

public static class SettingsBase
{

    #region PUBLIC PARAMETERS
    
    public enum Key
    {
        SubjectID,
        Gender,
        HeightMode,
        SubjectHeight,
        TimeOutMinutes,
        Language,
        Environment,
        COMPort,
    };

    public enum HeightMode
    {
        FromVirtualAvatar = 0,
        FromTrackingSystem = 1,
        FromSubject = 2,
    };
    
    public enum Genders
    {
        MALE = 0,
        FEMALE = 1,
    };
    
    public enum UserHandedness
    {
        RIGHT_HANDED = 0,
        LEFT_HANDED = 1,
    };
    
    public enum Languages
    {
        ENGLISH = 0,
        SPANISH = 1,
    };
    
    public enum Environment
    {
        LOCAL = 0,
        LAB1 = 1,
        LAB2 = 2,
    };
    
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

    public static HeightMode GetHeightMode()
    {
        return QuickPlayerPrefs.GetEnum<HeightMode>("HeightMode");
    }

    public static void SetHeightMode(HeightMode hMode)
    {
        QuickPlayerPrefs.SetValue("HeightMode", hMode);
    } 

    public static float GetSubjectHeight()
    {
        return QuickPlayerPrefs.GetFloat("SubjectHeight");
    }

    public static void SetSubjectHeight(float value)
    {
        QuickPlayerPrefs.SetValue("SubjectHeight", value);
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
    
    public static SettingsBase.Environment GetEnvironment()
    {
        return QuickPlayerPrefs.GetEnum<SettingsBase.Environment>("Environment");
    }
    
    public static void SetEnvironment(SettingsBase.Environment value)
    {
        QuickPlayerPrefs.SetValue("Environment", value);
    }
    
    public static int GetCOMPort()
    {
        return QuickPlayerPrefs.GetInt("COMPort");
    }
    
    public static void SetCOMPort(int value)
    {
        QuickPlayerPrefs.SetValue("COMPort", value);
    }

    public static float GetTimeOutMinutes()
    {
        return QuickPlayerPrefs.GetFloat("TimeOutMinutes");
    }

    public static void SetTimeOutMinutes(float value)
    {
        QuickPlayerPrefs.SetValue("TimeOutMinutes", value);
    }

    #endregion

}

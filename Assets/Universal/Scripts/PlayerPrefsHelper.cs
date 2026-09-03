using UnityEngine;
using System;

public static class PlayerPrefsHelper
{
    public static T GetValue<T>(string key, T defaultValue, T minValue = default, T maxValue = default)
    {
        Type type = typeof(T);

        if (type == typeof(int))
        {
            int val = PlayerPrefs.GetInt(key, Convert.ToInt32(defaultValue));
            Debug.Log("test int " + val);

            int min = Convert.ToInt32(minValue);
            int max = Convert.ToInt32(maxValue);

            if (val < min || val > max) return defaultValue;
            return (T)(object)val;
        }

        if (type == typeof(float))
        {
            float val = PlayerPrefs.GetFloat(key, Convert.ToSingle(defaultValue));

            float min = Convert.ToSingle(minValue);
            float max = Convert.ToSingle(maxValue);

            if (val < min || val > max) return defaultValue;
            return (T)(object)val;
        }

        if (type == typeof(string))
        {
            string val = PlayerPrefs.GetString(key, defaultValue.ToString());
            return (T)(object)val;
        }

        throw new Exception("Type non supporté par PlayerPrefsHelper: " + type);
    }
}

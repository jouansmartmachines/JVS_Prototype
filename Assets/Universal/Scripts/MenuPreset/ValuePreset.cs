using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class ValuePreset : ScriptableObject
{
    public enum PresetEnum
    {
        Easy,
        Medium,
        Hard
    }

    public abstract void SaveValue(PresetEnum level);
    public abstract void RetrieveValue(PresetEnum level);
}

public abstract class ValuePreset<T> : ValuePreset
{
    public string Key => _key;
    [SerializeField] protected string _key;

    public T ValueEasy
    {
        
        get
        {
            var loadedDatas = PresetValueManager.LoadDataFromCsv();
            if(loadedDatas.Any(x => x.Id == Key))
            {
                var data = loadedDatas.Find(x => x.Id == Key);
                return PresetValueManager.GetValue<T>(data.Easy);
            }
            return default;
        }
        set
        {
            var loadedDatas = PresetValueManager.LoadDataFromCsv();
            PresetValueManager.AddOrUpdateRow(loadedDatas, Key, value, ValueMedium, ValueHard);
            //_valueEasy = value;
        }
    }
    //protected T _valueEasy;
    public T ValueMedium
    {
        get
        {
            var loadedDatas = PresetValueManager.LoadDataFromCsv();
            if (loadedDatas.Any(x => x.Id == Key))
            {
                var data = loadedDatas.Find(x => x.Id == Key);
                return PresetValueManager.GetValue<T>(data.Normal);
            }
            return default;
        }
        set
        {
            var loadedDatas = PresetValueManager.LoadDataFromCsv();
            PresetValueManager.AddOrUpdateRow(loadedDatas, Key, ValueEasy, value, ValueHard);
            //_valueMedium = value;
        }
    }
    //protected T _valueMedium;
    public T ValueHard
    {
        get
        {
            var loadedDatas = PresetValueManager.LoadDataFromCsv();
            if (loadedDatas.Any(x => x.Id == Key))
            {
                var data = loadedDatas.Find(x => x.Id == Key);
                return PresetValueManager.GetValue<T>(data.Hard);
            }
            return default;
        }
        set
        {
            var loadedDatas = PresetValueManager.LoadDataFromCsv();
            PresetValueManager.AddOrUpdateRow(loadedDatas, Key, ValueEasy, ValueMedium, value);
            //_valueHard = value;
        }
    }
    //protected T _valueHard;

    public abstract T GetValue(PresetEnum level);
    public abstract void SetValue(PresetEnum level, T value, bool save = false);
}
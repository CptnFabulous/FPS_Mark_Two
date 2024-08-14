using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class VariableFromStringName<T>
{
    // TO FIGURE OUT: how does this affect performance at runtime?
    // And how does this affect access modifiers?

    public static readonly VariableFromStringName<T> value = new VariableFromStringName<T>();
    public T this[object reference, string valueName]
    {
        get => Get(reference, valueName);
        set => Set(reference, valueName, value);
    }

    static Dictionary<string, MemberInfo> memberDictionary = new Dictionary<string, MemberInfo>();

    static MemberInfo GetPropertyInfo(string valueName)
    {
        // Create the dictionary (if null) 
        //memberDictionary ??= new Dictionary<string, MemberInfo>();
        // Return the correct member (or generate and add if not already present)
        return memberDictionary[valueName] ??= typeof(T).GetMember(valueName)[0];
    }

    public static T Get(object toGetFrom, string valueName)
    {
        MemberInfo memberInfo = GetPropertyInfo(valueName);
        switch (memberInfo.MemberType)
        {
            case MemberTypes.Field:
                return (T)((FieldInfo)memberInfo).GetValue(toGetFrom);
            case MemberTypes.Property:
                return (T)((PropertyInfo)memberInfo).GetValue(toGetFrom);
        }

        throw new NotImplementedException();
    }
    public static void Set(object toSetIn, string valueName, T value)
    {
        MemberInfo memberInfo = GetPropertyInfo(valueName);
        switch (memberInfo.MemberType)
        {
            case MemberTypes.Field:
                ((FieldInfo)memberInfo).SetValue(toSetIn, value);
                break;
            case MemberTypes.Property:
                ((PropertyInfo)memberInfo).SetValue(toSetIn, value);
                break;
        }
    }

    /*
    static Dictionary<string, PropertyInfo> dictionary;

    static PropertyInfo GetPropertyInfo(string valueName)
    {
        dictionary ??= new Dictionary<string, PropertyInfo>();

        if (dictionary.ContainsKey(valueName) == false)
        {
            dictionary.Add(valueName, typeof(T).GetProperty(valueName));
        }

        return dictionary[valueName];
    }

    public static T Get(string valueName, object toGetFrom) => (T)GetPropertyInfo(valueName).GetValue(toGetFrom);
    public static void Set(string valueName, object toSetIn, T value) => GetPropertyInfo(valueName).SetValue(toSetIn, value);
    */
}
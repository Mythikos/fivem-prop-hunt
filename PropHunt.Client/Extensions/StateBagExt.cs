﻿using System;
using CitizenFX.Core;

namespace PropHunt.Client.Extensions
{
    /// <summary>
    /// State bag extensions. 
    /// Must exist in server scope for CitizenFX.Core for Client.
    /// </summary>
    internal static class StateBagExt
    {
        public static void Set<T>(this StateBag bag, string key, T value, bool replicated)
        {
            if (typeof(T).IsEnum)
            {
                bag.Set(key, Convert.ChangeType(value, typeof(int)), replicated);
            }
            else
            {
                bag.Set(key, value, replicated);
            }
        }

        public static T Get<T>(this StateBag bag, string key)
        {
            dynamic value = bag.Get(key);
            if (typeof(T).IsEnum)
            {
                if (value == null)
                    return default(T);
                else
                    return Enum.ToObject(typeof(T), bag.Get(key));
            }
            else
            {
                if (value == null)
                    return default(T);
                else
                    return (T)bag.Get(key);
            }
        }
    }
}

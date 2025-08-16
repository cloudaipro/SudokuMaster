using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;


public static class CommonExtensions
{
    #region also/let
    public static R Let<T, R>(this T self, Func<T, R> block)
    {
        return block(self);
    }

    public static T Also<T>(this T self, Action<T> block)
    {
        block(self);
        return self;
    }
    #endregion

    #region row/column method of list
    public static T[] column<T>(this T[,] multidimArray, int wanted_column)
    {
        int l = multidimArray.GetLength(0);
        T[] columnArray = new T[l];
        for (int i = 0; i < l; i++)
        {
            columnArray[i] = multidimArray[i, wanted_column];
        }
        return columnArray;
    }

    public static T[] row<T>(this T[,] multidimArray, int wanted_row)
    {
        int l = multidimArray.GetLength(1);
        T[] rowArray = new T[l];
        for (int i = 0; i < l; i++)
        {
            rowArray[i] = multidimArray[wanted_row, i];
        }
        return rowArray;
    }
    #endregion

    #region enumerable extensions
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> method)
    {
        foreach (T item in enumerable)
        {
            method(item);
        }
    }

    public static void ForEachWithIndex<T>(this IEnumerable<T> enumerable, Action<T, int> handler)
    {
        int idx = 0;
        foreach (T item in enumerable)
            handler(item, idx++);
    }

    public static void foreachReverse<T>(this IEnumerable<T> enumerable, Action<T, int> handler)
    {
        for (int idx = enumerable.Count() - 1; idx >= 0; idx--)
            handler(enumerable.ElementAt(idx), idx);
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action, Func<bool> breakOn)
    {
        foreach (var item in enumerable)
        {
            action(item);
            if (breakOn())
                break;
        }
    }

    public static void ForEachWithIndex<T>(this IEnumerable<T> enumerable, Action<T, int> action, Func<bool> breakOn)
    {
        int idx = 0;
        foreach (T item in enumerable)
        {
            action(item, idx++);
            if (breakOn())
                break;
        }
    }

    public static void foreachReverse<T>(this IEnumerable<T> enumerable, Action<T, int> action, Func<bool> breakOn)
    {
        for (int idx = enumerable.Count() - 1; idx >= 0; idx--)
        {
            action(enumerable.ElementAt(idx), idx);
            if (breakOn())
                break;
        }
    }
    public static IList<T> Shuffle<T>(this IEnumerable<T> list) => list.ToList().Shuffle();
    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        if (list.Count <= 1) return list;
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }
    #endregion

    public static string LeadingZero(this int n, int totalWidth) => n.ToString().PadLeft(totalWidth, '0');

    public static string GetDescription(this Enum value)
    {
        Type type = value.GetType();
        string name = Enum.GetName(type, value);
        if (name != null)
        {
            FieldInfo field = type.GetField(name);
            if (field != null)
            {
                DescriptionAttribute attr =
                       Attribute.GetCustomAttribute(field,
                         typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attr != null)
                {
                    return attr.Description;
                }
            }
        }
        return null;
    }
}

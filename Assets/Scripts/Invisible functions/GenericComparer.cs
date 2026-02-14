using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericComparer<T> : IComparer<T>
{
    public System.Func<T, IComparable> obtainValue;
    public bool reverse;

    public GenericComparer() { }
    public GenericComparer(System.Func<T, IComparable> obtainValue, bool reverse)
    {
        this.obtainValue = obtainValue;
        this.reverse = reverse;
    }

    public int Compare(T x, T y)
    {
        IComparable _x = obtainValue.Invoke(x);
        IComparable _y = obtainValue.Invoke(y);
        return reverse ? _y.CompareTo(_x) : _x.CompareTo(_y);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class UtilityTools 
{
	public static T ToEnum<T>(this string name)
    {
        return (T)Enum.Parse(typeof(T), name);
    }
}

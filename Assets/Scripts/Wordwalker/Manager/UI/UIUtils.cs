using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIUtils
{
    // "Xerp"- Exponential interpretation. Right now only uses one function
    public static Vector3 XerpStandard(Vector3 start, Vector3 end, float input)
    {
        float multiplier = - (Mathf.Pow((2.0f - 2.0f * input), 2.0f)) / 4.0f + 1.0f;
        return Vector3.Lerp(start, end, multiplier);
    }
}

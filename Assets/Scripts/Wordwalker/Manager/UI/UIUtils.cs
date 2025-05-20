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

    public static Vector2 XerpStandard(Vector2 start, Vector2 end, float input)
    {
        float multiplier = -(Mathf.Pow((2.0f - 2.0f * input), 2.0f)) / 4.0f + 1.0f;
        return Vector2.Lerp(start, end, multiplier);
    }


    /// <summary>
    /// Returns a coroutine for XERP moving a UI object from its current destination to another place.
    /// </summary>
    /// <param name="steps">How many frames / iterations of the coroutine to run</param>
    /// <param name="timeSec">Approximately the total amount of time the coroutine will run for</param>
    /// <param name="rectTransform">The rect transform of the UI object to move</param>
    /// <param name="destination">Where the UI object should end up</param>
    /// <returns></returns>
    public static IEnumerator XerpOnUiCoroutine(float steps, float timeSec, RectTransform rectTransform, Vector2 destination)
    {
        Vector2 pos = rectTransform.anchoredPosition;

        for (float i = 0; i <= steps; i++)
        {
            rectTransform.anchoredPosition = XerpStandard(pos,
                    destination,
                    i / steps);

            yield return new WaitForSeconds(1 / steps * timeSec);
        }
    }


    /// <summary>
    /// Returns a coroutine for Lerp moving a UI object from its current destination to another place.
    /// </summary>
    /// <param name="steps">How many frames / iterations of the coroutine to run</param>
    /// <param name="timeSec">Approximately the total amount of time the coroutine will run for</param>
    /// <param name="rectTransform">The rect transform of the UI object to move</param>
    /// <param name="destination">Where the UI object should end up</param>
    /// <returns></returns>
    public static IEnumerator LerpOnUiCoroutine(float steps, float timeSec, RectTransform rectTransform, Vector2 destination)
    {
        Vector2 pos = rectTransform.anchoredPosition;

        for (float i = 0; i <= steps; i++)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(pos,
                    destination,
                    i / steps);

            yield return new WaitForSeconds(1 / steps * timeSec);
        }
    }
}

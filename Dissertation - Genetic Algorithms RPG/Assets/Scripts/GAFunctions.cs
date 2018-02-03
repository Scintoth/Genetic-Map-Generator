using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GAFunctions
{
    public static bool FlipACoin()
    {
        return Random.Range(0f, 1f) <= 0.5f;
    }

    public static float Average(params float[] values)
    {
        float total = 0;

        for (int i = 0; i < values.Length - 1; ++i)
        {
            total += values[i];
        }

        total /= (values.Length - 1);

        return total;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class MyRandom
{
    public static T RouletteWheelSelection<T>(Dictionary<T, float> elemens)
    {
        float totalChance = 0;
        foreach (var elem in elemens.Values)
            totalChance += elem;

        float randomValue = Random.Range( 0, totalChance);
        foreach (var elem in elemens)
        {
            randomValue -= elem.Value;
            if (randomValue <= 0)
                return elem.Key;
        }

        return default;
    }
}
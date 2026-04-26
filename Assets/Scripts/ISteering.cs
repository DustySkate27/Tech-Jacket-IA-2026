using System.Collections.Generic;
using UnityEngine;

public interface ISteering
{
    public Vector3 GetDir( Vector3 currentSpeed );
}


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

        // 2 + 10 + 30 + 100 = 142
        //20
        // 20 -2 = 18
        // 18 - 10 = 8
        // 8 - 30 = -22
    }
}
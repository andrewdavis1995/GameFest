using System;
using System.Collections.Generic;
using UnityEngine;

enum FoodType { BreadRoll, Lettuce, Melon, Orange, ToiletRoll }

public class FoodFetcher : MonoBehaviour
{
    public Sprite[] Sprites;

    /// <summary>
    /// Gets the distribution of the items to spawn - how likely each is to be spawned
    /// </summary>
    /// <returns>A list of enum values and their % likelihood to be spawned</returns>
    List<Tuple<FoodType, int>> GetDistribution()
    {
        List<Tuple<FoodType, int>> distribution = new List<Tuple<FoodType, int>>();
        distribution.Add(new Tuple<FoodType, int>(FoodType.BreadRoll, 36));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Lettuce, 35));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Melon, 2));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Orange, 17));
        distribution.Add(new Tuple<FoodType, int>(FoodType.ToiletRoll, 12));

        return distribution;
    }

    public void GetFood(ShopDropBallScript ball)
    {
        var numValues = Enum.GetValues(typeof(FoodType)).Length;
        var value = SpawnItemDistributionFetcher<FoodType>.GetRandomEnumValue(GetDistribution());

        var points = 0;
        var foodName = "";

        var offset = new Vector3(0, 0);
        switch(value)
        {
            case FoodType.BreadRoll:
                foodName = "Bread";
                offset = new Vector2(-.05f, -.05f);
                points = UnityEngine.Random.Range(15, 25);
                break;
            case FoodType.Melon:
                foodName = "Melon";
                offset = new Vector2(+.095f, .025f);
                points = UnityEngine.Random.Range(25, 40);
                break;
            case FoodType.Orange:
                foodName = "Orange";
                offset = new Vector2(-.11f, -.11f);
                points = UnityEngine.Random.Range(10, 20);
                break;
            case FoodType.ToiletRoll:
                foodName = "Toilet Roll";
                offset = new Vector2(-.07f, -.07f);
                points = UnityEngine.Random.Range(-5, 5);
                break;
            case FoodType.Lettuce:
                foodName = "Lettuce";
                points = UnityEngine.Random.Range(5, 15);
                break;
        }

        ball.transform.localScale += offset;
        ball.GetComponent<SpriteRenderer>().sprite = Sprites[(int)value];
        ball.Points = points;
        ball.Food = foodName;
    }
}

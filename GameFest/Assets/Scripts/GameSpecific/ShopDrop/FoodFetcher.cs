using System;
using System.Collections.Generic;
using UnityEngine;

enum FoodType { Melon, Orange, ToiletRoll, Potato, Cake }

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
        distribution.Add(new Tuple<FoodType, int>(FoodType.Melon, 10));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Orange, 22));
        distribution.Add(new Tuple<FoodType, int>(FoodType.ToiletRoll, 23));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Potato, 45));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Cake, 3));

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
            case FoodType.Melon:
                foodName = "Melon";
                offset = new Vector2(+.09f, .02f);
                points = 15;
                break;
            case FoodType.Orange:
                foodName = "Orange";
                offset = new Vector2(-.14f, -.14f);
                points = 5;
                break;
            case FoodType.ToiletRoll:
                foodName = "Toilet Roll";
                offset = new Vector2(-.06f, -.06f);
                points = 8;
                break;
            case FoodType.Potato:
                foodName = "Potato";
                offset = new Vector2(-.095f, -.095f);
                points = 9;
                break;
            case FoodType.Cake:
                foodName = "Birthday Cake";
                offset = new Vector2(0.1f, 0.1f);
                points = 40;
                break;
        }

        ball.transform.localScale += offset;
        ball.GetComponent<SpriteRenderer>().sprite = Sprites[(int)value];
        ball.Points = points;
        ball.Food = foodName;
    }
}

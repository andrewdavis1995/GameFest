using System;
using System.Collections.Generic;
using UnityEngine;

enum FoodType { BirthdayCake, Cheese, Gammon, ChristmasPudding, Sweets, Watermelon, WashingUpLiquid, BreadRoll, Lettuce, Pineapple, Orange, Potato, Sponge, ToiletRoll }

public class FoodFetcher : MonoBehaviour
{
    public Sprite[] Sprites;
    public Sprite EmpireBiscuitSprite;
    public Sprite BombSprite;

    /// <summary>
    /// Gets the distribution of the items to spawn - how likely each is to be spawned
    /// </summary>
    /// <returns>A list of enum values and their % likelihood to be spawned</returns>
    List<Tuple<FoodType, int>> GetDistribution()
    {
        List<Tuple<FoodType, int>> distribution = new List<Tuple<FoodType, int>>();
        distribution.Add(new Tuple<FoodType, int>(FoodType.BirthdayCake, 1));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Cheese, 3));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Gammon, 3));
        distribution.Add(new Tuple<FoodType, int>(FoodType.ChristmasPudding, 3));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Sweets, 4));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Watermelon, 8));
        distribution.Add(new Tuple<FoodType, int>(FoodType.WashingUpLiquid, 6));
        distribution.Add(new Tuple<FoodType, int>(FoodType.BreadRoll, 9));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Lettuce, 10));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Pineapple, 11));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Orange, 12));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Potato, 11));
        distribution.Add(new Tuple<FoodType, int>(FoodType.Sponge, 7));
        distribution.Add(new Tuple<FoodType, int>(FoodType.ToiletRoll, 12));

        return distribution;
    }

    /// <summary>
    /// Populates the ball with the attributes of a randomised shopping item
    /// </summary>
    /// <param name="ball">The script to update</param>
    public void GetFood(ShopDropBallScript ball)
    {
        // get a (semi) random shopping element
        var numValues = Enum.GetValues(typeof(FoodType)).Length;
        var value = SpawnItemDistributionFetcher<FoodType>.GetRandomEnumValue(GetDistribution());

        var points = 0;
        var foodName = "";
        var offset = new Vector3(0, 0);

        // set values for points, size, and name based on the item type
        switch(value)
        {
            case FoodType.BirthdayCake:
                foodName = "Birthday Cake";
                offset = new Vector2(0.11f, 0.11f);
                points = 40;
                break;
            case FoodType.Gammon:
                foodName = "Gammon";
                offset = new Vector2(0.025f, 0.025f);
                points = 33;
                break;
            case FoodType.ChristmasPudding:
                foodName = "Christmas Pudding";
                offset = new Vector2(-0.1f, -0.1f);
                points = 29;
                break;
            case FoodType.Cheese:
                foodName = "Cheese";
                offset = new Vector2(0.04f, 0.04f);
                points = 25;
                break;
            case FoodType.Sweets:
                foodName = "Sweets";
                offset = new Vector2(-0.09f, -0.09f);
                points = 24;
                break;
            case FoodType.Watermelon:
                foodName = "Watermelon";
                offset = new Vector2(.09f, .02f);
                points = 20;
                break;
            case FoodType.WashingUpLiquid:
                foodName = "Washing-up Liquid";
                offset = new Vector2(-0.08f, 0.06f);
                points = 15;
                break;
            case FoodType.BreadRoll:
                foodName = "Bread Roll";
                offset = new Vector2(-0.08f, -0.08f);
                points = 14;
                break;
            case FoodType.Lettuce:
                foodName = "Lettuce";
                offset = new Vector2(0f, 0f);
                points = 12;
                break;
            case FoodType.Pineapple:
                foodName = "Pineapple";
                offset = new Vector2(-0.05f, 0.08f);
                points = 11;
                break;
            case FoodType.Orange:
                foodName = "Orange";
                offset = new Vector2(-.18f, -.18f);
                points = 10;
                break;
            case FoodType.Potato:
                foodName = "Potato";
                offset = new Vector2(-.1f, -.1f);
                points = 8;
                break;
            case FoodType.Sponge:
                foodName = "Sponge";
                offset = new Vector2(-.05f, -.05f);
                points = 5;
                break;
            case FoodType.ToiletRoll:
                foodName = "Toilet Roll";
                offset = new Vector2(-.07f, -.07f);
                points = 1;
                break;
        }

        // update item properties and appearance
        ball.transform.localScale += offset;
        ball.GetComponent<SpriteRenderer>().sprite = Sprites[(int)value];
        ball.Points = points;
        ball.Food = foodName;
    }

    /// <summary>
    /// Populates the ball with the attributes of an empire biscuit
    /// </summary>
    /// <param name="ball">The script to update</param>
    public void GetEmpireBiscuit(ShopDropBallScript ball)
    {
        ball.transform.localScale += new Vector3(-.1f, -.1f);
        ball.GetComponent<SpriteRenderer>().sprite = EmpireBiscuitSprite;
        ball.Points = 80;
        ball.Food = "Empire Biscuit";
    }

    /// <summary>
    /// Populates the ball with the attributes of an empire biscuit
    /// </summary>
    /// <param name="ball">The script to update</param>
    public void GetBomb(ShopDropBallScript ball)
    {
        ball.transform.localScale += new Vector3(-.02f, -.02f);
        ball.GetComponent<SpriteRenderer>().sprite = BombSprite;
        ball.Points = 0;
        ball.Food = "BOMB";
    }
}

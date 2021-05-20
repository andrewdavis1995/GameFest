using System;
using UnityEngine;

enum FoodType { BreadRoll, Lettuce, Melon, Orange, ToiletRoll }

public class FoodFetcher : MonoBehaviour
{
    public Sprite[] Sprites;
    
    public void GetFood(ShopDropBallScript ball)
    {
        var numValues = Enum.GetValues(typeof(FoodType)).Length;
        var random = UnityEngine.Random.Range(0, numValues);
        // TODO: Bell curve (melon less common etc.)

        var points = 0;
        var foodName = "";

        var offset = new Vector3(0, 0);
        switch((FoodType)random)
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
        ball.GetComponent<SpriteRenderer>().sprite = Sprites[random];
        ball.Points = points;
        ball.Food = foodName;
    }
}

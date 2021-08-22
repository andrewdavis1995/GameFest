using UnityEngine;

public class GameDescriptionScript : MonoBehaviour
{
    public static string GetDescription(Scene scene)
    {
        string description = "";

        switch (scene)
        {
            case Scene.Landslide:
                description = "First one to the top of the Munro wins! But it's not that simple... There are falling rocks to avoid, marsh area to avoid, and your fellow players will be trying to stop you too!";
                break;
            case Scene.MarshLand:
                description = "Your dreams have come true - you are in a giant cup of hot chocolate! But it's scalding hot... Follow the instructions and use the marshmallows to jump across the cup to the other side - fastest wins!";
                break;
            case Scene.PunchlineBling:
                description = "Fancy yourself a comedian? Prove it! Take it in turns to match the joke setup to its punchline, and earn bling for every pair you find!";
                break;
            case Scene.ShopDrop:
                description = "Shopping is falling from the sky! Move you paddles to make the items fall into your areas! Who will get the most valuable trolley!?";
                break;
            case Scene.XTinguish:
                description = "Your spaceship is about to blow up! But none of the escape pods have enough power... Compete against each other to collect batteries for the escape pods and escape from the danger!";
                break;
            case Scene.BeachBowles:
                description = "Everyones favourite beach game! Toss the ball and attempt to make it stop in the targets! Careful to not go out of bounds!";
                break;
        }

        return description;
    }
}

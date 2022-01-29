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
                description = "Bowling meets curling meets shot put meets psychological warfare! Get your ball into the target zones to score big points! Hit the centre stick for even bigger points!";
                break;
            case Scene.MineGames:
                description = "How good is your poker face? Take it in turns to choose where the gold goes. Get rewarded for telling the truth, or lie and hope for bigger rewards for fooling your fellow players!";
                break;
            case Scene.CartAttack:
                description = "How are your artistic skills? How your driving? This game combines both! Complete as many laps as possible whilst staying in the lines to create masterpieces!";
                break;
            case Scene.FollowBack:
                description = "Who is the most popular...? Let's find out! Steal followers off your fellow players and see who can grow the biggest social media presence!";
                break;
            case Scene.LicenseToGrill:
                description = "Who doesn't love a burger?! In this game - you are the one making them! Get through as many customers as possible before the time runs out! Pay attention to their orders!";
                break;
        }

        return description;
    }
}

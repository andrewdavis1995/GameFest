using System.Linq;
using UnityEngine;

public class CoinCompletionController : MonoBehaviour
{
    public CoinScript[] Coins;
    public PlatformBase[] PlatformsToEnable;
    public SpriteRenderer TrafficLight;
    public Sprite[] TrafficLightSprites;

    enum TrafficLightSpriteIndexes { Red, Amber, Green };

    /// <summary>
    /// Checks if all the coins have been collected
    /// </summary>
    public void CheckCompletion()
    {
        var count = Coins.Count(c => !c.IsActive());
    
        // if all coins complete, enable the platforms and update traffic light display
        if(count == Coins.Length)
        {
            // traffic light goes to green
            TrafficLight.sprite = TrafficLightSprites[(int)TrafficLightSpriteIndexes.Green];
        
            // platforms
            foreach (var p in PlatformsToEnable)
                p.Enabled(true);
        }
        // more than half have been collected, so traffic light goes to amber
        else if (count > (Coins.Length / 2))
        {        
            TrafficLight.sprite = TrafficLightSprites[(int)TrafficLightSpriteIndexes.Amber];
        }
    }
}

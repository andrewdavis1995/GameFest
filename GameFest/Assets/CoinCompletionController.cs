using System.Linq;
using UnityEngine;

public class CoinCompletionController : MonoBehaviour
{
    public CoinScript[] Coins;
    public PlatformBase[] PlatformsToEnable;

    public void CheckCompletion()
    {
        if(!Coins.Any(c => c.IsActive()) || !Coins.Any(c => c.gameObject.activeInHierarchy))
        {
            foreach (var p in PlatformsToEnable)
                p.Enabled(true);
        }
    }
}

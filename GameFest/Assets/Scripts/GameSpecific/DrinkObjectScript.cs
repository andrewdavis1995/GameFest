using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Script for the glasses in Drink Slide
/// </summary>
public class DrinkObjectScript : MonoBehaviour
{
    int _playerIndex;
    float _health = 100f;

    public void Initialise(int playerIndex)
    {
        _playerIndex = playerIndex;
        _nextShot.GetComponent<SpriteRenderer>().color = ColourFetcher.GetColour(_playerIndex);
    }
    
    public void Damage(float damage)
    {
        _health -= damage;
        
        if(_health <=0)
        {
            StartCoroutine(DestroyGlass_());
        }
    }
    
    IEnumerator DestroyGlass_()
    {
        // TODO: spawn glass shards
        // TODO: spawn spilled drink
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }
    
    // TODO: collision detection
}

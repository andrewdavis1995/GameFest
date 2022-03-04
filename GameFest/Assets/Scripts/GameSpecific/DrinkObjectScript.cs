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
    
    public Transform GlassShardPrefab;

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
        // spawn glass shards
        for(int i = 0; i < 20; i++)
        {
            var created = Instantiate(GlassShardPrefab, transform.position, Quaternion.identity);
            created.GetComponent<GlassShardScript>().Create();
        }
        
        // TODO: spawn spilled drink
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
    
    // TODO: collision detection
}

using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Class for controlling trolls
/// </summary>
public class TrollAttackScript : MonoBehaviour
{
    const float ATTACK_RATE = 0.8f;
    const int START_HEALTH = 5;

    public SpriteRenderer Renderer;
    public ParticleSystem SmokePuff;
    
    FollowBackInputHandler _victim;
    PlayerMovement _movement;
    bool _active = false;
    int _health = START_HEALTH;
    
    void Start()
    {
        _movement = GetComponent<PlayerMovement>();
        // TODO: ignore collisions between player and left and right bounds
    }
    
    /// <summary>
    /// Sets the player to attack and position
    /// <summary>
    /// <param id="vic">The player to attack</param>
    public void Setup(FollowBackInputHandler vic)
    {
        _victim = vic;
    
        // TODO: puff of smoke
    
        // TODO: set position and start attacking
        
        // start attacking
        _active = true;
        StartCoroutine(Attack_());
    }
    
    /// <summary>
    /// The player has damaged this troll
    /// <summary>
    public void Damaged()
    {
        // decrease health
        _health--;
        
        // if dead, destroy
        if(_health <= 0)
        {
            StartCoroutine(Destroy_());
        }
    }
    
    /// <summary>
    /// The player has run out of followers - make the troll run off
    /// <summary>
    public void PlayerReachedZero()
    {
        _active = false;
        _movement.Move(1, 0);
    }
    
    /// <summary>
    /// The troll has run out of health
    /// <summary>
    IEnumerator Destroy()
    {
        _active = false;
        
        // TODO: add message to say "BLOCKED"
    
        // fade out gradually
        var colour = 1f;
        while(colour >= 0)
        {
            Renderer.color = new Color(1, 1, 1, colour);
            colour -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        
        // remove entirely
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Attack the player
    /// <summary>
    IEnumerator Attack_()
    {
        while(_active)
        {
            // remove a follower every so often
            _victim.LoseFollower(true, 1);
            yield return new WaitForSeconds(ATTACK_RATE);
        }
    }
}

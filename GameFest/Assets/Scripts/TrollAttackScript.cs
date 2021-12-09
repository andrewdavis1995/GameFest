using UnityEngine;
using System.Collections;

/// <summary>
/// Class for controlling trolls
/// </summary>
public class TrollAttackScript : MonoBehaviour
{
    const float ATTACK_RATE = 0.8f;
    const int START_HEALTH = 5;

    public SpriteRenderer Renderer;
    public ParticleSystem SmokePuff;
    public GameObject BlockedImage;

    FollowBackInputHandler _victim;
    bool _active = false;
    int _health = START_HEALTH;

    void Update()
    {
        // destroy if too far off side
        if(transform.localPosition.x < -15 || transform.localPosition.x > 15)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets the player to attack and position
    /// <summary>
    /// <param id="vic">The player to attack</param>
    /// <param id="index">The index of the player</param>
    public void Setup(FollowBackInputHandler vic, int index)
    {
        _victim = vic;
    
        // TODO: puff of smoke as they appear
    
        // randomly adjust position, so that they are not all in front of each other
        var offset = Random.Range(-0.2f, 0.2f);
        transform.Translate(new Vector3(offset, offset, -1 +(0.1f * index)));
        
        // start attacking
        _active = true;
        StartCoroutine(Attack_());
    }
    
    /// <summary>
    /// The player has damaged this troll
    /// <summary>
    /// <returns>Whether the troll was destroyed</returns>
    public bool ApplyDamage()
    {
        bool destroyed = false;
    
        // decrease health
        _health--;
        
        // if dead, destroy
        if(_health <= 0)
        {
            destroyed = true;
            StartCoroutine(Destroy_());
        }

        return destroyed;
    }
    
    /// <summary>
    /// The player has run out of followers - make the troll run off
    /// <summary>
    void PlayerReachedZero_()
    {
        _active = false;
        Destroy();
        _victim.TrollsDone();
    }

    /// <summary>
    /// The troll has run out of health
    /// <summary>
    public void Destroy()
    {
        StartCoroutine(Destroy_());
    }

    /// <summary>
    /// The troll has run out of health
    /// <summary>
    IEnumerator Destroy_()
    {
        _active = false;
        BlockedImage.SetActive(true);

        // fade out gradually
        var colour = 1f;
        while(colour >= 0)
        {
            Debug.Log(colour);
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
        while(_active && _victim.GetFollowerCount() > 0)
        {
            // remove a follower every so often
            _victim.LoseFollower(true, 1);
            FollowBackController.Instance.UpdatePlayerUIs(_victim);
            yield return new WaitForSeconds(ATTACK_RATE);
        }
        
        // if the player ran out of followers, trolls just run off
        if(_victim.GetFollowerCount() <= 0)
        {
            PlayerReachedZero_();
        }
    }
}

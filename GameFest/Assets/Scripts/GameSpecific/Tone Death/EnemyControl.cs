using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    // components
    public Transform        ProjectilePrefab;
    public PlayerAnimation  AnimationHandler;
    public SpriteRenderer[] ColourElements;

    // config
    public Vector3          BulletOffset;
    public float            MOVEMENT_SPEED        = 1f;
    public float            SHOOT_RATE            = 1f;
    public float            HEALTH_POINTS         = 100f;
    public float            DAMAGE                = 100f;
    public int              PROJECTILE_BOUNCES    = 1;
    public float            DISABLED_TIME         = 4f;
    public float            HIDE_DELAY            = 1f;
    public bool             CAN_HIDE              = false;
    
    // fields
    private float           _health;
    private bool            _disabled;
    private bool            _claimed;
    private int             _claimedPlayerIndex;
    private bool            _isShootable;
    
    /// <summary>
    /// Called at startup
    /// </summary>
    void Start()
    {
        _health = HEALTH_POINTS;
        StartCoroutine(HandlingShooting_());
        _isShootable = true;
    }
    
    /// <summary>
    /// Enemy goes into hiding place
    /// </summary>
    IEnumerator Hide_()
    {
        yield return new WaitForSeconds(HIDE_DELAY);
        // TODO: update hiding appearance
        
        _isShootable = false;
    }
    
    /// <summary>
    /// Enemy appears from hiding place
    /// </summary>
    IEnumerator Appear_()
    {
        _isShootable = true;
        // TODO: update appearance
        yield return new WaitForSeconds(0.1f);  // TEMP
    }
    
    /// <summary>
    /// Handles shooting every so often
    /// </summary>
    IEnumerator HandleShooting_()
    {
        while (!_disabled && !_claimed)
        {
            yield return new WaitForSeconds(SHOOT_RATE);
            
            if(!_disabled && !_claimed)
            {
                // popout if necessary
                if(CAN_HIDE)
                    StartCoroutine(Appear_());

                // shoot
                Shoot();
            
                // hide again if necessary
                if(CAN_HIDE)
                    StartCoroutine(Hide_());
            }
        }
    }
    
    /// <summary>
    /// The enemy has been claimed by a player
    /// </summary>
    /// <param id="playerIndex">The index of the player who claimed this enemy</param>
    public void Claim(int playerIndex)
    {
        _claimed = true;
        _claimedPlayerIndex = playerIndex;
        
        // set appearance to match colour of player who claimed them
        foreach(var renderer in ColorRenderers)
            renderer.color = ColourFetcher.GetColour(playerIndex);
    }
    
    /// <summary>
    /// The enemy has taken damage
    /// </summary>
    /// <param id="damage">The amount of damage down</param>
    public void Damage(float damage)
    {
        _health -= damage;
        
        // if no more health, disable
        if(_health <= 0)
            StartCoroutine(Disable_());
    }
    
    /// <summary>
    /// Disable the enemy for a period of time
    /// </summary>
    IEnumerator Disable_()
    {
        _disabled = true;
        // TODO: update appearance
        yield return new WaitForSeconds(DISABLED_TIME);
        _disabled = false;
        
        // if not claimed yet, start shooting again
        if(!_claimed)
        {
            _health = HEALTH_POINTS;
            StartCoroutine(HandlingShooting_());
        }
    }
    
    /// <summary>
    /// Is the player allowed to shoot?
    /// </summary>
    /// <returns>Whether the player can shoot</returns>
    public bool CanShoot()
    {
        // default is to always be allowed to shoot
        // enemies can override this to only shoot when they have a sight of the player
        return true;
    }
    
    /// <summary>
    /// Fires a shot
    /// </summary>
    public void Shoot()
    {
        // shoot
        var bullet = Instantiate(ProjectilePrefab, transform.position + BulletOffset, Quaternion.identity);
        bullet.gameObject.tag = "Enemy";
        
        // set bullet config
        StartCoroutine(bullet.GetComponent<BulletScript>().SetBounces(NUM_BOUNCES));
        StartCoroutine(bullet.GetComponent<BulletScript>().SetDamage(DAMAGE));
        
        // set shooting direction
        bullet.transform.Rotate(new Vector3(0, 0, GetRotation()));
        // TODO: change animation
    }
    
    /// <summary>
    /// Fires a shot
    /// </summary>
    /// <returns>The angle to rotate the shot at</returns>
    public float GetRotation()
    {
        // TODO: override in parent classes
        return 0f;
    }
    
    /// <summary>
    /// Find the angle the player is at
    /// </summary>
    /// <returns>The angle to the player</returns>
    public float GetRotationToPlayer()
    {
        // TODO: find angle
        return 20f;
    }
    
    /// <summary>
    /// Find all players that can be seen
    /// </summary>
    /// <returns>The closest player that can be seen (or NULL) if none</returns>
    public bool CanSeePlayer()
    {
        // find all players
        var players = ToneDeathController.Instance.GetPlayers().ToList();
        // TODO: use raycast on each player
        return false;
    }
    
    /// <summary>
    /// Checks if the enemy can be shot
    /// </summary>
    /// <returns>If the player can be shot</returns>
    public bool IsShootable()
    {
        return _isShootable;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    // components
    public Transform        ProjectilePrefab;
    public PlayerAnimation  AnimationHandler;
    public SpriteRenderer[] ColourElements;
    public SpriteRenderer   Renderer;
    public Sprite           DisabledImage;
    public SpriteRenderer   HealthBarFill;
    public GameObject       HealthBar;
    private BoxCollider2D   _collider;
    private Rigidbody2D     _rigidbody;
    private Animator[]      _animators;

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
    public float            INCREASE_FACTOR       = 0.00125f;
    
    // fields
    private float           _health;
    private bool            _disabled;
    private bool            _claimed;
    private int             _claimedPlayerIndex;
    private bool            _isShootable;

    // capture tracking
    float[]                 _capturedValues       = new float[4];
    List<int>               _playersInZone        = new List<int>();

    // routines
    Coroutine _disableRoutine;
    Coroutine _shootingRoutine;

    // called at startup
    void Start()
    {
        _health = HEALTH_POINTS;
        _shootingRoutine = StartCoroutine(HandleShooting_());
        _isShootable = true;
        _animators = GetComponentsInChildren<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<BoxCollider2D>();
    }
    
    // called once per frame
    void Update()
    {
        // nothing to do if already claimed
        if(!_claimed && _disabled)
        {
            // update all players
            foreach (var p in _playersInZone)
            {
                Debug.Log(_capturedValues[0]);

                if (_capturedValues[p] <= 1)
                {
                    _capturedValues[p] += INCREASE_FACTOR;

                    // check if complete
                    if (Mathf.Abs(1 - _capturedValues[p]) < INCREASE_FACTOR && !_claimed)
                    {
                        // claim player
                        Claim(p);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Checks if the player is claimed or not
    /// </summary>
    /// <returns>Whether the player is claimed</returns>
    public bool Claimed()
    {
        return _claimed;
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
    /// Updates the health bar
    /// </summary>
    void UpdateHealthImage_()
    {
        // calculate size and position
        var width = _health / 100f;
        var left = -1 * (((100 - _health) / 100) / 2);

        // displays the current health
        HealthBarFill.size = new Vector2(width, 1);
        HealthBarFill.transform.localPosition = new Vector3(left, 0, 0);

        // set color
        var r = _health <= 50 ? 1f : ((100 - _health) / 50);
        var g = _health >= 50 ? 1f : (_health / 50f);
        HealthBarFill.color = new Color(r, g, 0);
    }

    /// <summary>
    /// The enemy has been claimed by a player
    /// </summary>
    /// <param id="playerIndex">The index of the player who claimed this enemy</param>
    public void Claim(int playerIndex)
    {
        _claimed = true;
        _claimedPlayerIndex = playerIndex;

        _rigidbody.isKinematic = true;
        _collider.isTrigger = true;
        
        // set appearance to match colour of player who claimed them
        foreach(var renderer in ColourElements)
            renderer.color = ColourFetcher.GetColour(playerIndex);

        // back to normal appearance
        if (_disableRoutine != null)
            StopCoroutine(_disableRoutine);

        Reenable_();

        // TODO: set animation
    }

    /// <summary>
    /// When the object collides with another
    /// </summary>
    /// <param name="collision">The object that was collided with</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            // stop movement (other than that controlled by this script)
            _rigidbody.isKinematic = true;
        }
    }

    /// <summary>
    /// The enemy has taken damage
    /// </summary>
    /// <param id="damage">The amount of damage down</param>
    public void Damage(float damage)
    {
        _health -= damage;

        // don't allow under 0
        if (_health < 0) _health =0;

        UpdateHealthImage_();

        // if no more health, disable
        if(_health <= 0)
            _disableRoutine = StartCoroutine(Disable_());
    }
    
    /// <summary>
    /// Disable the enemy for a period of time
    /// </summary>
    IEnumerator Disable_()
    {
        _disabled = true;

        if (_shootingRoutine != null)
            StopCoroutine(_shootingRoutine);
        
        // TODO: show stars around head

        // disable animations
        foreach (var anim in _animators)
            anim.enabled = false;

        Renderer.sprite = DisabledImage;

        yield return new WaitForSeconds(DISABLED_TIME);
        Reenable_();
    }

    /// <summary>
    /// Re-enables the player after being disabled
    /// </summary>
    private void Reenable_()
    {
        _disabled = false;

        // if not claimed yet, start shooting again
        if (!_claimed)
        {
            _health = HEALTH_POINTS;
            UpdateHealthImage_();
            _shootingRoutine = StartCoroutine(HandleShooting_());
        }

        // disable animations
        foreach (var anim in _animators)
            anim.enabled = true;
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
    public virtual void Shoot()
    {
        // shoot
        var bullet = Instantiate(ProjectilePrefab, transform.position + BulletOffset, Quaternion.identity);
        bullet.gameObject.tag = "Enemy";
        
        // set bullet config
        StartCoroutine(bullet.GetComponent<BulletScript>().SetBounces(PROJECTILE_BOUNCES));
        StartCoroutine(bullet.GetComponent<BulletScript>().IgnorePlayer(GetComponent<BoxCollider2D>(), -1));
        bullet.GetComponent<BulletScript>().SetDamage(DAMAGE);
        
        // set shooting direction
        bullet.transform.Rotate(new Vector3(0, 0, GetRotation()));
        // TODO: change animation
    }
    
    /// <summary>
    /// Fires a shot
    /// </summary>
    /// <returns>The angle to rotate the shot at</returns>
    public virtual float GetRotation()
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
    
    /// <summary>
    /// When a player starts to claim this speaker
    /// </summary>
    /// <param name="playerIndex">The index of the player</param>
    internal void StartClaim(int playerIndex)
    {
        _playersInZone.Add(playerIndex);
    }

    /// <summary>
    /// When a player stops claiming this speaker
    /// </summary>
    /// <param name="playerIndex">The index of the player</param>
    internal void StopClaim(int playerIndex)
    {
        _playersInZone.Remove(playerIndex);
    }

    /// <summary>
    /// Gets the collider for the player
    /// </summary>
    /// <returns>The collider</returns>
    public BoxCollider2D GetCollider()
    {
        return _collider;
    }
}

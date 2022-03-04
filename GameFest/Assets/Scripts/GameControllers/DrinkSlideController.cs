using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Diagnostics;

public class DrinkSlideController : MonoBehaviour
{
    const int THROW_POWER = 1000f;
    const int NUM_THROWS_PER_ROUND = 3;
    const float ANGLE_CORRECTION = 90f;

    public Transform DrinkPrefab;
    public Vector3 StartPosition;
    public Sprite[] GlassShardSprites;

    private Rigidbody2D _nextShot;
    List<DrinkSlideInputHandler> _players;

    int _playerIndex = 0;
    int _throwIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        // TEMP
        _players = FindObjectsOfType<DrinkSlideInputHandler>().ToList();

        //SpawnPlayers_();
        CreateDrink_();
    }

    /// <summary>
    /// Creates the player objects and assigns required script
    /// </summary>
    private void SpawnPlayers_()
    {
        // loop through all players
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(ToneDeathInputHandler));

            player.Spawn(null, Vector2.zero);
            var ih = player.GetComponent<DrinkSlideInputHandler>();

            _players.Add(ih);
        }
    }
    
    public void Fire(int playerIndex, float angle, float powerMultiplier)
    {        
        Debug.Assert(playerIndex == _playerIndex, "Incorrect player was allowed to fire");
    
        if(playerIndex == _playerIndex)
            Throw_(THROW_POWER * powerMultiplier, angle);
    }
    
    public void UpdatePointer(int playerIndex, float angle)
    {        
        Debug.Assert(playerIndex == _playerIndex, "Incorrect player was allowed to update pointer");
    
        if(playerIndex == _playerIndex)
        {
            // TODO: update pointer
        }
    }

    public void Throw_(float force, float angle)
    {
        angle += ANGLE_CORRECTION;

        Debug.Log("Throwing");

        float xcomponent = Mathf.Cos(angle * Mathf.PI / 180) * force;
        float ycomponent = Mathf.Sin(angle * Mathf.PI / 180) * force;

        _nextShot.AddForce(new Vector2(xcomponent, ycomponent));

        StartCoroutine(CheckForShotEnd_());
    }

    private IEnumerator CheckForShotEnd_()
    {
        yield return new WaitForSeconds(1f);

        while(_nextShot.velocity.y > 0.01f)
        {
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        NextShot_();
    }

    private void NextShot_()
    {
        _playerIndex++;
        if(_playerIndex >= _players.Count)
        {
            _playerIndex = 0;
            _throwIndex++;
        }

        if(_throwIndex >= NUM_THROWS_PER_ROUND)
        {
            Debug.Log("Round end");
        }
        else
        {
            CreateDrink_();
            
            // enable next player
            foreach(var p in _players)
                p.IsActive(p.GetPlayerIndex() == _playerIndex);
        }
    }

    private void CreateDrink_()
    {
        var item = Instantiate(DrinkPrefab, StartPosition, Quaternion.identity);
        _nextShot = item.GetComponent<Rigidbody2D>();
        var drinkScript = _nextShot.GetComponent<DrinkObjectScript>();
        drinkScript.Initialise(_playerIndex);
    }
    
    public Sprite GetRandomGlassShard()
    {
        var index = UnityEngine.Random.Range(0, GlassShardSprites.Length);
        return GlassShardSprites[index];
    }
}

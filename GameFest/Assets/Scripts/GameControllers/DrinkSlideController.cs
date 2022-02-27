using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrinkSlideController : MonoBehaviour
{
    const int NUM_THROWS_PER_ROUND = 3;

    public Transform DrinkPrefab;
    public Vector3 StartPosition;

    bool _canThrow = true;

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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _canThrow)
            Throw_(1000f, UnityEngine.Random.Range(-45f, 45f));
    }

    public void Throw_(float force, float angle)
    {
        angle += 90;

        _canThrow = false;
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
            _canThrow = true;
        }
    }

    private void CreateDrink_()
    {
        var item = Instantiate(DrinkPrefab, StartPosition, Quaternion.identity);
        _nextShot = item.GetComponent<Rigidbody2D>();
        _nextShot.GetComponent<SpriteRenderer>().color = ColourFetcher.GetColour(_playerIndex);
    }
}

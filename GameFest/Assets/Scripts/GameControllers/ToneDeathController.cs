using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ToneDeathController : GenericController
{
    float FLOOR_HEIGHT = 10f;
    const float ELEVATOR_SPEED = 0.25f;

    public static ToneDeathController Instance;

    public Transform PlayerPrefab;
    public Vector3[] StartPositions;
    public ElevatorScript[] Elevators;
    public Sprite[] ElevatorDoors;

    List<ToneDeathInputHandler> _players;
    int _elevatorIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        FLOOR_HEIGHT = Elevators[1].transform.position.y - Elevators[0].transform.position.y;

        // TEMP
        _players = FindObjectsOfType<ToneDeathInputHandler>().ToList();
        foreach (var pl in _players)
        {
            var movement = Instantiate(PlayerPrefab, StartPositions[0], Quaternion.identity).GetComponent<PlayerMovement>();
            pl.InitialisePlayer(movement);
        }

        //SpawnPlayers_();
    }

    public void CheckAllPlayersComplete()
    {
        if (_players.All(p => p.FloorComplete()))
        {
            StartCoroutine(NextLevel_());
        }
    }

    private IEnumerator NextLevel_()
    {
        for (var i = 0; i < ElevatorDoors.Count(); i++)
        {
            Elevators[_elevatorIndex].Doors.sprite = ElevatorDoors[i];
            yield return new WaitForSeconds(.05f);
        }

        foreach (var v in _players)
        {
            v.Hide();
        }

        for (float i = 0; i < FLOOR_HEIGHT; i += ELEVATOR_SPEED)
        {
            Camera.main.transform.Translate(new Vector3(0, ELEVATOR_SPEED, 0));
            Elevators[_elevatorIndex].Platform.Translate(new Vector3(0, ELEVATOR_SPEED, 0));
            yield return new WaitForSeconds(.01f);
        }

        foreach (var v in _players)
        {
            v.Show();
        }

        // extra bit so they are above the floor
        Elevators[_elevatorIndex].Platform.Translate(new Vector3(0, ELEVATOR_SPEED, 0));
        if (Elevators[_elevatorIndex].Top != null)
        {
            for (var i = ElevatorDoors.Count() - 1; i >= 0; i--)
            {
                Elevators[_elevatorIndex].Top.Doors.sprite = ElevatorDoors[i];
                yield return new WaitForSeconds(.05f);
            }
        }

        foreach (var v in _players)
        {
            v.FadeBackIn();
        }

        _elevatorIndex++;
    }

    /// <summary>
    /// Creates the player objects and assigns required script
    /// </summary>
    private void SpawnPlayers_()
    {
        // loop through all players
        var index = 0;
        foreach (var player in PlayerManagerScript.Instance.GetPlayers())
        {
            // switch to use an input handler suitable for this scene
            player.SetActiveScript(typeof(ToneDeathInputHandler));

            // create the "visual" player at the start point
            var tr = player.Spawn(PlayerPrefab, StartPositions[index++]);
            var ih = player.GetComponent<ToneDeathInputHandler>();
            ih.InitialisePlayer(tr.GetComponent<PlayerMovement>());

            _players.Add(ih);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToneDeathController : GenericController
{
    float FLOOR_HEIGHT = 10f;
    uint FLOOR_COUNT = 2;
    const float ELEVATOR_SPEED = 0.25f;

    public static ToneDeathController Instance;

    public Transform PlayerPrefab;
    public Vector3[] StartPositions;
    public ElevatorScript[] Elevators;
    public Sprite[] ElevatorDoors;
    public Sprite[] DisabledImages;

    List<ToneDeathInputHandler> _players;
    int _elevatorIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        // get the distance between floors
        FLOOR_HEIGHT = Elevators[1].transform.position.y - Elevators[0].transform.position.y;

        // TEMP
        _players = FindObjectsOfType<ToneDeathInputHandler>().ToList();
        foreach (var pl in _players)
        {
            var movement = Instantiate(PlayerPrefab, StartPositions[0], Quaternion.identity).GetComponent<PlayerMovement>();
            pl.InitialisePlayer(movement);
        }

        // TODO
        //SpawnPlayers_();
    }

    /// <summary>
    /// Checks if all players are complete, and moves to the next level if applicable
    /// </summary>
    public void CheckAllPlayersComplete()
    {
        // if all done, move elevator
        if (_players.All(p => p.FloorComplete()) && _players.Any(p => !p.Died()))
        {
            StartCoroutine(NextLevel_());
        }
        else if(_players.All(p => p.Died()))
        {
            // if all dead, end the game
            EndGame_();
        }
    }

    /// <summary>
    /// All players complete or dead. Move to next floor
    /// </summary>
    private IEnumerator NextLevel_()
    {
        // close doors
        for (var i = 0; i < ElevatorDoors.Count(); i++)
        {
            // loop through images
            Elevators[_elevatorIndex].Doors.sprite = ElevatorDoors[i];
            yield return new WaitForSeconds(.05f);
        }

        // hide all players (before moving up)
        foreach (var v in _players)
        {
            v.Hide();
        }

        // move elevator and camera up
        for (float i = 0; i < FLOOR_HEIGHT; i += ELEVATOR_SPEED)
        {
            Camera.main.transform.Translate(new Vector3(0, ELEVATOR_SPEED, 0));
            Elevators[_elevatorIndex].Platform.Translate(new Vector3(0, ELEVATOR_SPEED, 0));
            yield return new WaitForSeconds(.01f);
        }

        // show all players (behind doors)
        foreach (var v in _players)
        {
            v.Show();
        }

        // extra bit so they are above the floor
        Elevators[_elevatorIndex].Platform.Translate(new Vector3(0, ELEVATOR_SPEED, 0));

        // open the top doors
        if (Elevators[_elevatorIndex].Top != null)
        {
            // loop through each image
            for (var i = ElevatorDoors.Count() - 1; i >= 0; i--)
            {
                Elevators[_elevatorIndex].Top.Doors.sprite = ElevatorDoors[i];
                yield return new WaitForSeconds(.05f);
            }
        }

        // make players leave elevator
        foreach (var v in _players)
        {
            v.PlayerExitElevator();
        }

        // next floor
        _elevatorIndex++;

        // end game
        if (_elevatorIndex >= FLOOR_COUNT)
            EndGame_();
    }

    /// <summary>
    /// Ends the game
    /// </summary>
    private void EndGame_()
    {
        // TODO: Temp
        SceneManager.LoadScene((int)Scene.GameCentral);
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

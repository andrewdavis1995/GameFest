using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Script to handle the management of the player inputs
/// </summary>
public class PlayerManagerScript : MonoBehaviour
{
    public LobbyDisplayScript[] PlayerDisplays;

    public PlayerInputManager Manager;

    public static PlayerManagerScript Instance;

    List<PlayerControls> _players = new List<PlayerControls>();

    public RuntimeAnimatorController[] CharacterControllers;   // controllers to control players appearance and animations

    public TransitionFader EndFader;

    Scene _scene = Scene.Lobby;

    public GameObject[] PausePopups;

    private short _gameMovementIndex = 0;

    public Text TxtDescription;

    public bool LobbyComplete = false;

    public GameObject ModeSelection;

    /// <summary>
    /// Called when object is created
    /// </summary>
    private void Start()
    {
        // we want this to stay
        DontDestroyOnLoad(this);

        // easy access to this item
        Instance = this;

        // loop through the players and configure the pause request messages
        for (int i = 0; i < PausePopups.Length; i++)
        {
            // update the colour and the name on the popup
            PausePopups[i].GetComponentsInChildren<Image>()[1].color = ColourFetcher.GetColour(i);
        }

        EndFader.StartFade(1, 0, null);
    }

    /// <summary>
    /// Stores the list of players - should be called when the game starts (i.e. lobby complete)
    /// </summary>
    /// <param name="players">List of players who are in the game</param>
    public void SetPlayers(List<PlayerControls> players)
    {
        _players = players;
        Manager.DisableJoining();
    }

    /// <summary>
    /// Returns the list of stored players
    /// </summary>
    /// <returns>List of players who are in the game</returns>
    public List<PlayerControls> GetPlayers()
    {
        return _players.OrderBy(p => p.PlayerInput.playerIndex).ToList();
    }

    /// <summary>
    /// Returns the number of stored players
    /// </summary>
    /// <returns>Number of players who are in the game</returns>
    public int GetPlayerCount()
    {
        return _players.Count;
    }

    /// <summary>
    /// Move to the "Home" page where scores etc are shown
    /// </summary>
    public void NextScene(Scene index, bool fade = false)
    {
        _scene = index;

        if (fade)
        {
            // fade out
            EndFader.StartFade(0, 1, MoveToNextScene_);
        }
        else
            MoveToNextScene_();
    }

    /// <summary>
    /// Moves to the next scene as specified previously
    /// </summary>
    private void MoveToNextScene_()
    {
        SceneManager.LoadScene((int)_scene);
    }

    /// <summary>
    /// Checks if the index of the game movement matches what it was when the item was selected
    /// </summary>
    /// <param name="indexAtStart">The index that was set when the item was originally selected</param>
    /// <returns>Whether the index is still the same</returns>
    public bool MovedSinceVideoTriggered(short indexAtStart)
    {
        return _gameMovementIndex == indexAtStart;
    }

    /// <summary>
    /// Lobby is complete, so move on
    /// </summary>
    /// <param name="allPlayers"></param>
    internal void Complete(LobbyInputHandler[] allPlayers)
    {
        LobbyComplete = true;

        // ...store the player list in the manager
        SetPlayers(allPlayers.Select(p => p.GetComponent<PlayerControls>()).ToList());

        ModeSelection.SetActive(true);

        // move to the game central scene
        //NextScene(Scene.GameCentral, true);
    }
}

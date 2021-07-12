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
    public PlayerInputManager Manager;

    public static PlayerManagerScript Instance;

    List<PlayerControls> _players = new List<PlayerControls>();

    public RuntimeAnimatorController[] CharacterControllers;   // controllers to control players appearance and animations

    public TransitionFader EndFader;

    Scene _scene = Scene.Lobby;

    public GameObject[] PausePopups;

    public GameObject ImgGamesLocked;

    int _gameIndex;

    public GameOption[] Games;

    private List<Scene> _selectedGames = new List<Scene>();

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
    /// When the player presses X on a game
    /// </summary>
    internal void GameSelected()
    {
        if (_selectedGames.Count < 5)
        {
            _selectedGames.Add(Games[_gameIndex].SceneIndex);

           // TODO: update display
        }
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
    /// Sets the visibility of the image that "locks" the game selection
    /// </summary>
    /// <param name="state"></param>
    internal void SetGameSelectionState(bool state)
    {
        ImgGamesLocked.SetActive(state);
    }

    /// <summary>
    /// Moves the position of which game is selected
    /// </summary>
    /// <param name="direction">How much to move by</param>
    public void MoveGameSelection(int direction)
    {
        var originallySelected = _gameIndex;

        // if in bounds, movee to new index
        if (_gameIndex + direction >= 0 && _gameIndex + direction < Games.Length)
            _gameIndex += direction;

        // update to new position
        if (originallySelected != _gameIndex)
        {
            Games[originallySelected].Deselected();
            Games[_gameIndex].Selected();
        }
    }

    /// <summary>
    /// Lobby is complete, so move on
    /// </summary>
    /// <param name="allPlayers"></param>
    internal void Complete(LobbyInputHandler[] allPlayers)
    {
        // ...store the player list in the manager
        SetPlayers(allPlayers.Select(p => p.GetComponent<PlayerControls>()).ToList());

        GameCentralController.Instance.SetGames(_selectedGames);

        // move to the game central scene
        NextScene(Scene.GameCentral, true);
    }
}

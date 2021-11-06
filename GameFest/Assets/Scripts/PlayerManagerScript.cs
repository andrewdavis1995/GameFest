using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

enum GameMode { QuickPlayMode };        // TODO: expand when more modes added

/// <summary>
/// Script to handle the management of the player inputs
/// </summary>
public class PlayerManagerScript : MonoBehaviour
{
    public static PlayerManagerScript Instance;

    public LobbyDisplayScript[] PlayerDisplays;
    public PlayerInputManager Manager;
    public RuntimeAnimatorController[] CharacterControllers;   // controllers to control players appearance and animations
    public TransitionFader EndFader;
    public GameObject[] PausePopups;
    public Sprite[] FaderImages;

    List<PlayerControls> _players = new List<PlayerControls>();
    ProfileHandler _profileHandler = new ProfileHandler();

    Scene _scene = Scene.Lobby;
    private short _gameMovementIndex = 0;
    public bool LobbyComplete = false;
    GameMode _mode = GameMode.QuickPlayMode;

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

        // load values
        _profileHandler.Initialise();
    }

    /// <summary>
    /// Gets a list of all stored profiles
    /// </summary>
    /// <returns>Profile list</returns>
    public List<PlayerProfile> GetProfileList()
    {
        return _profileHandler.GetProfileList();
    }

    /// <summary>
    /// Adds a new profile to the list
    /// </summary>
    /// <param name="profile">The profile to add</param>
    public void AddProfile(PlayerProfile profile)
    {
        _profileHandler.AddProfile(profile);
    }

    /// <summary>
    /// Removes a new profile from the list
    /// </summary>
    /// <param name="profile">The profile to removes</param>
    public void RemoveProfile(Guid guid)
    {
        _profileHandler.RemoveProfile(guid);
    }

    /// <summary>
    /// Stores the list of players - should be called when the game starts (i.e. lobby complete)
    /// </summary>
    /// <param name="players">List of players who are in the game</param>
    public void SetPlayers(List<PlayerControls> players)
    {
        _players = players;
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
    public void NextScene(Scene index)
    {
        _scene = index;
        MoveToNextScene_();
    }

    /// <summary>
    /// Move to the "Home" page where scores etc are shown -- based on mode
    /// </summary>
    public void CentralScene()
    {
        NextScene(GetCentralScreen());
    }

    /// <summary>
    /// Work out which scene to show
    /// </summary>
    public Scene GetCentralScreen()
    {
        // TODO: Potential to add another mode in future - story mode of some sort
        Scene scene = Scene.QuickPlayLobby;
        return scene;
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
        Manager.DisableJoining();

        // ...store the player list in the manager
        SetPlayers(allPlayers.Select(p => p.GetComponent<PlayerControls>()).ToList());

        // move to the game central scene
        EndFader.StartFade(0, 1, CentralScene);
    }

    /// <summary>
    /// Gets the fader image on use, based on mode
    /// </summary>
    /// <returns></returns>
    public Sprite GetFaderImage()
    {
        return FaderImages[(int)_mode];
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Script to handle the management of the player inputs
/// </summary>
public class PlayerManagerScript : MonoBehaviour
{
    public PlayerInputManager Manager;

    public static PlayerManagerScript Instance;

    List<PlayerControls> _players = new List<PlayerControls>();

    public RuntimeAnimatorController[] CharacterControllers;   // controllers to control players appearance and animations

    /// <summary>
    /// Called when object is created
    /// </summary>
    private void Start()
    {
        // we want this to stay
        DontDestroyOnLoad(this);

        // easy access to this item
        Instance = this;
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
        return _players;
    }

    /// <summary>
    /// Move to the "Home" page where scores etc are shown
    /// </summary>
    public void NextScene(int index)
    {
        // TODO: Fade out
        SceneManager.LoadScene(index);
    }
}

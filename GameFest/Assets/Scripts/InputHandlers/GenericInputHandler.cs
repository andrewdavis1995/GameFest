using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class GenericInputHandler : MonoBehaviour
{
    [SerializeField]
    private int _points = 0;
    private int _bonusPoints = 0;
    private bool _winner;

    // status variables
    int _playerIndex;
    int _characterIndex;
    string _playerName;
    Guid _profileID;

    bool _pausePopupActive = false;

    public virtual Transform Spawn(Transform prefab, Vector3 position, int characterIndex, string playerName, int playerIndex, Guid profileID)
    {
        _playerIndex = playerIndex;
        _playerName = playerName;
        _characterIndex = characterIndex;
        _profileID = profileID;
        return null;
    }

    public virtual void OnMove(InputAction.CallbackContext ctx) { }
    public virtual void OnMoveRight(InputAction.CallbackContext ctx) { }
    public virtual void OnCross() { }
    public virtual void OnCircle() { }
    public virtual void OnTriangle() { }
    public virtual void OnSquare() { }
    public virtual void OnTouchpad() { }
    public virtual void OnL1() { }
    public virtual void OnR1() { }
    public virtual void OnL2() { }
    public virtual void OnR2() { }

    public virtual void TriggerEnter(Collision2D collision) { }
    public virtual void TriggerExit(Collision2D collision) { }

    /// <summary>
    /// Returns of the character being used by this player
    /// </summary>
    /// <returns>The character index</returns>
    internal int GetCharacterIndex()
    {
        return _characterIndex;
    }

    /// <summary>
    /// Returns of the character being used by this player
    /// </summary>
    /// <param name="index">The value to set</param>
    internal void SetCharacterIndex(int index)
    {
        _characterIndex = index;
    }

    /// <summary>
    /// Returns the name of the player
    /// </summary>
    /// <returns>The players name</returns>
    internal string GetPlayerName()
    {
        return _playerName;
    }

    /// <summary>
    /// Returns the name of the player
    /// </summary>
    /// <param id="plName">The players name</param>
    internal void SetPlayerName(string plName)
    {
        _playerName = plName;
    }

    /// <summary>
    /// Returns the GUID of the profile
    /// </summary>
    /// <returns>The ID</returns>
    internal Guid GetProfileID()
    {
        return _profileID;
    }

    /// <summary>
    /// Gets the index of the player
    /// </summary>
    /// <returns>The index of the player</returns>
    internal int GetPlayerIndex()
    {
        return _playerIndex;
    }

    /// <summary>
    /// Add points for the current game
    /// </summary>
    /// <param name="points">The points to add</param>
    public virtual void AddPoints(int points)
    {
        _points += points;
    }

    /// <summary>
    /// Sets how many bonus points this player got this game
    /// </summary>
    /// <param name="points">The points earned</param>
    public virtual void SetBonusPoints(int points)
    {
        _bonusPoints = points;
    }

    /// <summary>
    /// Sets how many bonus points this player got this game
    /// </summary>
    /// <returns>The points earned</returns>
    public virtual int GetBonusPoints()
    {
        return _bonusPoints;
    }

    /// <summary>
    /// Returns the number of points won by this player
    /// </summary>
    /// <returns>Points earned</returns>
    public int GetPoints()
    {
        return _points;
    }

    /// <summary>
    /// Checks if the player is the host of the game
    /// </summary>
    /// <returns>Whether the player is the host</returns>
    public bool IsHost()
    {
        return _playerIndex == 0;
    }

    /// <summary>
    /// Set the height of the player based on the selected character
    /// </summary>
    /// <param name="player">The transform the the player display</param>
    /// <param name="characterIndex">The index of the chosen character</param>
    public virtual void SetHeight(Transform player, int characterIndex)
    {
        float size = 0.2f;

        switch (characterIndex)
        {
            // Andrew
            case 0:
                size += 0.031f;
                break;
                // Rachel
            case 1:
                size -= 0.022f;
                break;
                // Naomi
            case 2:
                size += 0f;
                break;
                // Heather
            case 3:
                size -= 0.019f;
                break;
                // Mum & Dad
            case 4:
            case 5:
                size += 0.025f;
                break;
                // John & Fraser
            case 6:
            case 7:
                size += 0.022f;
                break;
                // Matthew
            case 8:
                size += 0.04f;
                break;
        }

        // set height
        player.localScale = new Vector3(size, size, 1);
    }

    /// <summary>
    /// Sets the animation to that of the correct character
    /// </summary>
    /// <param name="spawned">The instantiated object with an animator attached</param>
    /// <param name="characterIndex">The index of the selected character</param>
    public void SetAnimation(Transform spawned, int characterIndex)
    {
        // update the animation controller
        foreach (var anim in spawned.GetComponentsInChildren<Animator>())
            anim.runtimeAnimatorController = PlayerManagerScript.Instance.CharacterControllers[characterIndex];
    }

    /// <summary>
    /// Sets whether the pause popup for this player is active
    /// </summary>
    /// <param name="active">Whether the popup is active</param>
    public void PausePopupActive(bool active)
    {
        _pausePopupActive = active;
    }

    /// <summary>
    /// Sets whether the pause popup for this player is active
    /// </summary>
    /// <returns>Whether the popup is active</returns>
    public bool PausePopupActive()
    {
        return _pausePopupActive;
    }

    /// <summary>
    /// When the Options event is triggered
    /// </summary>
    public virtual void OnOptions()
    {
        if (PauseGameHandler.Instance == null) return;

        // pause
        if (IsHost())
            PauseGameHandler.Instance.TogglePause();
        // show popup for players who are not the host
        else if (!PausePopupActive() && !PauseGameHandler.Instance.IsPaused())
            StartCoroutine(PauseRequest_());
    }

    /// <summary>
    /// Displays the pause request message for this player
    /// </summary>
    /// <returns></returns>
    private IEnumerator PauseRequest_()
    {
        if (PauseGameHandler.Instance.ActiveGameController.CanPause())
        {
            PausePopupActive(true);
            PauseGameHandler.Instance.PausePopups[GetPlayerIndex() - 1].SetActive(true);
            yield return new WaitForSeconds(4);
            PauseGameHandler.Instance.PausePopups[GetPlayerIndex() - 1].SetActive(false);
            PausePopupActive(false);
        }
    }

    /// <summary>
    /// The player won
    /// </summary>
    internal void Winner()
    {
        _winner = true;
    }

    /// <summary>
    /// Check if the player won
    /// </summary>
    /// <returns>Whether they won</returns>
    internal bool IsWinner()
    {
        return _winner;
    }

    /// <summary>
    /// Sets the index of the player
    /// </summary>
    /// <param name="playerIndex">The  index of the player</param>
    public void SetPlayerIndex(int playerIndex)
    {
        _playerIndex = playerIndex;
    }
}
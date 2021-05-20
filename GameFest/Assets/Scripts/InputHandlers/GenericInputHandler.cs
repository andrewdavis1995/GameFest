using System;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class GenericInputHandler : MonoBehaviour
{
    private int _points = 0;

    public virtual void Spawn(Transform prefab, Vector2 position, int characterIndex, string playerName) { }

    public virtual void OnMove(InputAction.CallbackContext ctx) { }
    public virtual void OnCross() { }
    public virtual void OnCircle() { }
    public virtual void OnTriangle() { }
    public virtual void OnSquare() { }
    public virtual void OnTouchpad() { }
    public virtual void OnL1() { }
    public virtual void OnR1() { }

    public virtual void TriggerEnter(Collision2D collision) { }
    public virtual void TriggerExit(Collision2D collision) { }

    /// <summary>
    /// Add points for the current game
    /// </summary>
    /// <param name="points">The points to add</param>
    public virtual void AddPoints(int points)
    {
        _points = points;
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
        spawned.GetComponent<Animator>().runtimeAnimatorController = PlayerManagerScript.Instance.CharacterControllers[characterIndex];
    }
}

/* CODE DUMP
 * 
 * Paddle code
 * 
 * 
 *         // paddles - TODO: move to another script
        Transform[] _paddles;
        bool _paddleState = false;

        _paddles = GameObject.FindGameObjectsWithTag("Paddle").Select(e => e.transform).ToArray();

    /// <summary>
    /// Updates the angle of the paddles
    /// </summary>
    void SetPaddles_()
    {
        var paddleAngle = _paddleState ? 45 : -45;

        // loop through all the paddles
        foreach (var paddle in _paddles)
        {
            // change the angle
            paddle.eulerAngles = new Vector3(0, 0, paddleAngle);
        }
    }
*/

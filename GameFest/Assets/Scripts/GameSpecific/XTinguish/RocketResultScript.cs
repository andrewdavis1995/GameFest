﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class RocketResultScript : MonoBehaviour
{
    // unity configuration
    public SpriteRenderer Rocket;
    public TextMesh TxtPlayerName;
    public TextMesh TxtScore;

    // status variables
    bool _started = false;
    bool _complete = false;
    List<int> _values = new List<int>();
    float _moveSpeed = 6f;
    bool _powerRemaining = false;
    int _score = 0;

    // Update is called once per frame
    void Update()
    {
        // do nothing if complete - wait for others
        if (!_complete && _started)
        {
            // move the rocket
            transform.Translate(new Vector3(_moveSpeed * Time.deltaTime, 0));

            // if no longer accelerating, decrease speed
            if (!_powerRemaining)
            {
                // TODO: Change animation image (splutter then die)

                _moveSpeed -= 0.03f;

                // stop once no more battery
                if (_moveSpeed <= 0f)
                {
                    _complete = true;
                    StartCoroutine(DelayBeforeComplete());
                }
            }
        }
    }

    /// <summary>
    /// Delay slightly before completing
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayBeforeComplete()
    {
        yield return new WaitForSeconds(2);
        XTinguishController.Instance.CheckResultsComplete();
    }

    /// <summary>
    /// Initialises the result display with all required data
    /// </summary>
    /// <param name="points">List of items the player picked up</param>
    /// <param name="playerName">Name of the player</param>
    /// <param name="playerIndex">Index of the player</param>
    internal void Initialise(List<int> points, string playerName, int playerIndex)
    {
        Rocket.color = ColourFetcher.GetColour(playerIndex);
        _values = points;
        TxtPlayerName.text = playerName;
    }

    /// <summary>
    /// Starts the process of moving the item forward, and depletes power
    /// </summary>
    public void StartMove()
    {
        _started = true;
        _powerRemaining = true;
        StartCoroutine(ProcessPower_());
    }

    /// <summary>
    /// Processes each of the stored power values sequentially
    /// </summary>
    IEnumerator ProcessPower_()
    {
        int index = 0;

        // loop through each power source
        foreach (var value in _values)
        {
            Debug.Log("Processing " + value);
            int currentValue = value;
            do
            {
                // TODO: Update display

                // deplete value
                currentValue -= 10;

                // display score
                _score += 10;
                TxtScore.text = _score.ToString();

                // wait briefly - speed should be adjustable
                yield return new WaitForSeconds(.3f);
            } while (currentValue > 0);

            // TODO: make output grey/disabled
            index++;
        }

        // no longer has power - glide
        _powerRemaining = false;
    }

    /// <summary>
    /// Checks if the player result is complete
    /// </summary>
    /// <returns>Whether the player is complete</returns>
    internal bool IsComplete()
    {
        return _complete;
    }
}

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class CheckpointScript : MonoBehaviour
{
    public int[] PointsPerPosition;
    private List<Tuple<int, int>> _playerInfo = new List<Tuple<int, int>>();

    /// <summary>
    /// Add a player to the checkpoint list, as long as they are not already there
    /// </summary>
    /// <param name="playerIndex"></param>
    public bool AddPlayer(int playerIndex)
    {
        bool added = false;

        // if the player is not already on the last, add them and store
        if (!(_playerInfo.Any(p => p.Item1 == playerIndex)))
        {
            _playerInfo.Add(new Tuple<int, int>(playerIndex, PointsPerPosition[_playerInfo.Count]));
            added = true;
        }

        return added;
    }

    /// <summary>
    /// Returns the points won by the specified player for this checkpoint
    /// </summary>
    /// <param name="playerIndex">The player to get the value for</param>
    /// <returns>The number of points the player earned at this checkpoint</returns>
    internal int GetPlayerPoints(int playerIndex)
    {
        return (int)_playerInfo.Where(p => p.Item1 == playerIndex).FirstOrDefault()?.Item1;
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum FollowDirection { Left, Right, Up, Down }

public class CameraFollow : MonoBehaviour
{
    List<Transform> _players = new List<Transform>();
    FollowDirection _direction;

    private void Update()
    {
        // if no players, nothing to do here
        if (_players.Count == 0) return;

        // find the furthest forward player
        var leader = GetLeader_();
        if (leader != null)
        {
            // set the camera position to be in line with the current leader
            transform.position = new Vector3(leader.position.x, transform.position.y, transform.position.z);
        }
    }

    /// <summary>
    /// Removes a player from the list of players to follow
    /// </summary>
    /// <param name="player"></param>
    public void RemovePlayer(Transform player)
    {
        // loop through all
        for(int i = 0; i < _players.Count; i++)
        {
            // if there is a match, remove it
            if(_players[i] == player)
            {
                _players.RemoveAt(i);
                break;
            }
        }
    }

    /// <summary>
    /// Sets the players to watch and follow
    /// </summary>
    /// <param name="players">The list of players</param>
    /// <param name="direction">The direction the players are travelling in</param>
    public void SetPlayers(List<Transform> players, FollowDirection direction)
    {
        _players = players;
        _direction = direction;
    }

    /// <summary>
    /// Gets the player who is in the lead
    /// </summary>
    /// <returns>The player who is in the lead</returns>
    private Transform GetLeader_()
    {
        Transform transform = null;

        // based on the direction, find the furthest forward player
        switch(_direction)
        {
            case FollowDirection.Left:
                transform = _players.OrderBy(p => p.transform.position.x).FirstOrDefault();
                break;
            case FollowDirection.Right:
                transform = _players.OrderByDescending(p => p.transform.position.x).FirstOrDefault();
                break;
            case FollowDirection.Up:
                transform = _players.OrderByDescending(p => p.transform.position.y).FirstOrDefault();
                break;
            case FollowDirection.Down:
                transform = _players.OrderBy(p => p.transform.position.y).FirstOrDefault();
                break;
        }

        return transform;
    }
}

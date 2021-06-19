using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraZoomFollow : MonoBehaviour
{
    bool _enabled = false;
    List<Transform> _players = new List<Transform>();
    FollowDirection _direction;
    public float MaxZoomOut;
    float _normalZoom;
    float _xOffset = 2;

    private void Start()
    {
        _normalZoom = Camera.main.orthographicSize;
    }

    private void Update()
    {
        if (!enabled) return;

        // if no players, nothing to do here
        if (_players.Count == 0) return;

        var trailer = GetTrailer_();

        var camHeight = 2 * Camera.main.orthographicSize;
        var camWidth = camHeight * Camera.main.aspect;

        var leftPoint = transform.position.x - (camWidth/2) + trailer.localScale.x + _xOffset;

        Debug.Log(trailer.position + " vs " + leftPoint + " at " + Camera.main.aspect);

        if((trailer.position.x < leftPoint) && (Camera.main.orthographicSize < MaxZoomOut))
        {
            Camera.main.orthographicSize += 0.1f;
        }
        else if ((trailer.position.x > leftPoint + 4) && (Camera.main.orthographicSize > _normalZoom))
        {
            Camera.main.orthographicSize -= 0.1f;
        }
    }


    /// <summary>
    /// Gets the player who is in the lead
    /// </summary>
    /// <returns>The player who is in the lead</returns>
    private List<Transform> GetOrderedPlayerList_()
    {
        var orderedList = new List<Transform>();

        // based on the direction, find the furthest forward player
        switch (_direction)
        {
            case FollowDirection.Left:
                orderedList = _players.OrderBy(p => p.transform.position.x).ToList();
                break;
            case FollowDirection.Right:
                orderedList = _players.OrderByDescending(p => p.transform.position.x).ToList();
                break;
            case FollowDirection.Up:
                orderedList = _players.OrderByDescending(p => p.transform.position.y).ToList();
                break;
            case FollowDirection.Down:
                orderedList = _players.OrderBy(p => p.transform.position.y).ToList();
                break;
        }

        return orderedList;
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
        _enabled = true;
    }

    /// <summary>
    /// Gets the player who is in the back
    /// </summary>
    /// <returns>The player who is in the lead</returns>
    private Transform GetTrailer_()
    {
        return GetOrderedPlayerList_().LastOrDefault();
    }

    /// <summary>
    /// Gets the player at the front
    /// </summary>
    /// <param name="list">All players, in order</param>
    /// <returns>The transform of the player at the front</returns>
    Transform GetFirstPlayer(IEnumerable<Transform> list)
    {
        return GetOrderedPlayerList_().FirstOrDefault();
    }

    /// <summary>
    /// Gets the player at the back
    /// </summary>
    /// <param name="list">All players, in order</param>
    /// <returns>The transform of the player at the back</returns>
    Transform GetLastPlayer(IEnumerable<Transform> list)
    {
        return list.LastOrDefault();
    }

}

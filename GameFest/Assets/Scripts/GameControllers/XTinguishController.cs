using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class XTinguishController : MonoBehaviour
{
    // configuration
    private const int GAME_TIMEOUT = 120;

    // unity configuration
    public Text TxtCountdown;
    public Vector2 TopLeft;
    public Vector2 BottomRight;
    public Transform BatteryPrefab;

    // static instance
    public static XTinguishController Instance;

    // status variables
    bool _active = false;

    // time out
    TimeLimit _overallLimit;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        _active = true;

        _overallLimit = (TimeLimit)gameObject.AddComponent(typeof(TimeLimit));
        _overallLimit.Initialise(GAME_TIMEOUT, OnTimeLimitTick, OnTimeUp);

        StartCoroutine(SpawnBatteries());

        // start the timer
        _overallLimit.StartTimer();
    }

    private IEnumerator SpawnBatteries()
    {
        while(_active)
        {
            var positionX = UnityEngine.Random.Range(TopLeft.x, BottomRight.x);
            var positionY = UnityEngine.Random.Range(TopLeft.y, BottomRight.y);

            var spawned = Instantiate(BatteryPrefab, new Vector3(positionX, positionY, 0), Quaternion.identity);

            yield return new WaitForSeconds(UnityEngine.Random.Range(1, 10));
        }
    }

    /// <summary>
    /// Called every second
    /// </summary>
    /// <param name="seconds">How many seconds are left</param>
    private void OnTimeLimitTick(int seconds)
    {
        TxtCountdown.text = seconds <= 10 ? seconds.ToString() : "";
    }

    /// <summary>
    /// Called when time runs out
    /// </summary>
    void OnTimeUp()
    {
        _active = false;

        // show results
        StartCoroutine(EndGame_());
    }

    /// <summary>
    /// Ends the game
    /// </summary>
    private IEnumerator EndGame_()
    {
        yield return new WaitForSeconds(1);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningWheelScript : MonoBehaviour
{
    // unity elements
    public Transform SpinWheel;
    public WheelSegmentScript[] Segments;

    // status
    float _speed = 300f;
    bool _decreasing = false;
    bool _spinning = false;

    /// <summary>
    /// Initialises the spinning wheel for the players provided
    /// </summary>
    /// <param name="players">The list of players who are playing</param>
    public void Initialise(List<PunchlineBlingInputHandler> players)
    {
        int index = 0;
        // loop through each segment
        while (index < Segments.Length)
        {
            // loop through each player
            for (int j = 0; j < players.Count; j++)
            {
                // set the segment to the next player
                Segments[index++].Initialise(players[j].GetPlayerIndex(), players[j].GetPlayerName());
            }
        }
    }

    /// <summary>
    /// Makes the wheel start spinning
    /// </summary>
    public void StartSpin()
    {
        // reset status
        _speed = 300f;
        _decreasing = false;
        _spinning = true;
        
        // wait some time before slowing down
        StartCoroutine(SpinCountdown());
    }

    /// <summary>
    /// Waits a random period of time, before slowing the wheel down
    /// </summary>
    private IEnumerator SpinCountdown()
    {
        yield return new WaitForSeconds(Random.Range(1f, 1.95f));
        _decreasing = true;
    }

    // Update is called once per frame
    void Update()
    {
        // if spinning, make the wheel spin
        if (_spinning)
            SpinWheel.eulerAngles += new Vector3(0, 0, Time.deltaTime * _speed);

        // keep the angle under 360, to make it easier to calculate the winning segment
        if (SpinWheel.eulerAngles.z >= 360)
            SpinWheel.eulerAngles -= new Vector3(0, 0, 360);

        // if we are slowing down
        if (_decreasing)
        {
            // decrease speed
            _speed -= 1.5f;
            if(_speed < 1.5f)
            {
                // once we get to 0, end the spin and move on
                StartCoroutine(EndSpin_());
            }
        }
    }

    /// <summary>
    /// Shows the winning segment and moves on to next stage
    /// </summary>
    private IEnumerator EndSpin_()
    {
        // reset status
        _speed = 0;
        _decreasing = false;
        _spinning = false;

        // calculate the winning segment based on the angle
        var actualAngle = SpinWheel.eulerAngles.z;
        var segmentIndex = (int)(actualAngle / 30);

        // find which player the segment belongs to
        var winningPlayer = Segments[segmentIndex].GetPlayerIndex();

        // make the segment flash 5 times
        for(int i = 0; i < 5; i++)
        {
            // flash bright
            Segments[segmentIndex].SetBrightColour();
            yield return new WaitForSeconds(0.15f);
            // reset to normal
            Segments[segmentIndex].ResetColour();
            yield return new WaitForSeconds(0.15f);
        }

        // next player becomes active
        PunchlineBlingController.Instance.SetActivePlayer(winningPlayer);
    }
}

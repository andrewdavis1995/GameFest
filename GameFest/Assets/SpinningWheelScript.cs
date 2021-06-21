using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningWheelScript : MonoBehaviour
{
    public Transform SpinWheel;
    public WheelSegmentScript[] Segments;
    float _speed = 300f;
    bool _decreasing = false;
    bool _spinning = false;

    // Start is called before the first frame update
    void Start()
    {
        List<PunchlineBlingInputHandler> inputs = new List<PunchlineBlingInputHandler>();
        inputs.Add(new PunchlineBlingInputHandler());
    }

    public void Initialise(List<PunchlineBlingInputHandler> players)
    {
        int index = 0;
        while (index < Segments.Length)
        {
            for (int j = 0; j < players.Count; j++)
            {
                Segments[index++].Initialise(players[j].GetPlayerIndex(), players[j].GetPlayerName());
            }
        }
    }

    public void StartSpin()
    {
        _speed = 300f;
        _decreasing = false;
        _spinning = true;
        StartCoroutine(SpinCountdown());
    }

    private IEnumerator SpinCountdown()
    {
        yield return new WaitForSeconds(Random.Range(1.25f, 2.4f));
        _decreasing = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_spinning)
            SpinWheel.eulerAngles += new Vector3(0, 0, Time.deltaTime * _speed);

        if (SpinWheel.eulerAngles.z >= 360)
            SpinWheel.eulerAngles -= new Vector3(0, 0, 360);

        if (_decreasing)
        {
            _speed -= 1f;
            if(_speed < 1f)
            {
                StartCoroutine(EndSpin_());
            }
        }
    }

    private IEnumerator EndSpin_()
    {
        _speed = 0;
        _decreasing = false;
        _spinning = false;

        var actualAngle = SpinWheel.eulerAngles.z;
        var segmentIndex = (int)(actualAngle / 30);
        var winningPlayer = Segments[segmentIndex].GetPlayerIndex();

        for(int i = 0; i < 5; i++)
        {
            Segments[segmentIndex].SetBrightColour();
            yield return new WaitForSeconds(0.15f);
            Segments[segmentIndex].ResetColour();
            yield return new WaitForSeconds(0.15f);
        }

        PunchlineBlingController.Instance.SetActivePlayer(winningPlayer);
    }
}

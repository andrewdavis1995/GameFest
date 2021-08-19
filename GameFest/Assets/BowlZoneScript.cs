using UnityEngine;

public class BowlZoneScript : MonoBehaviour
{
    public int PointValue;
    private bool _containsBall;

    public void BallEntered()
    {
        _containsBall = true;
    }

    public void BallLeft()
    {
        _containsBall = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

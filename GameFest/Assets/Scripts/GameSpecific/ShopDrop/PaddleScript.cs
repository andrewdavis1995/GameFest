using UnityEngine;

public class PaddleScript : MonoBehaviour
{
    public SpriteRenderer ColouredPart;
    public float Movement = 0;

    // the rotation of the players paddles
    float _currentRotation = 0f;

    // controls the speed of turning
    const int SPEED = 250;

    // the maximum angle to turn to
    const float MAX_TURN_ANGLE = 45;

    internal void SetColour(int playerIndex)
    {
        ColouredPart.color = ColourFetcher.GetColour(playerIndex);
        name = "PADDLE_" + playerIndex;
    }

    public void SetMovement(float val)
    {
        Movement = val*-1;
    }

    private void Update()
    {
        // if the movement is within the valid range, update the rotation
        if ((Movement > 0 && _currentRotation < MAX_TURN_ANGLE) || Movement < 0 && _currentRotation > (-1 * MAX_TURN_ANGLE))
            _currentRotation += (Movement * Time.deltaTime * SPEED);

        transform.eulerAngles = new Vector3(0, 0, _currentRotation);
    }
}

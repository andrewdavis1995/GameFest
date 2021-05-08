using System.Collections;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    // components
    public Transform CardFront;
    public Transform CardBack;
    public GameObject CardSelected;

    // state variables
    private bool _isSpinning = false;

    /// <summary>
    /// Sets the "selected" image when the player enters the trigger zone
    /// </summary>
    /// <param name="inZone"></param>
    public void InZone(bool inZone)
    {
        // don't do this if it is already turning
        if (!_isSpinning)
        {
            CardSelected.SetActive(inZone);
        }
    }

    /// <summary>
    /// Controls the turning of the card to reveal the answer
    /// </summary>
    private IEnumerator SpinReveal()
    {
        CardSelected.SetActive(false);
        _isSpinning = true;

        // rotate the card 180 degrees
        for (float i = 0; i <= 180; i++)
        {
            // once it reaches halfway (i.e. invisible at the halfway point), show the other side of the card
            if (i == 90)
                CardFront.gameObject.SetActive(true);

            // set rotation
            CardBack.eulerAngles = new Vector3(0, i, 0);

            // wait briefly to allow the rotation to be seen
            yield return new WaitForSeconds(0.001f);
        }

        // wait a few seconds, then turn back
        yield return new WaitForSeconds(2);
        StartCoroutine(SpinHide());
    }

    /// <summary>
    /// Controls the turning of the card back to the start
    /// </summary>
    private IEnumerator SpinHide()
    {
        // turn from 180 degress back to the start point
        for (float i = 180; i >= 0; i--)
        {
            // once it reaches halfway (i.e. invisible at the halfway point), hide the other side of the card
            if (i == 90)
                CardFront.gameObject.SetActive(false);

            // set rotation
            CardBack.eulerAngles = new Vector3(0, i, 0);

            // wait briefly to allow the rotation to be seen
            yield return new WaitForSeconds(0.001f);
        }

        // no longer spinning, can begin to spin again
        _isSpinning = false;
    }

    /// <summary>
    /// Flip the card to reveal the content
    /// </summary>
    internal void Flip()
    {
        StartCoroutine(SpinReveal());
    }
}
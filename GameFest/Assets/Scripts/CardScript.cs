using System.Collections;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    // components
    public Transform CardFront;
    public Transform CardBack;
    public GameObject CardSelected;
    public TextMesh JokeText;

    // content of this card
    Joke _joke;
    bool _isPunchline = false;

    // state variables
    private bool _flipped = false;

    /// <summary>
    /// Sets the "selected" image when the player enters the trigger zone
    /// </summary>
    /// <param name="inZone"></param>
    public void InZone(bool inZone)
    {
        Debug.Log("FLIPPED " + _flipped);
        Debug.Log("State " + PunchlineBlingController.Instance.GetState());

        // don't do this if it is already turning
        if (!_flipped && AppropriateStage_())
        {
            CardSelected.SetActive(inZone);
        }
    }

    /// <summary>
    /// Returns the joke associated with this card
    /// </summary>
    /// <returns>The linked joke object</returns>
    public Joke GetJoke()
    {
        return _joke;
    }

    /// <summary>
    /// Returns whether this card relates to a punchline, or setup of the joke
    /// </summary>
    /// <returns>True if it is the punchline, false if it is the setup</returns>
    public bool IsPunchline()
    {
        return _isPunchline;
    }

    /// <summary>
    /// Controls the turning of the card to reveal the answer
    /// </summary>
    private IEnumerator SpinReveal()
    {
        CardSelected.SetActive(false);
        _flipped = true;

        PunchlineBlingController.Instance.CardSelected(this);

        // rotate the card 180 degrees
        for (float i = 0; i <= 180; i += 2)
        {
            // once it reaches halfway (i.e. invisible at the halfway point), show the other side of the card
            if (i == 90)
                CardFront.gameObject.SetActive(true);

            // set rotation
            CardBack.eulerAngles = new Vector3(0, i, 0);

            // wait briefly to allow the rotation to be seen
            yield return new WaitForSeconds(0.001f);
        }
    }

    /// <summary>
    /// Sets te joke linked to this card
    /// </summary>
    /// <param name="joke">The joke object linked to the card</param>
    /// <param name="punchline">Is this card showing the punchline? (If false, it shows the setup)</param>
    internal void SetJoke(Joke joke, bool punchline)
    {
        _joke = joke;
        _isPunchline = punchline;

        // set display text - punchline or setup?
        var stringToUse = punchline ? joke.Punchline : joke.Setup;
        JokeText.text = TextFormatter.GetCardJokeString(stringToUse);

        var imageIndex = IsPunchline() ? 1 : 0;
        CardBack.GetComponent<SpriteRenderer>().sprite = PunchlineBlingController.Instance.CardBacks[imageIndex];
    }

    /// <summary>
    /// Controls the turning of the card back to the start
    /// </summary>
    private IEnumerator SpinHide()
    {
        // turn from 180 degress back to the start point
        for (float i = 180; i >= 0; i -= 2)
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
        _flipped = false;
    }

    /// <summary>
    /// Flip the card to reveal the content
    /// </summary>
    internal void Flip()
    {
        if (!_flipped && AppropriateStage_())
            StartCoroutine(SpinReveal());
    }

    /// <summary>
    /// Flip the card to reveal the content
    /// </summary>
    internal void FlipBack()
    {
        StartCoroutine(SpinHide());
    }

    /// <summary>
    /// Is this card appropriate for the current stage
    /// </summary>
    /// <returns>Whether this is an appropriate stage<returns>
    bool AppropriateStage_()
    {
        var appropriate = false;

        // if setup stage and this is a setup
        if (!IsPunchline() && PunchlineBlingController.Instance.GetState() == SelectionState.PickingFirst)
            appropriate = true;
        // if punchline stage and this is a punchline
        else if (IsPunchline() && PunchlineBlingController.Instance.GetState() == SelectionState.PickingSecond)
            appropriate = true;

        return appropriate;
    }
}

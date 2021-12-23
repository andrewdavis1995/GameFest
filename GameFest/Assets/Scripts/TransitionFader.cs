using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TransitionFader : MonoBehaviour
{
    // Audio link
    public AudioSource Music;

    // Unity configurable
    public Image FadeImage;
    public float FadeSpeed;

    // stored data/info
    private Action _completionCallback;
    private Color _imageColour;

    /// <summary>
    /// Sets the initial fade of the display, and fades until it reaches the target value
    /// </summary>
    /// <param name="startAlpha">The value to start at</param>
    /// <param name="targetAlpha">The value to fade to</param>
    /// <param name="completionCallback">The function to call when fading is complete</param>
    public void StartFade(float startAlpha, float targetAlpha, Action completionCallback)
    {
        _imageColour = FadeImage.color;
        _completionCallback = completionCallback;

        // if fade is less than target, increase alpha value
        if (startAlpha < targetAlpha)
            StartCoroutine(FadeUp(startAlpha, targetAlpha));
        // if fade is more than target, decrease alpha value
        else if (startAlpha > targetAlpha)
            StartCoroutine(FadeDown(startAlpha, targetAlpha));
        // if nothing to do, skip to end
        else
            FadeComplete_();
    }

    /// <summary>
    /// Fades UPWARDS from the start point until the target is reached/exceeded
    /// </summary>
    /// <param name="start">The alpha value at which to start</param>
    /// <param name="target">The alpha value at which to end</param>
    IEnumerator FadeUp(float start, float target)
    {
        var alpha = start;

        // set the starting alpha
        SetImageColour(alpha);

        yield return new WaitForSeconds(0.5f);

        // increase alpha value until we reach the end point
        for (; alpha < target; alpha += FadeSpeed)
        {
            // adjust music volume
            if (Music != null && Music.volume > 0)
            {
                Music.volume -= FadeSpeed / 2;
            }

            // set colour of image
            SetImageColour(alpha);
            yield return new WaitForSeconds(0.01f);
        }

        SetImageColour(target);

        // we are done
        FadeComplete_();
    }

    /// <summary>
    /// Fades DOWNWARDS from the start point until the target is reached/exceeded
    /// </summary>
    /// <param name="start">The alpha value at which to start</param>
    /// <param name="target">The alpha value at which to end</param>
    IEnumerator FadeDown(float start, float target)
    {
        var alpha = start;

        // set the starting alpha
        SetImageColour(alpha);

        yield return new WaitForSeconds(0.5f);

        if (Music != null)
            Music.volume = 0;

        // increase alpha value until we reach the end point
        for (; alpha > target; alpha -= FadeSpeed)
        {
            // adjust music volume
            if (Music != null && Music.volume < 0.1f)
            {
                Music.volume += FadeSpeed / 2;
            }

            // set colour of image
            SetImageColour(alpha);
            yield return new WaitForSeconds(0.01f);
        }

        SetImageColour(target);

        // we are done
        FadeComplete_();
    }

    /// <summary>
    /// Sets the alpha value of the image to the specified value
    /// </summary>
    /// <param name="alpha">The new alpha value of the image</param>
    private void SetImageColour(float alpha)
    {
        FadeImage.color = new Color(_imageColour.r, _imageColour.g, _imageColour.b, alpha);
    }

    /// <summary>
    /// The fading has complete - call the callback function
    /// </summary>
    void FadeComplete_()
    {
        _completionCallback?.Invoke();
    }
}
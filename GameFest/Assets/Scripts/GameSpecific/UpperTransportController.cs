using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the shutter and note movement at the top of the device
/// </summary>
public class UpperTransportController : MonoBehaviour
{
    public Transform Shutter;
    public SpriteRenderer ShutterGlow;
    public UpperTransportNote[] Notes;
    public float NoteStartX;
    public float NoteResetX;
    public float DelayBetweenNotes;

    private bool _active = false;

    /// <summary>
    /// Starts moving the notes through the transportS
    /// </summary>
    public void StartNoteMovement()
    {
        _active = true;
        StartCoroutine(NoteMovement_());
    }

    /// <summary>
    /// Controls the movement of the notes
    /// </summary>
    private IEnumerator NoteMovement_()
    {
        while (_active)
        {
            var prePresentDelay = UnityEngine.Random.Range(2f, 7f);
            yield return new WaitForSeconds(prePresentDelay);

            // spin shutter up
            for (int i = 0; i <= 90; i++)
            {
                Shutter.eulerAngles = new Vector3(0, 0, i);
                ShutterGlow.color = new Color(1, 1, 1, (i / 255f));
                yield return new WaitForSeconds(0.001f);
            }

            var noteCount = UnityEngine.Random.Range(1f, Notes.Length);

            for (int i = 0; i < noteCount; i++)
            {
                Notes[i].StartMovement(NoteStartX, NoteResetX);
                yield return new WaitForSeconds(DelayBetweenNotes);
            }

            yield return new WaitForSeconds(2);

            for (int i = 90; i >= 0; i--)
            {
                Shutter.eulerAngles = new Vector3(0, 0, i);
                ShutterGlow.color = new Color(1, 1, 1, (i / 255f));
                yield return new WaitForSeconds(0.001f);
            }
        }
    }
}

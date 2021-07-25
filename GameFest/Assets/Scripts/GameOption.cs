using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameOption : MonoBehaviour
{
    public GameObject Selection;

    public Scene SceneIndex;

    public VideoPlayer Video;
    public RawImage VideoImage;

    private bool _fading = false;

    private short _movementIndex;

    private IEnumerator _activeCoroutine;

    public void Selected(short movementIndex)
    {
        _movementIndex = movementIndex;
        Selection.SetActive(true);
        _activeCoroutine = WaitBeforeVideo();
        StartCoroutine(_activeCoroutine);
        Video.loopPointReached -= Video_loopPointReached;

        Video.loopPointReached += Video_loopPointReached;
    }

    private void Video_loopPointReached(VideoPlayer source)
    {
        StartCoroutine(FadeVideoOut());
    }

    private IEnumerator FadeVideoOut()
    {
        for (float i = 1; i >= 0; i -= 0.01f)
        {
            VideoImage.color = new Color(1, 1, 1, i);
            yield return new WaitForSeconds(.005f);
        }
        VideoImage.color = new Color(1, 1, 1, 0);
    }

    private IEnumerator WaitBeforeVideo()
    {
        yield return new WaitForSeconds(2f);
        if (PlayerManagerScript.Instance.MovedSinceVideoTriggered(_movementIndex) && !_fading)
        {
            Video.time = 0;
            Video.Play();
            VideoImage.gameObject.SetActive(true);
            _fading = true;
            for (float i = 0; i < 1 && _fading; i += 0.01f)
            {
                VideoImage.color = new Color(1, 1, 1, i);
                yield return new WaitForSeconds(.005f);
            }
            if (_fading)
                VideoImage.color = new Color(1, 1, 1, 1);
        }
        _fading = false;
    }

    public void Deselected()
    {
        if (_activeCoroutine != null)
            StopCoroutine(_activeCoroutine);
        _fading = false;
        Selection.SetActive(false);
        Video.Stop();
        Video.time = 0;
        VideoImage.gameObject.SetActive(false);
        VideoImage.color = new Color(1, 1, 1, 0);
    }

}

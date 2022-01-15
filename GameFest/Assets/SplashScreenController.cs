using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreenController : MonoBehaviour
{
    // TODO: make theses serializefields instead of public
    public SpriteRenderer Icon;
    public SpriteRenderer NameImage;
    public Sprite EyebrowRaiseImage;

    public AudioSource EyebrowNoise;

    public Image FadeOutImage;

    private void Start()
    {
        // start animating
        StartCoroutine(SplashScreenAnimation_());
    }

    /// <summary>
    /// Shows the icon and then loads menu
    /// </summary>
    IEnumerator SplashScreenAnimation_()
    {
        // grow full size
        while (Icon.transform.localScale.x < 1)
        {
            Icon.transform.localScale += new Vector3(0.025f, 0.025f);
            yield return new WaitForSeconds(0.001f);
        }

        // bounce back a little
        while (Icon.transform.localScale.x > 0.85f)
        {
            Icon.transform.localScale -= new Vector3(0.025f, 0.025f);
            yield return new WaitForSeconds(0.001f);
        }

        // back to full size
        while (Icon.transform.localScale.x < 1)
        {
            Icon.transform.localScale += new Vector3(0.025f, 0.025f);
            yield return new WaitForSeconds(0.001f);
        }

        // brief pause
        yield return new WaitForSeconds(0.5f);

        // show name image
        NameImage.gameObject.SetActive(true);

        // pause
        yield return new WaitForSeconds(1.5f);

        // raise eyebrow
        Icon.sprite = EyebrowRaiseImage;
        EyebrowNoise.Play();

        // wait
        yield return new WaitForSeconds(1.5f);

        // fade to black
        var a = 0f;
        while(a < 1f)
        {
            FadeOutImage.color = new Color(0, 0, 0, a);
            a += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
        FadeOutImage.color = new Color(0, 0, 0, 1f);

        // next scene
        SceneManager.LoadScene(1);
    }

}

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
        StartCoroutine(SplashScreenAnimation_());
    }

    IEnumerator SplashScreenAnimation_()
    {
        while (Icon.transform.localScale.x < 1)
        {
            Icon.transform.localScale += new Vector3(0.025f, 0.025f);
            yield return new WaitForSeconds(0.001f);
        }

        while (Icon.transform.localScale.x > 0.85f)
        {
            Icon.transform.localScale -= new Vector3(0.025f, 0.025f);
            yield return new WaitForSeconds(0.001f);
        }

        while (Icon.transform.localScale.x < 1)
        {
            Icon.transform.localScale += new Vector3(0.025f, 0.025f);
            yield return new WaitForSeconds(0.001f);
        }

        yield return new WaitForSeconds(0.5f);

        NameImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        Icon.sprite = EyebrowRaiseImage;
        EyebrowNoise.Play();

         yield return new WaitForSeconds(1.5f);

        var a = 0f;
        while(a < 1f)
        {
            FadeOutImage.color = new Color(0, 0, 0, a);
            a += 0.05f;
            yield return new WaitForSeconds(0.05f);
        }
        FadeOutImage.color = new Color(0, 0, 0, 1f);

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(1);
    }

}

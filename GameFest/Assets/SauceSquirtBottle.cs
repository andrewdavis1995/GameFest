using System.Collections;
using UnityEngine;

public class SauceSquirtBottle : MonoBehaviour
{
    const float DROP_DELAY = 0.5f;
    const float GROW_RATE = 0.4f;

    public ParticleSystem SquirtParticle;
    public SpriteRenderer SauceImage;
    public SpriteRenderer BottleImage;
    public ChefScript Chef;

    bool _squirting = false;
    bool _done = false;

    Vector3 _saucePosition;


    void Start()
    {
        _done = false;
        _saucePosition = SauceImage.transform.localPosition;
    }

    /// <summary>
    /// Start the bottle squirt
    /// </summary>
    public void Squirt()
    {
        if (_done) return;

        _done = true;
        StartCoroutine(Squirt_());
    }

    /// <summary>
    /// Start the bottle squirt
    /// </summary>
    public void ResetSauce()
    {
        _done = false;
        SauceImage.transform.localPosition = _saucePosition;
        SauceImage.transform.localScale = new Vector3(0, 0, 1);
        Chef.SauceSlider.localPosition = new Vector3(0, -0.5f, Chef.SauceSlider.localPosition.z);
    }

    /// <summary>
    /// Stop the bottle squirt
    /// </summary>
    public void StopSquirt()
    {
        StartCoroutine(StopSquirt_());
    }

    /// <summary>
    /// Controls the squirting of sauce
    /// </summary>
    public IEnumerator Squirt_()
    {
        Chef.SauceBar.SetActive(true);
        _squirting = true;

        // start squirting
        var emission = SquirtParticle.emission;
        emission.enabled = true;

        yield return new WaitForSeconds(DROP_DELAY);

        // grow sauce image
        while(_squirting && SauceImage.transform.localScale.x < 1.2f)
        {
            SauceImage.transform.localScale += new Vector3(Time.deltaTime * GROW_RATE, Time.deltaTime * GROW_RATE);

            var percent = SauceImage.transform.localScale.x / 1.2f;
            Chef.SauceSlider.localPosition = new Vector3(0, -0.5f + percent, Chef.SauceSlider.localPosition.z);
            yield return new WaitForSeconds(0.01f);
        }

        _squirting = false;

        emission.enabled = false;
        Chef.SauceSqueezeComplete();
    }

    /// <summary>
    /// Stops the squirting of sauce
    /// </summary>
    public IEnumerator StopSquirt_()
    {
        yield return new WaitForSeconds(DROP_DELAY);
        _squirting = false;
        Chef.SauceBar.SetActive(false);
    }
}

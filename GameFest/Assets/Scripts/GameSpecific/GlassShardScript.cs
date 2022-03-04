using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Script for the glass shards in Drink Slide
/// </summary>
public class GlassShardScript : MonoBehaviour
{
    public SpriteRenderer Renderer;

    public void Create()
    {
        Renderer.sprite = DrinkSlideController.Instance.GetRandomGlassShard();
        transform.Translate(new Vector2(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f)));
        transform.eulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(0f, 360f));
        StartCoroutine(FadeOut_());
    }
    
    IEnumerator FadeOut_()
    {
        var a = 1f;
        while(a > 0)
        {
            a -= 0.01f;
            Renderer.color = new Color(1f, 1f, 1f, a);
            yield return new WaitForSeconds(0.1f);
        }
        
        // all gone
        Destroy(gameObject);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimator : MonoBehaviour
{
    public Image MyRenderer;
    public Sprite[] Sprites;

    public float Delay;

    int _index = 0;

    private void Start()
    {
        StartCoroutine(Animate_());
    }

    private IEnumerator Animate_()
    {
        while(true)
        {
            yield return new WaitForSeconds(Delay);
            MyRenderer.sprite = Sprites[_index];
            _index++;
            if(_index >= Sprites.Length)
            {
                _index = 0;
            }
        }
    }
}

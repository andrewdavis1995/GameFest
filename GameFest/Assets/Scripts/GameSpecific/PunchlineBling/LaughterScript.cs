using UnityEngine;

public class LaughterScript : MonoBehaviour
{
    float _alpha = 1f;
    TextMesh _text;

    // Start is called before the first frame update
    void Start()
    {
        _text = GetComponent<TextMesh>();
        transform.eulerAngles = new Vector3(0, 0, Random.Range(-40, 40));
        var scale = Random.Range(0.15f, 0.55f);
        transform.localScale = new Vector3(scale, scale, 1);
    }

    // Update is called once per frame
    void Update()
    {
        _alpha -= 0.007f;
        _text.color = new Color(.9f, .9f, .9f, _alpha);

        if (_alpha <= 0f)
            Destroy(gameObject);
    }
}

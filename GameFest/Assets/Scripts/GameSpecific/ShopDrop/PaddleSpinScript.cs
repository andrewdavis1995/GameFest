using UnityEngine;

public class PaddleSpinScript : MonoBehaviour
{
    float _spinRate = 0f;

    private void Start()
    {
        _spinRate = Random.Range(0.1f, 0.3f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += new Vector3(0, 0, 0.1f);
    }
}

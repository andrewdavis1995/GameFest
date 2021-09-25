using System.Collections;
using UnityEngine;

public class FloatingCountdownSign : MonoBehaviour
{
    private bool _floating = true;

    private void Start()
    {
        StartCoroutine(Float_());
    }

    IEnumerator Float_()
    {
        while (_floating)
        {
            for (var i = 0; i < 16; i++)
            {
                transform.position -= new Vector3(0, 0.016f, 0);
                yield return new WaitForSeconds(0.07f);
            }
            for (var i = 0; i < 16; i++)
            {
                transform.position += new Vector3(0, 0.016f, 0);
                yield return new WaitForSeconds(0.07f);
            }
        }
    }
}

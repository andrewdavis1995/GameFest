using UnityEngine;

public class EscrowScript : MonoBehaviour
{
    public Transform[] Platforms;
    public float Speed;

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += new Vector3(0, 0, Time.deltaTime * Speed);

        foreach (var platform in Platforms)
            platform.eulerAngles = Vector3.zero;
    }
}

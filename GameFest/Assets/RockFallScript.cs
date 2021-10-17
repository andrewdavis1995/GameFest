using UnityEngine;

public class RockFallScript : MonoBehaviour
{
    public SpriteRenderer Renderer;
    public Sprite[] RockImages;

    private void Start()
    {
        gameObject.name = "Rock";

        // change image
        Renderer.sprite = RockImages[Random.Range(0, RockImages.Length)];

        // adjust size
        var sizeOffset = Random.Range(-0.1f, 0.1f);
        Renderer.transform.localScale += new Vector3(sizeOffset, sizeOffset, 0);

        // adjust rotation
        var rotationOffset = Random.Range(-90, 90);
        Renderer.transform.eulerAngles += new Vector3(0, 0, rotationOffset);
    }

    /// <summary>
    /// Check for collisions with this object
    /// </summary>
    /// <param name="collision">The item that was collided with</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // destroy when hit the ground
        if (collision.gameObject.tag == "Ground"
         || collision.gameObject.name.Contains("Bucket"))
        {
            Destroy(gameObject);
        }
    }
}

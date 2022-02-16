using System.Collections;
using System.Linq;
using UnityEngine;

public class Crushendo : MonoBehaviour
{
    public static float CRUSHENDO_DAMAGE = 35f;

    public float PRE_START_PAUSE;
    public float BETWEEN_BLOCKS_PAUSE;

    private float BLOCK_START_Y;

    public Rigidbody2D[] Blocks;

    // Start is called before the first frame update
    void Start()
    {
        BLOCK_START_Y = Blocks.FirstOrDefault().transform.localPosition.y;
        StartCoroutine(ControlMovement_());
    }

    /// <summary>
    /// Controls the movement of the blocks
    /// </summary>
    IEnumerator ControlMovement_()
    {
        while (true)
        {
            // briefly wait
            yield return new WaitForSeconds(PRE_START_PAUSE);

            // drop all blocks
            foreach (var block in Blocks)
            {
                block.isKinematic = false;
                yield return new WaitForSeconds(BETWEEN_BLOCKS_PAUSE);
            }

            // briefly wait
            yield return new WaitForSeconds(PRE_START_PAUSE / 2f);

            // raise all blocks back to starting position
            foreach (var block in Blocks)
            {
                block.isKinematic = true;
                StartCoroutine(MoveUp(block));
                yield return new WaitForSeconds(BETWEEN_BLOCKS_PAUSE);
            }
        }
    }

    /// <summary>
    /// Moves the specified block back to starting point
    /// </summary>
    /// <param name="block">The block to move up</param>
    private IEnumerator MoveUp(Rigidbody2D block)
    {
        // move up slowly
        while(block.transform.localPosition.y < BLOCK_START_Y)
        {
            block.transform.Translate(new Vector3(0, 10 * Time.deltaTime, 0));
            yield return new WaitForSeconds(0.01f);
        }

        // adjust so it is in correcr position
        block.transform.Translate(new Vector3(0, BLOCK_START_Y - block.transform.localPosition.y, 0));
    }
}

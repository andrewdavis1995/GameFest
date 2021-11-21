using UnityEngine;

public class CartAttackController : MonoBehaviour
{
    public Collider2D[] Checkpoints;

    public static CartAttackController Instance;

    private void Start()
    {
        Instance = this;
    }
}

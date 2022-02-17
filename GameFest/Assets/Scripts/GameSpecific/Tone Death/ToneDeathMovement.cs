using UnityEngine;

public class ToneDeathMovement : MonoBehaviour
{
    public Vector3 BulletOffset;
    public Transform Pointer;
    public Transform BulletPrefab;

    public void Shoot()
    {
        var bullet = Instantiate(BulletPrefab, BulletOffset, Quaternion.identity);
        bullet.SetParent(transform);
        bullet.localPosition = BulletOffset;
        bullet.transform.eulerAngles = Pointer.eulerAngles;
        bullet.SetParent(null);
    }
}

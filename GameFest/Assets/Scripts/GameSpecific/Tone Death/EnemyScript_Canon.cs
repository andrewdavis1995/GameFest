using System.Collections;
using UnityEngine;

public class EnemyScript_Canon : EnemyControl
{
    /// <summary>
    /// Get the rotation to the player
    /// </summary>
    /// <returns>The rotation to use</returns>
    public override float GetRotation()
    {
        return GetRotationToPlayer(ToneDeathController.Instance.GetPlayers()[0]);
    }

    /// <summary>
    /// Shoots a canonball
    /// </summary>
    public override void Shoot()
    {
        StartCoroutine(ShotManager_());
    }

    /// <summary>
    /// Controls the firing of multiple shots
    /// </summary>
    IEnumerator ShotManager_()
    {
        var rotation = new Vector3(0, 0, GetRotation());

        // big cannon-ball
        Bullet(1f, rotation);
        yield return new WaitForSeconds(0.5f);

        // little cannon-ball
        Bullet(0.5f, rotation);
    }

    /// <summary>
    /// Creates a bullet
    /// </summary>
    /// <param name="sizeMultiplier">Muliplier to increase/decrease size by</param>
    /// <param name="rotation">Angle to fire at</param>
    void Bullet(float sizeMultiplier, Vector3 rotation)
    {
        // shoot
        var bullet = Instantiate(ProjectilePrefab, transform.position + BulletOffset, Quaternion.identity);
        bullet.localScale *= sizeMultiplier;

        // set config
        var bs = bullet.GetComponent<BulletScript>();
        StartCoroutine(bs.SetBounces(0));
        StartCoroutine(bs.IgnorePlayer(GetComponent<BoxCollider2D>(), -1));
        bs.SetSpeed(30f);

        // set shooting direction

        bullet.transform.Rotate(rotation);
    }
}

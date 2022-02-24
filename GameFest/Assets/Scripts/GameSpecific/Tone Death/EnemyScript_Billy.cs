using UnityEngine;

public class EnemyScript_Billy : EnemyControl
{
    public override float GetRotation()
    {
        return Random.Range(-20f, 20f);
    }

    public override void Shoot()
    {
        // shoot
        var baseDrop = Instantiate(ProjectilePrefab, transform.position + BulletOffset, Quaternion.identity);
        baseDrop.gameObject.tag = "Enemy";

        // set config
        var drop = baseDrop.GetComponent<BassDropScript>();
        drop.SetDamage(DAMAGE);
        drop.IgnoreShooter(GetCollider());

        // set shooting direction
        baseDrop.transform.Rotate(new Vector3(0, 0, GetRotation()));
        drop.Fire();
        // TODO: change animation
    }
}

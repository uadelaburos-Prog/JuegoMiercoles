using UnityEngine;

public class Spear : MonoBehaviour, IWeapon
{
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private float damage = 50f;
    [SerializeField] private float range = 3f;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private Transform playerTransform;
    private float nextFireTime = 0f;

    public void SetOwner(Transform owner) => playerTransform = owner;

    public Weapons Type => Weapons.Long_Distance;

    public bool CanShoot() => Time.time >= nextFireTime;

    public void CoolDown()
    {
        nextFireTime = Time.time + fireRate;
    }

    public void DoDamage(RaycastHit2D hit)
    {
        Debug.Log($"Daño a {hit.collider.name} por {damage} de daño");
    }

    public void Shoot()
    {
        CoolDown();
        Vector2 mouseWPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWPos - (Vector2)playerTransform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(playerTransform.position, direction, range, hitMask);
        if (hit.collider != null)
            DoDamage(hit);
    }

    private void OnDrawGizmos()
    {
        if (playerTransform == null) return;

        Vector2 origin = playerTransform.position;
        Vector2 mouseWPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mouseWPos - origin).normalized;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, range, hitMask);

        Gizmos.color = hit.collider != null ? Color.green : Color.red;
        Vector2 endPoint = hit.collider != null ? hit.point : origin + direction * range;
        Gizmos.DrawLine(origin, endPoint);

        if (hit.collider != null)
            Gizmos.DrawWireSphere(hit.point, 0.1f);
    }
}

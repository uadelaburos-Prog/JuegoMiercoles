using UnityEngine;

public class CamaraFollower : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 0, -10);
    public float smoothing = 5f;
    public LayerMask wallMask;
    private Vector3 velocity = Vector3.zero;
    private Vector3 posicionNormal => target.position + offset + new Vector3(0f,0f,0f);

    private void LateUpdate()
    {
        bool cerca = Physics.Raycast(target.position, Vector2.left, 2f, wallMask) || Physics.Raycast(target.position, Vector2.right, 2f, wallMask);

        Vector3 objetivo = cerca ? new Vector3(target.position.x, target.position.y, transform.position.z) : posicionNormal;
        Vector3 destination = Vector3.Lerp(transform.position, objetivo, smoothing * Time.fixedDeltaTime);
        transform.position = destination;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 1.0f;
    private PlayerWeapons weapons;

    private void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        if (moveX != 0 || moveY != 0)
        {
            transform.Translate(moveX * speed * Time.deltaTime, moveY * speed * Time.deltaTime, 0);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {

        }
    }
}

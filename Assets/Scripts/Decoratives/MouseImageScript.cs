using UnityEngine;

public class MouseImageScript : MonoBehaviour
{
    public Texture2D customCursor;
    public Vector2 hotspot;

    private void Awake()
    {
        Cursor.SetCursor(customCursor,hotspot, CursorMode.ForceSoftware);
    }

    private void Update()
    {
        Vector3 mousePos = Input.mousePosition;

        mousePos.z = Camera.main.nearClipPlane;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        worldPos.z = 0;

        transform.position = worldPos;
    }
}


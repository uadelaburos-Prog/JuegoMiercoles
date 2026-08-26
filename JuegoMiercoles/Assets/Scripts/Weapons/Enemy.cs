using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] public string id;
    [SerializeField] private float speed;
    //[SerializeField] private float dmg;
    [SerializeField]private Transform playerTrm;
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private string[] names;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    //private void Update()
    //{
    //    transform.position = Vector2.MoveTowards(transform.position, playerTrm.transform.position, speed * Time.deltaTime);
    //}

    private void FixedUpdate()
    {
        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        Vector2 gofuckAway = Vector2.zero;

        for (int i = 0; i < collider.Length; i++)
        {
            names.AddRange(collider[i].name.Split(new char[] { ' ' }));
        }   

        foreach(Collider2D hitcollider in collider)
        {
            if (hitcollider.CompareTag("Enemy") && hitcollider.gameObject != gameObject)
            {
                Vector2 push = (transform.position - hitcollider.transform.position);
                gofuckAway += push.normalized / push.magnitude;
            }
        }

        Vector2 player = (playerTrm.transform.position - transform.position).normalized;
        Vector2 finalDir = (player + gofuckAway * 0.5f).normalized;

        rb.MovePosition(rb.position + finalDir * speed * Time.fixedDeltaTime);
    }
}

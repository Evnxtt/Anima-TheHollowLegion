using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Perbaikan: moveSpeed 20f terlalu cepat, mari kita mulai dengan nilai yang masuk akal seperti 5f
    public float moveSpeed = 5f;
    public bool isMoving;
    private Vector2 input;
    private Animator animator;
    public LayerMask SolidObjectsLayer;

    // Tambahan (Praktik Terbaik): Dapatkan collider Hero (BoxCollider2D) 
    // agar Hero juga memiliki keberadaan fisik untuk tabrakan masa depan.
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        if (!isMoving)
        {
            var keyboard = Keyboard.current;

            if (keyboard != null)
            {
                input.x = 0;
                input.y = 0;

                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y = 1;
                else if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y = -1;

                if (input.y == 0)
                {
                    if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x = 1;
                    else if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x = -1;
                }
            }

            if (input != Vector2.zero)
            {
                if (input.x != 0)
                {
                    float scaleX = (input.x > 0) ? 3 : -3;
                    transform.localScale = new Vector3(scaleX, 3, 3);
                }

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (isWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }

        animator.SetBool("isMoving", isMoving);
    }

    IEnumerator Move(Vector3 targetpos)
    {
        isMoving = true;

        while ((targetpos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetpos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetpos;

        isMoving = false;
    }

    private bool isWalkable(Vector3 targetpos)
    {
        // Perbaikan Kritis: Menggunakan Physics2D.BoxCast untuk pemeriksaan yang lebih tangguh.
        // Asumsi grid berukuran 1x1. Kita memeriksa area sedikit lebih kecil dari 1x1 (misalnya, 0.8x0.8) 
        // agar toleran terhadap ketidaksempurnaan grid dan tidak mendeteksi dinding di sampingnya.

        Vector2 checkSize = new Vector2(0.8f, 0.8f);
        Vector2 checkOrigin = (Vector2)targetpos; // Cast dari posisi target

        // BoxCast ini seperti overlapBox tapi sedikit lebih hemat memori karena kita tidak menembakkan sinar.
        RaycastHit2D hit = Physics2D.BoxCast(checkOrigin, checkSize, 0f, Vector2.zero, 0f, SolidObjectsLayer);

        if (hit.collider != null)
        {
            // Deteksi tabrakan dengan objek padat!
            return false;
        }
        return true;
    }
}
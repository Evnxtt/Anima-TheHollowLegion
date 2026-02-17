using System.Collections;
using UnityEngine;
// 1. Tambahkan library Input System
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed;
    public bool isMoving;
    private Vector2 input;
    private Animator animator;
    public LayerMask SolidObjectsLayer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!isMoving)
        {
            // 2. Ganti Input.GetAxisRaw dengan cara New Input System
            // Kita mengambil input langsung dari Keyboard saat ini
            var keyboard = Keyboard.current;

            if (keyboard != null)
            {
                input.x = 0;
                input.y = 0;

                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y = 1;
                else if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y = -1;

                // Prioritas: Jika tidak gerak vertikal, baru cek horizontal (mencegah gerak diagonal)
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
                    // Jika x > 0 (kanan) skala x jadi 1, jika x < 0 (kiri) skala x jadi -1
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
        if (Physics2D.OverlapCircle(targetpos, 0.2f, SolidObjectsLayer) != null)
        {
            return false;
        }
        return true;
    }

}
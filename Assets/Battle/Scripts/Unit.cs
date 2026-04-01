using UnityEngine;

// Wajib terpasang di GameObject karakter bersama Animator
[RequireComponent(typeof(Animator))]
public class Unit : MonoBehaviour
{
    public string unitName;
    public bool isPlayerTeam;

    [Header("Stats")]
    public int maxHP;
    public int currentHP;
    public int attack;
    public int defense;
    public int speed;

    [Header("UI")]
    public HealthBar healthBar;

    [Header("Visual Effects")]
    public SpriteRenderer unitSprite;

    [HideInInspector]
    public Vector3 startPosition;
    [HideInInspector]
    public int temporaryDefenseBoost = 0;

    // --- BAGIAN TAMBAHAN UNTUK ANIMASI ---
    private Animator animator;

    private void Awake()
    {
        // Mengambil komponen Animator saat game dimulai
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentHP = maxHP;
        startPosition = transform.position;

        // --- KODE UI KAMU TETAP AMAN DI SINI ---
        if (healthBar != null)
        {
            healthBar.SetUnitData(unitName, maxHP);
        }
    }

    // --- FUNGSI HELPER UNTUK DIPANGGIL BATTLEMANAGER (BARU) ---

    public void PlayWalkAnimation(bool moving)
    {
        if (animator != null) animator.SetBool("isMoving", moving);
    }

    public void PlayAttackAnimation()
    {
        // Pastikan nama Trigger persis seperti yang dibuat di Animator Unity
        if (animator != null) animator.SetTrigger("attackTrigger");
    }

    public void PlayDefenseAnimation()
    {
        if (animator != null) animator.SetTrigger("defenseTrigger");
    }

    public void PlayDieAnimation()
    {
        if (animator != null) animator.SetTrigger("dieTrigger");
    }

    public void SetHighlight(Color color)
    {
        if (unitSprite != null)
        {
            unitSprite.color = color;
        }
    }

    public void ResetHighlight()
    {
        if (unitSprite != null)
        {
            unitSprite.color = Color.white;
        }
    }
}
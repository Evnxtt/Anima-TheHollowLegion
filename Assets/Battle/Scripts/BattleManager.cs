using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Wajib untuk komponen UI Image
using TMPro;

public enum BattleState { START, NEXT_TURN, WAITING_PLAYER_INPUT, SELECTING_TARGET, PERFORMING_ACTION, BATTLE_OVER }

public class BattleManager : MonoBehaviour
{
    public BattleState state;
    public static BattleManager instance;

    void Awake() { instance = this; }

    [Header("Setup Tim")]
    public List<Unit> playerTeam;
    public List<Unit> enemyTeam;

    [Header("Pengaturan Scene")]
    public string sceneTujuanSetelahMenang;
    public string sceneTujuanSetelahKalah = "Main_Menu";

    [Header("Pengaturan UI & Audio")]
    public AudioManager audioManager;
    public TextMeshProUGUI battleLogText;
    public GameObject battleEndOverlay;
    private CanvasGroup overlayCanvasGroup;
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    // --- FITUR BARU: DIALOG SETELAH MENANG ---
    [Header("Pengaturan Dialog Kemenangan")]
    public GameObject dialoguePanel; // Panel penampung gambar dialog
    public Image dialogueImage; // Komponen Image yang bakal diganti-ganti gambarnya
    public List<Sprite> victoryDialogues; // Tarik gambar-gambar dialogmu ke sini
    private int currentDialogueIndex = 0;

    [Header("Pengaturan Animasi")]
    public float animationDuration = 0.5f;
    public float moveSpeed = 10f;
    public float stopDistance = 1.2f;

    private List<Unit> turnQueue = new List<Unit>();
    private Unit activeUnit;
    private Unit lastHoveredUnit;

    void Start()
    {
        state = BattleState.START;
        if (battleEndOverlay != null)
        {
            overlayCanvasGroup = battleEndOverlay.GetComponent<CanvasGroup>();
            battleEndOverlay.SetActive(false);
        }
        victoryPanel?.SetActive(false);
        defeatPanel?.SetActive(false);

        // Pastikan panel dialog disembunyikan di awal
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        StartCoroutine(SetupBattle());
    }

    void Update()
    {
        if (state == BattleState.SELECTING_TARGET)
        {
            HandleTargetSelectionUI();
        }
    }

    // --- LOGIKA HOVER & CLICK GLOW ---
    void HandleTargetSelectionUI()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null)
        {
            Unit currentUnit = hit.collider.GetComponent<Unit>();

            if (currentUnit != null && !currentUnit.isPlayerTeam && currentUnit.currentHP > 0)
            {
                if (lastHoveredUnit != null && lastHoveredUnit != currentUnit)
                    lastHoveredUnit.ResetHighlight();

                currentUnit.SetHighlight(Color.yellow);
                lastHoveredUnit = currentUnit;

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    currentUnit.SetHighlight(Color.red);
                    SelectTarget(currentUnit);
                }
            }
        }
        else
        {
            if (lastHoveredUnit != null)
            {
                lastHoveredUnit.ResetHighlight();
                lastHoveredUnit = null;
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (lastHoveredUnit != null) lastHoveredUnit.ResetHighlight();
            CancelTargetSelection();
        }
    }

    public void UpdateLog(string message)
    {
        if (battleLogText != null) battleLogText.text = message;
        Debug.Log(message);
    }

    IEnumerator SetupBattle()
    {
        UpdateLog("Pertarungan Dimulai!");
        turnQueue.AddRange(playerTeam);
        turnQueue.AddRange(enemyTeam);
        turnQueue = turnQueue.OrderByDescending(u => u.speed).ToList();

        yield return new WaitForSeconds(1f);
        state = BattleState.NEXT_TURN;
        NextTurn();
    }

    void NextTurn()
    {
        if (state == BattleState.BATTLE_OVER) return;

        activeUnit = turnQueue[0];
        turnQueue.RemoveAt(0);
        turnQueue.Add(activeUnit);

        if (activeUnit.currentHP <= 0) { NextTurn(); return; }

        activeUnit.temporaryDefenseBoost = 0;

        if (activeUnit.isPlayerTeam)
        {
            UpdateLog("Giliran " + activeUnit.unitName + ". Pilih aksi!");
            state = BattleState.WAITING_PLAYER_INPUT;
        }
        else
        {
            UpdateLog("Giliran musuh: " + activeUnit.unitName);
            state = BattleState.PERFORMING_ACTION;
            StartCoroutine(EnemyTurn());
        }
    }

    public void OnAttackButton()
    {
        if (state != BattleState.WAITING_PLAYER_INPUT) return;
        state = BattleState.SELECTING_TARGET;
        UpdateLog("Pilih target musuh!");
    }

    public void SelectTarget(Unit targetEnemy)
    {
        if (state != BattleState.SELECTING_TARGET) return;
        state = BattleState.PERFORMING_ACTION;
        StartCoroutine(PerformAttack(activeUnit, targetEnemy));
    }

    public void OnDefenseButton()
    {
        if (state != BattleState.WAITING_PLAYER_INPUT) return;
        state = BattleState.PERFORMING_ACTION;
        UpdateLog(activeUnit.unitName + " mengambil posisi bertahan.");
        StartCoroutine(PerformDefense(activeUnit));
    }

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(1f);
        Unit bestTarget = null;
        float highestScore = -Mathf.Infinity;

        foreach (Unit player in playerTeam)
        {
            if (player.currentHP > 0)
            {
                float targetScore = EvaluateTarget(player);
                if (targetScore > highestScore)
                {
                    highestScore = targetScore;
                    bestTarget = player;
                }
            }
        }

        if (bestTarget != null) StartCoroutine(PerformAttack(activeUnit, bestTarget));
        else NextTurn();
    }

    IEnumerator PerformAttack(Unit attacker, Unit target)
    {
        UpdateLog(attacker.unitName + " menyerang " + target.unitName + "!");

        Vector3 originalPos = attacker.startPosition;
        Vector3 direction = (target.transform.position - attacker.transform.position).normalized;
        Vector3 targetPos = target.transform.position - (direction * stopDistance);

        attacker.PlayWalkAnimation(true);
        while (Vector3.Distance(attacker.transform.position, targetPos) > 0.1f)
        {
            attacker.transform.position = Vector3.MoveTowards(attacker.transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        attacker.PlayWalkAnimation(false);

        attacker.PlayAttackAnimation();
        yield return new WaitForSeconds(0.4f);

        int totalDefense = target.defense + target.temporaryDefenseBoost;
        int damage = Mathf.Max(1, attacker.attack - totalDefense);
        target.currentHP = Mathf.Max(0, target.currentHP - damage);

        UpdateLog(target.unitName + " menerima " + damage + " damage!");

        if (target.healthBar != null) target.healthBar.SetHealth(target.currentHP);
        yield return new WaitForSeconds(0.4f);

        attacker.PlayWalkAnimation(true);
        while (Vector3.Distance(attacker.transform.position, originalPos) > 0.1f)
        {
            attacker.transform.position = Vector3.MoveTowards(attacker.transform.position, originalPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        attacker.transform.position = originalPos;
        attacker.PlayWalkAnimation(false);

        target.ResetHighlight();

        if (target.currentHP <= 0)
        {
            UpdateLog(target.unitName + " tumbang!");
            target.PlayDieAnimation();
            yield return new WaitForSeconds(1.5f);
            target.gameObject.SetActive(false);
        }

        CheckBattleEnd();
    }

    IEnumerator PerformDefense(Unit defender)
    {
        defender.PlayDefenseAnimation();
        defender.temporaryDefenseBoost = defender.defense;
        yield return new WaitForSeconds(1f);
        CheckBattleEnd();
    }

    void CheckBattleEnd()
    {
        bool isPlayerAlive = playerTeam.Any(u => u.currentHP > 0);
        bool isEnemyAlive = enemyTeam.Any(u => u.currentHP > 0);

        if (!isPlayerAlive)
        {
            UpdateLog("Kekalahan...");
            state = BattleState.BATTLE_OVER;
            StartCoroutine(LoseAndChangeScene());
        }
        else if (!isEnemyAlive)
        {
            UpdateLog("Kemenangan!");
            state = BattleState.BATTLE_OVER;
            StartCoroutine(WinAndChangeScene());
        }
        else
        {
            state = BattleState.NEXT_TURN;
            NextTurn();
        }
    }

    // --- FUNGSI ANIMASI UI & TRANSISI SCENE ---
    IEnumerator WinAndChangeScene()
    {
        if (audioManager != null) { audioManager.StopMusic(); audioManager.playSFX(audioManager.victory); }
        if (battleEndOverlay != null) { battleEndOverlay.SetActive(true); victoryPanel.SetActive(true); StartCoroutine(AnimateBattleEndUI(victoryPanel)); }

        // Kasih jeda 2 detik buat player ngerayain tulisan "VICTORY"
        yield return new WaitForSeconds(2f);

        // Cek apakah ada gambar dialog yang disiapkan
        if (victoryDialogues != null && victoryDialogues.Count > 0)
        {
            victoryPanel.SetActive(false); // Sembunyiin tulisan Victory
            dialoguePanel.SetActive(true); // Munculin panel dialog
            currentDialogueIndex = 0;
            ShowDialogue(); // Tampilkan gambar pertama
        }
        else
        {
            // Kalau nggak ada dialog sama sekali, langsung pindah scene
            SceneManager.LoadScene(sceneTujuanSetelahMenang);
        }
    }

    IEnumerator LoseAndChangeScene()
    {
        if (audioManager != null) { audioManager.StopMusic(); audioManager.playSFX(audioManager.defeat); }
        if (battleEndOverlay != null) { battleEndOverlay.SetActive(true); defeatPanel.SetActive(true); StartCoroutine(AnimateBattleEndUI(defeatPanel)); }
        yield return new WaitForSeconds(animationDuration + 3f);
        SceneManager.LoadScene(sceneTujuanSetelahKalah);
    }

    // --- FUNGSI UNTUK KONTROL DIALOG GAMBAR ---
    void ShowDialogue()
    {
        if (currentDialogueIndex < victoryDialogues.Count)
        {
            dialogueImage.sprite = victoryDialogues[currentDialogueIndex];
        }
    }

    // FUNGSI INI YANG AKAN DIPANGGIL OLEH TOMBOL "NEXT"
    public void NextDialogueButton()
    {
        currentDialogueIndex++; // Lanjut ke gambar berikutnya

        if (currentDialogueIndex < victoryDialogues.Count)
        {
            ShowDialogue(); // Tampilkan gambar selanjutnya
        }
        else
        {
            // Jika sudah di gambar terakhir, pindah scene!
            SceneManager.LoadScene(sceneTujuanSetelahMenang);
        }
    }

    IEnumerator AnimateBattleEndUI(GameObject panelToPop)
    {
        float timer = 0f;
        Vector3 targetScale = panelToPop.transform.localScale;
        panelToPop.transform.localScale = Vector3.zero;
        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / animationDuration;
            overlayCanvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            panelToPop.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, BounceEasing(progress));
            yield return null;
        }
        panelToPop.transform.localScale = targetScale;
    }

    float BounceEasing(float x) { return x == 0f ? 0f : x == 1f ? 1f : Mathf.Pow(2f, -10f * x) * Mathf.Sin((x * 10f - 0.75f) * ((2f * Mathf.PI) / 3f)) + 1f; }

    public void CancelTargetSelection() { state = BattleState.WAITING_PLAYER_INPUT; UpdateLog("Batal menyerang."); }

    float EvaluateTarget(Unit target)
    {
        float score = 0f;

        // 1. FAKTOR DARAH (HP) - Makin sekarat, makin diincar
        // Pastikan maxHP di Unit.cs tidak 0 agar tidak error pembagian
        float hpPercentage = (float)target.currentHP / target.maxHP;

        if (hpPercentage <= 0.25f)
            score += 60f; // Darah tinggal 25%? Prioritas utama buat dibunuh!
        else if (hpPercentage <= 0.5f)
            score += 30f; // Darah setengah, lumayan menarik.
        else
            score += 10f; // Darah masih banyak.

        // 2. FAKTOR ANCAMAN (Attack) - Serang yang damage-nya paling sakit
        score += (target.attack * 2f);

        // 3. FAKTOR ALOT (Defense) - Males nyerang yang defense-nya gede atau lagi bertahan
        int totalDefense = target.defense + target.temporaryDefenseBoost;
        score -= (totalDefense * 2.5f);

        // 4. FAKTOR FUZZY / RANDOM - Sifat tidak tertebak
        // Kadang musuh bikin keputusan "bodoh" atau random di luar hitungan
        score += Random.Range(0f, 30f);

        return score;
    }
}
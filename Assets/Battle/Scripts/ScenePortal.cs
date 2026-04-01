using UnityEngine;
using UnityEngine.SceneManagement; // Wajib dipanggil untuk mengurus perpindahan scene

public class ScenePortal : MonoBehaviour
{
    [Tooltip("Tulis nama scene tujuan di sini persis seperti nama file scene-nya")]
    public string sceneTujuan;

    // Fungsi ini otomatis terpanggil ketika ada objek masuk ke dalam area collider "Is Trigger"
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Mengecek apakah yang menginjak portal ini adalah Hero (yang punya Tag "Player")
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Hero menginjak portal! Pindah ke scene: " + sceneTujuan);

            // Memuat scene baru
            SceneManager.LoadScene(sceneTujuan);
        }
    }
}
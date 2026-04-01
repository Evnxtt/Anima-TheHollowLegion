using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Text nameText; // Tambahkan ini untuk teks nama (Legacy Text)
    // Catatan: Jika kamu menggunakan TextMeshPro di Unity 6.3, gunakan:
    // public TMPro.TextMeshProUGUI nameText; 

    // Ubah nama fungsi ini agar lebih pas, karena sekarang menerima nama juga
    public void SetUnitData(string name, int maxHealth)
    {
        // Mengisi teks nama UI dengan nama dari karakter
        if (nameText != null)
        {
            nameText.text = name;
        }

        slider.maxValue = maxHealth;
        slider.value = maxHealth;
    }

    public void SetHealth(int health)
    {
        slider.value = health;
    }
}
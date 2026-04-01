using UnityEngine;

public class Trigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            // 1. Cek apakah ADA yang menyentuh kotak trigger
            Debug.Log("Halo! Objek yang menyentuhku adalah: " + collision.gameObject.name);

            if (collision.CompareTag("Player"))
            {
                // 2. Cek apakah objek itu benar-benar Player
                Debug.Log("Sip, Player terdeteksi! Mencoba pindah scene...");
                SceneController.instance.NextScene();
            }

            SceneController.instance.NextScene();
        }
    }
}

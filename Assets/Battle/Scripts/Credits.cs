using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 

public class Credits : MonoBehaviour
{
    [Header("Pengaturan Scroll")]
    public float scrollSpeed = 40f;

    [Tooltip("Titik Y di mana credits dianggap selesai dan pindah scene")]
    public float endPositionY = 1500f;

    [Header("Pengaturan Scene")]
    public string nextSceneName;

    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {

        rectTransform.anchoredPosition += new Vector2(0, scrollSpeed * Time.deltaTime);

        if (rectTransform.anchoredPosition.y >= endPositionY)
        {
            LoadNextScene();
        }

        bool isMouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool isEscPressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;

        if (isMouseClicked || isEscPressed)
        {
            LoadNextScene();
        }
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Waduh, Nama Scene Tujuan belum diisi di Inspector nih!");
        }
    }
}
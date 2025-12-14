using UnityEngine;
using TMPro;
using UnityEngine.InputSystem; // New Input System

public class ARTapToCollect : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] int scorePerItem = 10;

    int score;

    void Start() => UpdateUI();

    void Update()
    {
        // New Input System: 터치/마우스 통합 (폰 터치도 Pointer로 들어옴)
        if (Pointer.current == null) return;

        if (!Pointer.current.press.wasPressedThisFrame) return;

        Vector2 screenPos = Pointer.current.position.ReadValue();
        TryHitAndCollect(screenPos);
    }

    void TryHitAndCollect(Vector2 screenPos)
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 999f))
        {
            Debug.Log("HIT: " + hit.collider.name);

            GameObject target = hit.collider.transform.root.gameObject;
            Destroy(target);

            score += scorePerItem;
            UpdateUI();
        }
        else
        {
            Debug.Log("NO HIT");
        }
    }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = $"Score: {score}";
    }
}

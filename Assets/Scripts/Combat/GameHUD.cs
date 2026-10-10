using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    public Text healthText;
    public Text staminaText;
    public Text statusText;
    public PlayerController player;

    private void Start()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        if (healthText == null || staminaText == null || statusText == null)
            CreateDefaultHUD();
    }

    private void Update()
    {
        if (player == null)
            player = FindObjectOfType<PlayerController>();

        if (player == null)
            return;

        UpdateHUD(player);
    }

    public void UpdateHUD(PlayerController playerController)
    {
        if (playerController == null)
            return;

        if (healthText != null)
            healthText.text = "Health: " + playerController.currentHealth + " / " + playerController.maxHealth;

        if (staminaText != null)
            staminaText.text = "Stamina: " + Mathf.Ceil(playerController.stamina) + " / " + playerController.maxStamina;
    }

    public void ShowStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    private void CreateDefaultHUD()
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        GameObject healthGO = new GameObject("HealthText", typeof(Text));
        healthGO.transform.SetParent(transform, false);
        RectTransform healthRect = healthGO.GetComponent<RectTransform>();
        healthRect.anchorMin = new Vector2(0f, 1f);
        healthRect.anchorMax = new Vector2(0f, 1f);
        healthRect.pivot = new Vector2(0f, 1f);
        healthRect.anchoredPosition = new Vector2(30f, -30f);
        healthRect.sizeDelta = new Vector2(300f, 40f);
        healthText = healthGO.GetComponent<Text>();
        healthText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        healthText.fontSize = 24;
        healthText.color = Color.white;

        GameObject staminaGO = new GameObject("StaminaText", typeof(Text));
        staminaGO.transform.SetParent(transform, false);
        RectTransform staminaRect = staminaGO.GetComponent<RectTransform>();
        staminaRect.anchorMin = new Vector2(0f, 1f);
        staminaRect.anchorMax = new Vector2(0f, 1f);
        staminaRect.pivot = new Vector2(0f, 1f);
        staminaRect.anchoredPosition = new Vector2(30f, -70f);
        staminaRect.sizeDelta = new Vector2(300f, 40f);
        staminaText = staminaGO.GetComponent<Text>();
        staminaText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        staminaText.fontSize = 22;
        staminaText.color = Color.cyan;

        GameObject statusGO = new GameObject("StatusText", typeof(Text));
        statusGO.transform.SetParent(transform, false);
        RectTransform statusRect = statusGO.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.5f, 1f);
        statusRect.anchorMax = new Vector2(0.5f, 1f);
        statusRect.pivot = new Vector2(0.5f, 1f);
        statusRect.anchoredPosition = new Vector2(0f, -50f);
        statusRect.sizeDelta = new Vector2(600f, 60f);
        statusText = statusGO.GetComponent<Text>();
        statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        statusText.fontSize = 28;
        statusText.alignment = TextAnchor.MiddleCenter;
        statusText.color = Color.yellow;
    }
}

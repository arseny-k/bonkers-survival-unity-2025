using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("UI Text Elements")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI premiumText;

    [Header("Panels")]
    public GameObject shopPanel;
    public GameObject mainMenuPanel;

    private void Start()
    {
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OnPlayerDataUpdated += RefreshUI;
            RefreshUI();
        }

        mainMenuPanel.SetActive(true);
        shopPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OnPlayerDataUpdated -= RefreshUI;
        }
    }

    private void RefreshUI()
    {
        int currentGold = IAPManager.Instance.Gold;
        bool hasNoAds = IAPManager.Instance.HasNoAds;
        int premiumDays = IAPManager.Instance.PremiumDays;

        goldText.text = currentGold.ToString("N0");

        if (premiumDays > 0)
        {
            premiumText.text = $"Premium: {premiumDays}d";
            premiumText.color = Color.green;
            statusText.text = "VIP Status";
        }
        else
        {
            premiumText.text = "Get Premium";
            premiumText.color = Color.white;
            statusText.text = hasNoAds ? "No Ads" : "Free User";
        }
    }

    public void OnPlayClick() => SceneManager.LoadScene("Level1");
    public void OnShopClick(){ shopPanel.SetActive(true); mainMenuPanel.SetActive(false); }
    public void CloseShop() { shopPanel.SetActive(false); mainMenuPanel.SetActive(true); }
    public void OnQuitClick() => Application.Quit();
}
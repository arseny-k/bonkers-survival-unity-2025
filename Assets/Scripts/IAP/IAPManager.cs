using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

public class IAPManager : MonoBehaviour
{
    public static IAPManager Instance { get; private set; }

    private StoreController _storeController;

    public event Action OnPlayerDataUpdated;

    public int Gold { get; private set; } = 100;
    public bool HasNoAds { get; private set; } = false;
    public int PremiumDays { get; private set; } = 0;

    private string _currentPurchasingProductId;

    public const string ProductGold100 = "com.haloga.games.gold100";
    public const string ProductNoAds = "com.haloga.games.noads";
    public const string ProductPremium = "com.haloga.games.premium";

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private async void Start()
    {
        await InitializeIAP();
    }

    private async Task InitializeIAP()
    {
        try
        {
            _storeController = UnityIAPServices.StoreController();

            _storeController.OnPurchasePending += OnPurchasePending;
            _storeController.OnPurchasesFetched += OnPurchasesFetched;
            _storeController.OnPurchaseFailed += OnPurchaseFailed;
            _storeController.OnStoreDisconnected += OnStoreDisconnected;
            _storeController.OnProductsFetchFailed += OnProductsFetchFailed;

            await _storeController.Connect();

            var productsToFetch = new List<ProductDefinition>
            {
                new ProductDefinition(ProductGold100, ProductType.Consumable),
                new ProductDefinition(ProductNoAds, ProductType.NonConsumable),
                new ProductDefinition(ProductPremium, ProductType.Subscription)
            };

            _storeController.FetchProducts(productsToFetch);
        }
        catch (Exception e)
        {
            Debug.LogError($"IAP Init Error: {e.Message}");
        }
    }

    private void OnPurchasePending(PendingOrder order)
    {
        _storeController.ConfirmPurchase(order);

        if (!string.IsNullOrEmpty(_currentPurchasingProductId))
        {
            ProcessReward(_currentPurchasingProductId);
            _currentPurchasingProductId = null;
        }
        else
        {
            CheckEntitlements();
        }
    }

    private void ProcessReward(string productId)
    {
        Debug.Log($"IAP: ProcessReward called for ID: '{productId}'");

        bool dataChanged = false;

        if (productId == ProductGold100)
        {
            Debug.Log("IAP: Match found! Adding Gold...");
            Gold += 100;
            dataChanged = true;
        }
        else if (productId == ProductNoAds)
        {
            Debug.Log("IAP: Match found! Removing Ads...");
            HasNoAds = true;
            dataChanged = true;
        }
        else if (productId == ProductPremium)
        {
            Debug.Log("IAP: Match found! Adding Premium...");
            PremiumDays += 30;
            dataChanged = true;
        }
        else
        {
            Debug.LogError($"IAP: ID MISMATCH! Received '{productId}' but expected '{ProductGold100}'");
        }

        if (dataChanged)
        {
            Debug.Log("IAP: Data updated. Invoking Event...");
            OnPlayerDataUpdated?.Invoke();
        }
        else
        {
            Debug.LogWarning("IAP: Data did NOT change, so UI event was not fired.");
        }
    }

    private void OnPurchasesFetched(Orders orders)
    {
        CheckEntitlements();
    }

    public void CheckEntitlements()
    {
        if (_storeController == null) return;

        bool dataChanged = false;
        var products = _storeController.GetProducts();

        foreach (var product in products)
        {
#pragma warning disable CS0618      // depricated product.hasReceipt, должно работать
            if (product.hasReceipt)
            {
                if (product.definition.id == ProductNoAds && !HasNoAds)
                {
                    HasNoAds = true;
                    dataChanged = true;
                }
            }
#pragma warning restore CS0618
        }

        if (dataChanged)
        {
            OnPlayerDataUpdated?.Invoke();
        }
    }

    public void BuyProduct(string productId)
    {
        if (_storeController == null) return;

        _currentPurchasingProductId = productId;
        var product = _storeController.GetProducts().FirstOrDefault(p => p.definition.id == productId);

        if (product != null && product.availableToPurchase)
        {
            _storeController.PurchaseProduct(product);
        }
        else
        {
            Debug.LogError($"IAP: Cannot buy {productId}");
            _currentPurchasingProductId = null;
        }
    }

    // Boilerplate error handlers
    private void OnPurchaseFailed(FailedOrder failed) { _currentPurchasingProductId = null; Debug.LogError($"Failed: {failed}"); }
    private void OnProductsFetchFailed(ProductFetchFailed failed) => Debug.LogError($"Fetch Failed: {failed}");
    private void OnStoreDisconnected(StoreConnectionFailureDescription f) => Debug.LogError($"Disconnected: {f}");
}
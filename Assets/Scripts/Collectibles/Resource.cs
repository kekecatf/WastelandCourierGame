using UnityEngine;

public class Resource : MonoBehaviour
{
    public ResourceType type;
    public int amount = 1;
    public GameObject qtePrefab;

    private GameObject currentQTE;

    public void TryCollectWithQTE()
    {
        if (qtePrefab == null)
        {
            Debug.LogError("❌ QTE Prefab atanmadı!");
            return;
        }

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("❌ Canvas bulunamadı!");
            return;
        }

        currentQTE = Instantiate(qtePrefab, canvas.transform);

        var qte = currentQTE.GetComponent<QTESystem>();
        if (qte == null)
        {
            Debug.LogError("❌ QTESystem scripti yok!");
            return;
        }

        qte.StartQTE(
            () => { Collect(); },
            () =>
            {
                Debug.Log("❌ QTE başarısız, kaynak kaybedildi.");
                Destroy(gameObject); // 🔥 KAYNAĞI YOK ET
            }
        );
    }


    public void Collect()
    {
        var stats = GameObject.FindWithTag("Player").GetComponent<PlayerStats>();
        if (stats == null) return;

        switch (type)
        {
            case ResourceType.Stone: stats.AddResource("Stone", amount); break;
            case ResourceType.Wood: stats.AddResource("Wood", amount); break;
            case ResourceType.scrapMetal: stats.AddResource("scrapMetal", amount); break;
        }

        Destroy(gameObject);
    }
}

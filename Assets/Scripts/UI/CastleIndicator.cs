using UnityEngine;
using UnityEngine.UI;

public class CastleIndicator : MonoBehaviour
{
    public Transform player;
    public Transform castle;
    public RectTransform canvasRect;
    public RectTransform indicator;

    void Update()
    {
        if (player == null || castle == null) return;

        Vector3 direction = castle.position - player.position;

        // Kale ekranda görünüyorsa göstergeyi gizle
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(castle.position);
        bool isCastleVisible = viewportPos.x >= 0f && viewportPos.x <= 1f &&
                               viewportPos.y >= 0f && viewportPos.y <= 1f &&
                               viewportPos.z > 0f;

        if (isCastleVisible)
        {
            indicator.gameObject.SetActive(false);
            return;
        }

        indicator.gameObject.SetActive(true);

        // Gösterge yönü (ok simgesi döndürülür)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        indicator.rotation = Quaternion.Euler(0, 0, angle - 90f); // ok yukarı bakıyorsa

        // Gösterge konumu (ekranın kenarına sabitle)
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(player.position + direction.normalized * 5f);
        screenPoint = new Vector3(Mathf.Clamp01(screenPoint.x), Mathf.Clamp01(screenPoint.y), 0);

        Vector2 canvasSize = canvasRect.sizeDelta;
        Vector2 uiPos = new Vector2(
            (screenPoint.x * canvasSize.x) - (canvasSize.x / 2),
            (screenPoint.y * canvasSize.y) - (canvasSize.y / 2)
        );

        indicator.anchoredPosition = uiPos;
    }

}

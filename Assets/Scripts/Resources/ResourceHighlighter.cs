using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ResourceHighlighter : MonoBehaviour
{
    public float detectionRange = 1.5f;
    private Transform player;
    private SpriteRenderer sr;
    private Color defaultColor;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        sr = GetComponent<SpriteRenderer>();
        defaultColor = sr.color;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(player.position, transform.position);
        if (distance <= detectionRange)
        {
            sr.color = Color.white; // Yakındaysa beyaz parlasın
        }
        else
        {
            sr.color = defaultColor; // Uzaksa orijinal renge dön
        }
    }
}

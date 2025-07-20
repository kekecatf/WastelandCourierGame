using UnityEngine;
using UnityEngine.InputSystem; // YENİ Input System için şart

public class QTESystem : MonoBehaviour
{
    public RectTransform pointer;
    public RectTransform targetZone;
    public float rotateSpeed = 120f;

    public System.Action onSuccess;
    public System.Action onFail;

    private bool isRunning = false;
    private float angle = 0f;

    void Update()
    {
        if (!isRunning) return;

        angle += rotateSpeed * Time.deltaTime;
        pointer.localEulerAngles = new Vector3(0, 0, -angle);

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            float pointerZ = pointer.localEulerAngles.z;
            float targetZ = targetZone.localEulerAngles.z;

            float diff = Mathf.DeltaAngle(pointerZ, targetZ); // -180° ile +180° arasında fark verir

            if (Mathf.Abs(diff) < 15f)  // 🔥 15 derece içinde ise başarılı (isteğe göre daraltabilirsin)
            {
                onSuccess?.Invoke();
            }
            else
            {
                onFail?.Invoke();
            }

            isRunning = false;
            gameObject.SetActive(false);
        }

    }

    public void StartQTE(System.Action successCallback, System.Action failCallback)
    {
        onSuccess = successCallback;
        onFail = failCallback;
        angle = 0f;
        isRunning = true;
        gameObject.SetActive(true);
    }
}

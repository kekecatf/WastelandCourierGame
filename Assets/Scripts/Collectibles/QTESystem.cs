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
            float diff = Mathf.Abs(Mathf.DeltaAngle(angle, targetZone.localEulerAngles.z));
            if (diff < 20f)
                onSuccess?.Invoke();
            else
                onFail?.Invoke();

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

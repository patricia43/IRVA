using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;

public class ARInfoDisplay : MonoBehaviour
{
    [Header("AR Managers")]
    public ARPlaneManager planeManager;
    public ARPointCloudManager pointCloudManager;

    [Header("References")]
    public Camera arCamera;
    public TextMeshProUGUI infoText;

    private float updateInterval = 0.5f; // update la fiecare jumătate de secundă
    private float timer = 0f;

    void Update()
    {
        if (planeManager == null || pointCloudManager == null || arCamera == null || infoText == null)
            return;

        timer += Time.deltaTime;
        if (timer < updateInterval)
            return;

        timer = 0f;

        // numărul de plane detectate
        int planeCount = planeManager.trackables.count;

        // numărul total de puncte caracteristice
        int pointCount = 0;
        foreach (var pointCloud in pointCloudManager.trackables)
        {
            if (pointCloud.positions.HasValue)
                pointCount += pointCloud.positions.Value.Length;
        }

        // poziția și rotația camerei
        Vector3 camPos = arCamera.transform.position;
        Vector3 camRot = arCamera.transform.eulerAngles;

        // actualizăm textul
        infoText.text =
            $"<b>Planes:</b> {planeCount}\n" +
            $"<b>Feature Points:</b> {pointCount}\n" +
            $"<b>Camera Pos:</b> {camPos:F2}\n" +
            $"<b>Camera Rot:</b> {camRot:F2}";
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FaceMaterialCycler : MonoBehaviour
{
    [Tooltip("List of materials to cycle through for all detected AR faces.")]
    public List<Material> faceMaterials;

    [Tooltip("Optional: ARFaceManager in the scene (will be found automatically if left null).")]
    public ARFaceManager faceManager;

    [Tooltip("If true, apply the currently selected material to faces that appear after the change.")]
    public bool applyToFutureFaces = true;

    private int currentIndex = -1;

    void Awake()
    {
        if (faceManager == null)
            faceManager = FindObjectOfType<ARFaceManager>();

        if (applyToFutureFaces && faceManager != null)
            faceManager.facesChanged += OnFacesChanged;
    }

    void OnDestroy()
    {
        if (faceManager != null)
            faceManager.facesChanged -= OnFacesChanged;
    }

    /// <summary>
    /// Call this from your UI Button's OnClick event.
    /// </summary>
    public void CycleFaceMaterials()
    {
        if (faceMaterials == null || faceMaterials.Count == 0)
        {
            Debug.LogWarning("[FaceMaterialCyclerV2] No face materials assigned!");
            return;
        }

        currentIndex = (currentIndex + 1) % faceMaterials.Count;
        Material mat = faceMaterials[currentIndex];

        Debug.Log($"[FaceMaterialCyclerV2] Cycling to material index {currentIndex} ({mat.name}).");

        ApplyMaterialToAllFaces(mat);
    }

    private void ApplyMaterialToAllFaces(Material mat)
    {
        // Find all ARFace components in the scene
        ARFace[] faces = FindObjectsOfType<ARFace>();
        Debug.Log($"[FaceMaterialCyclerV2] Found {faces.Length} ARFace(s) in scene.");

        foreach (var face in faces)
        {
            if (face == null)
                continue;

            // First try SkinnedMeshRenderer (common for face prefabs), then MeshRenderer.
            var skinned = face.GetComponentInChildren<SkinnedMeshRenderer>(true);
            var meshR = face.GetComponentInChildren<MeshRenderer>(true);

            Renderer rendererToUse = (Renderer)skinned ?? meshR;

            if (rendererToUse == null)
            {
                Debug.LogWarning($"[FaceMaterialCyclerV2] No renderer found on ARFace '{face.name}'. Check your face prefab.");
                continue;
            }

            // If the renderer has multiple sub-meshes, replace all materials
            var mats = rendererToUse.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            rendererToUse.sharedMaterials = mats;

            Debug.Log($"[FaceMaterialCyclerV2] Applied material '{mat.name}' to renderer on '{face.name}' (Renderer type: {rendererToUse.GetType().Name}, submeshes: {mats.Length}).");
        }
    }

    private void OnFacesChanged(ARFacesChangedEventArgs args)
    {
        if (currentIndex < 0) // no selection yet
            return;

        Material mat = faceMaterials[currentIndex];

        // Apply only to newly added faces (so existing faces keep already applied material)
        foreach (var added in args.added)
        {
            if (added == null) continue;

            var skinned = added.GetComponentInChildren<SkinnedMeshRenderer>(true);
            var meshR = added.GetComponentInChildren<MeshRenderer>(true);
            Renderer rendererToUse = (Renderer)skinned ?? meshR;

            if (rendererToUse == null)
            {
                Debug.LogWarning($"[FaceMaterialCyclerV2] (facesChanged) No renderer found on newly added ARFace '{added.name}'.");
                continue;
            }

            var mats = rendererToUse.sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            rendererToUse.sharedMaterials = mats;

            Debug.Log($"[FaceMaterialCyclerV2] (facesChanged) Applied material '{mat.name}' to new face '{added.name}'.");
        }
    }
}

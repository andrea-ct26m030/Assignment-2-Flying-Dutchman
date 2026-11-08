using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [SerializeField] private Material alternativeMaterial;
    [SerializeField] private Material defaultMaterial;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }


    public void OnReachedTop()
    {
        Debug.Log("Reached Top!");
        ChangeMaterial(alternativeMaterial);
    }

    public void OnReachedBottom()
    {
        Debug.Log("Reached Bottom!");
        ChangeMaterial(defaultMaterial);
    }

    private void ChangeMaterial(Material material)
    {
        meshRenderer.sharedMaterial = material;
        Debug.Log("Change Material");
    }
}
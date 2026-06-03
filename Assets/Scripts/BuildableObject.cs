using UnityEngine;

public sealed class BuildableObject : MonoBehaviour
{
    [SerializeField] private string buildableId;
    [SerializeField] private BuildableType buildableType;
    [SerializeField] private bool rotated;

    public string BuildableId => string.IsNullOrEmpty(buildableId) ? name : buildableId;
    public BuildableType BuildableType => buildableType;
    public bool Rotated => rotated;

    public void Configure(string id, BuildableType type, bool isRotated)
    {
        buildableId = id;
        buildableType = type;
        rotated = isRotated;
    }
}

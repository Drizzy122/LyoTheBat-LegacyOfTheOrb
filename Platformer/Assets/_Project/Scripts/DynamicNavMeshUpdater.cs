using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[DefaultExecutionOrder(-115)]
public class DynamicNavMeshUpdater : MonoBehaviour {

    [Header("Player Tracking")]
    [SerializeField, Tooltip("Assign the Player transform")]
    Transform player;

    [SerializeField, Range(0.01f, 1f), Tooltip("Quantization factor for position updates (lower = more frequent updates)")]
    float quantizationFactor = 0.1f;

    NavMeshSurface surface;
    Vector3 volumeSize;

    void Awake() {
        surface = GetComponent<NavMeshSurface>();
    }

    void Start() {
        // Auto-find player if not assigned
        if (player == null) {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        volumeSize = surface.size;
        surface.center = GetQuantizedCenter();
        surface.BuildNavMesh();
    }

    void Update() {
        if (player == null) return;

        var updatedCenter = GetQuantizedCenter();
        var updateNavMesh = false;

        if (surface.center != updatedCenter) {
            surface.center = updatedCenter;
            updateNavMesh = true;
        }

        if (surface.size != volumeSize) {
            volumeSize = surface.size;
            updateNavMesh = true;
        }

        if (updateNavMesh)
            surface.UpdateNavMesh(surface.navMeshData);
    }

    Vector3 GetQuantizedCenter() {
        if (player == null) return surface.center;
        return player.position.Quantize(quantizationFactor * surface.size);
    }

    void OnDrawGizmosSelected() {
        if (surface == null) surface = GetComponent<NavMeshSurface>();
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawCube(transform.position + surface.center, surface.size);
        Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
        Gizmos.DrawWireCube(transform.position + surface.center, surface.size);
    }
}

public static class Vector3Extensions {
    public static Vector3 Quantize(this Vector3 position, Vector3 quantization) {
        return Vector3.Scale(
            quantization,
            new Vector3(
                Mathf.Floor(position.x / quantization.x),
                Mathf.Floor(position.y / quantization.y),
                Mathf.Floor(position.z / quantization.z)
            ));
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class ARTapToStartSpawner : MonoBehaviour
{
    [Header("References")]
    public ARPlaneManager planeManager;

    [Header("Relic Spawn (Multiple)")]
    public List<GameObject> relicPrefabs = new List<GameObject>(); //  ¿©·¯ °³
    public int spawnCount = 5;
    public float spawnRadius = 1.2f;
    public float minDistanceFromCenter = 0.3f;

    [Header("Options")]
    public bool disablePlaneVisualAfterStart = true;

    private ARRaycastManager _raycast;
    private static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();
    private bool _hasSpawnedThisRound = false;

    private void Awake()
    {
        _raycast = GetComponent<ARRaycastManager>();
    }

    private void Update()
    {
        if (ARGameManager.I == null) return;
        if (_hasSpawnedThisRound && ARGameManager.I.IsRunning) return;

        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;
        if (IsPointerOverUI(touch.position)) return;

        if (_raycast.Raycast(touch.position, Hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = Hits[0].pose;

            if (!_hasSpawnedThisRound)
            {
                SpawnRelicsAround(pose.position);
                _hasSpawnedThisRound = true;
            }

            if (!ARGameManager.I.IsRunning)
                ARGameManager.I.BeginRound();

            if (disablePlaneVisualAfterStart)
                DisablePlanes();
        }
    }

    private void SpawnRelicsAround(Vector3 center)
    {
        if (relicPrefabs == null || relicPrefabs.Count == 0)
        {
            Debug.LogError("[ARTapToStartSpawner] relicPrefabs is empty.");
            return;
        }

        ARGameManager.I.ClearSpawned();

        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab = relicPrefabs[Random.Range(0, relicPrefabs.Count)];
            if (prefab == null) continue;

            Vector3 pos = GetRandomPositionOnXZ(center, spawnRadius, minDistanceFromCenter);
            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            GameObject go = Instantiate(prefab, pos, rot);
            ARGameManager.I.RegisterSpawned(go);
        }
    }

    private Vector3 GetRandomPositionOnXZ(Vector3 center, float radius, float minR)
    {
        float t = Random.Range(0f, 1f);
        float r = Mathf.Sqrt(t) * radius;
        r = Mathf.Max(r, minR);

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float x = Mathf.Cos(angle) * r;
        float z = Mathf.Sin(angle) * r;

        return new Vector3(center.x + x, center.y, center.z + z);
    }

    private void DisablePlanes()
    {
        if (planeManager == null) return;

        planeManager.enabled = false;
        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(false);
    }

    private bool IsPointerOverUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        var eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPos;

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    public void ResetSpawnState()
    {
        _hasSpawnedThisRound = false;
    }
}

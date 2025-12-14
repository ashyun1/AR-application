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

    [Header("Prefabs")]
    public List<GameObject> relicPrefabs = new List<GameObject>();

    [Header("Spawn Rule (C)")]
    public int firstSpawnCount = 5;     // 첫 시작 때 N개
    public int addSpawnCount = 1;       // 게임 중 추가는 1개
    public float addSpawnCooldown = 1.0f;
    public int maxTotalSpawn = 30;      // 너무 많이 쌓이는 거 방지(원하면 늘려)

    [Header("Distance Rule (Donut)")]
    public float minTapDistanceFromCamera = 1.5f;  // 가까운 바닥 터치는 무시(걸어가게)
    public float minSpawnDistanceFromCamera = 1.5f;
    public float maxSpawnDistanceFromCamera = 3.0f;

    [Header("Placement")]
    public float yOffset = 0.02f;

    [Header("Options")]
    public bool disablePlaneVisualAfterStart = true;

    ARRaycastManager _raycast;
    static readonly List<ARRaycastHit> Hits = new List<ARRaycastHit>();

    bool _started;              // 라운드 시작했는지(첫 스폰 완료)
    float _nextAddTime;         // 추가 스폰 쿨타임
    int _totalSpawnedThisRound; // 이번 라운드 총 스폰 수

    void Awake()
    {
        _raycast = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        if (ARGameManager.I == null) return;

        if (!TryGetPressDown(out Vector2 screenPos, out int fingerId))
            return;

        if (IsPointerOverUI(screenPos, fingerId))
            return;

        // 바닥(Plane)에서만 시작/추가 스폰
        if (!_raycast.Raycast(screenPos, Hits, TrackableType.PlaneWithinPolygon | TrackableType.PlaneEstimated))
            return;

        Vector3 hitPos = Hits[0].pose.position;
        var cam = Camera.main;
        if (cam == null) return;

        float tapDist = Vector3.Distance(new Vector3(cam.transform.position.x, hitPos.y, cam.transform.position.z), hitPos);
        if (tapDist < minTapDistanceFromCamera)
            return; // 가까우면 무시 → 멀리 걸어가서 터치해야 됨

        // 1) 첫 시작: N개 스폰 + 게임 시작
        if (!_started && !ARGameManager.I.IsRunning)
        {
            ARGameManager.I.ClearSpawned();
            _totalSpawnedThisRound = 0;

            SpawnMultiple(firstSpawnCount, hitPos);

            _started = true;
            _nextAddTime = Time.time + addSpawnCooldown;

            ARGameManager.I.BeginRound();

            if (disablePlaneVisualAfterStart)
                DisablePlanes();
            return;
        }

        // 2) 게임 중: 쿨타임 후 1개 추가 스폰
        if (ARGameManager.I.IsRunning && _started)
        {
            if (Time.time < _nextAddTime) return;
            if (_totalSpawnedThisRound >= maxTotalSpawn) return;

            SpawnMultiple(addSpawnCount, hitPos);
            _nextAddTime = Time.time + addSpawnCooldown;
        }
    }

    void SpawnMultiple(int count, Vector3 around)
    {
        if (relicPrefabs == null || relicPrefabs.Count == 0) return;

        var cam = Camera.main;
        if (cam == null) return;

        for (int i = 0; i < count; i++)
        {
            if (_totalSpawnedThisRound >= maxTotalSpawn) break;

            Vector3 pos = FindDonutPosition(around, cam.transform.position, minSpawnDistanceFromCamera, maxSpawnDistanceFromCamera);
            pos.y = around.y + yOffset;

            GameObject prefab = relicPrefabs[Random.Range(0, relicPrefabs.Count)];
            if (prefab == null) continue;

            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            GameObject go = Instantiate(prefab, pos, rot);

            ARGameManager.I.RegisterSpawned(go);
            _totalSpawnedThisRound++;
        }
    }

    // around(바닥 터치 지점) 근처에서 랜덤 뽑되,
    // 카메라 기준 거리 조건(min~max) 만족하는 점을 찾는 방식
    Vector3 FindDonutPosition(Vector3 around, Vector3 camPos, float minD, float maxD)
    {
        // 최대 20번 시도해서 조건 만족하는 점 찾기(실패하면 그냥 around 근처로)
        for (int tries = 0; tries < 20; tries++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(0.2f, 1.2f); // 터치 지점 주변 퍼짐(원하면 조절)
            Vector3 candidate = new Vector3(
                around.x + Mathf.Cos(angle) * radius,
                around.y,
                around.z + Mathf.Sin(angle) * radius
            );

            float d = Vector3.Distance(new Vector3(camPos.x, candidate.y, camPos.z), candidate);
            if (d >= minD && d <= maxD)
                return candidate;
        }

        return around; // 못 찾으면 그냥 터치 지점
    }

    void DisablePlanes()
    {
        if (planeManager == null) return;

        planeManager.enabled = false;
        foreach (var plane in planeManager.trackables)
            plane.gameObject.SetActive(false);
    }

    bool IsPointerOverUI(Vector2 screenPos, int fingerId)
    {
        if (EventSystem.current == null) return false;

        // 터치면 fingerId 기반 체크가 더 정확
        if (fingerId >= 0)
            if (EventSystem.current.IsPointerOverGameObject(fingerId)) return true;

        var eventData = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    bool TryGetPressDown(out Vector2 screenPos, out int fingerId)
    {
        fingerId = -1;

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                screenPos = t.position;
                fingerId = t.fingerId;
                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            screenPos = Input.mousePosition;
            return true;
        }

        screenPos = default;
        return false;
    }

    // Restart에서 호출
    public void ResetSpawnState(bool reEnablePlanes = true)
    {
        _started = false;
        _nextAddTime = 0f;
        _totalSpawnedThisRound = 0;

        if (reEnablePlanes && planeManager != null)
        {
            planeManager.enabled = true;
            foreach (var plane in planeManager.trackables)
                plane.gameObject.SetActive(true);
        }
    }
}

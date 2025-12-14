using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlaneRandomSpawner : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] ARPlaneManager planeManager;
    [SerializeField] ARRaycastManager raycastManager;

    [Header("Spawn")]
    [SerializeField] List<GameObject> spawnPrefabs = new List<GameObject>();
    [SerializeField] int maxAlive = 6;
    [SerializeField] float spawnInterval = 1.0f;
    [SerializeField] float yOffset = 0.02f;

    float timer;
    readonly List<GameObject> alive = new List<GameObject>();
    static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        if (spawnPrefabs.Count == 0) return;
        if (planeManager == null || raycastManager == null) return;

        alive.RemoveAll(x => x == null);

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        if (alive.Count >= maxAlive) return;

        // 바닥이 실제로 인식되었는지 체크 (0이면 스폰 안 함)
        if (planeManager.trackables.count == 0)
        {
            Debug.Log("planes=0");
            return;
        }

        SpawnByRaycast();

        Debug.Log("planes=" + planeManager.trackables.count);
    }

    void SpawnByRaycast()
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        if (!raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinInfinity))
        {
            Debug.Log("NO RAYCAST HIT");
            return;
        }

        Pose pose = hits[0].pose;
        // 화면 중앙 바닥 기준으로 반경 0.5m 랜덤 퍼뜨리기
        Vector2 r = Random.insideUnitCircle * 0.5f;
        Vector3 pos = pose.position + new Vector3(r.x, 0f, r.y);


        var prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Count)];
        var go = Instantiate(prefab, pos + Vector3.up * yOffset,
       Quaternion.Euler(0, Random.Range(0, 360f), 0));

        go.transform.localScale = Vector3.one * 0.3f; // 스케일 먼저

        SetLayerRecursively(go, 0); // 레이어 먼저

        AutoAddBoxCollider(go); // 콜라이더는 스케일 적용된 뒤에 만들기

        alive.Add(go);
        Debug.Log("SPAWNED: " + prefab.name);

     

        var cam = Camera.main.transform;
       

        void AutoAddBoxCollider(GameObject go)
        {
            // 이미 Collider 있으면 루트 Collider만 쓰게 보장
            var rootCol = go.GetComponent<BoxCollider>();
            if (rootCol == null) rootCol = go.AddComponent<BoxCollider>();

            var rends = go.GetComponentsInChildren<Renderer>(true);
            if (rends.Length == 0)
            {
                // 렌더러 없으면 대충 크게
                rootCol.center = Vector3.zero;
                rootCol.size = Vector3.one;
                return;
            }

            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);

            // 월드 바운즈 -> 로컬로 변환
            Vector3 centerLocal = go.transform.InverseTransformPoint(b.center);

            Vector3 sizeLocal = go.transform.InverseTransformVector(b.size);
            sizeLocal = new Vector3(Mathf.Abs(sizeLocal.x), Mathf.Abs(sizeLocal.y), Mathf.Abs(sizeLocal.z));

            // 너무 작은 콜라이더 방지(최소 크기)
            sizeLocal.x = Mathf.Max(sizeLocal.x, 0.15f);
            sizeLocal.y = Mathf.Max(sizeLocal.y, 0.15f);
            sizeLocal.z = Mathf.Max(sizeLocal.z, 0.15f);

            rootCol.center = centerLocal;
            rootCol.size = sizeLocal;
        }

        void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (Transform t in obj.transform)
                SetLayerRecursively(t.gameObject, layer);
        }

    }
}

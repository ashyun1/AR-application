using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class RelicCollectible : MonoBehaviour
{
    [Header("Collect")]
    public int scoreValue = 1;
    public GameObject collectVfxPrefab;   // 선택: 파티클 프리팹
    public AudioClip collectSfx;          // 선택
    public float destroyDelay = 0f;       // VFX 남기고 싶으면 0.1~0.3

    [Header("Raycast")]
    public LayerMask hitMask = ~0;        // 필요하면 Relic 레이어만 지정

    private AudioSource _oneShotSource;

    private void Awake()
    {
        // 간단 원샷 오디오 소스
        _oneShotSource = gameObject.AddComponent<AudioSource>();
        _oneShotSource.playOnAwake = false;
    }

    private void Update()
    {
        if (ARGameManager.I == null) return;
        if (!ARGameManager.I.IsRunning) return;

        if (Input.touchCount == 0) return;
        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        // UI 위 터치 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        Ray ray = Camera.main.ScreenPointToRay(touch.position);
        if (Physics.Raycast(ray, out RaycastHit hit, 50f, hitMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Collect();
            }
        }
    }

    private void Collect()
    {
        // 점수
        ARGameManager.I.AddScore(scoreValue);

        // VFX
        if (collectVfxPrefab != null)
        {
            Instantiate(collectVfxPrefab, transform.position, Quaternion.identity);
        }

        // SFX
        if (collectSfx != null)
        {
            _oneShotSource.PlayOneShot(collectSfx);
        }

        // 콜라이더 비활성으로 중복 수집 방지
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 시각적으로도 사라지게
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        Destroy(gameObject, Mathf.Max(0f, destroyDelay));
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
public class RelicCollectible : MonoBehaviour
{
    public int scoreValue = 1;
    public float rayDistance = 50f;
    public LayerMask hitMask = ~0;

    Camera _cam;

    void Awake()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        if (ARGameManager.I == null) return;
        if (!ARGameManager.I.IsRunning) return;

        if (!TryGetPressDown(out Vector2 pos, out int fingerId))
            return;

        if (EventSystem.current != null && fingerId >= 0 && EventSystem.current.IsPointerOverGameObject(fingerId))
            return;

        if (_cam == null) _cam = Camera.main;
        if (_cam == null) return;

        Ray ray = _cam.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
            {
                ARGameManager.I.AddScore(scoreValue);
                Destroy(gameObject);
            }
        }
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
}

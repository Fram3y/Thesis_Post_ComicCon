using UnityEngine;
using Unity.Cinemachine;

public class MapTransition : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D mapBoundary;
    private CinemachineConfiner confiner;

    void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && confiner.m_BoundingShape2D != mapBoundary)
        {
            confiner.m_BoundingShape2D = mapBoundary;
            confiner.InvalidateCache();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AISpawner : MonoBehaviour
{
    [Header("Prefabs & Count")]
    [SerializeField] GameObject aiPrefab;
    [SerializeField] Transform aiTransformParent;
    [SerializeField, Min(0)] int spawnCount = 100;

    [Header("Spawn Area")]
    [SerializeField] Vector2 center = Vector2.zero;
    [SerializeField] Vector2 size = new Vector2(200, 200);

    [ContextMenu("InstantiateAI")]
    public void InstantiateAI()
    {
        Spawn(spawnCount);
    }

    void Spawn(int n)
    {
        for (int i = 0; i < n; i++)
        {
            var pos = RandomInsideRect(center, size);
            var go = Instantiate(aiPrefab, aiTransformParent);
            go.transform.position = new Vector3(pos.x, pos.y, 0f);
        }
    }

    Vector2 RandomInsideRect(Vector2 c, Vector2 s)
    {
        float x = Random.Range(c.x - s.x * 0.5f, c.x + s.x * 0.5f);
        float y = Random.Range(c.y - s.y * 0.5f, c.y + s.y * 0.5f);
        return new Vector2(x, y);
    }

    [ContextMenu("DestroyAI")]
    public void DestroyAI()
    {
        foreach (Transform t in aiTransformParent)
            Destroy(t.gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.lightBlue;
        Gizmos.DrawWireCube(center, new Vector3(size.x, size.y, 0));
    }

}

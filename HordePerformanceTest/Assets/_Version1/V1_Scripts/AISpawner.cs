using System.Collections;
using System.Collections.Generic;
using Unity.Transforms;
using UnityEngine;

public class AISpawner : MonoBehaviour
{
    [Header("Prefabs & Parent")]
    [SerializeField] GameObject aiPrefab;
    [SerializeField] Transform aiTransformParent;

    [Header("Spawn Area")]
    [SerializeField] Vector2 center = Vector2.zero;
    [SerializeField] Vector2 size = new Vector2(70f, 40f);

    [Header("Agent Settings")]
    [SerializeField] float agentSpeed = 0.2f;
    [SerializeField] Vector2 agentDecisionRange = new Vector2(1f, 8f);
    public (Vector2 center, Vector2 size) GetSpawnArea()
    {
        return (center, size);
    }

    public List<AIMove> SpawnWave(int count)
    {
        var newAgents = new List<AIMove>(count);

        if (aiPrefab == null)
        {
            Debug.LogWarning("AISpawner: no aiPrefab assigned.");
            return newAgents;
        }

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = RandomInsideRect(center, size);
            var go = Instantiate(aiPrefab, aiTransformParent);

            go.transform.position = new Vector3(pos.x, pos.y, 0f);

            var mover = go.GetComponent<AIMove>();
            if (mover != null)
            {
                mover.Init(size, agentSpeed, agentDecisionRange);
                newAgents.Add(mover);
            }
        }

        return newAgents;
    }

    Vector2 RandomInsideRect(Vector2 c, Vector2 s)
    {
        float halfX = s.x * 0.5f;
        float halfY = s.y * 0.5f;
        float x = Random.Range(c.x - halfX, c.x + halfX);
        float y = Random.Range(c.y - halfY, c.y + halfY);
        return new Vector2(x, y);
    }

    [ContextMenu("DestroyAI")]
    public void DestroyAI()
    {
        for (int i = aiTransformParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(aiTransformParent.GetChild(i).gameObject);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(center, new Vector3(size.x, size.y, 0f));
    }

}

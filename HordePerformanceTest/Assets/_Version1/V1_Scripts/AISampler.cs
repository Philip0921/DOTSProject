using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class AISampler : MonoBehaviour
{
    [Header("Spawner Reference")]
    [SerializeField] AISpawner spawner;           // Scene spawner that knows how to spawn N more
    [SerializeField] Transform aiRoot;            // Parent that holds all spawned agents

    [Header("Wave Settings")]
    [SerializeField] int initialBatchSize = 100; // First total target
    [SerializeField] int batchStepSize = 100; // How much to INCREASE total each step
    [SerializeField] float stepInterval = 2f;  // Seconds between growth steps

    [Header("Debug (read-only)")]
    [SerializeField] int currentTargetTotal;     // 100 -> 200 -> 300
    [SerializeField] int liveAgentCount;         // How many exist right now
    [SerializeField] float runTimeSeconds;         // How long since start
    float lastFrameMS;            
    float lastDecisionMS;
    float lastMoveMS;
    float lastSpawnMS;
    bool ok60;

    float timeSinceLastStep;
    float elapsed;
    bool bootstrapped;

    int targetTotalAgents;              // Growing total target
    List<AIMove> agents;                  // All active AI scripts we manage

    // Logging
    StringBuilder csv;
    string filePath;
    int frameCount;

    void Awake()
    {
        agents = new List<AIMove>();
        csv = new StringBuilder(4096);
        filePath = System.IO.Path.Combine(Application.persistentDataPath, "oop_perf_log.json");

        timeSinceLastStep = 0f;
        elapsed = 0f;
        bootstrapped = false;
        frameCount = 0;
    }

    void Start()
    {
        // Do initial wave
        targetTotalAgents = initialBatchSize;
        SpawnToReachTarget();
        bootstrapped = true;
    }

    void Update()
    {
        float dt = Time.deltaTime;
        elapsed += dt;
        runTimeSeconds = elapsed;
        timeSinceLastStep += dt;
        frameCount++;

        // Decision phase 
        double t0 = Time.realtimeSinceStartupAsDouble;
        for (int i = 0; i < agents.Count; i++)
        {
            agents[i].TickDecision(dt);
        }
        double t1 = Time.realtimeSinceStartupAsDouble;
        lastDecisionMS = (float)((t1 - t0) * 1000.0);

        // Move phase 
        for (int i = 0; i < agents.Count; i++)
        {
            agents[i].TickMove(dt);
        }
        double t2 = Time.realtimeSinceStartupAsDouble;
        lastMoveMS = (float)((t2 - t1) * 1000.0);

        // Frame-level timing for budget check
        lastFrameMS = Time.deltaTime * 1000f;
        ok60 = lastFrameMS <= 16.67f;

        // Live counts
        liveAgentCount = agents.Count;
        currentTargetTotal = targetTotalAgents;

        // Log one row every frame
        csv.AppendLine(
            $"{{\"t\":{elapsed:F2},\"frame\":{frameCount},\"agents\":{liveAgentCount},\"frameMs\":{lastFrameMS:F3}," +
            $"\"decisionMs\":{lastDecisionMS:F3},\"moveMs\":{lastMoveMS:F3},\"spawnMs\":{lastSpawnMS:F3}," +
            $"\"ok60\":{(ok60 ? "true" : "false")}}},"
        );

        if (bootstrapped && timeSinceLastStep >= stepInterval)
        {
            timeSinceLastStep = 0f;
            targetTotalAgents += batchStepSize;
            SpawnToReachTarget();
        }
    }

    void OnDisable()
    {
        // Save log as JSON array
        if (csv == null) return;

        string json = "[\n" + csv.ToString().TrimEnd(',', '\n', '\r') + "\n]";
        System.IO.File.WriteAllText(filePath, json);
        Debug.Log($"[AISampler] OOP perf log saved to: {filePath}");
    }

    void SpawnToReachTarget()
    {
        if (spawner == null)
        {
            Debug.LogWarning("[AISampler] No AISpawner assigned.");
            return;
        }

        // Measure spawn cost like spawnMs
        double spawnStart = Time.realtimeSinceStartupAsDouble;

        // How many do we have already
        int current = agents.Count;

        int delta = targetTotalAgents - current;
        if (delta > 0)
        {
            List<AIMove> newAgents = spawner.SpawnWave(delta);
            agents.AddRange(newAgents);
        }

        double spawnEnd = Time.realtimeSinceStartupAsDouble;
        lastSpawnMS = (float)((spawnEnd - spawnStart) * 1000.0f);

    }

}

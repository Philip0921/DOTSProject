using UnityEngine;
using Unity.Entities;

/// <summary>
/// Put this on a GameObject in the scene. This is your test harness UI.
/// </summary>
public class PerfTestController : MonoBehaviour
{
    [Header("Spawn settings")]
    public int InitialBatchSize = 100;    // starting agents
    public int BatchStepSize = 100;    // how many MORE each step
    public float StepInterval = 5f;     // seconds between steps

    [Header("Debug (read-only at runtime)")]
    public int currentTargetTotal;         // what we'll ask next wave to spawn
    public int liveAgentCount;
    public float runTimeSeconds;

    PerfSampler samplerSystem;

    void Awake()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        samplerSystem = world.GetOrCreateSystemManaged<PerfSampler>();

        samplerSystem.InitFromMono(
            InitialBatchSize,
            BatchStepSize,
            StepInterval
        );

        Debug.Log("[PerfTestController] InitFromMono called.");
    }

    void Update()
    {
        if (samplerSystem == null) return;

        // pull runtime info so you can SEE it live in Inspector
        currentTargetTotal = samplerSystem.NextBatchSize;
        liveAgentCount = samplerSystem.AgentCount;
        runTimeSeconds = samplerSystem.RunSeconds;
    }
}

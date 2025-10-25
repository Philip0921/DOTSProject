using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using System.Text;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class PerfSampler : SystemBase
{
    // Static fieldsm, filled by other systems.
    public static float LastMoveMs;
    public static float LastDecisionMs;
    public static float LastSpawnMs;

    public static void RecordMoveMs(float ms) => LastMoveMs = ms;
    public static void RecordDecisionMs(float ms) => LastDecisionMs = ms;
    public static void RecordSpawnMs(float ms) => LastSpawnMs = ms;

    // Config
    private int _initialBatchSize; // Agent count per step.
    private int _batchStepSize;
    private float _stepInterval;    // Seconds between scaling.

    // Flags
    bool _initialized;
    bool _bootstrapped;

    // Runtime state
    private float _timeSinceLastStep;
    private float _timeSinceStart;
    private int _frameCount;

    private int _targetTotalAgents;

    private Entity _spawnerTemplate; // Reference to "spawner archetype" prefab or template.
                                     // This entity needs to have AISpawnConfig & PrefabRef configured beforehand.
    // Logging
    private StringBuilder _csv;
    private string _filePath;

    // Inspector-facing values
    public int AgentCount { get; private set; }
    public int NextBatchSize { get; private set; }
    public float RunSeconds { get; private set; } // How long the test has been running
    public string LastSavedPath { get; private set; }


    public void InitFromMono(int initialBatchSize, int batchStepSize, float stepInterval)
    {

        _initialBatchSize = initialBatchSize;
        _batchStepSize = batchStepSize;
        _stepInterval = stepInterval;

        _initialized = true;

    }

    protected override void OnUpdate()
    {
        if (!_initialized) return;

        if (!_bootstrapped)
        {
            DoBootstrap();
        }

        float dt = SystemAPI.Time.DeltaTime;
        _timeSinceLastStep += dt;
        _timeSinceStart += dt;
        RunSeconds = _timeSinceStart;
        _frameCount++;

        // Count current agents
        AgentCount = SystemAPI.QueryBuilder().WithAll<MoveDir>().Build().CalculateEntityCount();

        // frame timing
        double frameMs = UnityEngine.Time.deltaTime * 1000f;
        bool ok60 = frameMs <= 16.67; // ~60 FPS target

        // Append a log line (JSON-ish array entry)
        _csv.AppendLine(
            $"{{\"t\":{UnityEngine.Time.time:F2},\"frame\":{_frameCount},\"agents\":{AgentCount},\"frameMs\":{frameMs:F3},\"decisionMs\"" +
            $":{LastDecisionMs:F3},\"moveMs\":{LastMoveMs:F3},\"spawnMs\":{LastSpawnMs:F3},\"ok60\":{(ok60 ? "true" : "false")}}},");

        // Scale increase of entites by interval
        if (_timeSinceLastStep >= _stepInterval)
        {
            _timeSinceLastStep = 0f;

            _targetTotalAgents += _batchStepSize;
            NextBatchSize = _targetTotalAgents; // Grow wave size

            int deltaToSpawn = _targetTotalAgents - AgentCount;
            if (deltaToSpawn > 0 && _spawnerTemplate != Entity.Null)
            {
                RequestSpawnWave(deltaToSpawn);
                Debug.Log($"[PerfSampler] Requested +{deltaToSpawn} (target {_targetTotalAgents} total).");
            }
        }

        //if (!ok60) { SaveFile(); Enabled = false; }
        
    }
    void DoBootstrap()
    {
        // Find a template spawner (the first AISpawnAuthoring-converted entity)
        using (var spawners = EntityManager.CreateEntityQuery(
                   ComponentType.ReadOnly<AISpawnConfig>(),
                   ComponentType.ReadOnly<PrefabRef>())
               .ToEntityArray(Allocator.Temp))
        {
            if (spawners.Length > 0)
                _spawnerTemplate = spawners[0];
            else
                Debug.LogWarning("PerfSampler: no spawner template found. Do you have AISpawnAuthoring in scene?\"");
        }

        _csv = new StringBuilder(4096);
        _filePath = System.IO.Path.Combine(Application.persistentDataPath, "perf_log.json");

        _timeSinceLastStep = 0f;
        _timeSinceStart = 0f;
        _frameCount = 0;

        // first wave
        _targetTotalAgents = _initialBatchSize;
        NextBatchSize = _targetTotalAgents;

        if (_spawnerTemplate != Entity.Null)
        {
            RequestSpawnWave(_targetTotalAgents);
            Debug.Log($"[PerfSampler] First wave: +{_targetTotalAgents} (target {_targetTotalAgents}).");
        }

        _bootstrapped = true;
    }

    protected override void OnDestroy()
    {
        SaveFile();
    }

    void SaveFile()
    {
        // Save performance log as JSON array
        if (_csv == null) return; // if we never even inited, don't write

        string json = "[\n" + _csv.ToString().TrimEnd(',', '\n', '\r') + "\n]";
        System.IO.File.WriteAllText(_filePath, json);
        LastSavedPath = _filePath;
        Debug.Log($"Perf log saved to: {_filePath}");
    }

    // Gives SpawnSystem a new AISpawnState to trigger more spawns
    private void RequestSpawnWave(int countToAdd)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        Entity newSpawner = ecb.Instantiate(_spawnerTemplate);

        // Entity spawn amount per wave
        var cfg = EntityManager.GetComponentData<AISpawnConfig>(_spawnerTemplate);
        cfg.SpawnCount = countToAdd;
        ecb.SetComponent(newSpawner, cfg);
        ecb.AddComponent<AISpawnState>(newSpawner);

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }
}

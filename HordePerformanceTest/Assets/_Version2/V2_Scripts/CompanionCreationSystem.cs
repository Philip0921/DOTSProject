using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// Lägg CompanionGO när entiteten saknar den men har VisualPrefabRef
// Kör i Presentation så det sker efter EndSimulation ECB-playback.
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class CompanionCreationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Bygg en query på entiteter som behöver en companion
        var q = SystemAPI.QueryBuilder()
            .WithAll<LocalTransform, VisualPrefabRef>()  // har prefab-ref
            .WithNone<CompanionGO>()                     // har ännu ingen GO
            .Build();

        // Viktigt: ta en SNAPSHOT av entiteter först!
        using var entities = q.ToEntityArray(Unity.Collections.Allocator.Temp);

        foreach (var e in entities)
        {
            // Läs prefab-referensen (managed) från entiteten
            var vis = EntityManager.GetComponentObject<VisualPrefabRef>(e);
            if (vis?.Prefab == null) continue;

            // Skapa GO
            var go = Object.Instantiate(vis.Prefab);
            go.name = $"AI_{e.Index}";

            // Sätt initial position från ECS-transform
            var lt = EntityManager.GetComponentData<LocalTransform>(e).Position;
            go.transform.position = new Vector3(lt.x, lt.y, lt.z);

            // Lägg managed komponenten på entiteten (tillåtet här, vi itererar inte "live")
            EntityManager.AddComponentObject(e, new CompanionGO { Instance = go });
        }
    }
}

// Din managed komponenter:
public class VisualPrefabRef : IComponentData { public GameObject Prefab; }
public class CompanionGO : IComponentData { public GameObject Instance; }

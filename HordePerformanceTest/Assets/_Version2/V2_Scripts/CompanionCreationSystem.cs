using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

// L�gg CompanionGO n�r entiteten saknar den men har VisualPrefabRef
// K�r i Presentation s� det sker efter EndSimulation ECB-playback.
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class CompanionCreationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Bygg en query p� entiteter som beh�ver en companion
        var q = SystemAPI.QueryBuilder()
            .WithAll<LocalTransform, VisualPrefabRef>()  // har prefab-ref
            .WithNone<CompanionGO>()                     // har �nnu ingen GO
            .Build();

        // Viktigt: ta en SNAPSHOT av entiteter f�rst!
        using var entities = q.ToEntityArray(Unity.Collections.Allocator.Temp);

        foreach (var e in entities)
        {
            // L�s prefab-referensen (managed) fr�n entiteten
            var vis = EntityManager.GetComponentObject<VisualPrefabRef>(e);
            if (vis?.Prefab == null) continue;

            // Skapa GO
            var go = Object.Instantiate(vis.Prefab);
            go.name = $"AI_{e.Index}";

            // S�tt initial position fr�n ECS-transform
            var lt = EntityManager.GetComponentData<LocalTransform>(e).Position;
            go.transform.position = new Vector3(lt.x, lt.y, lt.z);

            // L�gg managed komponenten p� entiteten (till�tet h�r, vi itererar inte "live")
            EntityManager.AddComponentObject(e, new CompanionGO { Instance = go });
        }
    }
}

// Din managed komponenter:
public class VisualPrefabRef : IComponentData { public GameObject Prefab; }
public class CompanionGO : IComponentData { public GameObject Instance; }

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class CompanionCreationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Build query for entites that need a companion
        var q = SystemAPI.QueryBuilder()
            .WithAll<LocalTransform, VisualPrefabRef>()  // Has prefab-ref
            .WithNone<CompanionGO>()                     // Has no GO
            .Build();

        // Important: take a SNAPSHOT of the entities first!
        using var entities = q.ToEntityArray(Unity.Collections.Allocator.Temp);

        foreach (var e in entities)
        {
            // Read prefab-reference (managed) from entity
            var vis = EntityManager.GetComponentObject<VisualPrefabRef>(e);
            if (vis?.Prefab == null) continue;

            // Create GO
            var go = Object.Instantiate(vis.Prefab);
            go.name = $"AI_{e.Index}";

            // Initial position from ECS-transform
            var lt = EntityManager.GetComponentData<LocalTransform>(e).Position;
            go.transform.position = new Vector3(lt.x, lt.y, lt.z);

            // Add managed component to the entity 
            EntityManager.AddComponentObject(e, new CompanionGO { Instance = go });
        }
    }
}

public class VisualPrefabRef : IComponentData { public GameObject Prefab; }
public class CompanionGO : IComponentData { public GameObject Instance; }

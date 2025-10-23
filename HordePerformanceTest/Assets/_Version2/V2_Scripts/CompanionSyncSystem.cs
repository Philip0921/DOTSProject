using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

// Kör i presentation-fasen (render/sync)
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class CompanionSyncSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach ((RefRO<LocalTransform> lt, CompanionGO comp) in SystemAPI.Query<RefRO<LocalTransform>, CompanionGO>())
        {
            if (comp.Instance != null)
            {
                var p = lt.ValueRO.Position;
                comp.Instance.transform.position = new Vector3(p.x, p.y, p.z);
            }
        }
    }
}

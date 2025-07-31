using UnityEngine;

public class SpawnerRuneState : ActiveRuneStateBase
{
    private GameObject prefab;

    public SpawnerRuneState(GameObject squarePrefab)
        : base(new StatsBundle(), 2f) // No cambia stats, cooldown de 2s
    {
        prefab = squarePrefab;
    }

    protected override bool CanTrigger()
    {
        return true; // Siempre puede activarse cuando el cooldown llegue a 0
    }

    protected override void OnTrigger()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        GameObject square = GameObject.Instantiate(prefab, owner.transform.position, Quaternion.identity);

        if (square.TryGetComponent(out ProjectileMover mover))
        {
            mover.SetDirection(randomDirection);
        }
    }
}

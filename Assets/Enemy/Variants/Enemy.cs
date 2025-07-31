using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    private StatsComponent stats;

    protected Transform player;

    protected virtual void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        stats = StatsComponent.Get(gameObject);
        stats.OnDie += Die;
    }

    protected virtual void Update()
    {
        if (player == null)
        {
            player = GameObject.FindWithTag("Player")?.transform;
            if (player == null) return;
        }

        MoveTowardPlayer();
    }

    protected virtual void MoveTowardPlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        transform.position += (Vector3)(dir * stats.currentStats.moveSpeed * Time.deltaTime);
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}

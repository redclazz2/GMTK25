using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public double maxHealth = 100;
    public double currentHealth;
    public float moveSpeed = 2f;

    protected Transform player;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindWithTag("Player")?.transform;
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
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    public virtual void ReceiveDamage(double damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}

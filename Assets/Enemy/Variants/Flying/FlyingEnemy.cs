using UnityEngine;

public class FlyingEnemy : Enemy
{
    protected override void Update()
    {
        if (player == null || stats == null)
            return;

        FlipSprite();

        MoveToPlayer();
    }

    private void MoveToPlayer()
    {
        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float moveDistance = stats.currentStats.moveSpeed * Time.deltaTime;
        
        transform.position += (Vector3)(direction * moveDistance);
    }
}
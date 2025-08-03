using UnityEngine;

public class DistanceEnemyProjectile : MonoBehaviour
{
    public float lifetime = 5f;
    public int damage = 5;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Deal damage
            StatsComponent.Get(collision.gameObject).ApplyModifiers(new StatsBundle { health = -damage });
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}

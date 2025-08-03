using UnityEngine;

public class WallObstacle : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f; // Duraci�n del objeto en segundos
    private float currentLifetime = 0f;

    void Start()
    {
        Debug.Log($"[Doomie] Spawned with lifetime: {lifetime} seconds");
    }

    void Update()
    {
        // Incrementamos el tiempo transcurrido
        currentLifetime += Time.deltaTime;

        // Si ha pasado el tiempo de vida, destruimos el objeto
        if (currentLifetime >= lifetime)
        {
            DestroyDoomie();
        }
    }

    private void DestroyDoomie()
    {
        Debug.Log($"[Doomie] Destroyed after {currentLifetime:F2} seconds");
        Destroy(gameObject);
    }

    // M�todo p�blico para cambiar el lifetime si es necesario
    public void SetLifetime(float newLifetime)
    {
        lifetime = newLifetime;
    }

    // M�todo p�blico para obtener el tiempo restante
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, lifetime - currentLifetime);
    }
}
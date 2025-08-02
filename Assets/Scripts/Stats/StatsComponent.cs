using System;
using UnityEngine;

[DisallowMultipleComponent]
public class StatsComponent : MonoBehaviour, IDamageable
{
    [Tooltip("Estadísticas base sin modificaciones")]
    public StatsBundle baseStats;
    public StatsBundle currentStats;

    // Estados especiales manejados internamente
    private bool _isInvulnerable = false;
    private bool _canModifySize = true;

    // Propiedades públicas para acceder a estados especiales
    public bool IsInvulnerable => _isInvulnerable;
    public bool CanModifySize => _canModifySize;
    public bool SetCanModifySize(bool canModify)
    {
        _canModifySize = canModify;
        return _canModifySize;
    }

    public event Action OnHit;
    public event Action OnPreDie;
    public event Action OnDie;

    void Awake()
    {
        ResetToBase();
    }

    void Update()
    {
        // Regeneración de vida
        if (currentStats.regeneration > 0 && currentStats.health > 0)
        {
            currentStats.health = Mathf.Min(
                currentStats.health + currentStats.regeneration * Time.deltaTime,
                currentStats.maxHealth
            );
        }

        // Size update
        if (CanModifySize)
        {
            if (gameObject.transform.localScale.x != currentStats.size || gameObject.transform.localScale.y != currentStats.size)
            {

                gameObject.transform.localScale = new Vector3(currentStats.size, currentStats.size, gameObject.transform.localScale.z);
            }
        }

        // Cap values to prevent exceeding limits
        currentStats.health = Mathf.Clamp(currentStats.health, 0f, currentStats.maxHealth);
        currentStats.criticalChance = Mathf.Clamp01(currentStats.criticalChance);
    }

    public void ResetToBase()
    {
        currentStats = baseStats;
        currentStats.health = baseStats.maxHealth;
        _isInvulnerable = false;
    }

    public void ApplyModifiers(StatsBundle delta)
    {
        currentStats = currentStats + delta;
    }

    // Métodos específicos para manejar estados especiales
    public void SetInvulnerable(bool isInvulnerable)
    {
        _isInvulnerable = isInvulnerable;
    }

    public static StatsComponent Get(GameObject go)
    {
        return go.GetComponent<StatsComponent>();
    }

    public float ReceiveDamage(DamageInfo info)
    {
        if (IsInvulnerable)
        {
            return 0f;
        }

        OnHit?.Invoke();

        float finalDamage = CalculateDamageAfterArmor(info.BaseAmount);
        currentStats.health -= finalDamage;

        if (currentStats.health <= 0)
        {
            OnPreDie?.Invoke();
            if (currentStats.health <= 0)
            {
                currentStats.health = 0;
                OnDie?.Invoke();
            }
        }

        return finalDamage;
    }

    private float CalculateDamageAfterArmor(float baseDamage)
    {
        float damageReduction = currentStats.armor / (currentStats.armor + 100f);
        return baseDamage * (1f - damageReduction);
    }
}
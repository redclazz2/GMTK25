using UnityEngine;

public enum LaserEnemyState
{
    Cooldown,
    Charging,
    Shooting
}

public class LaserEnemy : Enemy
{
    [Header("Laser Enemy Settings")]
    [SerializeField] private float idleRadius = 2f;
    [SerializeField] private float cooldownDuration = 3f;
    [SerializeField] private float chargingDuration = 2f;
    [SerializeField] private float shootingDuration = 1.5f;
    [SerializeField] private float aimPredictionStrength = 0.3f;
    [SerializeField] private LayerMask obstacleLayerMask = -1;
    [SerializeField] private float laserMaxDistance = 20f;

    private LaserEnemyState currentState = LaserEnemyState.Cooldown;
    private float stateTimer = 0f;
    private Vector2 idlePosition;
    private Vector2 targetAimPosition;
    private Vector2 currentAimDirection;
    private Vector2 frozenAimDirection;

    private LineRenderer aimLine;
    private LineRenderer shootLine;

    private float laserDamage = 25f;
    private float damageInterval = 0.1f;
    private float lastDamageTime = 0f;

    public Texture2D laserTexture;

    protected override void Start()
    {
        base.Start();
        PickIdlePosition();
        SetupLaserVisuals();
        EnterState(LaserEnemyState.Cooldown);
    }

    void PickIdlePosition()
    {
        Vector2 spawnPos = transform.position;
        Vector2 randomOffset = Random.insideUnitCircle * idleRadius;
        idlePosition = spawnPos + randomOffset;
    }

    void SetupLaserVisuals()
    {
        aimLine = new GameObject("AimLaser").AddComponent<LineRenderer>();
        shootLine = new GameObject("ShootLaser").AddComponent<LineRenderer>();

        SetupLine(aimLine, new Color(1f, 1f, 0f, 0.5f), 0.05f);  // Yellow, translucent
        SetupLine(shootLine, new Color(1f, 0.5f, 0.6f, 0.8f), 0.1f); // Light red
    }

    void SetupLine(LineRenderer line, Color color, float width)
    {
        line.positionCount = 2;
        line.startColor = line.endColor = color;
        line.startWidth = line.endWidth = width;
        line.sortingLayerName = "Entities";

        Material laserMat = new Material(Shader.Find("Sprites/Default"));
        Texture2D tex = laserTexture;

        if (tex != null)
        {
            laserMat.mainTexture = tex;
            laserMat.mainTextureScale = new Vector2(100f, 0.5f);
        }

        line.material = laserMat;
        line.sortingLayerName = "Entities";
        line.sortingOrder = 100;
        line.enabled = false;
    }


    protected override void Update()
    {
        if (player == null || stats == null)
            return;

        stateTimer += Time.deltaTime;

        switch (currentState)
        {
            case LaserEnemyState.Cooldown: UpdateCooldown(); break;
            case LaserEnemyState.Charging: UpdateCharging(); break;
            case LaserEnemyState.Shooting: UpdateShooting(); break;
        }

        if (currentState != LaserEnemyState.Shooting)
            MoveTowardsIdlePosition();
    }

    void MoveTowardsIdlePosition()
    {
        float distance = Vector2.Distance(transform.position, idlePosition);
        if (distance > 0.1f)
        {
            Vector2 dir = (idlePosition - (Vector2)transform.position).normalized;
            float speed = stats.currentStats.moveSpeed * 0.5f;
            transform.position = Vector2.MoveTowards(transform.position, idlePosition, speed * Time.deltaTime);
        }
    }

    void UpdateCooldown()
    {
        if (stateTimer >= cooldownDuration)
        {
            if (player == null) { stateTimer = 0f; return; }

            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= laserMaxDistance)
                EnterState(LaserEnemyState.Charging);
            else
                stateTimer = 0f;
        }
    }

    void UpdateCharging()
    {
        UpdateAimDirection();
        UpdateAimLaser();

        if (stateTimer >= chargingDuration)
            EnterState(LaserEnemyState.Shooting);
    }

    void UpdateShooting()
    {
        UpdateShootLaser();
        DealLaserDamage();

        if (stateTimer >= shootingDuration)
            EnterState(LaserEnemyState.Cooldown);
    }

    void UpdateAimDirection()
    {
        if (player == null) return;

        Vector2 myPos = transform.position;
        Vector2 playerPos = player.position;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null && aimPredictionStrength > 0f)
        {
            Vector2 predicted = playerPos + rb.linearVelocity * (Vector2.Distance(myPos, playerPos) / 15f) * aimPredictionStrength;
            targetAimPosition = predicted;
        }
        else
        {
            targetAimPosition = playerPos;
        }

        currentAimDirection = Vector2.Lerp(currentAimDirection, (targetAimPosition - myPos).normalized, Time.deltaTime * 3f);
    }

    void UpdateAimLaser()
    {
        if (aimLine == null) return;

        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)(currentAimDirection * laserMaxDistance);

        aimLine.SetPosition(0, start);
        aimLine.SetPosition(1, end);
        aimLine.enabled = true;

        float flicker = 0.5f + 0.3f * Mathf.Sin(Time.time * 4f);
        aimLine.startColor = aimLine.endColor = new Color(1f, 1f, 0f, flicker);
    }

    void UpdateShootLaser()
    {
        if (shootLine == null) return;

        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)(frozenAimDirection * laserMaxDistance);

        shootLine.SetPosition(0, start);
        shootLine.SetPosition(1, end);
        shootLine.enabled = true;

        float intensity = 0.7f + 0.3f * Mathf.Sin(Time.time * 20f);
        shootLine.startColor = shootLine.endColor = new Color(1f * intensity, 0.4f, 0.4f, 1f);

        if (shootLine.material != null && shootLine.material.mainTexture != null)
        {
            float scrollSpeed = -Time.time * 10f;
            shootLine.material.mainTextureOffset = new Vector2(scrollSpeed, 0f);
        }
    }
    void DealLaserDamage()
    {
        if (Time.time - lastDamageTime < damageInterval) return;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, frozenAimDirection, laserMaxDistance);
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            StatsComponent playerStats = hit.collider.GetComponent<StatsComponent>();
            if (playerStats != null)
            {
                playerStats.ReceiveDamage(new DamageInfo { BaseAmount = laserDamage });
                lastDamageTime = Time.time;
            }
        }
    }

    void EnterState(LaserEnemyState newState)
    {
        switch (currentState)
        {
            case LaserEnemyState.Charging:
                if (aimLine != null) aimLine.enabled = false;
                break;
            case LaserEnemyState.Shooting:
                if (shootLine != null) shootLine.enabled = false;
                break;
        }

        currentState = newState;
        stateTimer = 0f;

        switch (newState)
        {
            case LaserEnemyState.Cooldown:
                stats.SetInvulnerable(true);
                if (Random.value < 0.2f) PickIdlePosition();
                break;
            case LaserEnemyState.Charging:
                stats.SetInvulnerable(false);
                break;
            case LaserEnemyState.Shooting:
                stats.SetInvulnerable(false);
                frozenAimDirection = currentAimDirection;
                break;
        }
    }

    void OnDestroy()
    {
        if (aimLine != null)
            Destroy(aimLine.gameObject);

        if (shootLine != null)
            Destroy(shootLine.gameObject);
    }
}

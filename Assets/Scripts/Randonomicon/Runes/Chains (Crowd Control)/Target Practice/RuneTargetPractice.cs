using UnityEngine;

public class RuneTargetPractice : ActiveRuneStateBase
{
    public RuneTargetPractice(RuneStateData runeStateData) : base(runeStateData, 4)
    {
    }

    protected override void OnTrigger()
    {
        Vector3 playerPos = GameObject.FindGameObjectWithTag("CastPlayer").transform.position;

        // Generamos una posición aleatoria alrededor del jugador
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = playerPos + (Vector3)(randomDirection);

        Quaternion spawnRot = Quaternion.identity;
        GameObject.Instantiate(_stateData.GetPrefab("wall01"), spawnPos, spawnRot);
    }
}
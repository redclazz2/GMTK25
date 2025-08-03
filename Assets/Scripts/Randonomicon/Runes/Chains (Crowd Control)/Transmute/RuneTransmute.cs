using UnityEngine;
using System.Collections.Generic;

public class RuneTransmute : ActiveRuneStateBase
{
    public RuneTransmute(RuneStateData runeStateData) : base(runeStateData, 10)
    {
    }

    protected override void OnTrigger()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(Vector2.zero, 4f);
        List<Vector3> enemyPositions = new();
        int count = 0;

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                enemyPositions.Add(col.transform.position);
                GameObject.Destroy(col.gameObject);
                count++;

                if (count >= 4)
                    break;
            }
        }

        foreach (Vector3 pos in enemyPositions)
        {
            GameObject.Instantiate(_stateData.GetPrefab("sheep01"), pos, Quaternion.identity);
        }
    }
}

using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    StatsComponent stats;

    private void Start()
    {
        stats = StatsComponent.Get(GameObject.FindGameObjectWithTag("Player"));
    }

    private void Update()
    {
        if (textMesh != null)
        {
            textMesh.text = Mathf.CeilToInt(stats.currentStats.health).ToString();
        }
    }
}

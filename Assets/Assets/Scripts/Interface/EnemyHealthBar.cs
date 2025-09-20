using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : NetworkBehaviour
{
    [SerializeField] private Slider hpBar;
    [SerializeField] private float showDistance = 10f;

    private EnemyController enemy;
    private Transform player;

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            hpBar.gameObject.SetActive(false);
            return;
        }

        player = NetworkManager.Singleton.LocalClient.PlayerObject.transform;

        enemy.CurrentHP.OnValueChanged += OnHealthChanged;
        OnHealthChanged(enemy.CurrentHP.Value, enemy.CurrentHP.Value);
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        hpBar.value = newValue / (float)enemy.MaxHP;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        hpBar.gameObject.SetActive(distance <= showDistance && enemy.CurrentHP.Value < enemy.MaxHP);
    }

    public override void OnDestroy()
    {
        if (enemy != null)
        {
            enemy.CurrentHP.OnValueChanged -= OnHealthChanged;
        }
    }
}

using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : NetworkBehaviour
{
    [SerializeField] private Image hpBarBackground;
    [SerializeField] private Image hpBarWhiteBackground;
    [SerializeField] private Image hpBarfront;
    [SerializeField] private float showDistance = 10f;

    private EnemyController enemy;
    private Transform player;

    private void Awake()
    {
        enemy = GetComponent<EnemyController>();
    }

    public override void OnNetworkSpawn()
    {
        enemy.CurrentHP.OnValueChanged += OnHealthChanged;
        
        if (!IsOwner)
        {            
            SwitchHpBarVisuals(false);
            return;
        }

        player = NetworkManager.Singleton.LocalClient.PlayerObject.transform;

        OnHealthChanged(enemy.CurrentHP.Value, enemy.CurrentHP.Value);
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        hpBarfront.fillAmount = newValue / (float)enemy.MaxHP;
        CheckShowDistance();
    }

    private void Update()
    {
        if (player == null) return;
        CheckShowDistance();
    }

    private void CheckShowDistance()
    {
        float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
        SwitchHpBarVisuals(distance <= showDistance && enemy.CurrentHP.Value < enemy.MaxHP && enemy.CurrentHP.Value > 0);
    }

    private void SwitchHpBarVisuals(bool toggle)
    {
        hpBarBackground.enabled = toggle;
        hpBarWhiteBackground.enabled = toggle;
        hpBarfront.enabled = toggle;
    }

    public override void OnDestroy()
    {
        if (enemy != null)
        {
            enemy.CurrentHP.OnValueChanged -= OnHealthChanged;
        }
    }
}

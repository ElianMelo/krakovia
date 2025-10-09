using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : NetworkBehaviour
{
    [SerializeField] private Image hpBarBackground;
    [SerializeField] private Image hpBarWhiteBackground;
    [SerializeField] private Image hpBarfront;
    [SerializeField] private float showDistance = 10f;

    private BossController enemy;
    private Transform player;

    private void Awake()
    {
        enemy = GetComponent<BossController>();
    }

    public override void OnNetworkSpawn()
    {
        enemy.CurrentHP.OnValueChanged += OnHealthChanged;

        SwitchHpBarVisuals(true);

        player = NetworkManager.Singleton.LocalClient.PlayerObject.transform;

        OnHealthChanged(enemy.CurrentHP.Value, enemy.CurrentHP.Value);
    }

    private void OnHealthChanged(float oldValue, float newValue)
    {
        hpBarfront.fillAmount = newValue / (float)enemy.MaxHP;
    }

    private void Update()
    {
        if (player == null) return;
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

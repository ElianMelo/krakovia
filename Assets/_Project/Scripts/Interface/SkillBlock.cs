using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillBlock : MonoBehaviour
{
    public GameObject timeOverlay;
    public Image timeOverlayDuration;
    public TextMeshProUGUI timeText;
    private float currentTimer = 0f;
    private Coroutine cooldownCoroutine;

    private float maxTimer;

    public void StartCooldown(float timer)
    {
        currentTimer = timer;
        maxTimer = timer;
        timeOverlay.SetActive(true);
        timeOverlayDuration.gameObject.SetActive(true);
        timeText.gameObject.SetActive(true);
        if(cooldownCoroutine != null) StopCoroutine(cooldownCoroutine);
        cooldownCoroutine = StartCoroutine(StopCooldown());
    }

    private IEnumerator StopCooldown()
    {
        while (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            timeOverlayDuration.fillAmount = currentTimer / maxTimer;
            timeText.text = currentTimer.ToString("F1");
            yield return null;
        }
        timeOverlay.SetActive(false);
        timeOverlayDuration.gameObject.SetActive(false);
        timeText.gameObject.SetActive(false);
        yield return null;
    }
}

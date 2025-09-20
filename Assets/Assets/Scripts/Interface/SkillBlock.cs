using System.Collections;
using TMPro;
using UnityEngine;

public class SkillBlock : MonoBehaviour
{
    public GameObject timeOverlay;
    public TextMeshProUGUI timeText;
    private float currentTimer = 0f;
    private Coroutine cooldownCoroutine;

    public void StartCooldown(float timer)
    {
        currentTimer = timer;
        timeOverlay.SetActive(true);
        timeText.gameObject.SetActive(true);
        if(cooldownCoroutine != null) StopCoroutine(cooldownCoroutine);
        cooldownCoroutine = StartCoroutine(StopCooldown());
    }

    private IEnumerator StopCooldown()
    {
        while (currentTimer > 0)
        {
            currentTimer -= Time.deltaTime;
            timeText.text = currentTimer.ToString("F1");
            yield return null;
        }
        timeOverlay.SetActive(false);
        timeText.gameObject.SetActive(false);
        yield return null;
    }
}

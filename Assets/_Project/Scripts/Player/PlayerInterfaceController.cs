using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInterfaceController : MonoBehaviour
{
    public Image playerHpSlide;
    public Image playerExperienceSlide;
    public TextMeshProUGUI playerLevel;

    public SkillBlock playerDashSkill;
    public SkillBlock playerSkillFirst;
    public SkillBlock playerSkillSecond;
    public SkillBlock playerSkillThird;
    public SkillBlock playerSkillForth;

    public GameObject lockedMouseRightSKill;
    public GameObject lockedQSkill;
    public GameObject lockedFSkill;

    public void UpdatePlayerHp(float currentValue, float maxValue)
    {
        playerHpSlide.fillAmount = currentValue / maxValue;
    }
    public void UpdatePlayerExperience(int currentValue, int maxValue)
    {
        playerExperienceSlide.fillAmount = (float)currentValue / maxValue;
    }
    public void UpdatePlayerLevel(int level)
    {
        string levelText = level.ToString();
        if(level < 10)
        {
            levelText = "0" + levelText;
        }
        playerLevel.text = levelText;
    }

    public void UnlockMouseRightSkill()
    {
        lockedMouseRightSKill.SetActive(false);
    }
    public void UnlockQSkill()
    {
        lockedQSkill.SetActive(false);
    }
    public void UnlockFSkill()
    {
        lockedFSkill.SetActive(false);
    }
    public void UpdatePlayerSkillFirstCooldown(float time)
    {
        playerSkillFirst.StartCooldown(time);
    }
    public void UpdatePlayerSkillSecondCooldown(float time)
    {
        playerSkillSecond.StartCooldown(time);
    }
    public void UpdatePlayerSkillThirdCooldown(float time)
    {
        playerSkillThird.StartCooldown(time);
    }
    public void UpdatePlayerSkillForthCooldown(float time)
    {
        playerSkillForth.StartCooldown(time);
    }

    public void UpdateDashSkillCooldown(float time)
    {
        playerDashSkill.StartCooldown(time);
    }
}

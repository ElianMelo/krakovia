using UnityEngine;
using UnityEngine.UI;

public class PlayerInterfaceController : MonoBehaviour
{
    public Slider playerHpSlide;

    public SkillBlock playerSkillFirst;
    public SkillBlock playerSkillSecond;
    public SkillBlock playerSkillThird;
    public SkillBlock playerSkillForth;

    public void UpdatePlayerHp(int currentValue, int maxValue)
    {
        playerHpSlide.value = (float) currentValue / maxValue;
    }

    public void UpdatePlayerSkillFirstCooldown(float time)
    {
        playerSkillFirst.StartCooldown(time);
    }
}

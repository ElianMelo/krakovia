using UnityEngine;
using UnityEngine.UI;

public class PlayerInterfaceController : MonoBehaviour
{
    public Slider playerHpSlide;

    public void UpdatePlayerHp(int currentValue, int maxValue)
    {
        playerHpSlide.value = (float) currentValue / maxValue;
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PvPInterfaceController : MonoBehaviour
{
    public const string PVP_ENABLED = "PVP ativado";
    public const string PVP_DISABLED = "PVP desativado";

    public TMP_Text pvpTextBox;
    public TMP_Text supportTextBox;
    public Image logo;
    public Color baseColor;
    public Color activeColor;

    public void EnablePvPInterface()
    {
        pvpTextBox.text = PVP_ENABLED;
        supportTextBox.enabled = false;
        pvpTextBox.color = activeColor;
        supportTextBox.color = activeColor;
        logo.color = activeColor;
    }

    public void DisablePvPInterface()
    {
        pvpTextBox.text = PVP_DISABLED;
        supportTextBox.enabled = true;
        pvpTextBox.color = baseColor;
        supportTextBox.color = baseColor;
        logo.color = baseColor;
    }
}

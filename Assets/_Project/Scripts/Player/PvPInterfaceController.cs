using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PvPInterfaceController : MonoBehaviour
{
    public const string PVP_ENABLED = "PVP ativado";
    public const string PVP_DISABLED = "PVP desativado";

    public const string PVP_SUP_ENABLED = "Ate a morte";
    public const string PVP_SUP_DISABLED = "P - Ativar";

    public TMP_Text pvpTextBox;
    public TMP_Text supportTextBox;
    public Image logo;
    public Color baseColor;
    public Color activeColor;

    public void EnablePvPInterface()
    {
        pvpTextBox.text = PVP_ENABLED;
        supportTextBox.text = PVP_SUP_ENABLED;
        pvpTextBox.color = activeColor;
        supportTextBox.color = activeColor;
        logo.color = activeColor;
    }

    public void DisablePvPInterface()
    {
        pvpTextBox.text = PVP_DISABLED;
        supportTextBox.text = PVP_SUP_DISABLED;
        pvpTextBox.color = baseColor;
        supportTextBox.color = baseColor;
        logo.color = baseColor;
    }
}

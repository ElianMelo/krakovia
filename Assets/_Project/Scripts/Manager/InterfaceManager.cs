using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager Instance;

    public Canvas thisCanvas;
    public Canvas otherCanvas;
    public TMP_InputField playerNameInputField;

    [Header("Class Selection")]
    public TMP_Dropdown classSelectionDropdown;
    public TMP_Text firstDescription;
    public Image iamgeField;
    public GameObject horseModel;
    public GameObject fairyModel;
    public GameObject skeletonModel;

    [TextArea] public string horseFirstDescription;
    public Sprite horseImage;
    [TextArea] public string fairyFirstDescription;
    public Sprite fairyImage;
    [TextArea] public string skeletonFirstDescription;
    public Sprite skeletonImage;

    public PlayerInterfaceController _playerInterfaceController;
    public PvPInterfaceController _pvPInterfaceController;
    public ObjectiveInterfaceController _objectiveInterfaceController;
    public MenuInterfaceController _menuInterfaceController;

    public PlayerInterfaceController PlayerInterfaceController
    {
        get => _playerInterfaceController;
        set
        {
            _playerInterfaceController = value;
        }
    }

    public ObjectiveInterfaceController ObjectiveInterfaceController
    {
        get => _objectiveInterfaceController;
        set
        {
            _objectiveInterfaceController = value;
        }
    }

    public PvPInterfaceController PvPInterfaceController => _pvPInterfaceController;

    private void Awake()
    {
        Instance = this;   
    }

    private void Start()
    {
        classSelectionDropdown.onValueChanged.AddListener(delegate { DropdownValueChanged(classSelectionDropdown); });
    }

    void DropdownValueChanged(TMP_Dropdown change)
    {
        PlayerClass playerClass = (PlayerClass) change.value;

        switch (playerClass)
        {
            case PlayerClass.Fairy:
                firstDescription.text = fairyFirstDescription;
                iamgeField.sprite = fairyImage;
                horseModel.SetActive(false);
                fairyModel.SetActive(true);
                skeletonModel.SetActive(false);
                break;
            case PlayerClass.Skeleton:
                firstDescription.text = skeletonFirstDescription;
                iamgeField.sprite = skeletonImage;
                horseModel.SetActive(false);
                fairyModel.SetActive(false);
                skeletonModel.SetActive(true);
                break;
            case PlayerClass.Horse:
                firstDescription.text = horseFirstDescription;
                iamgeField.sprite = horseImage;
                horseModel.SetActive(true);
                fairyModel.SetActive(false);
                skeletonModel.SetActive(false);
                break;
            default:
                break;
        }
    }

    public PlayerClass GetSelectedClass()
    {
        thisCanvas.enabled = true;
        otherCanvas.enabled = false;
        return (PlayerClass) classSelectionDropdown.value;
    }

    public string GetPlayerName()
    {
        if(playerNameInputField.text.Length > 15)
        {
            return playerNameInputField.text.Substring(0, 14) + ".";
        } else
        {
            return playerNameInputField.text;
        }
    }

    public void UnlockMouseRightSkill()
    {
        PlayerInterfaceController.UnlockMouseRightSkill();
    }

    public void UnlockQSkill()
    {
        PlayerInterfaceController.UnlockQSkill();
    }

    public void UnlockFSkill()
    {
        PlayerInterfaceController.UnlockFSkill();
    }

    public void UpdatePlayerSkillFirstCooldown(float time)
    {
        PlayerInterfaceController.UpdatePlayerSkillFirstCooldown(time);
    }

    public void UpdatePlayerSkillSecondCooldown(float time)
    {
        PlayerInterfaceController.UpdatePlayerSkillSecondCooldown(time);
    }

    public void UpdatePlayerSkillThirdCooldown(float time)
    {
        PlayerInterfaceController.UpdatePlayerSkillThirdCooldown(time);
    }

    public void UpdatePlayerSkillForthCooldown(float time)
    {
        PlayerInterfaceController.UpdatePlayerSkillForthCooldown(time);
    }

    public void UpdateDashSkillCooldown(float time)
    {
        PlayerInterfaceController.UpdateDashSkillCooldown(time);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            NetworkManager.Singleton.StartHost();
            SystemManager.Instance.StartGame();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            NetworkManager.Singleton.StartClient();
            SystemManager.Instance.StartGame();
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            _menuInterfaceController.ShowMenuVisuals();
            SystemManager.Instance.ForceUnlockedMouse();
        }
    }
}

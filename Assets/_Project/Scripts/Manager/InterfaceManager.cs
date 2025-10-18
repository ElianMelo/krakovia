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
    public TMP_Dropdown classSkinSelectionDropdown;
    public TMP_Text firstDescription;
    public Image iamgeField;
    public GameObject horseModel;
    public GameObject fairyModel;
    public GameObject skeletonModel;

    [Header("Skin Selection")]
    public SkinnedMeshRenderer horseModelWithMaterial;
    public SkinnedMeshRenderer fairyModelWithMaterial;
    public SkinnedMeshRenderer skeletonModelWithMaterial;
    public Material horseSkinMatA;
    public Material horseSkinMatB;
    public Material horseSkinMatC;
    public Material horseSkinMatD;
    public Material fairySkinMatA;
    public Material fairySkinMatB;
    public Material fairySkinMatC;
    public Material fairySkinMatD;
    public Material skeletonSkinMatA;
    public Material skeletonSkinMatB;
    public Material skeletonSkinMatC;
    public Material skeletonSkinMatD;

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
        classSelectionDropdown.onValueChanged.AddListener(delegate { ClassSelectionDropdownValueChanged(classSelectionDropdown); });
        classSkinSelectionDropdown.onValueChanged.AddListener(delegate { ClassSkinDropdownValueChanged(classSkinSelectionDropdown); });
    }

    void ClassSkinDropdownValueChanged(TMP_Dropdown change)
    {
        PlayerClass playerClass = (PlayerClass) classSelectionDropdown.value;
        PlayerClassSkin playerClassSkin = (PlayerClassSkin)change.value;

        ChangeClassSkinVariation(playerClass, playerClassSkin);
    }

    void ClassSelectionDropdownValueChanged(TMP_Dropdown change)
    {
        PlayerClass playerClass = (PlayerClass)change.value;
        PlayerClassSkin playerClassSkin = (PlayerClassSkin)classSkinSelectionDropdown.value;

        switch (playerClass)
        {
            case PlayerClass.Fairy:
                firstDescription.text = fairyFirstDescription;
                iamgeField.sprite = fairyImage;
                horseModel.SetActive(false);
                fairyModel.SetActive(true);
                skeletonModel.SetActive(false);
                ChangeClassSkinVariation(playerClass, playerClassSkin);
                break;
            case PlayerClass.Skeleton:
                firstDescription.text = skeletonFirstDescription;
                iamgeField.sprite = skeletonImage;
                horseModel.SetActive(false);
                fairyModel.SetActive(false);
                skeletonModel.SetActive(true);
                ChangeClassSkinVariation(playerClass, playerClassSkin);
                break;
            case PlayerClass.Horse:
                firstDescription.text = horseFirstDescription;
                iamgeField.sprite = horseImage;
                horseModel.SetActive(true);
                fairyModel.SetActive(false);
                skeletonModel.SetActive(false);
                ChangeClassSkinVariation(playerClass, playerClassSkin);
                break;
            default:
                break;
        }
    }

    private void ChangeClassSkinVariation(PlayerClass playerClass, PlayerClassSkin playerClassSkin)
    {
        switch (playerClass)
        {
            case PlayerClass.Fairy:
                switch (playerClassSkin)
                {
                    case PlayerClassSkin.VariationA:
                        fairyModelWithMaterial.material = fairySkinMatA;
                        break;
                    case PlayerClassSkin.VariationB:
                        fairyModelWithMaterial.material = fairySkinMatB;
                        break;
                    case PlayerClassSkin.VariationC:
                        fairyModelWithMaterial.material = fairySkinMatC;
                        break;
                    case PlayerClassSkin.VariationD:
                        fairyModelWithMaterial.material = fairySkinMatD;
                        break;
                    default:
                        break;
                }
                break;
            case PlayerClass.Skeleton:
                switch (playerClassSkin)
                {
                    case PlayerClassSkin.VariationA:
                        skeletonModelWithMaterial.material = skeletonSkinMatA;
                        break;
                    case PlayerClassSkin.VariationB:
                        skeletonModelWithMaterial.material = skeletonSkinMatB;
                        break;
                    case PlayerClassSkin.VariationC:
                        skeletonModelWithMaterial.material = skeletonSkinMatC;
                        break;
                    case PlayerClassSkin.VariationD:
                        skeletonModelWithMaterial.material = skeletonSkinMatD;
                        break;
                    default:
                        break;
                }
                break;
            case PlayerClass.Horse:
                switch (playerClassSkin)
                {
                    case PlayerClassSkin.VariationA:
                        horseModelWithMaterial.material = horseSkinMatA;
                        break;
                    case PlayerClassSkin.VariationB:
                        horseModelWithMaterial.material = horseSkinMatB;
                        break;
                    case PlayerClassSkin.VariationC:
                        horseModelWithMaterial.material = horseSkinMatC;
                        break;
                    case PlayerClassSkin.VariationD:
                        horseModelWithMaterial.material = horseSkinMatD;
                        break;
                    default:
                        break;
                }
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

    public PlayerClassSkin GetSelectedSkinClass()
    {
        return (PlayerClassSkin) classSkinSelectionDropdown.value;
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
        //if (Input.GetKeyDown(KeyCode.F1))
        //{
        //    NetworkManager.Singleton.StartHost();
        //    SystemManager.Instance.StartGame();
        //}
        //if (Input.GetKeyDown(KeyCode.F2))
        //{
        //    NetworkManager.Singleton.StartClient();
        //    SystemManager.Instance.StartGame();
        //}
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            _menuInterfaceController.ShowMenuVisuals();
            SystemManager.Instance.ForceUnlockedMouse();
        }
    }
}

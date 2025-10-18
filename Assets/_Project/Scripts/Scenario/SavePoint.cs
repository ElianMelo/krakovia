using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class SavePoint : MonoBehaviour
{
    private const string POINT_SAVED = "Ponto Salvo";
    private const string TOSAVE_POINT = "E - Salvar Ponto";

    [SerializeField] private TMP_Text savePointText;
    [SerializeField] private List<Transform> respawnPoints = new();

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClip;

    private void Start()
    {
        StartCoroutine(delayedCurrentSavePointCheck());
        IEnumerator delayedCurrentSavePointCheck()
        {
            while(true)
            {
                yield return new WaitForSeconds(1f + Time.deltaTime);
                if (SystemManager.Instance.CurrentSavePoint == this)
                {
                    savePointText.text = POINT_SAVED;
                }
                else
                {
                    savePointText.text = TOSAVE_POINT;
                }
            }
        }
    }

    public void ShowButton()
    {
        savePointText.enabled = true;
    }

    public void HideButton()
    {
        savePointText.enabled = false;
    }

    public void SelectThisSavePoint()
    {
        audioSource.PlayOneShot(audioClip);
        SystemManager.Instance.CurrentSavePoint = this;
    }

    public Vector3 GetRandomPositionAround()
    {
        return respawnPoints[Random.Range(0, respawnPoints.Count)].position;
    }
}

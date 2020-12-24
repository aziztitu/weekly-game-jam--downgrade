using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public GameObject root;
    public Image healthBarFill;
    public Image playerIcon;
    public TextMeshProUGUI nameText;
    public List<Image> stages;

    public Color stageActiveColor = Color.yellow;
    public Color stageInActiveColor = Color.black;

    [HideInInspector] public CharacterModel trackingCharacter = null;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!trackingCharacter)
        {
            root.SetActive(false);
            return;
        }

        root.SetActive(true);

        healthBarFill.fillAmount = trackingCharacter.health.normalizedHealth;
        playerIcon.gameObject.SetActive(trackingCharacter.isLocalPlayer);
        nameText.text = trackingCharacter.characterSelectionData.character.name;

        if (BattleManager.Instance?.battleData?.characterStages != null)
        {
            for (int i = 0; i < stages.Count; i++)
            {
                stages[i].color = BattleManager.Instance.battleData.characterStages[trackingCharacter.characterIndex] >
                                  i
                    ? stageActiveColor
                    : stageInActiveColor;
            }
        }
    }
}
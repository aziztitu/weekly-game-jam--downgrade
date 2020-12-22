using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUD : SingletonMonoBehaviour<HUD>
{
    [Header("Round Intro")] public MenuPage roundIntro;
    public TextMeshProUGUI roundText;

    [Header("Core HUD")] public MenuPage coreHUD;

    public List<PlayerStats> playerStatsList;

    // Start is called before the first frame update
    void Start()
    {
        coreHUD.gameObject.SetActive(false);
        ShowRoundIntro();
    }

    // Update is called once per frame
    void Update()
    {
    }

    void ShowRoundIntro()
    {
        roundText.text = $"Round {BattleManager.Instance.battleData.currentRound + 1}";
        roundIntro.Show();
        this.WaitAndExecute(() =>
        {
            roundIntro.Hide();
            coreHUD.Show();
        }, 3f);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    void Awake()
    {
        Time.timeScale = 1f;
        HelperUtilities.UpdateCursorLock(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlaySinglePlayer(float difficulty)
    {
        GameManager.Instance.SelectedBattleSettings = new BattleManager.BattleSettings()
        {
            characterSelections = new List<BattleManager.CharacterSelection>()
            {
                new BattleManager.CharacterSelection()
                {
                    character = GameManager.Instance.characters[0],
                    isLocalPlayer = true,
                },
                new BattleManager.CharacterSelection()
                {
                    character = GameManager.Instance.characters[0],
                    isLocalPlayer = false,
                    aiDifficulty = difficulty,
                },
            }
        };

        GameManager.Instance.battleData = new BattleManager.BattleData();
        GameManager.Instance.GoToScene(GameManager.Instance.battleArenaScene);
    }

    public void Exit()
    {
        GameManager.Instance.QuitGame();
    }
}

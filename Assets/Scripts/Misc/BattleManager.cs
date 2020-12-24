using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BattleManager : SingletonMonoBehaviour<BattleManager>
{
    const int MaxCharacters = 2;

    [Serializable]
    public class BattleSettings
    {
        public List<CharacterSelection> characterSelections;
    }

    [Serializable]
    public class CharacterSelection
    {
        public CharacterData character;
        public bool isLocalPlayer;
        public bool isAI => !isLocalPlayer;
    }

    [Serializable]
    public class BattleData
    {
        public bool expired = true;

        public int currentRound = 0;
        public List<int> roundResults = new List<int>();

        public List<int> characterStages;

        public void Initialize(int numCharacters)
        {
            characterStages = new List<int>();
            for (int i = 0; i < numCharacters; i++)
            {
                characterStages.Add(0);
            }

            expired = false;
        }
    }

    [Serializable]
    public class DowngradeStage
    {
        public GameObject weaponPrefab;
        public GameObject shieldPrefab;
    }

    [Header("Settings")] public List<DowngradeStage> downgradeStages;
    public BattleSettings battleSettings;

    [Header("References")] public List<Transform> characterSpawnPoints;
    private Randomizer<Transform> characterSpawnRandomizer;

    [Header("Ending")]
    public MenuPage roundEndScreen;
    public TextMeshProUGUI roundEndMessage;
    public float roundEndMessageDuration = 2f;

    public MenuPage battleEndScreen;
    public TextMeshProUGUI battleEndMessage;

    [HideInInspector] public List<CharacterModel> spawnedCharacters = new List<CharacterModel>();

    public int numCharacters => Mathf.Min(MaxCharacters, battleSettings.characterSelections.Count);
    public int totalDowngradeStages => downgradeStages.Count;

    public BattleData battleData => GameManager.Instance.battleData;

    public bool roundOver = false;

    new void Awake()
    {
        base.Awake();

        if (GameManager.Instance.hasBattleSettings)
        {
            this.battleSettings = GameManager.Instance.SelectedBattleSettings;
        }

        characterSpawnRandomizer = new Randomizer<Transform>(characterSpawnPoints);

        if (GameManager.Instance.battleData.expired)
        {
            GameManager.Instance.battleData = new BattleData();
            GameManager.Instance.battleData.Initialize(numCharacters);
        }

        Time.timeScale = 1;
        HelperUtilities.UpdateCursorLock(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        SpawnCharacters();

        SoundEffectsManager.Instance.Play("Gong");
    }

    // Update is called once per frame
    void Update()
    {
    }

    void SpawnCharacters()
    {
        for (int i = 0; i < numCharacters; i++)
        {
            var characterSelection = battleSettings.characterSelections[i];

            var spawnPoint = characterSpawnRandomizer.GetRandomItem();
            var characterModel =
                Instantiate(characterSelection.character.prefab, spawnPoint.position, spawnPoint.rotation)
                    .GetComponent<CharacterModel>();

            characterModel.playerInputController.enabled = characterSelection.isLocalPlayer;
            characterModel.aiController.enabled = characterSelection.isAI;
            characterModel.characterSelectionData = characterSelection;
            characterModel.characterIndex = i;

            if (characterSelection.isLocalPlayer)
            {
                ThirdPersonCamera.Instance.SetTargetObject(characterModel.playerTarget);
            }

            if (downgradeStages.Count > 0)
            {
                var downgradeStageIndex = battleData.characterStages[i];
                if (downgradeStageIndex >= totalDowngradeStages)
                {
                    downgradeStageIndex = totalDowngradeStages - 1;
                }

                var downgradeStage = downgradeStages[downgradeStageIndex];
                characterModel.characterMeleeController.SpawnWeapon(downgradeStage.weaponPrefab);
                characterModel.characterMeleeController.SpawnShield(downgradeStage.shieldPrefab);
            }
            else
            {
                Debug.LogError("Please provide the Downgrade Stages in Battle Manager");
            }

            characterModel.health.OnHealthDepleted.AddListener(
                () => { OnCharacterDied(characterModel.characterIndex); });

            if (HUD.Instance.playerStatsList.Count > i)
            {
                HUD.Instance.playerStatsList[i].trackingCharacter = characterModel;
            }

            spawnedCharacters.Add(characterModel);
        }

        if (spawnedCharacters.Count == 2)
        {
            spawnedCharacters[0].lockedOnTarget = spawnedCharacters[1].transform;
            spawnedCharacters[1].lockedOnTarget = spawnedCharacters[0].transform;
        }

        foreach (var character in spawnedCharacters)
        {
            character.OnInitialized();
        }
    }

    void OnCharacterDied(int characterIndex)
    {
        // Debug.Log(characterIndex);
        var aliveCharacters = spawnedCharacters.Where((model => model.isAlive)).ToList();
        if (aliveCharacters.Count == 1)
        {
            var winner = aliveCharacters[0];
            battleData.roundResults.Add(winner.characterIndex);
            battleData.characterStages[winner.characterIndex]++;

            roundEndMessage.text = winner.isLocalPlayer ? "You win!" : "You lose!";
            roundEndMessage.text = $"<b>{roundEndMessage.text}</b>";
            roundEndScreen.Show();

            roundOver = true;

            this.WaitAndExecute(() => { NextRound(winner); }, roundEndMessageDuration);
        }
        else if (aliveCharacters.Count == 0)
        {
            battleData.roundResults.Add(-1);

            roundEndMessage.text = "Tied";
            roundEndMessage.text = $"<b>{roundEndMessage.text}</b>";
            roundEndScreen.Show();

            roundOver = true;

            this.WaitAndExecute(() => NextRound(), roundEndMessageDuration);
        }
    }

    void NextRound(CharacterModel winner = null)
    {
        if (winner)
        {
            if (battleData.characterStages[winner.characterIndex] >= totalDowngradeStages)
            {
                OnBattleEnded(winner);
                return;
            }
        }
        GameManager.Instance.RestartCurrentScene(() => { battleData.currentRound++; });
    }

    void OnBattleEnded(CharacterModel winner)
    {
        roundEndScreen.Hide();

        winner.PlayVictoryTaunt();

        battleEndMessage.text = winner.isLocalPlayer ? "You live to fight another day!" : "So long, fallen warrior!";
        battleEndMessage.text = $"<b>{battleEndMessage.text}</b>";
        battleEndScreen.Show();

        HelperUtilities.UpdateCursorLock(false);
    }

    public void GoToMainMenu()
    {
        GameManager.Instance.GoToMainMenu();
    }

    public void RestartBattle()
    {
        GameManager.Instance.battleData = new BattleData();
        GameManager.Instance.RestartCurrentScene();
    }
}
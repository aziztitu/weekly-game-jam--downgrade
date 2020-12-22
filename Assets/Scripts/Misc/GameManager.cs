using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class GameSettings
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
}

public class GameManager : MonoBehaviour
{
    private const string gameSettingsFileName = "gameSettings.dat";

    public static GameManager Instance = null;
    public const int maxPlayers = 4;

    public List<CharacterData> characters;

    [Header("Scene Names")] public string mainMenuScene = "Main Menu";
    public string characterSelectionScene = "Character Selection";
    public string battleSettingsScene = "Battle Settings";

    public bool hasBattleSettings => _selectedBattleSettings != null;

    [Header("Battle Data")] [ReadOnly]
    public BattleManager.BattleData battleData = null;

    public readonly Dictionary<string, object> metaData = new Dictionary<string, object>();

    public BattleManager.BattleSettings SelectedBattleSettings
    {
        get => _selectedBattleSettings ?? (_selectedBattleSettings = new BattleManager.BattleSettings());
        set => _selectedBattleSettings = value;
    }

    private BattleManager.BattleSettings _selectedBattleSettings = null;

    private Vector2 lastScreenSize;

    public event Action onScreenSizeChanged; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        if (lastScreenSize != screenSize)
        {
            lastScreenSize = screenSize;
            onScreenSizeChanged?.Invoke();
        }
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
    }
    
    public void ResetRaceConfig()
    {
        GameManager.Instance.SelectedBattleSettings = null;
    }

    public void QuitGame()
    {
        ScreenFader.Instance.FadeOut(-1, () =>
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif

            Application.Quit();
        });
    }

    public void GoToMainMenu()
    {
        GoToScene(mainMenuScene);
    }

    public void GoToCharacterSelectionScene()
    {
        GoToScene(characterSelectionScene);
    }

    public void RestartCurrentScene()
    {
        GoToScene(SceneManager.GetActiveScene().name);
    }

    public void GoToScene(string sceneName)
    {
        ScreenFader.Instance.FadeOut(-1, () =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }

    public void DeleteAllSaveData()
    {
        string dir = Application.persistentDataPath;
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }
    }

    public GameSettings GetGameSettings()
    {
        return SaveSystem.LoadData<GameSettings>(gameSettingsFileName) ?? new GameSettings()
        {
            masterVolume = 1f,
            musicVolume = 1f,
            sfxVolume = 1f,
        };
    }

    public void SaveGameSettings(GameSettings gameSettings)
    {
        SaveSystem.SaveData(gameSettings, gameSettingsFileName);
    }
}
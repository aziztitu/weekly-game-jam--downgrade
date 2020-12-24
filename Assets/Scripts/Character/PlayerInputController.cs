using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    private CharacterModel _characterModel;

    private void Awake()
    {
        _characterModel = GetComponent<CharacterModel>();
    }

    public void Update()
    {
        UpdatePlayerInput();
    }

    public void UpdatePlayerInput()
    {
        if (Time.timeScale < 0.1f || BattleManager.Instance.roundOver)
        {
            _characterModel.characterInput = new CharacterModel.CharacterInput();
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        _characterModel.characterInput.Move = new Vector3(horizontal, 0f, vertical);
        _characterModel.characterInput.AttemptParry = Input.GetButtonDown("Block");
        _characterModel.characterInput.IsBlocking = Input.GetButton("Block");
        _characterModel.characterInput.Dodge = Input.GetButtonDown("Dodge");
        //_characterModel.characterInput.Sprint = Input.GetButton("Sprint");

        _characterModel.characterInput.HeavyAttack = (Input.GetKey(KeyCode.LeftShift) && Input.GetButtonDown("Fire1")) || Input.GetButtonDown("Fire2");
        _characterModel.characterInput.LightAttack =
            !_characterModel.characterInput.HeavyAttack && Input.GetButtonDown("Fire1");
    }
}
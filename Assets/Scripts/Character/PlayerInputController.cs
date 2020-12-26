using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private CharacterModel _characterModel;

    PlayerInput playerInput;
    InputAction movement;
    InputAction lightAttack;
    InputAction heavyAttack;
    InputAction dodge;
    InputAction block;

    bool test;

    private void Awake()
    {
        _characterModel = GetComponent<CharacterModel>();

        playerInput = GetComponent<PlayerInput>();
        movement = playerInput.actions["Movement"];
        lightAttack = playerInput.actions["LightAttack"];
        heavyAttack = playerInput.actions["HeavyAttack"];
        dodge = playerInput.actions["DodgeRoll"];
        block = playerInput.actions["Block"];
    }

    public void Update()
    {
        UpdatePlayerInput();

        Debug.Log(_characterModel.characterInput.AttemptParry);
    }

    public void UpdatePlayerInput()
    {
        if (Time.timeScale < 0.1f || BattleManager.Instance.roundOver)
        {
            _characterModel.characterInput = new CharacterModel.CharacterInput();
            return;
        }

        Vector2 movementValue = movement.ReadValue<Vector2>();

        _characterModel.characterInput.Move = new Vector3(movementValue.x, 0f, movementValue.y);
        _characterModel.characterInput.AttemptParry = block.triggered;
        //_characterModel.characterInput.IsBlocking = block.triggered; ///TODO: Figure out how to recreate a GetButton() event.

        if (!_characterModel.characterInput.IsBlocking)
        {
            block.started += Block_started =>  _characterModel.characterInput.IsBlocking = true;
        }
        else
        {
            block.canceled += Block_stopped =>  _characterModel.characterInput.IsBlocking = false;
        }

        _characterModel.characterInput.Dodge = dodge.triggered;
        //_characterModel.characterInput.Sprint = Input.GetButton("Sprint");

        _characterModel.characterInput.HeavyAttack = heavyAttack.triggered;
        _characterModel.characterInput.LightAttack = !_characterModel.characterInput.HeavyAttack && lightAttack.triggered;
        ///TODO: Find out why shift continues the light attack. Find out how to make shift not activate the heavy attack.

        //float horizontal = Input.GetAxis("Horizontal");
        //float vertical = Input.GetAxis("Vertical");

        //_characterModel.characterInput.Move = new Vector3(horizontal, 0f, vertical);
        //_characterModel.characterInput.AttemptParry = false;// Input.GetButtonDown("Block");
        //_characterModel.characterInput.IsBlocking = false;// Input.GetButton("Block");
        //_characterModel.characterInput.Dodge = false;// Input.GetButtonDown("Dodge");
        //_characterModel.characterInput.Sprint = Input.GetButton("Sprint");

        //_characterModel.characterInput.HeavyAttack = false;//(Input.GetKey(KeyCode.LeftShift) && Input.GetButtonDown("Fire1")) || Input.GetButtonDown("Fire2");
        //_characterModel.characterInput.LightAttack = false;
        //!_characterModel.characterInput.HeavyAttack && Input.GetButtonDown("Fire1");
    }
}
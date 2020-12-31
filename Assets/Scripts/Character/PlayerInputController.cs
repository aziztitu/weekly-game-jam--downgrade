using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private CharacterModel _characterModel;

    private string PS4ControlSchemeName = "PS4";
    private string KeyboardControlSchemeName = "Keyboard";
    private string XBoxControlSchemeName = "Xbox";

    PlayerInput playerInput;
    InputAction movement;
    InputAction lightAttack;
    InputAction heavyHelper;
    InputAction heavyAttack;
    InputAction dodge;
    InputAction block;

    bool isHeavyHelper;

    private void Awake()
    {
        _characterModel = GetComponent<CharacterModel>();

        playerInput = GetComponent<PlayerInput>();
        movement = playerInput.actions["Movement"];
        lightAttack = playerInput.actions["LightAttack"];
        heavyHelper = playerInput.actions["HeavyAttackHelperMouse"];
        heavyAttack = playerInput.actions["HeavyAttack"];
        dodge = playerInput.actions["DodgeRoll"];
        block = playerInput.actions["Block"];
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

        //Gets movement
        Vector2 movementValue = movement.ReadValue<Vector2>();
        _characterModel.characterInput.Move = new Vector3(movementValue.x, 0f, movementValue.y);

        //Resets parry attempt for next frame
        if (_characterModel.characterInput.AttemptParry)
        {
            _characterModel.characterInput.AttemptParry = false;
        }
        
        //Resets heavy attack for next frame
        if (_characterModel.characterInput.HeavyAttack)
        {
            _characterModel.characterInput.HeavyAttack = false;
        }

        //Checks to see if the character has started blocking, if they have, set parry to true
        if (!_characterModel.characterInput.IsBlocking)
        {
            block.started += Block_started =>  _characterModel.characterInput.IsBlocking = true;
            block.started += Parry_started => _characterModel.characterInput.AttemptParry = true;
        }
        else
        {
            block.canceled += Block_stopped =>  _characterModel.characterInput.IsBlocking = false;
        }

        _characterModel.characterInput.Dodge = dodge.triggered;

        //Checks to see if shift is being pressed down for heavy keyboard & mouse attack
        if (!isHeavyHelper)
        {
            heavyHelper.started += Heavy_started => isHeavyHelper = true;
        }
        else
        {
            heavyHelper.canceled += Heavy_stopped => isHeavyHelper = false;
        }
        

        if (isHeavyHelper || (playerInput.currentControlScheme == PS4ControlSchemeName || playerInput.currentControlScheme == XBoxControlSchemeName))
        {
            _characterModel.characterInput.HeavyAttack = heavyAttack.triggered;
        }

        _characterModel.characterInput.LightAttack = !_characterModel.characterInput.HeavyAttack && lightAttack.triggered;
        //_characterModel.characterInput.Sprint = Input.GetButton("Sprint");


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
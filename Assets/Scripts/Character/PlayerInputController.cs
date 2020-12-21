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
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        _characterModel.characterInput.Move = new Vector3(horizontal, 0f, vertical);
        //_characterModel.characterInput.Sprint = Input.GetButton("Sprint");
    }
}
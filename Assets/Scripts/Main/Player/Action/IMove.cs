using UnityEngine.InputSystem;

namespace Main.Player.Action
{
    public interface IMove
    {
        void OnMove(InputAction.CallbackContext context);
    }
}

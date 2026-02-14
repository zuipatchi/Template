using UnityEngine.InputSystem;

namespace Main.Player.Action
{
    public interface IDash
    {
        void OnDash(InputAction.CallbackContext context);
    }
}

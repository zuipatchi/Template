using Main.Player.Action.Dash;
using Main.Player.Action.Move;
using UnityEngine.InputSystem;

namespace Main.Player.Action
{
    public class PlayerInputCallbacks : PlayerControls.IPlayerActions
    {
        private readonly IMove _moveHandler;
        private readonly IDash _dashHandler;

        public PlayerInputCallbacks(IMove moveHandler, IDash dashHandler)
        {
            _moveHandler = moveHandler;
            _dashHandler = dashHandler;
        }

        public void OnMove(InputAction.CallbackContext context) => _moveHandler.OnMove(context);

        public void OnDash(InputAction.CallbackContext context) => _dashHandler.OnDash(context);
    }
}

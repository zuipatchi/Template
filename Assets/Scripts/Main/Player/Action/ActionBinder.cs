using VContainer;
using VContainer.Unity;

namespace Main.Player.Action
{
    public class ActionBinder : IStartable
    {
        private PlayerControls _actions;
        private PlayerInputCallbacks _callbacks;

        [Inject]
        public ActionBinder(PlayerControls playerControls, PlayerInputCallbacks playerInputCallbacks)
        {
            _actions = playerControls;
            _callbacks = playerInputCallbacks;
        }

        public void Start()
        {
            _actions.Player.AddCallbacks(_callbacks);
            _actions.Player.Enable();
        }
    }
}

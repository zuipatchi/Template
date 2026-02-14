using UnityEngine;
using UnityEngine.InputSystem;

namespace Main.Player.Action.Dash
{

    public class PlayerDash : MonoBehaviour, IDash
    {
        public void OnDash(InputAction.CallbackContext context)
        {
            Debug.Log("だっしゅ");
        }
    }
}

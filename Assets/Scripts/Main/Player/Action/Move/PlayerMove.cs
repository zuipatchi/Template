using UnityEngine;
using UnityEngine.InputSystem;

namespace Main.Player.Action.Move
{
    /// <summary>
    /// プレイヤーを上下左右に移動させるクラス
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMove : MonoBehaviour, IMove
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _turnSpeed = 720f;

        private CharacterController _controller;
        private Vector2 _moveInput;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            Vector3 moveDir = new Vector3(_moveInput.x, 0f, _moveInput.y);
            if (moveDir.sqrMagnitude <= 0f) return;

            moveDir = moveDir.normalized;

            // 1) 移動方向へ回転（水平のみ）
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                _turnSpeed * Time.deltaTime
            );

            // 2) 移動
            _controller.Move(moveDir * _moveSpeed * Time.deltaTime);
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }
    }
}

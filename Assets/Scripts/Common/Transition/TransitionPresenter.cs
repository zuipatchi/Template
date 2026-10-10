using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace Common.Transition
{
    public class TransitionPresenter : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.4f;

        private VisualElement _overlay;
        private Tweener _currentTween;

        private void Awake()
        {
            UIDocument uiDocument = GetComponent<UIDocument>();
            // 見た目は Transition.uss の .transition-overlay に定義
            _overlay = uiDocument.rootVisualElement.Q<VisualElement>("TransitionOverlay");
            if (_overlay == null)
            {
                Debug.LogError("TransitionOverlay が見つかりませんでした。UIDocument に Transition.uxml が割り当てられているか確認してください。");
            }
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();
            _currentTween = null;
        }

        public async UniTask CoverAsync()
        {
            await AnimateAsync(from: 0f, to: 1f, Ease.InQuad, hideOnComplete: false);
        }

        public async UniTask RevealAsync()
        {
            await AnimateAsync(from: 1f, to: 0f, Ease.OutQuad, hideOnComplete: true);
        }

        private async UniTask AnimateAsync(float from, float to, Ease ease, bool hideOnComplete)
        {
            float opacity = from;
            _overlay.style.opacity = opacity;
            _overlay.style.display = DisplayStyle.Flex;
            // 暗幕が出ている間はクリックを受け止め、下のシーン UI やオプションを操作させない
            // （遷移中に別の遷移が始まり、暗幕が残ったまま操作不能になるのを防ぐ）
            _overlay.pickingMode = PickingMode.Position;

            _currentTween?.Kill();
            UniTaskCompletionSource tcs = new();
            _currentTween = DOTween.To(
                () => opacity,
                v =>
                {
                    opacity = v;
                    _overlay.style.opacity = v;
                },
                to,
                _duration
            )
            .SetEase(ease)
            .OnComplete(() =>
            {
                if (hideOnComplete)
                {
                    _overlay.style.display = DisplayStyle.None;
                    _overlay.pickingMode = PickingMode.Ignore;
                }
                tcs.TrySetResult();
            })
            .OnKill(() => tcs.TrySetResult());

            await tcs.Task;
        }
    }
}

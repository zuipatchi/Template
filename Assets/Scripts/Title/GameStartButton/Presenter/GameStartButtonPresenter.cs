using Common.SceneManagement;
using Common.SoundManagement;
using Common.Store;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Title.GameStartButton
{
    // クリックされたらネクストシーンに遷移する
    [RequireComponent(typeof(UIDocument))]
    public class GameStartButtonPresenter : MonoBehaviour
    {
        private SceneTransitioner _sceneTransitioner;
        private SoundStore _soundStore;
        private SoundPlayer _soundPlayer;
        [SerializeField] private Scenes _nextScene;
        // 点滅の片道（不透明 → 最も薄い状態）にかける秒数
        [SerializeField] private float _blinkDuration = 0.6f;
        [SerializeField] private float _blinkMinOpacity = 0.15f;

        private UIDocument _uiDocument;
        private Button _gameStartButton;
        private Tweener _blinkTween;

        [Inject]
        public void Construct(SceneTransitioner sceneTransitioner, SoundStore soundStore, SoundPlayer soundPlayer)
        {
            _sceneTransitioner = sceneTransitioner;
            _soundStore = soundStore;
            _soundPlayer = soundPlayer;
        }

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            VisualElement root = _uiDocument.rootVisualElement;
            _gameStartButton = root.Q<Button>("GameStartButton");
            if (_gameStartButton == null)
            {
                Debug.LogError("GameStartButton が見つかりませんでした。");
                return;
            }
            _gameStartButton.clicked += OnClickGameStart;
            StartBlink();
        }

        private void OnDisable()
        {
            StopBlink();
            if (_gameStartButton != null) _gameStartButton.clicked -= OnClickGameStart;
            _gameStartButton = null;
        }

        private void OnClickGameStart()
        {
            StopBlink();
            _gameStartButton.SetEnabled(false);
            _soundPlayer.PlaySE(_soundStore.Enter1SE);
            _sceneTransitioner.Transit(_nextScene).Forget();
        }

        // 「PRESS START」をチカチカさせる（透明度の往復ループ）
        private void StartBlink()
        {
            StopBlink();

            // スタイルプロパティを直接ゲッターにせず、ローカル float を仲介させる（docs/patterns.md 参照）
            float opacity = 1f;
            _gameStartButton.style.opacity = opacity;

            _blinkTween = DOTween.To(
                () => opacity,
                v =>
                {
                    opacity = v;
                    _gameStartButton.style.opacity = v;
                },
                _blinkMinOpacity,
                _blinkDuration
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
        }

        private void StopBlink()
        {
            _blinkTween?.Kill();
            _blinkTween = null;
            if (_gameStartButton != null)
            {
                _gameStartButton.style.opacity = 1f;
            }
        }
    }
}

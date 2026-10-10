using System;
using Common.SoundManagement;
using Common.Store;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace Common.Option
{
    public class OptionPresenter : MonoBehaviour
    {
        private OptionModel _optionModel;
        private ModalStore _modalStore;
        private UiSoundPlayer _uiSoundPlayer;
        private OptionExitRouter _exitRouter;
        private OptionModalPresenter _modalPresenter;
        private VisualElement _optionIcon;
        private VisualElement _overlay;
        private VisualElement _host;
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public void Construct(ModalStore modalStore, OptionModel optionModel, UiSoundPlayer uiSoundPlayer, OptionExitRouter exitRouter)
        {
            _modalStore = modalStore;
            _optionModel = optionModel;
            _uiSoundPlayer = uiSoundPlayer;
            _exitRouter = exitRouter;
        }

        private void Awake()
        {
            UIDocument uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("UIDocument が見つかりませんでした。");
                return;
            }

            VisualElement root = uiDocument.rootVisualElement;
            _optionIcon = root.Q<Image>("OptionSliders");
            _overlay = root.Q<VisualElement>("ModalOverlay");
            _host = root.Q<VisualElement>("ModalHost");

            if (_optionIcon == null)
            {
                Debug.LogError("OptionSliders が見つかりませんでした。");
                return;
            }

            _optionIcon.RegisterCallback<ClickEvent>(_ => OpenModal());
        }

        private void Start()
        {
            SetupAsync().Forget();
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }

        private async UniTask SetupAsync()
        {
            if (_host == null)
            {
                Disable("ModalHost が見つかりませんでした。");
                return;
            }

            try
            {
                await UniTask.WhenAll(_modalStore.Loaded, _uiSoundPlayer.Loaded);
            }
            catch (Exception e)
            {
                // ロード失敗はほぼ Addressables の設定ミス。閉じられないオーバーレイを残さないよう、オプション自体を使えなくする
                Disable($"オプションモーダルのアセットをロードできませんでした: {e.Message}");
                return;
            }

            TemplateContainer modal = _modalStore.Modal.Instantiate();

            _modalPresenter = new OptionModalPresenter(_optionModel, OnClickClose, OnClickExit);
            _modalPresenter.Setup(modal, _disposables);

            _host.Add(modal);
            CloseModal();
        }

        // オーバーレイとオプションアイコンを隠し、オプションを開けなくする
        private void Disable(string reason)
        {
            Debug.LogError($"{reason} オプションアイコンを非表示にします。");
            if (_overlay != null)
            {
                _overlay.style.display = DisplayStyle.None;
            }
            if (_optionIcon != null)
            {
                _optionIcon.style.display = DisplayStyle.None;
            }
        }

        private void OpenModal()
        {
            // モーダルの準備が終わるまでは開かない（中身も閉じるボタンも無いオーバーレイが出てしまうため）
            if (_modalPresenter == null)
            {
                return;
            }

            _modalPresenter.SetExit(_exitRouter.CurrentLabel, _exitRouter.IsExitAvailable);
            _overlay.style.display = DisplayStyle.Flex;
            _uiSoundPlayer.PlayOpen();
        }

        private void CloseModal()
        {
            _overlay.style.display = DisplayStyle.None;
        }

        private void OnClickClose()
        {
            _uiSoundPlayer.PlayClose();
            CloseModal();
        }

        private void OnClickExit()
        {
            _uiSoundPlayer.PlayDecide();
            CloseModal();
            _exitRouter.Execute();
        }
    }
}

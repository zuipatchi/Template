using System.Threading.Tasks;
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
        private UIDocument _uIDocument;
        private OptionModel _optionModel;
        private ModalStore _modalStore;
        private VisualTreeAsset _modal;
        private VisualElement _overlay;
        private VisualElement _host;
        private CompositeDisposable _disposables = new();

        [Inject]
        public void Construct(ModalStore modalStore, OptionModel optionModel)
        {
            _modalStore = modalStore;
            _optionModel = optionModel;
        }

        private void Awake()
        {
            _uIDocument = GetComponent<UIDocument>();

            if (_uIDocument == null) Debug.LogError("UIDocument が見つかりませんでした。");

            var root = _uIDocument.rootVisualElement;
            var optionSliders = root.Q<Image>("OptionSliders");
            _overlay = root.Q<VisualElement>("ModalOverlay");
            _host = root.Q<VisualElement>("ModalHost");

            optionSliders.RegisterCallback<ClickEvent>(_ => OpenModal());
        }

        private void Start()
        {
            SetupAsync().Forget();
        }

        private async UniTask SetupAsync()
        {
            await _modalStore.Loaded;
            _modal = _modalStore.Modal;
            var modal = _modal.Instantiate();

            var closeButton = modal.Q<Button>("CloseButton");
            closeButton.clicked += CloseModal;

            var bgmSlider = modal.Q<Slider>("BGMSlider");
            bgmSlider.value = _optionModel.BGMVolume.CurrentValue;

            _optionModel.BGMVolume
                .Subscribe(v => bgmSlider.SetValueWithoutNotify(v))
                .AddTo(_disposables);

            bgmSlider.RegisterValueChangedCallback(OnBGMSliderChange);

            var seSlider = modal.Q<Slider>("SESlider");
            seSlider.value = _optionModel.SEVolume.CurrentValue;

            _optionModel.SEVolume
                .Subscribe(v => seSlider.SetValueWithoutNotify(v))
                .AddTo(_disposables);

            seSlider.RegisterValueChangedCallback(OnSESliderChange);

            _host.Add(modal);
            _overlay.style.display = DisplayStyle.None;
        }

        private void OpenModal()
        {
            _overlay.style.display = DisplayStyle.Flex;
        }

        private void CloseModal()
        {
            _overlay.style.display = DisplayStyle.None;
        }

        private void OnBGMSliderChange(ChangeEvent<float> evt)
        {
            _optionModel.SetBGMVolume(evt.newValue);
        }

        private void OnSESliderChange(ChangeEvent<float> evt)
        {
            _optionModel.SetSEVolume(evt.newValue);
        }
    }
}

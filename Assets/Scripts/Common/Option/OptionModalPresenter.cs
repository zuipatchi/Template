using System;
using R3;
using UnityEngine.UIElements;

namespace Common.Option
{
    public class OptionModalPresenter
    {
        private readonly OptionModel _optionModel;
        private readonly Action _onClose;
        private readonly Action _onExit;

        private Button _exitButton;

        public OptionModalPresenter(OptionModel optionModel, Action onClose, Action onExit)
        {
            _optionModel = optionModel;
            _onClose = onClose;
            _onExit = onExit;
        }

        public void Setup(TemplateContainer modal, CompositeDisposable disposables)
        {
            Button closeButton = modal.Q<Button>("CloseButton");
            closeButton.clicked += _onClose;

            _exitButton = modal.Q<Button>("ExitButton");
            _exitButton.clicked += _onExit;

            BindVolumeSlider(modal, "BGMSlider", _optionModel.BGMVolume, _optionModel.SetBGMVolume, disposables);
            BindVolumeSlider(modal, "SESlider", _optionModel.SEVolume, _optionModel.SetSEVolume, disposables);
        }

        // Model → Slider（表示の同期）と Slider → Model（操作の反映）を双方向に繋ぐ。
        private static void BindVolumeSlider(
            TemplateContainer modal,
            string sliderName,
            ReadOnlyReactiveProperty<float> volume,
            Action<float> setVolume,
            CompositeDisposable disposables)
        {
            Slider slider = modal.Q<Slider>(sliderName);
            slider.value = volume.CurrentValue;
            volume
                .Subscribe(v => slider.SetValueWithoutNotify(v))
                .AddTo(disposables);
            slider.RegisterValueChangedCallback(evt => setVolume(evt.newValue));
        }

        // モーダルは Common で一度だけ生成して使い回すため、開くたびに現在シーンに応じたラベルへ差し替える。
        public void SetExitLabel(string label)
        {
            if (_exitButton == null)
            {
                return;
            }
            _exitButton.text = label;
        }
    }
}

using Common.SceneManagement;
using Common.SoundManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace Home
{
    /// <summary>
    /// Home シーンの UI 操作をシーン遷移・クレジット表示に橋渡しするクラス
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class HomePresenter : MonoBehaviour, IStartable
    {
        private CreditModel _creditModel;
        private SceneTransitioner _sceneTransitioner;
        private UiSoundPlayer _uiSoundPlayer;

        private HomeView _view;

        [Inject]
        public void Construct(
            CreditModel creditModel,
            SceneTransitioner sceneTransitioner,
            UiSoundPlayer uiSoundPlayer)
        {
            _creditModel = creditModel;
            _sceneTransitioner = sceneTransitioner;
            _uiSoundPlayer = uiSoundPlayer;
        }

        // UIDocument の visualTree は OnEnable で構築されるため、Awake ではなく
        // VContainer の Build 完了後に呼ばれる IStartable.Start で View を組み立てる
        void IStartable.Start()
        {
            UIDocument uiDocument = GetComponent<UIDocument>();
            _view = new HomeView(uiDocument.rootVisualElement);

            _view.BuildCredits(_creditModel.Entries);
            _view.SetCreditVisible(false);

            _view.GameStartClicked += OnClickGameStart;
            _view.CreditClicked += OnClickCredit;
            _view.CloseCreditClicked += OnClickCloseCredit;
        }

        private void OnClickGameStart()
        {
            _view.SetInteractable(false);
            _uiSoundPlayer.PlayDecide();
            _sceneTransitioner.Transit(Scenes.Main).Forget();
        }

        private void OnClickCredit()
        {
            _uiSoundPlayer.PlayOpen();
            _view.SetCreditVisible(true);
        }

        private void OnClickCloseCredit()
        {
            _uiSoundPlayer.PlayClose();
            _view.SetCreditVisible(false);
        }
    }
}

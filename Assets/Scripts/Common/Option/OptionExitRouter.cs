using Common.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Common.Option
{
    /// <summary>
    /// オプションモーダルの退出ボタンの文言と遷移先を、現在のシーンに応じて決めるクラス。
    /// Main では「ゲームをやめる」で Home へ、それ以外では「タイトルに戻る」で Title へ。
    /// </summary>
    public sealed class OptionExitRouter
    {
        private const string BackToTitleLabel = "タイトルに戻る";
        private const string QuitGameLabel = "ゲームをやめる";

        private readonly SceneTransitioner _sceneTransitioner;

        public OptionExitRouter(SceneTransitioner sceneTransitioner)
        {
            _sceneTransitioner = sceneTransitioner;
        }

        // モーダルは Common で一度だけ生成して使い回すため、開くたびに現在シーンの文言を取り直す。
        public string CurrentLabel => IsInMainScene() ? QuitGameLabel : BackToTitleLabel;

        /// <summary>
        /// 退出を実行する。Main では Home へ、それ以外は Title へ遷移する。
        /// </summary>
        public void Execute()
        {
            Scenes next = IsInMainScene() ? Scenes.Home : Scenes.Title;
            _sceneTransitioner.Transit(next).Forget();
        }

        private static bool IsInMainScene()
        {
            return SceneManager.GetActiveScene().buildIndex == (int)Scenes.Main;
        }
    }
}

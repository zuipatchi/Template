using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Common.SceneManagement
{
    // 共通シーンをロードするクラス
    public class CommonSceneLoader : MonoBehaviour
    {
        private static bool _loaded = false;

        private async void Awake()
        {
            // 2重起動させない
            if (_loaded) return;
            _loaded = true;

            var commonScene = SceneManager.GetSceneByBuildIndex(0);

            // 共通シーンが存在しなければAdditiveでロード    
            if (!commonScene.IsValid())
            {
                var token = this.GetCancellationTokenOnDestroy();
                await SceneManager.LoadSceneAsync(0, LoadSceneMode.Additive).WithCancellation(token);
            }

            BuildLifetimeScopeInActiveScene();
        }

        // アクティブなシーンに置いてある LifetimeScope をビルドする
        private void BuildLifetimeScopeInActiveScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            var rootObjects = activeScene.GetRootGameObjects();

            foreach (var root in rootObjects)
            {
                var scopes = root.GetComponentsInChildren<LifetimeScope>(true);
                foreach (var scope in scopes)
                {
                    scope.Build();
                }
            }
        }
    }
}

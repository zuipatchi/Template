using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.Common
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
        }
    }
}

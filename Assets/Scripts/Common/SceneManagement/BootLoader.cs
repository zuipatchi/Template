using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Common.SceneManagement
{
    // ビルド起動時: Common がエントリポイントの場合に Title をロードする
    // Editor では Title から起動するため sceneCount > 1 になり何もしない
    public class BootLoader : MonoBehaviour
    {
        private async void Start()
        {
            if (SceneManager.sceneCount > 1)
            {
                return;
            }

            try
            {
                await SceneManager.LoadSceneAsync((int)Scenes.Title, LoadSceneMode.Additive)
                    .WithCancellation(this.GetCancellationTokenOnDestroy());
            }
            catch (OperationCanceledException)
            {
                // ロード中の Destroy によるキャンセルは正常系なので何もしない
                return;
            }

            Scene titleScene = SceneManager.GetSceneByBuildIndex((int)Scenes.Title);
            titleScene.BuildLifetimeScopes();

            SceneManager.SetActiveScene(titleScene);
        }
    }
}

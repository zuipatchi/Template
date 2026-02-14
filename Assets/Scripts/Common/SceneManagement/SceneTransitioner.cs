using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Common.SceneManagement
{
    public enum Scenes
    {
        Common = 0,
        Title = 1,
        Main = 2,
        Sample = 3
    }
    /// <summary>
    /// アクティブシーンを変更するクラス
    /// </summary>
    public sealed class SceneTransitioner : MonoBehaviour
    {
        private readonly SemaphoreSlim _gate = new(1, 1);

        // 「今走ってる遷移」を止めるためのCTS
        private CancellationTokenSource _runningCts;

        // アクティブシーンを next に変更する
        // 複数同時に実行された場合は一番最後の処理のみ実行される
        public async UniTask Transit(Scenes next)
        {
            // コモンシーンはアクティブにしない
            if (next == Scenes.Common) return;

            // 実行中の Transit はキャンセル
            _runningCts?.Cancel();
            _runningCts?.Dispose();
            _runningCts = new CancellationTokenSource();

            await _gate.WaitAsync();
            try
            {
                // Destroyされるまたは、キャンセルされたら止まるトークンを作成する
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(
                    _runningCts.Token,
                    this.GetCancellationTokenOnDestroy()
                );
                var ct = linked.Token;

                var activeScene = SceneManager.GetActiveScene();

                // 同じシーンに遷移できなくする
                if (activeScene.buildIndex == (int)next) return;

                // nextScene が未ロードならロード
                var nextScene = SceneManager.GetSceneByBuildIndex((int)next);
                if (!nextScene.IsValid() || !nextScene.isLoaded)
                {
                    await SceneManager.LoadSceneAsync((int)next, LoadSceneMode.Additive)
                        .WithCancellation(ct);

                    nextScene = SceneManager.GetSceneByBuildIndex((int)next);
                }

                // nextScene をメイン(Active)にする
                SceneManager.SetActiveScene(nextScene);

                // Commonは残す
                if (activeScene.buildIndex != (int)Scenes.Common)
                {
                    await SceneManager.UnloadSceneAsync(activeScene).WithCancellation(ct);
                }

                BuildLifetimeScope(nextScene);
            }
            catch (OperationCanceledException)
            {
                // 連打やDestroyでキャンセルされたのは正常系なので何もしない
            }
            finally
            {
                _gate.Release();
            }
        }

        private void BuildLifetimeScope(Scene scene)
        {
            var rootObjects = scene.GetRootGameObjects();

            foreach (var root in rootObjects)
            {
                var scopes = root.GetComponentsInChildren<LifetimeScope>(true);
                foreach (var scope in scopes)
                {
                    scope.Build();
                }
            }
        }

        private void OnDestroy()
        {
            _runningCts?.Cancel();
            _runningCts?.Dispose();
        }
    }
}

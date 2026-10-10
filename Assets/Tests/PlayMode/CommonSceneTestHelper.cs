using System;
using System.Collections;
using Common.Injector;
using Common.SoundManagement;
using Common.Store;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using VContainer;
using Object = UnityEngine.Object;

namespace Tests.PlayMode
{
    /// <summary>
    /// Common シーン（オプション・サウンド）を対象にしたシーンテストの共通処理
    /// </summary>
    internal static class CommonSceneTestHelper
    {
        // Addressables のロードを待つ上限
        private const float TimeoutSeconds = 10f;

        // Common シーン内の UIDocument から名前で要素を探す
        public static T FindInCommon<T>(string name) where T : VisualElement
        {
            Scene commonScene = SceneManager.GetSceneByName("Common");
            foreach (GameObject root in commonScene.GetRootGameObjects())
            {
                foreach (UIDocument doc in root.GetComponentsInChildren<UIDocument>())
                {
                    T element = doc.rootVisualElement?.Q<T>(name);
                    if (element != null)
                    {
                        return element;
                    }
                }
            }
            return null;
        }

        // オプションモーダルは Addressables のロード後に生成されるので、ExitButton が現れるまで待つ
        public static IEnumerator WaitForOptionModal()
        {
            float deadline = Time.realtimeSinceStartup + TimeoutSeconds;
            while (FindInCommon<Button>("ExitButton") == null)
            {
                if (Time.realtimeSinceStartup > deadline)
                {
                    Assert.Fail("オプションモーダルが生成されませんでした");
                }
                yield return null;
            }
        }

        // オプションアイコンのクリックでモーダルを開く（OptionPresenter は ClickEvent を購読している）
        public static void OpenOptionModal()
        {
            VisualElement icon = FindInCommon<VisualElement>("OptionSliders");
            Assert.IsNotNull(icon, "OptionSliders が見つかりません");
            using ClickEvent clickEvent = ClickEvent.GetPooled();
            clickEvent.target = icon;
            icon.SendEvent(clickEvent);
        }

        // SoundPlayer の loop == true の AudioSource が BGM
        public static AudioSource FindBgmSource()
        {
            SoundPlayer soundPlayer = Object.FindAnyObjectByType<SoundPlayer>();
            if (soundPlayer == null)
            {
                return null;
            }
            return Array.Find(soundPlayer.GetComponents<AudioSource>(), s => s.loop);
        }

        // SceneBgmPlayer は SoundStore のロード後に BGM を流すので、再生が始まるまで待つ
        public static IEnumerator WaitForBgm()
        {
            float deadline = Time.realtimeSinceStartup + TimeoutSeconds;
            while (true)
            {
                AudioSource bgm = FindBgmSource();
                if (bgm != null && bgm.clip != null && bgm.isPlaying)
                {
                    yield break;
                }
                if (Time.realtimeSinceStartup > deadline)
                {
                    Assert.Fail("BGM が再生されませんでした");
                }
                yield return null;
            }
        }

        public static SoundStore ResolveSoundStore()
        {
            CommonLifetimeScope scope = Object.FindAnyObjectByType<CommonLifetimeScope>();
            Assert.IsNotNull(scope, "CommonLifetimeScope が見つかりません");
            return scope.Container.Resolve<SoundStore>();
        }
    }
}

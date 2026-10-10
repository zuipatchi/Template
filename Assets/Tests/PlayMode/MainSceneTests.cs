using System.Collections;
using System.Reflection;
using Common.SceneManagement;
using Common.SoundManagement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Tests.PlayMode
{
    public class MainSceneTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
            // Common シーンのロード完了を待つ
            yield return new WaitUntil(() => SceneManager.GetSceneByName("Common").isLoaded);
            // VContainer スコープビルド + DI 注入完了を待つ
            yield return null;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // static フィールドをリセットして次のテストで Common シーンが再ロードされるようにする
            typeof(CommonSceneLoader)
                .GetField("_loaded", BindingFlags.NonPublic | BindingFlags.Static)
                ?.SetValue(null, false);
            yield return null;
        }

        [UnityTest]
        public IEnumerator Mainシーンではメイン曲が流れる()
        {
            yield return CommonSceneTestHelper.WaitForBgm();

            AudioClip expected = CommonSceneTestHelper.ResolveSoundStore().GetBgm(Bgm.Main);
            Assert.IsNotNull(expected, "メイン曲がロードされていません");
            Assert.AreEqual(expected, CommonSceneTestHelper.FindBgmSource().clip);
        }

        [UnityTest]
        public IEnumerator オプションの退出ボタンはゲームをやめるとして表示される()
        {
            yield return CommonSceneTestHelper.WaitForOptionModal();

            CommonSceneTestHelper.OpenOptionModal();
            yield return null;

            Button exitButton = CommonSceneTestHelper.FindInCommon<Button>("ExitButton");
            Assert.AreEqual(DisplayStyle.Flex, exitButton.resolvedStyle.display, "退出ボタンが表示されていません");
            Assert.AreEqual("ゲームをやめる", exitButton.text);
        }
    }
}

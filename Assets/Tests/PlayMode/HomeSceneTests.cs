using System.Collections;
using System.Reflection;
using Common.SceneManagement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Tests.PlayMode
{
    public class HomeSceneTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Home", LoadSceneMode.Single);
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
        public IEnumerator ゲーム開始ボタンとクレジットボタンがシーンに存在する()
        {
            Assert.IsNotNull(Find<Button>("GameStartButton"), "GameStartButton が見つかりません");
            Assert.IsNotNull(Find<Button>("CreditButton"), "CreditButton が見つかりません");
            yield return null;
        }

        [UnityTest]
        public IEnumerator クレジットオーバーレイが初期状態で非表示()
        {
            VisualElement overlay = Find<VisualElement>("CreditOverlay");
            Assert.IsNotNull(overlay, "CreditOverlay が見つかりません");
            Assert.AreEqual(DisplayStyle.None, overlay.resolvedStyle.display);
            yield return null;
        }

        [UnityTest]
        public IEnumerator クレジット項目が5件表示される()
        {
            VisualElement list = Find<VisualElement>("CreditList");
            Assert.IsNotNull(list, "CreditList が見つかりません");
            Assert.AreEqual(5, list.childCount);
            yield return null;
        }

        [UnityTest]
        public IEnumerator クレジットボタンでオーバーレイが開閉する()
        {
            VisualElement overlay = Find<VisualElement>("CreditOverlay");
            Assert.IsNotNull(overlay, "CreditOverlay が見つかりません");

            Submit(Find<Button>("CreditButton"));
            yield return null;
            Assert.AreEqual(DisplayStyle.Flex, overlay.resolvedStyle.display, "クレジットが開きません");

            Submit(Find<Button>("CloseCreditButton"));
            yield return null;
            Assert.AreEqual(DisplayStyle.None, overlay.resolvedStyle.display, "クレジットが閉じません");
        }

        [UnityTest]
        public IEnumerator ゲーム開始ボタン押下後にボタンが無効化される()
        {
            Button button = Find<Button>("GameStartButton");
            Assert.IsNotNull(button, "GameStartButton が見つかりません");

            Submit(button);
            yield return null;

            Assert.IsFalse(button.enabledSelf, "クリック後にボタンが disabled になっていません");
        }

        // NavigationSubmitEvent（Enter/Space キー相当）で Clickable を発火させる
        private static void Submit(VisualElement element)
        {
            element.Focus();
            using NavigationSubmitEvent submitEvent = NavigationSubmitEvent.GetPooled();
            element.SendEvent(submitEvent);
        }

        // Home シーン内の UIDocument から名前で要素を探す
        private static T Find<T>(string name) where T : VisualElement
        {
            Scene homeScene = SceneManager.GetSceneByName("Home");
            foreach (GameObject root in homeScene.GetRootGameObjects())
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
    }
}

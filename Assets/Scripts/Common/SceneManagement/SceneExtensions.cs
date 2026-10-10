using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Common.SceneManagement
{
    internal static class SceneExtensions
    {
        internal static void BuildLifetimeScopes(this Scene scene)
        {
            foreach (LifetimeScope scope in scene.GetComponentsInScene<LifetimeScope>())
            {
                if (scope.Container != null)
                {
                    continue;
                }
                ResolveParentReference(scope);
                scope.Build();
            }
        }

        // シーンの全ルート GameObject 配下（非アクティブ含む）から T を列挙する。
        // ルートごとに遅延で取得するため、列挙中に後続ルートの状態が変わっても元のループと同じ順序で拾う
        internal static IEnumerable<T> GetComponentsInScene<T>(this Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (T component in root.GetComponentsInChildren<T>(true))
                {
                    yield return component;
                }
            }
        }

        // ロード中シーンのスコープを誤検出しないよう、FindAnyObjectByType ではなく全シーンを直接走査する
        private static void ResolveParentReference(LifetimeScope scope)
        {
            if (scope.parentReference.Object != null)
            {
                return;
            }
            if (scope.parentReference.Type == null)
            {
                return;
            }

            Type parentType = scope.parentReference.Type;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                foreach (GameObject root in s.GetRootGameObjects())
                {
                    LifetimeScope candidate = root.GetComponentInChildren(parentType, true) as LifetimeScope;
                    if (candidate != null && candidate.Container != null)
                    {
                        scope.parentReference.Object = candidate;
                        return;
                    }
                }
            }
        }
    }
}

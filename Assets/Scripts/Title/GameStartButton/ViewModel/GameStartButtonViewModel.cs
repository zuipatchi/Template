using UnityEngine;
using UnityEngine.UIElements;

namespace Scripts.Title.GameStartButton
{

    [RequireComponent(typeof(UIDocument))]
    public class GameStartButtonViewModel : MonoBehaviour
    {
        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
        }
    }
}

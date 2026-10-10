using Cysharp.Threading.Tasks;
using UnityEngine.UIElements;

namespace Common.Store
{
    public class ModalStore : AssetStoreBase
    {
        private readonly string _modalAddressable = "Modal/OptionModal";

        public VisualTreeAsset Modal => _modal;

        private VisualTreeAsset _modal;

        protected override string AssetCategory => "モーダル";

        protected override async UniTask LoadAssetsCore()
        {
            _modal = await LoadAsync<VisualTreeAsset>(_modalAddressable);
        }
    }
}

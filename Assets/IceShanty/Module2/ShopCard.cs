using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace IceShanty
{
    public sealed class ShopCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
    {
        public ShopView shop;
        public int index;
        public Text label;
        public Image background;
        public void OnPointerEnter(PointerEventData e) => shop.Inspect(index);
        public void OnPointerExit(PointerEventData e) => shop.Inspect(shop.Selected);
        public void OnSelect(BaseEventData e) => shop.Select(index);
        public void Click() => shop.Select(index);
    }
}

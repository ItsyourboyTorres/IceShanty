using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace IceShanty
{
    public sealed class ShopView : MonoBehaviour
    {
        public StrategyManager game;
        public GameObject storefront;
        public Text dialogue, description, buyLabel;
        public Button buy;
        public Button[] tabs;
        public ShopCard[] cards;
        public int Selected { get; private set; }=-1;
        int inspected=-1, category;
        bool wasShop;
        public void Talk() { storefront.SetActive(true); dialogue.text="Take a look, small fry.\nNo refunds on fish stories."; Tab(category); }
        public void Tab(int value)
        {
            category=value; Selected=inspected=-1;
            foreach(var card in cards) card.gameObject.SetActive(false);
            Arrange(value==3?ShopCategory.Tools:(ShopCategory)value,55,130,value==3?135:160);
            Arrange(ShopCategory.Accessories,-102,100,160);
        }
        void Arrange(ShopCategory group,float y,float height,float spacing)
        {
            var sorted=cards.Where(c=>game.catalog.items[c.index].category==group).OrderBy(c=>game.catalog.items[c.index].price).ThenBy(c=>c.index).ToArray();
            for(int i=0;i<sorted.Length;i++)
            {
                var card=sorted[i]; card.gameObject.SetActive(true); var rt=card.GetComponent<RectTransform>();
                rt.anchoredPosition=new Vector2((i-(sorted.Length-1)*.5f)*spacing,y); rt.sizeDelta=new Vector2(group==ShopCategory.Tools?125:145,height);
            }
        }
        public void Inspect(int index) => inspected=index;
        public void Select(int index) { Selected=index; inspected=index; }
        public void Buy()
        {
            if(Selected<0 || !game.CanBuy(Selected)) return;
            game.Buy(Selected); dialogue.text="Good eye, small fry.\nNow make it earn its keep.";
        }
        void Update()
        {
            bool inShop=game.screens.Current==ScreenManager.Screen.Shop;
            if(inShop && !wasShop) { storefront.SetActive(false); dialogue.text="What're you looking for,\nsmall fry?"; }
            wasShop=inShop; if(!inShop || game.run.State==null) return;
            for(int i=0;i<tabs.Length;i++) tabs[i].GetComponent<Image>().color=i==category?new Color(.7f,.47f,.2f):new Color(.14f,.22f,.24f);
            foreach(var card in cards)
            {
                var item=game.catalog.items[card.index]; bool locked=game.run.State.Period<item.unlockDay;
                card.label.text=item.title+"\n"+(locked?"DAY "+item.unlockDay:game.Owned(item.upgrade)?OwnedLabel(item.upgrade):"$"+item.price);
                card.background.color=card.index==inspected?new Color(.4f,.3f,.18f):locked?new Color(.13f,.15f,.18f):new Color(.12f,.23f,.25f);
            }
            description.text=inspected<0?"Hover to inspect. Select an item to buy.":game.catalog.items[inspected].description+(game.run.State.Period<game.catalog.items[inspected].unlockDay?"\nAvailable on day "+game.catalog.items[inspected].unlockDay+" of this run.":"");
            buy.interactable=Selected>=0 && game.CanBuy(Selected);
            buyLabel.text=Selected<0?"SELECT AN ITEM":game.run.State.Period<game.catalog.items[Selected].unlockDay?"LOCKED UNTIL DAY "+game.catalog.items[Selected].unlockDay:game.Owned(game.catalog.items[Selected].upgrade)?OwnedLabel(game.catalog.items[Selected].upgrade):game.run.State.Cash<game.catalog.items[Selected].price?"NEED $"+game.catalog.items[Selected].price:"BUY / $"+game.catalog.items[Selected].price;
            if(Selected>=0 && !game.Owned(game.catalog.items[Selected].upgrade) && !game.ToolAvailable(game.catalog.items[Selected].upgrade)) buyLabel.text="BUY PREVIOUS TOOL FIRST";
        }
        string OwnedLabel(Upgrade item) => StrategyManager.ToolTier(item)>0 && StrategyManager.ToolTier(item)!=(int)game.Tool?"OWNED":"EQUIPPED";
    }
}

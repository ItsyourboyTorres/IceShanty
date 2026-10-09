using UnityEngine;
using UnityEngine.UI;
namespace IceShanty
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class ShopIcon : MaskableGraphic
    {
        public Upgrade item;
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();
            if(item>=Upgrade.IceChisel)
            { Bar(v,0,-4,5,46,0); Bar(v,0,22,32,5,0); if(item!=Upgrade.IceChisel) for(int y=-20;y<15;y+=9) Bar(v,0,y,22,3,25); return; }
            if(item==Upgrade.HeavyWinch || item==Upgrade.GhostReel)
            { Bar(v,-22,-18,43,5,48); Ring(v,-12,-8,12); Bar(v,20,10,2,24,0); }
            else if(item==Upgrade.ReinforcedLine || item==Upgrade.SilkLine)
            { Ring(v,0,0,23); Ring(v,0,0,14); Ring(v,0,0,5); Bar(v,23,-19,24,2,20); }
            else if(item==Upgrade.Chum) { Bar(v,0,-5,37,32,0); Ring(v,0,15,17); }
            else if(item==Upgrade.GlowBait) { Ring(v,0,0,18); Bar(v,-23,0,13,22,0); Ring(v,7,3,3); }
            else if(item==Upgrade.Sonar) { Bar(v,0,0,45,32,0); Ring(v,0,0,11); Bar(v,0,-23,24,4,0); }
            else if(item==Upgrade.Heater) { Bar(v,0,-2,29,42,0); Ring(v,0,22,8); }
            else { Ring(v,0,12,15); Bar(v,0,-13,20,20,0); Bar(v,0,-25,34,4,0); }
        }
        void Ring(VertexHelper v,float x,float y,float r)
        { for(int i=0;i<24;i++) { float a=i*Mathf.PI/12; Bar(v,x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r,3,r*.3f,a*Mathf.Rad2Deg); } }
        void Bar(VertexHelper v,float x,float y,float w,float h,float angle)
        {
            int start=v.currentVertCount; var q=Quaternion.Euler(0,0,angle);
            foreach(var p in new[]{new Vector3(-w/2,-h/2),new Vector3(-w/2,h/2),new Vector3(w/2,h/2),new Vector3(w/2,-h/2)})
                v.AddVert(q*p+new Vector3(x,y),color,Vector2.zero);
            v.AddTriangle(start,start+1,start+2); v.AddTriangle(start,start+2,start+3);
        }
    }
}

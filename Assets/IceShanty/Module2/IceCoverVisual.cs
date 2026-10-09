using UnityEngine;
namespace IceShanty
{
    [ExecuteAlways, RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
    public sealed class IceCoverVisual : MonoBehaviour
    {
        public StrategyManager game;
        public Transform water;
        const int Segments=96;
        Mesh mesh;
        Vector3[] vertices;
        void OnEnable()
        {
            mesh=new Mesh { name="Inward Ice Cover",hideFlags=HideFlags.DontSave };
            vertices=new Vector3[(Segments+1)*2];
            var triangles=new int[Segments*6];
            for(int i=0;i<Segments;i++) { int v=i*2,t=i*6; triangles[t]=v;triangles[t+1]=v+1;triangles[t+2]=v+2;triangles[t+3]=v+1;triangles[t+4]=v+3;triangles[t+5]=v+2; }
            mesh.vertices=vertices;mesh.triangles=triangles;GetComponent<MeshFilter>().sharedMesh=mesh;
            LateUpdate();
        }
        void LateUpdate()
        {
            if(!game || !water || !mesh) return;
            float amount=game.HoleOpen?Mathf.Clamp01(game.Ice):1;
            transform.position=water.position+Vector3.up*.025f;transform.rotation=Quaternion.identity;transform.localScale=Vector3.one;
            float radius=water.lossyScale.x*.5f;
            // Square root makes covered area increase steadily; the outer edge never moves.
            float inner=radius*Mathf.Sqrt(1-amount),outer=radius+.018f;
            for(int i=0;i<=Segments;i++)
            {
                float angle=i*(Mathf.PI*2/Segments);var direction=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
                vertices[i*2]=direction*outer;vertices[i*2+1]=direction*inner;
            }
            mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();
            GetComponent<MeshRenderer>().enabled=amount>0;
        }
        void OnDisable()
        {
            if(!mesh) return;
            if(Application.isPlaying) Destroy(mesh);else DestroyImmediate(mesh);
        }
    }
}

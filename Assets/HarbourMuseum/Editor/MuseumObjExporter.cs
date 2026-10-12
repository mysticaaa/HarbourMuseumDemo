using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace HarbourMuseum.EditorTools
{
    // Optional interchange copies: the prefabs and saved Unity meshes remain the editable masters.
    public static class MuseumObjExporter
    {
        public static void Export(GameObject root,string name)
        {
            string folder=Path.Combine(Application.dataPath,"HarbourMuseum/Models");Directory.CreateDirectory(folder);
            var obj=new StringBuilder("# Original Harbour Museum teaching model\n# Units: metres. Interpretive reconstruction.\nmtllib "+name+".mtl\n");
            var materials=new Dictionary<string,Material>();int offset=1;
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                var mesh=filter.sharedMesh;var renderer=filter.GetComponent<MeshRenderer>();
                if(mesh==null || renderer==null || filter.GetComponent<TextMesh>()!=null)continue;
                AppendMesh(obj,mesh,filter.transform,root.transform,renderer.sharedMaterials,filter.name,materials,ref offset);
            }
            foreach(var line in root.GetComponentsInChildren<LineRenderer>(true))
            {
                var baked=new Mesh();
                try
                {
                    line.BakeMesh(baked,Camera.main,false);
                    AppendMesh(obj,baked,line.transform,root.transform,line.sharedMaterials,line.name,materials,ref offset);
                }
                finally { Object.DestroyImmediate(baked); }
            }
            var mtl=new StringBuilder("# Material colours for original teaching models\n");
            foreach(var pair in materials)
            {
                var c=pair.Value.HasProperty("_Color")?pair.Value.color:Color.white;
                mtl.Append("newmtl ").Append(pair.Key).Append("\nKd ").Append(F(c.r)).Append(' ').Append(F(c.g)).Append(' ').Append(F(c.b)).Append("\nKs 0.22 0.22 0.22\nNs 32\nd 1\n\n");
            }
            File.WriteAllText(Path.Combine(folder,name+".obj"),obj.ToString());File.WriteAllText(Path.Combine(folder,name+".mtl"),mtl.ToString());
        }
        static void AppendMesh(StringBuilder obj,Mesh mesh,Transform transform,Transform root,Material[] mats,string name,Dictionary<string,Material> materials,ref int offset)
        {
            if(mats.Length==0)return;
            Matrix4x4 matrix=root.worldToLocalMatrix*transform.localToWorldMatrix;
            obj.Append("g ").Append(Safe(name)).Append('\n');
            foreach(var v in mesh.vertices){var p=matrix.MultiplyPoint3x4(v);obj.Append("v ").Append(F(-p.x)).Append(' ').Append(F(p.y)).Append(' ').Append(F(p.z)).Append('\n');}
            for(int s=0;s<mesh.subMeshCount;s++)
            {
                var mat=mats[Mathf.Min(s,mats.Length-1)];if(mat==null)continue;
                string materialName=Safe(mat.name);materials[materialName]=mat;obj.Append("usemtl ").Append(materialName).Append('\n');
                int[] tris=mesh.GetTriangles(s);
                for(int i=0;i<tris.Length;i+=3)obj.Append("f ").Append(offset+tris[i]).Append(' ').Append(offset+tris[i+2]).Append(' ').Append(offset+tris[i+1]).Append('\n');
            }
            offset+=mesh.vertexCount;
        }
        static string F(float v){return v.ToString("0.######",CultureInfo.InvariantCulture);}
        static string Safe(string text){return text.Replace(' ','_').Replace('/','_').Replace('|','_');}
    }
}

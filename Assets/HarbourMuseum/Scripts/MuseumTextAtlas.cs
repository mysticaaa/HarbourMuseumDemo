using UnityEngine;

namespace HarbourMuseum
{
    // Dynamic fonts may rebuild their atlas. Keep the depth-tested exhibit labels bound to it.
    [ExecuteAlways]
    public sealed class MuseumTextAtlas : MonoBehaviour
    {
        public Material labelMaterial;
        Font font;
        void OnEnable(){Font.textureRebuilt+=Rebuilt;RefreshAtlas();}
        void OnDisable(){Font.textureRebuilt-=Rebuilt;}
        void Rebuilt(Font changed){if(changed==font)Bind();}
        void Bind(){if(font!=null && labelMaterial!=null)labelMaterial.mainTexture=font.material.mainTexture;}
        public void RefreshAtlas()
        {
            if(labelMaterial==null)return;
            var labels=FindObjectsOfType<TextMesh>(true);
            foreach(var label in labels)
            {
                if(label.font==null)continue;font=label.font;
                font.RequestCharactersInTexture(label.text,label.fontSize,label.fontStyle);
            }
            Bind();
        }
    }
}

using UnityEngine;

namespace HarbourMuseum
{
    public enum MuseumAction
    {
        Ship, Helmet, TimelineWork, TimelineMigration, TimelineLife,
        InteriorSeating, InteriorInstruments, InteriorMachinery
    }

    /// <summary>Information attached to handmade exhibits and cabin points of interest.</summary>
    public sealed class MuseumTarget : MonoBehaviour
    {
        public MuseumAction action;
        public string title;
        [TextArea(2, 6)] public string description;

        private Renderer[] renderers;
        private MaterialPropertyBlock[] originals;
        private MaterialPropertyBlock highlight;
        private bool hovered;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            originals = new MaterialPropertyBlock[renderers.Length];
            highlight = new MaterialPropertyBlock();
            for (int i = 0; i < renderers.Length; i++)
            {
                originals[i] = new MaterialPropertyBlock();
                renderers[i].GetPropertyBlock(originals[i]);
            }
        }

        public void SetHovered(bool value)
        {
            if (hovered == value) return;
            hovered = value;
            if (renderers == null) return;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer item = renderers[i];
                if (item == null) continue;
                if (!value) { item.SetPropertyBlock(originals[i]); continue; }
                item.GetPropertyBlock(highlight);
                Material material = item.sharedMaterial;
                if (material != null && material.HasProperty("_Color"))
                    highlight.SetColor("_Color", Color.Lerp(material.color, new Color(1f, .9f, .62f), .32f));
                item.SetPropertyBlock(highlight);
            }
        }

        private void OnDisable() { SetHovered(false); }
    }
}

using Evolia.Model;
using Evolia.Shared;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Evolia.GameScene
{
    public class PhylogeneticTree : MaskableGraphic
    {
        [SerializeField]
        private int lineWidth = 2;

        [SerializeField]
        private Color lineColor = Color.black;

        [SerializeField]
        private int cornerOffset = 8;

        private Planet planet => GameController.planet;

        private IEnumerable<PhylogeneticLink> lifeLinks;

        public void SetLifeLinks(IEnumerable<PhylogeneticLink> lifeLinks)
        {
            this.lifeLinks = lifeLinks;

            UpdateContent();

            SetVerticesDirty();
        }

        public void OnTick()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            UpdateContent();
        }

        private void UpdateContent()
        {
            if (lifeLinks != null)
            {
                foreach (PhylogeneticLink lifeLink in lifeLinks)
                {
                    if (lifeLink != null)
                    {
                        bool found = planet.Found(lifeLink.icon.species);
                        if (lifeLink.icon.gameObject.activeSelf != found)
                        {
                            lifeLink.icon.gameObject.SetActive(found);
                            SetVerticesDirty();
                        }
                    }
                }
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (lifeLinks != null)
            {
                foreach (PhylogeneticLink lifeLink in lifeLinks)
                {
                    if (lifeLink.icon.gameObject.activeInHierarchy)
                    {
                        DrawLifeLink(vh, lifeLink);
                    }
                }
            }

        }

        private void DrawLifeLink(VertexHelper vh, PhylogeneticLink lifeLink)
        {
            foreach (PhylogeneticLink next in lifeLink.nexts)
            {
                if (next.icon.gameObject.activeInHierarchy)
                {
                    DrawLifeLink(vh, lifeLink, next);
                }
            }
        }

        private void DrawLifeLink(VertexHelper vh, PhylogeneticLink lifeLink, PhylogeneticLink next)
        {
            Vector2 start = (Vector2)lifeLink.icon.GetComponent<RectTransform>().localPosition +
                new Vector2(lifeLink.icon.GetComponent<RectTransform>().rect.max.x,0);
            Vector2 end = (Vector2)next.icon.GetComponent<RectTransform>().localPosition +
                new Vector2(next.icon.GetComponent<RectTransform>().rect.min.x, 0);

            float cornerX = end.x - cornerOffset;
            GraphicsUtils.DrawLine(vh, start, new Vector2(cornerX, start.y), lineWidth, lineColor);
            GraphicsUtils.DrawLine(vh, new Vector2(cornerX, start.y), new Vector2(cornerX, end.y), lineWidth, lineColor);
            GraphicsUtils.DrawLine(vh, new Vector2(cornerX, end.y), end, lineWidth, lineColor);
        }

    }
}
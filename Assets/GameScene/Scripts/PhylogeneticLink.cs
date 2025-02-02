using Evolia.Model;
using System.Collections.Generic;
using UnityEngine;

namespace Evolia.GameScene
{
    public class PhylogeneticLink
    {
        public readonly LifeIcon icon;

        public readonly List<PhylogeneticLink> nexts = new();

        private List<PhylogeneticLink> parents = new();

        private int tier;

        public PhylogeneticLink(LifeIcon icon)
        {
            this.icon = icon;
            this.tier = 0;
        }

        public void Connect(Dictionary<Species, PhylogeneticLink> lifeDictionary)
        {
            for (int i = 0; i < icon.species.transforms.Length; ++i)
            {
                int nspec = icon.species.transforms[i];

                if (nspec < icon.species.id)
                {
                    continue;
                }

                Add(lifeDictionary[CoL.instance.species[nspec]]);
            }

            if (icon.species.uniteTo != 0)
            {
                Add(lifeDictionary[CoL.instance.species[icon.species.uniteTo]]);
            }
        }

        private void Add(PhylogeneticLink next)
        {
            nexts.Add(next);
            nexts.Sort((a, b) => a.icon.species.id - b.icon.species.id);
            if (!next.parents.Contains(this))
            {
                next.parents.Add(this);
            }

            next.SetMinTier(tier + 1);
        }

        public void SetMinTier(int t)
        {
            if (t > tier)
            {
                tier = t;
                foreach (PhylogeneticLink next in nexts)
                {
                    next.SetMinTier(tier + 1);
                }
            }
        }

        public void SetTierX(int tierX)
        {
            icon.transform.localPosition = new Vector3(tier * tierX, 0, 0);
        }

        public void ArrangeY(int usableY)
        {
            // 1->0
            // 2->-256, 256
            // 3-> 1024 を 3 で割って、-512 から n 個目の 1024/3/2 のいち
            int usedChildCount = 0;
            int totalChildCount = TotalChildCount();

            if (totalChildCount == 0)
            {
                for (int n = 0; n < nexts.Count; ++n)
                {
                    if (nexts[n].IsPrimaryParent(this))
                    {
                        nexts[n].icon.transform.localPosition = new Vector3(nexts[n].icon.transform.localPosition.x,
                        icon.transform.localPosition.y, 0);
                        nexts[n].ArrangeY(usableY);
                    }
                }
            }
            else
            {

                int usableYPerChild = usableY / totalChildCount;

                for (int n = 0; n < nexts.Count; ++n)
                {
                    int childCount = nexts[n].TotalChildCount();

                    int ny = usableY / 2 - usedChildCount * usableYPerChild - childCount * usableYPerChild / 2;

                    if (nexts[n].IsPrimaryParent(this))
                    {
                        nexts[n].icon.transform.localPosition = new Vector3(nexts[n].icon.transform.localPosition.x,
                        icon.transform.localPosition.y + ny, 0);
                        nexts[n].ArrangeY(childCount * usableYPerChild);
                    }

                    usedChildCount += childCount;
                }
            }
        }


        public void CenterizeY(int gridY)
        {
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            int childCount = 0;
            foreach (PhylogeneticLink next in nexts)
            {
                ++childCount;
                if (next.parents.Count == 1)
                {
                    minY = Mathf.Min(minY, (int)next.icon.transform.localPosition.y);
                    maxY = Mathf.Max(maxY, (int)next.icon.transform.localPosition.y);
                }
                else
                {
                    int o = next.parents.IndexOf(this);

                    minY = Mathf.Min(minY, (int)next.icon.transform.localPosition.y - (o + 1) * gridY * 2 + gridY + next.parents.Count * gridY * 2 / 2);
                    maxY = Mathf.Max(maxY, (int)next.icon.transform.localPosition.y);
                }
            }
            if (minY != int.MaxValue)
            {
                int centerY = (minY + maxY) / 2;

                if (childCount <= 2)
                {
                    icon.transform.localPosition = new Vector3(icon.transform.localPosition.x, centerY, 0);
                }
                else
                {
                    int nearestY = centerY;
                    int nearestDy = int.MaxValue;
                    foreach (PhylogeneticLink next in nexts)
                    {
                        int dY = (int)Mathf.Abs(next.icon.transform.localPosition.y - centerY);
                        if (dY < nearestDy)
                        {
                            nearestY = (int)next.icon.transform.localPosition.y;
                            nearestDy = dY;
                        }
                    }
                    icon.transform.localPosition = new Vector3(icon.transform.localPosition.x, nearestY, 0);
                }
            }
        }

        public bool IsPrimaryParent(PhylogeneticLink parent)
        {
            return parents[0] == parent;
        }


        public int TotalChildCount()
        {
            if (nexts.Count == 0)
            {
                return 1;
            }

            int count = 0;
            foreach (PhylogeneticLink next in nexts)
            {
                if (next.IsPrimaryParent(this))
                {
                    count += next.TotalChildCount();
                }
            }

            return count == 0 ? 0 : count;
        }
    }

}
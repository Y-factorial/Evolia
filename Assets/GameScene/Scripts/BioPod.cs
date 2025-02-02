using Evolia.GameScene;
using Evolia.Model;
using Evolia.Shared;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evolia.GameScene
{

    public class BioPod : MonoBehaviour
    {
        [SerializeField]
        private UIDocument ui;

        [SerializeField]
        private LifeIcon icon;

        [SerializeField]
        private PlanetSimulator simulator;

        private Life pickedLife;

        public Life PickedLife => pickedLife;

        public void Awake()
        {
            ui.rootVisualElement.Q("bio-pod-content").RegisterCallback<GeometryChangedEvent>(e => 
                StartCoroutine(ScreenUtils.Fit2UICoroutine(icon.GetComponent<RectTransform>(), ui.rootVisualElement.Q("bio-pod-content")))
                );
        }

        public void Pick(Vector2Int focus)
        {
            simulator.AbductLife(focus, (life) =>
            {
                if (this==null)
                {
                    return;
                }

                if (life.Exists)
                {
                    pickedLife = life;

                    icon.gameObject.SetActive(true);

                    icon.SetTile(CoL.instance.species[pickedLife.species],
                        CoL.instance.species[pickedLife.species].scale,
                        pickedLife.variant, 0.9f);
                }
            });
        }

        public void Show(bool pickMode)
        {
            ui.rootVisualElement.Q("bio-pod").style.display = pickMode ? DisplayStyle.Flex : DisplayStyle.None;

            icon.gameObject.SetActive(pickMode && pickedLife.species!=0);
        }

        public void Put(Vector2Int focus)
        {
            if (pickedLife.Exists)
            {
                simulator.IntroduceLife(focus, pickedLife);
                pickedLife = new();
                Show(false);

            }
        }
    }
}

using Evolia.Model;
using System;
using UnityEngine;

namespace Evolia.SelectPlanetScene
{
    public class Planet : MonoBehaviour
    {
        private Universe.PlanetInfo info;

        public enum PlanetMode
        {
            Normal,
            Creating,
            Chosen
        }

        private PlanetMode mode;

        [SerializeField] Sprite[] planetSprites;

        [SerializeField] SpriteRenderer image;

        [SerializeField] GameObject frame;

        [SerializeField] GameObject chosenFrame;

        [SerializeField] GameObject creatingFrame;

        void Awake()
        {
            Mode = mode;
        }

        public Universe.PlanetInfo Info {
            get => info;
            set {
                info = value;

                image.sprite = planetSprites[info.iconType];
                transform.localPosition = new Vector2(info.x, info.y);  
            }
        }

        public PlanetMode Mode
        { 
            get => mode; 
            set { mode = value; 
                frame.SetActive(mode== PlanetMode.Normal); 
                chosenFrame.SetActive(mode == PlanetMode.Chosen);
                creatingFrame.SetActive(mode == PlanetMode.Creating);
            }
        }
    }

}
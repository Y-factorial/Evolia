using Evolia.CreatePlanetScene;
using Evolia.GameScene;
using Evolia.Model;
using Evolia.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evolia.SelectPlanetScene
{
    public class SelectPlanetController : MonoBehaviour
    {
        [SerializeField]
        SceneFader fader;

        [SerializeField]
        GameObject container;

        [SerializeField]
        Planet planetPrefab;

        [SerializeField]
        UIDocument ui;

        [SerializeField]
        Animator cameraAnimator;

        [SerializeField]
        MoveTo cameraSlide;

        [SerializeField]
        MoveTo cameraTarget;

        Universe universe;

        VisualElement createPopup;

        VisualElement planetPopup;

        Texture2D thumbnail;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Awake()
        {
            universe = Universe.Load();

            planetPopup = ui.rootVisualElement.Q("planet-popup");
            planetPopup.Q<Button>("start-button").clicked += OnClickStart;
            planetPopup.Q<Button>("delete-button").clicked += OnClickDelete;
            planetPopup.style.display = DisplayStyle.None;

            createPopup = ui.rootVisualElement.Q("create-popup");
            createPopup.Q<Button>("random-button").clicked += OnClickRandomName;
            createPopup.Q<Button>("ok-button").clicked += OnClickCreate;
            createPopup.Q<Button>("cancel-button").clicked += OnCancelCreate;
            createPopup.style.display = DisplayStyle.None;
        }
        public void Start()
        {
            OnOrientationChange();

            StartCoroutine(SpawnPlanets());
        }

        public void OnDestroy()
        {
            Destroy(thumbnail);
        }

        public void OnOrientationChange()
        {
            if (Screen.width > Screen.height)
            {
                cameraTarget.transform.localPosition = new Vector3(0, 0, 1);
                Camera.main.fieldOfView = 30;
            }
            else
            {
                cameraTarget.transform.localPosition = new Vector3(0, 0, 2);
                Camera.main.fieldOfView = 60;
            }
        }


        IEnumerator<object> SpawnPlanets()
        {
            yield return new WaitForSeconds(1.5f);

            for (int i = 0; i < universe.planets.Count && this!=null; i++)
            {
                SpawnPlanet(universe.planets[i]);

                yield return new WaitForSeconds(0.1f);
            }

            yield return null;
        }

        public void OnGalaxyClicked(GameObject target, Vector3 position)
        {
            if (target == container)
            {
                // 何もないところがクリックされた

                if (ChosenPlanet != null)
                {
                    // 今既に選んでいる惑星があるなら、選択解除

                    ChosenPlanet = null;

                }
                else
                {
                    // 何も選択していないなら、新しい惑星を作成

                    ChosenPlanet =  SpawnPlanet(universe.NewPlanet(position.x, position.y));

                }

            }
            else if ( target.GetComponent<Planet>() is Planet planet)
            {
                // 惑星がクリックされた

                ChosenPlanet = planet;

            }
        }

        void OnClickCreate()
        {
            if (ChosenPlanet != null)
            {
                ChosenPlanet.Mode = Planet.PlanetMode.Chosen;

                ChosenPlanet.Info.name = createPopup.Q<TextField>("name-input").value;

                universe.planets.Add(ChosenPlanet.Info);
                universe.Save();
            }

            ChosenPlanet = null;
        }

        void OnCancelCreate()
        {
            ChosenPlanet = null;
        }

        private Planet SpawnPlanet(Universe.PlanetInfo planetInfo)
        {

            Planet planet = Instantiate<Planet>(planetPrefab, container.transform, false);
            planet.Info = planetInfo;

            return planet;
        }

        Planet _chosenPlanet;


        private Planet ChosenPlanet
        {
            get => _chosenPlanet;
            set
            {
                createPopup.style.display = DisplayStyle.None;
                planetPopup.style.display = DisplayStyle.None;

                if (_chosenPlanet != null)
                {
                    if (_chosenPlanet.Mode == Planet.PlanetMode.Creating)
                    {
                        Destroy(_chosenPlanet.gameObject);
                    }
                    else
                    {
                        _chosenPlanet.Mode = Planet.PlanetMode.Normal;
                    }
                }

                _chosenPlanet = value;

                if (_chosenPlanet != null)
                {
                    if (_chosenPlanet.Info.name == null)
                    {
                        // 名前がついていない惑星なら、名前を入力するポップアップを表示

                        _chosenPlanet.Mode = Planet.PlanetMode.Creating;

                        createPopup.Q<TextField>("name-input").value = planetNames[(int)(UnityEngine.Random.value * planetNames.Length)];

                        createPopup.style.display = DisplayStyle.Flex;
                    }
                    else
                    {
                        // 名前がついている惑星なら、詳細情報を表示

                        _chosenPlanet.Mode = Planet.PlanetMode.Chosen;

                        planetPopup.Q<Label>("planet-name-label").text = _chosenPlanet.Info.name;
                        planetPopup.Q<Label>("created-at-label").text = $"{new DateTime(_chosenPlanet.Info.createdAt):yyyy-MM-dd HH:mm:ss}";
                        planetPopup.Q<Label>("last-played-at-label").text = _chosenPlanet.Info.lastPlayedAt==long.MinValue ? "N/A" : $"{new DateTime(_chosenPlanet.Info.lastPlayedAt):yyyy-MM-dd HH:mm:ss}";
                        planetPopup.Q<Label>("planet-age-label").text = _chosenPlanet.Info.age==0 ? "N/A" : GetAgeText(_chosenPlanet.Info.age);

                        if (thumbnail != null)
                        {
                            Destroy(thumbnail);
                        }
                        thumbnail = Model.Planet.LoadThumbnail(_chosenPlanet.Info.id);
                        if (thumbnail == null)
                        {
                            planetPopup.Q("thumbnail").style.display = DisplayStyle.None;
                        }
                        else
                        {
                            planetPopup.Q("thumbnail").style.display = DisplayStyle.Flex;
                            planetPopup.Q("thumbnail").style.backgroundImage = new StyleBackground(thumbnail);
                        }

                        planetPopup.style.display = DisplayStyle.Flex;
                    }
                }
            }
        }

        private string GetAgeText(long age)
        {
            long oku = age / 100000000L;
            long man = age / 10000 % 10000;
            long nen = age % 10000;
            string text = "";
            if (oku != 0)
            {
                text += $"{oku}億";
            }
            if (man != 0)
            {
                text += $"{man}万";
            }
            if (nen != 0 || text.Length == 0)
            {
                text += $"{nen}";
            }
            return $"{text}年";
        }

        private void OnClickRandomName()
        {
            createPopup.Q<TextField>("name-input").value = planetNames[(int)(UnityEngine.Random.value * planetNames.Length)];

        }

        private void OnClickStart()
        {
            planetPopup.style.display = DisplayStyle.None;

            cameraAnimator.SetTrigger("Out");

            cameraSlide.from = cameraSlide.transform.position;
            cameraSlide.target = ChosenPlanet.gameObject;
            cameraSlide.timeElapsed = 0;

            cameraTarget.from = cameraTarget.transform.position;
            cameraTarget.target = ChosenPlanet.gameObject;
            cameraTarget.timeElapsed = 0;

            if (ChosenPlanet.Info.IsSaved)
            {
                Model.Planet planet = Model.Planet.Load(ChosenPlanet.Info.id);
                if (planet.tiles != null)
                {
                    GameController.planet = planet;
                    fader.FadeScene("GameScene/GameScene", Color.white);
                }
                else
                {
                    CreatePlanetController.info = ChosenPlanet.Info;
                    fader.FadeScene("CreatePlanetScene/CreatePlanetScene", Color.white);
                }
            }
            else
            {
                CreatePlanetController.info = ChosenPlanet.Info;
                fader.FadeScene("CreatePlanetScene/CreatePlanetScene", Color.white);
            }
        }

        private void OnClickDelete()
        {
            if (ChosenPlanet != null)
            {
                if (universe.lastPlayedPlanetId == ChosenPlanet.Info.id)
                {
                    universe.lastPlayedPlanetId = -1;
                }
                ChosenPlanet.Info.Delete();
                universe.planets.Remove(ChosenPlanet.Info);
                universe.Save();

                Destroy(ChosenPlanet.gameObject);

                ChosenPlanet = null;
            }
        }

        private void OnClickExit()
        {
            QuitOnEsc.Exit();
        }


        public static string[] planetNames = new string[]{
            "Astrion",
"Zephara",
"Lumeris",
"Cynthora",
"Velinor",
"Nyxara",
"Thalora",
"Syrentha",
"Brionis",
"Aeranthe",
"Solvian",
"Caldera",
"Drayxis",
"Orythia",
"Xenara",
"Althera",
"Eryndor",
"Zolara",
"Thyxion",
"Meridia",
"Vylora",
"Crynthia",
"Terranis",
"Aurion",
"Lirathis",
"Phyraxia",
"Alstara",
"Velmor",
"Mythera",
"Sorenthia",
"Creonix",
"Valora",
"Halcion",
"Trynos",
"Thandara",
"Zyntora",
"Vorian",
"Elmyra",
"Ixalyn",
"Pyronis",
"Celthys",
"Tyrelis",
"Zethora",
"Maronix",
"Cyraxis",
"Theronil",
"Sylvra",
"Nolterra",
"Elyndor",
"Kalythos",
"Rhaylon",
"Arvion",
"Dynthera",
"Lytheron",
"Clyvora",
"Solynia",
"Ithoril",
"Valnyra",
"Crethon",
"Xyphora",
"Ventoris",
"Quelyra",
"Yllenthis",
"Draxion",
"Artheron",
"Syrentis",
"Krythara",
"Thalvion",
"Zaryllis",
"Verenthis",
"Norythia",
"Vytherion",
"Aerisca",
"Miralis",
"Myvalis",
"Cyllora",
"Crenthis",
"Selora",
"Phyronis",
"Glythara",
"Alvianis",
"Oranthil",
"Zylvion",
"Korynthis",
"Felora",
"Velyndis",
"Iridion",
"Thryllis",
"Varenthia",
"Bryxis",
"Xalora",
"Pyllon",
"Serenthos",
"Olyndris",
"Tralix",
"Zoralis",
"Elyraxis",
"Krylonis",
"Marenthos",
"Astrithia",
        };

    }

}
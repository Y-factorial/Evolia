using Evolia.Model;
using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using System.Linq;

namespace Evolia.GameScene
{
    /// <summary>
    /// 生物を検索するポップアップ
    /// </summary>
    public class SearchLifePopup : Popup
    {
        [SerializeField]
        private GameController controller;

        [SerializeField]
        private GameObject listContainer;

        // child[0] がテキスト、child[1] がアイコンの親
        [SerializeField]
        private GameObject listTemplate;

        [SerializeField]
        private GameObject notExistMessage;

        private Dictionary<LifeLayer, GameObject> list = new();

        [SerializeField]
        LifeIcon iconPrefab;

        Planet planet => GameController.planet;

        public void Awake()
        {
            foreach (LifeLayer layer in Enum.GetValues(typeof(LifeLayer)))
            {
                GameObject container = Instantiate(listTemplate, listContainer.transform);
                container.SetActive(true);

                container.GetComponentInChildren<TMP_Text>().text = layer.Label();
                list.Add(layer, container);
            }

            foreach (Species spec in CoL.instance.species)
            {
                LifeIcon icon = Instantiate(iconPrefab, list[spec.layer].transform.GetChild(1));
                icon.SetTile(spec);

                icon.OnClick.AddListener((spec) => OnClickLife(spec));
            }

        }


        public override void Show()
        {
            gameObject.SetActive(true);

            CreateContent();
        }

        public override void Hide()
        {
            gameObject.SetActive(false);
        }

        public void OnTick()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            UpdateContent();
        }

        private void CreateContent()
        {
            UpdateContent();

        }

        private void UpdateContent()
        {
            // 生物の可視性を更新
            foreach (LifeIcon icon in listContainer.GetComponentsInChildren<LifeIcon>(true))
            {
                icon.gameObject.SetActive(planet.Found(icon.species));
            }

            // 生物が存在するコンテナの表示を更新
            foreach (GameObject container in list.Values)
            {
                container.transform.GetChild(0).gameObject.SetActive(container.GetComponentsInChildren<LifeIcon>(false).Length > 0);
            }

            // 全ての生物が非表示の場合、代わりにメッセージを表示
            notExistMessage.SetActive(listContainer.GetComponentsInChildren<LifeIcon>().Length == 0);
        }

        public void OnClickClose()
        {
            Hide();
        }

        private void OnClickLife(in Species spec)
        {

            controller.FindLife(spec,()=> Hide());

        }
    }

}
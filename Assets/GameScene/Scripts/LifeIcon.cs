using Evolia.Model;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace Evolia.GameScene
{
    /// <summary>
    /// 生物アイコン
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class LifeIcon : MonoBehaviour
    {
        [SerializeField]
        private Material lifeMaterial;

        [SerializeField]
        private Material plantMaterial;

        [SerializeField]
        public UnityEvent<Species> OnClick;

        private Texture2D texture;

        public Species species;

        private RawImage image;

        public void Awake()
        {
            image = GetComponent<RawImage>();

            // クリックイベントを追加
            EventTrigger.Entry item = new EventTrigger.Entry();
            item.eventID = EventTriggerType.PointerClick;
            item.callback.AddListener((data) => OnClick?.Invoke(species));
            gameObject.AddComponent<EventTrigger>().triggers.Add(item);

        }
        public void OnDestroy()
        {
            Destroy(texture);
        }


        public void Update()
        {
            if (Mathf.Floor(Time.time) != Mathf.Floor(Time.time - Time.deltaTime))
            {
                // マスク内にいるとアニメーションしなくなるので、強制的に再表示する
                image.materialForRendering.SetFloat("_Timer", (float)(Time.timeAsDouble % 60));
            }
        }


        public void SetTile(in Species spec, float scale = 1, Variant variant = Variant.Original, float alpha = 1)
        {
            Destroy(texture);

            this.species = spec;

            if (spec.layer == LifeLayer.Plant)
            {
                image.material = plantMaterial;
                texture = new Texture2D(1, 1, GraphicsFormat.R16_UNorm, TextureCreationFlags.None);
                texture.SetPixel(0, 0, new Color((spec.palette +3) / 255.0f, 1.0f, 0, 1));
            }
            else
            {
                image.material = lifeMaterial;
                texture = new Texture2D(1, 1, GraphicsFormat.R8G8B8A8_UNorm, TextureCreationFlags.None);
                texture.SetPixel(0, 0, new Color(spec.palette / 255.0f, scale, (int)variant / 255.0f, alpha));
            }

            texture.Apply();
            image.texture = texture;

        }
    }

}
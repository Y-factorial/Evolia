using Evolia.Model;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace Evolia.GameScene
{
    /// <summary>
    /// 環境を表すアイコン
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class EnvIcon : MonoBehaviour
    {
        [SerializeField]
        Material surfaceMaterial;

        [SerializeField]
        Material overlayMaterial;

        private Texture2D texture;

        private RawImage image;

        public void Awake()
        {
            image = GetComponent<RawImage>();
        }

        public void OnDestroy()
        {
            Destroy(texture);
        }

        public void SetEnv(EnvStatisticsMode env, float value)
        {
            Destroy(texture);

            switch (env)
            {
                case EnvStatisticsMode.Temperature:
                    {
                        image.material = overlayMaterial;
                        texture = new Texture2D(1, 1, GraphicsFormat.R8G8B8A8_UNorm, TextureCreationFlags.None);
                    }
                    break;
                case EnvStatisticsMode.Humidity:
                    {
                        image.material = overlayMaterial;
                        texture = new Texture2D(1, 1, GraphicsFormat.R8G8B8A8_UNorm, TextureCreationFlags.None);
                    }
                    break;
                case EnvStatisticsMode.Elevation:
                    {
                        image.material = surfaceMaterial;
                        texture = new Texture2D(1, 1, GraphicsFormat.R16_UNorm, TextureCreationFlags.None);
                    }
                    break;
                default:
                    throw new System.NotImplementedException($"EnvStatisticsMode {env} is not implemented");
            }

            texture.SetPixel(0, 0, new Color(env.GetTile(value) / 255.0f, 1, 0, 1));
            texture.Apply();
            image.texture = texture;

        }
    }

}
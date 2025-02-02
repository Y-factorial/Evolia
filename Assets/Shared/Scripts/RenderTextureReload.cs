using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Evolia.Shared
{

    public class RenderTextureReload : MonoBehaviour
    {
        public void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                HashSet<RenderTexture> textures = new HashSet<RenderTexture>();


                foreach (RawImage image in FindObjectsByType<RawImage>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (image.texture is RenderTexture texture)
                    {
                        if (textures.Contains(texture))
                        {
                            // 初期化済み
                            continue;
                        }
                        textures.Add(texture);

                        if (!texture.IsCreated())
                        {
                            texture.Release();
                            texture.Create();
                        }
                    }
                }
            }
        }
    }

}
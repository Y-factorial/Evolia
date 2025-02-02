using Evolia.Model;
using Evolia.Shared;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Evolia.GameScene
{
    public class BGMPlayer : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        private Toast toast;

        [SerializeField]
        private AudioResource heatSound;

        [SerializeField]
        private List<AudioResource> bgms;

        [SerializeField]
        public float fadeDuration = 5.0f;

        [SerializeField]
        public float playDelay = 2.0f;

        private Planet planet => GameController.planet;

        private bool heated;

        private GeologicalTimescale currentTimescale = (GeologicalTimescale)( -1);

        private Coroutine fadeCoroutine;

        public void Update()
        {
            if (!heated && planet.atmosphere.temperature >= 1000)
            {
                // 前まで熱くなかったのに、熱くなった
                heated = true;

                // BGMを止めて
                if (fadeCoroutine != null)
                {
                    StopCoroutine(fadeCoroutine);
                    fadeCoroutine = null;
                }

                // ゴゴゴゴ音を鳴らす
                audioSource.resource = heatSound;
                audioSource.Play();
            }
            else if (heated)
            {
                // 熱い間、音量は温度の影響を受ける
                float volume = Mathf.Lerp(0, 1, (planet.atmosphere.temperature - 800) / 200);
                audioSource.volume = volume;

                // 温度が下がったのでゴゴゴゴ音を止めてBGMを再生する
                if (volume == 0)
                {
                    audioSource.Stop();
                    heated = false;

                    PlayBgm();
                }
            }
            else if (planet.timescale!=currentTimescale)
            {
                // 時代が変わったので新しいBGMを再生する
                currentTimescale = planet.timescale;

                PlayBgm();
            }

        }

        private void PlayBgm()
        {
            if (audioSource.resource == bgms[(int)currentTimescale])
            {
                // 今再生中のBGMと同じなら何もしない
                return;
            }

            if (fadeCoroutine != null)
            {
                // 既にフェード中ならそのまま続行でよい
            }
            else
            {
                fadeCoroutine = StartCoroutine(FadeBgm());
            }
        }

        private IEnumerator<object> FadeBgm()
        {
            // フェードアウト
            if (audioSource.isPlaying)
            {
                yield return AnimateUtils.Animate(1, 0, fadeDuration, (t) => t, (v) => audioSource.volume = v);
            }

            // 新しいBGMを再生
            PlayNewBgm();

            fadeCoroutine = null;
        }

        private void PlayNewBgm()
        {
            audioSource.Stop();

            audioSource.resource = bgms[(int)currentTimescale];

            audioSource.volume = 1.0f;
            audioSource.PlayDelayed(playDelay);

            toast.Show($"BGM: {audioSource.resource.name}");
        }
    }

}
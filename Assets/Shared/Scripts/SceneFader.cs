using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Evolia.Shared
{
    public class SceneFader : MonoBehaviour
    {
        public static Color color;

        public static SceneFader instance;

        [SerializeField]
        Image image;

        [SerializeField]
        AnimationCurve fadeInCurve;

        [SerializeField]
        AnimationCurve fadeOutCurve;

        [SerializeField]
        public UnityEvent OnFadeInStart;

        [SerializeField]
        public UnityEvent OnFadeInEnd;

        [SerializeField]
        public UnityEvent OnFadeOutStart;

        [SerializeField]
        public UnityEvent OnFadeOutEnd;

        [SerializeField]
        float fadeInDuration = 1.5f;

        [SerializeField]
        float fadeOutDuration = 1.5f;

        string nextScene;

        bool fadingOut;

        bool fadingIn;

        float fadeTime;

        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {
            fadingIn = true;
            fadeTime = 0;
            image.color = new Color(color.r, color.g, color.b, 1);
            OnFadeInStart?.Invoke();
        }

        // Update is called once per frame
        public void Update()
        {
            fadeTime += Time.deltaTime;

            if (fadingIn)
            {
                image.color = new Color(color.r, color.g, color.b, fadeInCurve.Evaluate(fadeTime / fadeInDuration));

                if (fadeTime >= fadeInDuration)
                {
                    this.gameObject.SetActive(false);
                    fadingIn = false;
                    OnFadeInEnd?.Invoke();
                }
            }

            if (fadingOut)
            {
                image.color = new Color(color.r, color.g, color.b, fadeOutCurve.Evaluate(fadeTime / fadeOutDuration));
                if (fadeTime >= fadeOutDuration)
                {
                    fadingOut = false;
                    OnFadeOutEnd?.Invoke();
                    SceneManager.LoadSceneAsync(nextScene);
                }
            }

        }

        public void FadeScene(string name, Color c)
        {
            this.gameObject.SetActive(true);

            nextScene = name;
            color = c;

            fadingIn = false;
            fadingOut = true;
            fadeTime = 0;
            image.color = new Color(color.r, color.g, color.b, 0);

            OnFadeOutStart?.Invoke();
        }
    }


}
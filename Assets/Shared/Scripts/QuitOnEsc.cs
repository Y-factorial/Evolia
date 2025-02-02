using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Evolia.Shared
{
    public class QuitOnEsc : MonoBehaviour
    {
        [SerializeField]
        public UnityEvent OnExit;

        public void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (OnExit != null && OnExit.GetPersistentEventCount()!=0)
                {
                    OnExit?.Invoke();
                }
                else
                {
                    Exit();
                }
            }
        }

        public static void Exit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // エディタで停止
#else
            Application.Quit(); // ビルドされたアプリケーションで終了
#endif
        }
    }

}
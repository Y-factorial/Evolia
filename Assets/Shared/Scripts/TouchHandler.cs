using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UIElements;

namespace Evolia.Shared
{
    public class TouchHandler : MonoBehaviour
    {
        public static TouchHandler instance;

        [SerializeField]
        public float tapTimeThreshold = 0.5f; // タップとドラッグの時間閾値

        [SerializeField]
        public float doubleTapTimeThreshold = 0.5f; // ダブルタップの時間閾値

        [SerializeField]
        public float longPressTimeThreshold = 1f; // ロングプレスの時間閾値

        [SerializeField]
        public float tapDistanceThreshold = 10f; // タップとドラッグの距離閾値

        public abstract class TouchFilter : MonoBehaviour
        {
            public abstract bool IsTouchAllowed(Vector2 position);
        }

        [SerializeField]
        public List<TouchFilter> touchFilters;

        [Serializable]
        public class TouchEvent : UnityEvent<Vector2> { }

        [Serializable]
        public class NthTouchEvent : UnityEvent<Vector2, int> { }

        [Serializable]
        public class DragEvent : UnityEvent<Vector2, Vector2, Vector2> { }

        [Serializable]
        public class ZoomEvent : UnityEvent<Vector2, float> { }


        [SerializeField]
        public TouchEvent onPress = new TouchEvent();

        [SerializeField]
        public TouchEvent onTap = new TouchEvent();

        [SerializeField]
        public TouchEvent onDoubleTap = new TouchEvent();

        [SerializeField]
        public NthTouchEvent onNthTap = new NthTouchEvent();

        [SerializeField]
        public TouchEvent onLongPress = new TouchEvent();

        [SerializeField]
        public DragEvent onDrag = new DragEvent();

        [SerializeField]
        public TouchEvent onDragEnd = new TouchEvent();

        [SerializeField]
        public ZoomEvent onZoom = new ZoomEvent();

        public float zoomSpeed = 0.1f;

        private float lastTapTime = 0f;  // 前回のタップ/クリックの時間
        public bool touching = false; // ドラッグ中かどうか
        public bool dragging = false; // ドラッグ中かどうか
        private Vector2 initialPosition;
        private Vector2 lastTouchPosition;    // 最後のタッチ/クリック位置
        private int tapCount = 0;   // タップ/クリック回数

        public void Awake()
        {
            instance = this;
        }

        public void Update()
        {
            HandleTouchInput();
            HandleMouseInput();
        }

        private void HandleTouchInput()
        {
            if (Touchscreen.current != null)
            {
                int touchCount = 0;

                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch.press.isPressed || touch.press.wasPressedThisFrame || touch.press.wasReleasedThisFrame)
                    {
                        touchCount++;
                    }
                }

                if (touchCount == 1) // シングルタッチ
                {
                    TouchControl touch = Touchscreen.current.touches[0];


                    if (touch.press.wasPressedThisFrame)
                    {
                        Began(touch.position.ReadValue());
                    }

                    if (touch.press.isPressed)
                    {
                        Moved(touch.position.ReadValue());
                    }
                    if (touch.press.wasReleasedThisFrame)
                    {
                        Ended(touch.position.ReadValue());
                    }
                }
                else if (touchCount == 2) // ピンチズーム
                {
                    TouchControl touch1 = Touchscreen.current.touches[0];
                    TouchControl touch2 = Touchscreen.current.touches[1];

                    // 現在の距離と前回の距離を計算
                    Vector2 position1 = touch1.position.ReadValue();
                    Vector2 position2 = touch2.position.ReadValue();
                    float currentDistance = Vector2.Distance(position1, position2);
                    float previousDistance = Vector2.Distance(position1 - touch1.delta.ReadValue(), position2 - touch2.delta.ReadValue());

                    Zoom((position1 + position2) / 2, currentDistance / previousDistance);
                }
            }
        }

        /// <summary>
        /// マウス入力の処理
        /// </summary>
        private void HandleMouseInput()
        {
            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    Began(Mouse.current.position.ReadValue());
                }
                if (Mouse.current.leftButton.isPressed)
                {
                    Moved(Mouse.current.position.ReadValue());
                }

                if (Mouse.current.leftButton.wasReleasedThisFrame)
                {
                    Ended(Mouse.current.position.ReadValue());
                }

                // マウスホイールでズーム
                float scrollDelta = Mouse.current.scroll.ReadValue().y * zoomSpeed;
                if (Mathf.Abs(scrollDelta) > 0)
                {
                    if (scrollDelta > 0)
                    {
                        Zoom(Mouse.current.position.ReadValue(), 1 + scrollDelta);
                    }
                    else
                    {
                        Zoom(Mouse.current.position.ReadValue(), 1.0f / (1 - scrollDelta));
                    }
                }
            }
        }


        private void Began(Vector2 position)
        {
            if ( position.x < 0 || position.x > Screen.width || position.y < 0 || position.y > Screen.height)
            {
                // 画面内になければ問題外
                return;
            }

            foreach (var touchFilter in touchFilters)
            {
                if (!touchFilter.IsTouchAllowed(position))
                {
                    // UI にかぶっているので無視
                    return;
                }
            }

            if ((Time.time - lastTapTime) >= doubleTapTimeThreshold)
            {
                // 前回タップから時間がたちすぎている、タップ回数を0に戻す
                tapCount = 0;
            }

            if (Vector2.Distance(initialPosition, position) > tapDistanceThreshold)
            {
                // 前回タップから距離が離れすぎている、タップ回数を0に戻す
                tapCount = 0;
            }

            initialPosition = position;
            lastTouchPosition = position;
            lastTapTime = Time.time;
            touching = true;
            dragging = false;
            ++tapCount;

            onPress?.Invoke(position);

            Invoke(nameof(LongPressTest), longPressTimeThreshold);
        }

        private void LongPressTest()
        {
            if (!touching)
            {
                return;
            }

            if (!dragging && (Time.time - lastTapTime >= longPressTimeThreshold))
            {
                onLongPress?.Invoke(lastTouchPosition);
            }
        }

        private void Moved(Vector2 position)
        {
            if (!touching)
            {
                return;
            }

            if (!dragging && Vector2.Distance(initialPosition, position) > tapDistanceThreshold)
            {
                dragging = true;
            }

            if (dragging && Vector2.Distance(lastTouchPosition, position) > 1.0f)
            {
                // ドラッグ中
                onDrag?.Invoke(initialPosition, lastTouchPosition, position);

                lastTouchPosition = position;
            }

        }

        private void Ended(Vector2 position)
        {
            if (!touching)
            {
                return;
            }

            if (dragging)
            {
                onDragEnd?.Invoke(initialPosition);
            }
            else if ((Time.time - lastTapTime) < tapTimeThreshold)
            {
                if (tapCount == 1)
                {
                    onTap?.Invoke(position);
                }
                else if (tapCount == 2)
                {
                    onDoubleTap?.Invoke(position);
                }

                onNthTap?.Invoke(position, tapCount);
            }

            touching = false;
            dragging = false;
        }

        /// <summary>
        /// ズーム処理
        /// </summary>
        private void Zoom(Vector2 position, float scale)
        {
            if (position.x < 0 || position.x > Screen.width || position.y < 0 || position.y > Screen.height)
            {
                // 画面内になければ問題外
                return;
            }

            foreach (var touchFilter in touchFilters)
            {
                if (!touchFilter.IsTouchAllowed(position))
                {
                    // UI にかぶっているので無視
                    return;
                }
            }

            touching = false;
            dragging = false;

            onZoom?.Invoke(position, scale);
        }

    }

}
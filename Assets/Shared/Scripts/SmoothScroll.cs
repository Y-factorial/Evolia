
using UnityEngine;
using UnityEngine.Events;

namespace Evolia.Shared
{
    /// <summary>
    /// ドラッグによる移動とピンチによる拡大縮小をスムーズに行う
    /// </summary>
    public class SmoothScroll : MonoBehaviour
    {
        [SerializeField]
        private RectTransform target;

        [SerializeField]
        private UnityEvent OnScroll;

        [SerializeField]
        private RectOffset marginRatio;

        [SerializeField]
        private float zoomMin = 0.25f;

        [SerializeField]
        private float zoomMax = 8.0f;

        [SerializeField]
        private float zoomSpeed = 10.0f;

        [SerializeField]
        private float dragWeight = 0.9f;

        [SerializeField]
        private float dragFriction = 2.0f;

        private float zoomScale = 1.0f;

        private Vector2 zoomCenterInScreen;

        private Vector2 dragVectorInScreen;

        private Vector2 dragVectorAverage;

        private bool fpsBoost = false;

        private float realDelta;

        private double prevTime;

        public void Start()
        {
            prevTime = Time.timeAsDouble;
        }

        public void Update()
        {
            realDelta = Mathf.Clamp( (float)(Time.timeAsDouble-prevTime), 0, 0.1f);
            prevTime = Time.timeAsDouble;

            if (zoomScale != 1.0f)
            {
                // 倍率が設定されているので拡大する

                // このフレームでの拡大量
                // 拡大は掛け算なので、N乗根を取ればN回で目的の倍率になる
                float pacedScale = Mathf.Pow(zoomScale, Mathf.Clamp(zoomSpeed * realDelta, 0, 1));

                // 倍率限界を考慮した拡大率
                float clampedScale = Mathf.Clamp(pacedScale, zoomMin / target.localScale.x, zoomMax / target.localScale.x);

                if (clampedScale != 1)
                {
                    // center が右にあるほど、拡大で左にずれて見えるので、右にずらす必要がある
                    // scale が 1.5 倍なら、center を右に 0.5 倍ずらす
                    Vector3 zoomCenter = ScreenUtils.ScreenToWorldPoint(target, zoomCenterInScreen);
                    Vector3 newPosition = target.position + (target.position - zoomCenter) * (clampedScale - 1);
                    MoveTo(newPosition);

                    ZoomTo(target.localScale.x * clampedScale);

                    OnScroll?.Invoke();
                }

                // 拡大したので倍率を更新しておく
                zoomScale /= pacedScale;
                if (Mathf.Abs(zoomScale - 1) <= 0.01f)
                {
                    zoomScale = 1.0f;
                }
            }

            if (dragVectorInScreen.x != 0 || dragVectorInScreen.y != 0)
            {
                // 移動量が設定されているので動かす

                // 新しい座標を計算、移動量はスクリーン座標なので、ワールド座標に変換する
                Vector3 newPosition = ScreenUtils.ScreenToWorldPoint(target, ScreenUtils.WorldToScreenPoint(target, target.position) + (Vector3)dragVectorInScreen);

                MoveTo(newPosition);
            }

            if (TouchHandler.instance.touching)
            {
                // タッチ中なら、慣性は効かない代わりに、ドラッグスピードの移動平均を覚えておく
                dragVectorAverage = dragVectorAverage * dragWeight + dragVectorInScreen * (1 - dragWeight);
                dragVectorInScreen = Vector2.zero;
            }
            else
            {
                // タッチしていないなら、慣性を効かせる
                dragVectorInScreen *= Mathf.Exp(-dragFriction * realDelta);

                if (dragVectorInScreen.sqrMagnitude <= 0.01f)
                {
                    dragVectorInScreen = Vector2.zero;
                }

            }

            if (fpsBoost && zoomScale == 1.0f && dragVectorInScreen.x == 0 && dragVectorInScreen.y == 0 && !TouchHandler.instance.touching)
            {
                LimitFPS.instance.StartOndemandRendering();
                fpsBoost = false;
            }

        }
        private void ScaleTo(float zoom)
        {
            // target.localScale *= clampedScale;
            float clampedZoom = Mathf.Clamp(zoom, zoomMin, zoomMax);
            target.localScale = new Vector3(clampedZoom, clampedZoom, 1);
        }

        private void MoveTo(Vector3 newPosition)
        {
            // 画面の端を超えないようにする
            // 画面の隅の座標を計算
            Vector2 viewportMin = new Vector2(Screen.width * marginRatio.left / 100.0f, Screen.height * marginRatio.bottom / 100.0f);
            Vector2 viewportMax = new Vector2(Screen.width * (1 - marginRatio.right / 100.0f), Screen.height * (1 - marginRatio.top / 100.0f));

            // 移動限界、画面の隅から target の大きさ分移動した場所が限界になる
            Vector3 maxPosition = ScreenUtils.ScreenToWorldPoint(this.target, viewportMin) - this.target.TransformVector((Vector3)(this.target.rect.min));
            Vector3 minPosition = ScreenUtils.ScreenToWorldPoint(this.target, viewportMax) - this.target.TransformVector((Vector3)(this.target.rect.max));

            // 限界を考慮した新しい座標
            Vector3 clampedPosition = new Vector3(
                            newPosition.x, //SafeClamp(newPosition.x, minPosition.x, maxPosition.x),
                            SafeClamp(newPosition.y, minPosition.y, maxPosition.y),
                            newPosition.z);

            if (clampedPosition != target.position)
            {
                // 移動しているので座標を更新

                target.position = clampedPosition;

                OnScroll?.Invoke();
            }

        }

        private float SafeClamp(float value, float min, float max)
        {
            if (min >= max)
            {
                return (min + max) / 2;
            }
            else
            {
                return Mathf.Clamp(value, min, max);
            }
        }

        public void OnPress(Vector2 pos)
        {
            dragVectorInScreen = new Vector2(0, 0);

            LimitFPS.instance.StopOndemandRendering();
            fpsBoost = true;
        }

        public void OnDrag(Vector2 start, Vector2 prev, Vector2 next)
        {
            dragVectorInScreen += next - prev;
        }

        public void OnDragEnd(Vector2 pos)
        {
            dragVectorInScreen = dragVectorAverage;
        }

        public void OnZoom(Vector2 pos, float scale)
        {
            zoomCenterInScreen = pos;
            zoomScale *= scale;

            LimitFPS.instance.StopOndemandRendering();
            fpsBoost = true;
        }

        public void ScrollTo(Vector3 localPosition, bool stopMoving)
        {
            MoveTo(localPosition);
            if (stopMoving)
            {
                dragVectorInScreen = Vector2.zero;
            }

            OnScroll?.Invoke();
        }

        public void ZoomTo(float zoom)
        {
            ScaleTo(zoom);

            OnScroll?.Invoke();
        }

    }

}
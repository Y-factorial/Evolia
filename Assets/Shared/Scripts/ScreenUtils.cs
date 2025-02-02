using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evolia.Shared
{
    public static class ScreenUtils
    {
        public static Vector3 LocalToScreenPoint(GameObject obj, Vector3 localPos)
        {
            return LocalToScreenPoint(obj.transform, localPos);
        }

        public static Vector3 LocalToScreenPoint(Transform obj, Vector3 localPos)
        {
            Vector3 worldPos = obj.TransformPoint(localPos);

            return WorldToScreenPoint(obj.GetComponentInParent<Canvas>(), worldPos);
        }

        public static Vector3 WorldToScreenPoint(GameObject obj, Vector3 worldPos)
        {
            return WorldToScreenPoint(obj.GetComponentInParent<Canvas>(), worldPos);
        }

        public static Vector3 WorldToScreenPoint(Transform obj, Vector3 worldPos)
        {
            return WorldToScreenPoint(obj.GetComponentInParent<Canvas>(), worldPos);
        }

        public static Vector3 WorldToScreenPoint(Canvas canvas, Vector3 worldPos)
        {
            if (canvas != null)
            {
                switch (canvas.renderMode)
                {
                    case RenderMode.ScreenSpaceOverlay:
                        return worldPos;
                    case RenderMode.ScreenSpaceCamera:
                        return canvas.worldCamera == null ? worldPos : canvas.worldCamera.WorldToScreenPoint(worldPos);
                    case RenderMode.WorldSpace:
                        return Camera.main.WorldToScreenPoint(worldPos);
                    default:
                        throw new NotImplementedException($"RenderMode {canvas.renderMode} is not implemented");
                }
            }
            else
            {
                return Camera.main.WorldToScreenPoint(worldPos);
            }
        }

        public static Vector3 ScreenToLocalPoint(GameObject obj, Vector2 screenPos)
        {
            return ScreenToLocalPoint(obj.transform, screenPos);
        }

        public static Vector3 ScreenToLocalPoint(Transform obj, Vector2 screenPos)
        {
            Vector3 world = ScreenToWorldPoint(obj.GetComponentInParent<Canvas>(), screenPos);

            return obj.InverseTransformPoint(world);
        }

        public static Vector3 ScreenToWorldPoint(GameObject obj, Vector2 screenPos)
        {
            return ScreenToWorldPoint(obj.GetComponentInParent<Canvas>(), screenPos);
        }

        public static Vector3 ScreenToWorldPoint(Transform obj, Vector2 screenPos)
        {
            return ScreenToWorldPoint(obj.GetComponentInParent<Canvas>(), screenPos);
        }

        public static Vector3 ScreenToWorldPoint(Canvas canvas, Vector2 screenPos)
        {
            Vector3 center = WorldToScreenPoint(canvas, canvas.transform.position);

            return ScreenToWorldPoint(canvas, new Vector3(screenPos.x, screenPos.y, center.z));
        }
        public static Vector3 ScreenToWorldPoint(GameObject obj, Vector3 screenPos)
        {
            return ScreenToWorldPoint(obj.GetComponentInParent<Canvas>(), screenPos);
        }

        public static Vector3 ScreenToWorldPoint(Transform obj, Vector3 screenPos)
        {
            return ScreenToWorldPoint(obj.GetComponentInParent<Canvas>(), screenPos);
        }


        private static Vector3 ScreenToWorldPoint(Canvas canvas, Vector3 screenPos)
        {
            if (canvas != null)
            {
                switch (canvas.renderMode)
                {
                    case RenderMode.ScreenSpaceOverlay:
                        return screenPos;
                    case RenderMode.ScreenSpaceCamera:
                        return canvas.worldCamera == null ? screenPos : canvas.worldCamera.ScreenToWorldPoint(screenPos);
                    case RenderMode.WorldSpace:
                        return Camera.main.ScreenToWorldPoint(screenPos);
                    default:
                        throw new NotImplementedException($"RenderMode {canvas.renderMode} is not implemented");
                }
            }
            else
            {
                return Camera.main.ScreenToWorldPoint(screenPos);
            }
        }

        public static Vector2 ScreenToUIPoint(VisualElement root, Vector2 screenPos)
        {
            // UI 座標はスクリーン座標とは違うので、変換が必要
            Rect uiBounds = root.worldBound;

            return new Vector2(
                uiBounds.width * screenPos.x / Screen.width + uiBounds.xMin,
                uiBounds.height * (Screen.height - screenPos.y) / Screen.height + uiBounds.yMin
                );
        }

        public static Vector2 UIToScreenPoint(VisualElement root, Vector2 uiPos)
        {
            // UI 座標はスクリーン座標とは違うので、変換が必要
            Rect uiBounds = root.worldBound;

            // 画面上での中心
            return new Vector2(
                uiPos.x * Screen.width / uiBounds.width + uiBounds.xMin,
                Screen.height - (uiPos.y * Screen.height / uiBounds.height + uiBounds.yMin)
                );
        }


        public static void Fit2UI(RectTransform target, VisualElement uiElement)
        {
            Rect contentRect = uiElement.LocalToWorld(uiElement.contentRect);

            // 画面上での中心
            Vector2 lt = ScreenUtils.UIToScreenPoint(uiElement.panel.visualTree, contentRect.min);
            Vector2 rb = ScreenUtils.UIToScreenPoint(uiElement.panel.visualTree, contentRect.max);

            // Canvas上での中心
            Vector2 clb = ScreenUtils.ScreenToWorldPoint(target, new Vector2(lt.x, rb.y));
            Vector2 crt = ScreenUtils.ScreenToWorldPoint(target, new Vector2(rb.x, lt.y));

            target.position = (crt - clb) * target.pivot + clb;
            Vector2 size = target.InverseTransformVector(crt - clb);
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
        }

        public static IEnumerator<object> Fit2UICoroutine(RectTransform target, VisualElement uiElement)
        {
            for (int i = 0; i < 30; ++i)
            {
                Fit2UI(target, uiElement);
                yield return null;
            }
        }

        public static void SetSize(RectTransform target, Vector2 size)
        {
            SetSize(target, size.x, size.y);
        }

        public static void SetSize(RectTransform target, float width, float height)
        {
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public static void ToggleClasses(VisualElement e, IEnumerable<string>  classes, string newClass = null)
        {
            foreach(string c in classes)
            {
                e.RemoveFromClassList(c);
            }
            if (newClass != null)
            {
                e.AddToClassList(newClass);
            }
        }
        public static void ToggleClasses(VisualElement e, Type enumType, Enum newClass = null)
        {
            foreach (string c in Enum.GetNames(enumType))
            {
                e.RemoveFromClassList(c);
            }

            if (newClass != null)
            {
                e.AddToClassList($"{newClass}");
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Evolia.Shared
{
    [RequireComponent(typeof(TouchHandler))]
    public class TouchRaycaster : MonoBehaviour
    {
        [Serializable]
        public class ObjectTouchEvent : UnityEvent<GameObject, Vector3> { }

        [Serializable]
        public class ObjectNthTouchEvent : UnityEvent<GameObject, Vector3, int> { }

        [Serializable]
        public class ObjectDragEvent : UnityEvent<GameObject, Vector3, Vector3, Vector3> { }

        [Serializable]
        public class ObjectZoomEvent : UnityEvent<GameObject, Vector3, float> { }

        [SerializeField]
        public ObjectTouchEvent onPress = new ObjectTouchEvent();

        [SerializeField]
        public ObjectTouchEvent onTap = new ObjectTouchEvent();

        [SerializeField]
        public ObjectTouchEvent onDoubleTap = new ObjectTouchEvent();

        [SerializeField]
        public ObjectNthTouchEvent onNthTap = new ObjectNthTouchEvent();

        [SerializeField]
        public ObjectTouchEvent onLongPress = new ObjectTouchEvent();

        [SerializeField]
        public ObjectDragEvent onDrag = new ObjectDragEvent();

        [SerializeField]
        public ObjectTouchEvent onDragEnd = new ObjectTouchEvent();

        [SerializeField]
        public ObjectZoomEvent onZoom = new ObjectZoomEvent();


        struct Hit
        {
            public GameObject target;
            public Vector3 point;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Awake()
        {
            TouchHandler touchHandler = GetComponent<TouchHandler>();

            touchHandler.onPress.AddListener((position) =>
            {
                if (Raycast(position, out Hit hit))
                {
                    Vector3 point = hit.target.transform.InverseTransformPoint(hit.point);

                    if (hit.target.GetComponent<ITouchTarget>() is ITouchTarget touchTarget)
                    {
                        touchTarget.OnPress(point);
                    }
                    onPress?.Invoke(hit.target, point);
                }

            });

            touchHandler.onTap.AddListener((position) =>
            {
                if (Raycast(position, out Hit hit))
                {
                    Vector3 point = hit.target.transform.InverseTransformPoint(hit.point);

                    if (hit.target.GetComponent<ITouchTarget>() is ITouchTarget touchTarget)
                    {
                        touchTarget.OnTap(point);
                    }
                    onTap?.Invoke(hit.target, point);
                }

            });

            touchHandler.onDoubleTap.AddListener((position) =>
            {
                if (Raycast(position, out Hit hit))
                {
                    Vector3 point = hit.target.transform.InverseTransformPoint(hit.point);

                    if (hit.target.GetComponent<ITouchTarget>() is ITouchTarget touchTarget)
                    {
                        touchTarget.OnDoubleTap(point);
                    }
                    onDoubleTap?.Invoke(hit.target, point);
                }
            });

            touchHandler.onNthTap.AddListener((position, count) =>
            {
                if (Raycast(position, out Hit hit))
                {
                    Vector3 point = hit.target.transform.InverseTransformPoint(hit.point);

                    if (hit.target.GetComponent<ITouchTarget>() is ITouchTarget touchTarget)
                    {
                        touchTarget.OnNthTap(point, count);
                    }
                    onNthTap?.Invoke(hit.target, point, count);
                }
            });

            touchHandler.onLongPress.AddListener((position) =>
            {
                if (Raycast(position, out Hit hit))
                {
                    Vector3 point = hit.target.transform.InverseTransformPoint(hit.point);

                    if (hit.target.GetComponent<ITouchTarget>() is ITouchTarget touchTarget)
                    {
                        touchTarget.OnLongPress(point);
                    }
                    onLongPress?.Invoke(hit.target, point);
                }
            });

            touchHandler.onDrag.AddListener((startPosition, prevPosition, endPosition) =>
            {
                if (Raycast(startPosition, out Hit startHit))
                {
                    Vector3 startPoint = startHit.target.transform.InverseTransformPoint(startHit.point);

                    Vector3 prevWorld = Camera.main.ScreenToWorldPoint(new Vector3(prevPosition.x, prevPosition.y, 1));
                    Vector3 prevPoint = startHit.target.transform.InverseTransformPoint(prevWorld);

                    Vector3 endWorld = Camera.main.ScreenToWorldPoint(new Vector3(endPosition.x, endPosition.y, 1));

                    Vector3 endPoint = startHit.target.transform.InverseTransformPoint(endWorld);

                    if (startHit.target.GetComponent<ITouchTarget>() is ITouchTarget touchTarget)
                    {
                        touchTarget.OnDrag(startPoint, prevPoint, endPoint);
                    }

                    this.onDrag?.Invoke(startHit.target, startPoint, prevPoint, endPoint);
                }
            });
            touchHandler.onDragEnd.AddListener((position) =>
            {
                if (Raycast(position, out Hit hit))
                {
                    Vector3 point = hit.target.transform.InverseTransformPoint(hit.point);

                    if (hit.target.GetComponent<ITouchTarget>() is ITouchTarget touchTarget)
                    {
                        touchTarget.OnDragEnd(point);
                    }
                    onDragEnd?.Invoke(hit.target, point);
                }
            });

            touchHandler.onZoom.AddListener((position, delta) =>
            {
                if (Raycast(position, out Hit hit))
                {
                    Vector3 point = hit.target.transform.InverseTransformPoint(hit.point);

                    if (hit.target.GetComponent<ITouchTarget>() is ITouchTarget touchTarget)
                    {
                        touchTarget.OnZoom(point, delta);
                    }
                    onZoom?.Invoke(hit.target, point, delta);
                }
            });
        }

        private bool Raycast(Vector3 position, out Hit hit)
        {
            hit = new Hit();

            Hit hit2D = Raycast2D(position);
            if (hit2D.target != null)
            {
                hit = hit2D;
            }

            Hit hit3D = Raycast3D(position);
            if (hit3D.target != null)
            {
                if (hit.target == null || hit.target.transform.position.z > hit3D.target.transform.position.z)
                {
                    hit = hit3D;
                }
            }

            Hit hitUI = RaycastUI(position);
            if (hitUI.target != null)
            {
                if (hit.target == null || hit.target.transform.position.z > hitUI.target.transform.position.z)
                {
                    hit = hitUI;
                }
            }

            return hit.target != null;
        }

        private Hit Raycast2D(Vector3 position)
        {
            RaycastHit2D[] hit2Ds = Physics2D.RaycastAll(position, Vector2.zero);

            Hit hit = new Hit();

            if (hit2Ds.Length == 0)
            {
                return hit;
            }

            hit.target = hit2Ds[0].collider.gameObject;
            hit.point = hit2Ds[0].point;

            foreach (RaycastHit2D h in hit2Ds)
            {
                Transform parent = h.collider.transform.parent;
                while (parent != null)
                {
                    if (parent == hit.target.transform)
                    {
                        hit.target = h.collider.gameObject;
                        hit.point = h.point;
                        break;
                    }
                    parent = parent.parent;
                }
            }

            return hit;
        }

        private Hit Raycast3D(Vector3 position)
        {
            RaycastHit[] hit3Ds = Physics.RaycastAll(Camera.main.ScreenPointToRay(position));


            Hit hit = new Hit();

            if (hit3Ds.Length == 0)
            {
                return hit;
            }

            hit.target = hit3Ds[0].collider.gameObject;
            hit.point = hit3Ds[0].point;

            foreach (RaycastHit h in hit3Ds)
            {
                Transform parent = h.collider.transform.parent;
                while (parent != null)
                {
                    if (parent == hit.target.transform)
                    {
                        hit.target = h.collider.gameObject;
                        hit.point = h.point;
                        break;
                    }
                    parent = parent.parent;
                }
            }

            return hit;
        }

        private Hit RaycastUI(Vector3 position)
        {
            List<RaycastResult> hitUIs = new List<RaycastResult>();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, hitUIs);

            Hit hit = new Hit();
            if (hitUIs.Count == 0)
            {
                return hit;
            }

            hit.target = hitUIs[0].gameObject;
            hit.point = hitUIs[0].worldPosition;

            foreach (RaycastResult h in hitUIs)
            {
                Transform parent = h.gameObject.transform.parent;
                while (parent != null)
                {
                    if (parent == hit.target.transform)
                    {
                        hit.target = h.gameObject;
                        hit.point = h.worldPosition;
                        break;
                    }
                    parent = parent.parent;
                }
            }

            return hit;
        }
    }


}
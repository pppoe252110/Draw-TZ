using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using static Dreamteck.Splines.ParticleController;

public class PathDrawer : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] 
    private LineRenderer _lineRenderer;
    [SerializeField] 
    private RectTransform _drawPanel;
    [SerializeField]
    private ParticleSystem _particleSystem;
    [SerializeField]
    private float distance = 1;
    private Camera _cam;
    public static UnityAction<PathDataCallback> onPathDone;

    private List<Vector3> positions = new List<Vector3>();

    private void Start()
    {
        _cam = Camera.allCameras[0];
    }

    public void OnDrag(PointerEventData eventData)
    {
        AddPosition(eventData.position);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ClearPositions();
        AddPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var corners = new Vector3[4];
        _drawPanel.GetWorldCorners(corners);

        if (!(GetPoint(corners[0], out var bMin) && GetPoint(corners[2], out var bMax)))
        {
            return;
        }
        onPathDone?.Invoke(new PathDataCallback
        {
            positions = new List<Vector3>(positions).ToArray(),
            borderMin = _cam.transform.InverseTransformPoint(bMin),
            borderMax = _cam.transform.InverseTransformPoint(bMax)
        });
        ClearPositions();
    }

    public void AddPosition(Vector3 screenSpace)
    {
        var corners = new Vector3[4];
        _drawPanel.GetWorldCorners(corners);
        screenSpace.y = Mathf.Clamp(screenSpace.y, corners[0].y, corners[1].y);
        screenSpace.x = Mathf.Clamp(screenSpace.x, corners[0].x, corners[3].x);
        if (GetPoint(screenSpace, out var point))
        {
            positions.Add(_cam.transform.InverseTransformPoint(point));
            _particleSystem.Stop();
            _particleSystem.transform.position = point;
            _particleSystem.Play();
        }
        _lineRenderer.positionCount = positions.Count;
        _lineRenderer.SetPositions(positions.ToArray());
    }

    private bool GetPoint(Vector3 screenSpace, out Vector3 point)
    {
        var camCenter = _cam.transform.position + _cam.transform.forward * distance;
        Plane plane = new Plane(camCenter, camCenter + _cam.transform.up, camCenter + _cam.transform.right);
        var ray = _cam.ScreenPointToRay(screenSpace);
        if (plane.Raycast(ray, out float enter))
        {
            var hitPoint = ray.GetPoint(enter);
            point = hitPoint;
            return true;
        }
        point = Vector3.zero;
        return false;
    }

    private void ClearPositions()
    {
        positions = new List<Vector3>();
        positions.Capacity = 128;
        _lineRenderer.positionCount = 0;
        _particleSystem.Stop();
    }
}
public class PathDataCallback
{
    public Vector3[] positions;
    public Vector2 borderMin;
    public Vector2 borderMax;
}

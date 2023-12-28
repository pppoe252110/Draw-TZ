using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UnitsController : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem _deathBoom;
    [SerializeField] 
    private SplineFollower _follower;
    [SerializeField] 
    private Unit _unitPrefab;
    [SerializeField]
    private Transform _unitsParent;
    [SerializeField]
    private Transform _leftPoint;
    [SerializeField]
    private Transform _rightPoint;
    private PathDataCallback _pathData;
    private List<Unit> _units = new List<Unit>();
    private bool _isReachedEnd = false;

    private static UnitsController instance;
    private static UnitsController Instance
    {
        get
        {
            if(instance == null)
                instance = FindAnyObjectByType<UnitsController>();
            return instance;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        PathDrawer.onPathDone += OnPathPathDone;
    }

    private void OnDisable()
    {
        PathDrawer.onPathDone -= OnPathPathDone;
    }

    public void OnPathPathDone(PathDataCallback pathData)
    {
        if (pathData != null && pathData.positions != null) 
            _pathData = pathData;
    }

    public static void AddUnit()
    {
        var pos = MultilerpFunction(new Vector3[] { Instance._leftPoint.position, Instance._rightPoint.position }, 0.5f);
        AddUnit(pos);
    }
    
    public static void KillUnit(Unit unit)
    {
        Instantiate(Instance._deathBoom, unit.transform.position, Quaternion.identity);
        Instance._units.Remove(unit);
        Instance._pathData = null;
        if (Instance._units.Count <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public static void AddUnit(Vector3 point)
    {
        var unit = Instantiate(Instance._unitPrefab);
        unit.transform.parent = Instance._unitsParent;
        unit.transform.position = point;
        unit.transform.localRotation = Quaternion.Euler(0, 180, 0);
        Instance._units.Add(unit);
        unit.SetSpeed(Instance._follower.follow ? 1 : 0);
        unit.transform.localRotation = Instance._follower.follow ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);

    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Mouse0) && !_follower.follow && !_isReachedEnd) 
        {
            _follower.follow = true;
            for (int i = 0; i < _units.Count; i++)
            {

                _units[i].SetSpeed(1);
                _units[i].transform.localRotation = Quaternion.Euler(0, 0, 0);
            }
        }
        _follower.onEndReached += EndReached;
        if (_pathData?.positions?.Length > 0)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                var deltaT = math.remap(0f, _units.Count, 0f, 1f, i);
                var pos = MultilerpFunction(_pathData?.positions, deltaT);
                pos.x = math.remap(_pathData.borderMin.x, _pathData.borderMax.x, _leftPoint.position.x, _rightPoint.position.x, pos.x);
                pos.y = math.remap(_pathData.borderMin.y, _pathData.borderMax.y, _leftPoint.position.z, _rightPoint.position.z, pos.y);
                _units[i].transform.localPosition = Vector3.MoveTowards(_units[i].transform.localPosition, _unitsParent.TransformPoint(new Vector3(pos.x, 0, pos.y)), 10f * Time.deltaTime);
                _units[i].SetSpeed(_follower.follow?1:0);
                _units[i].transform.localRotation = _follower.follow ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
            }
        }
    }

    private void EndReached()
    {
        _follower.follow = false;
        _isReachedEnd = true;
        for (int i = 0; i < _units.Count; i++)
        {
            _units[i].SetGameEnded();
        }
    }

    private void Start()
    {
        Generate();
    }

    private void Generate()
    {
        for (int i = 0; i < 10; i++)
        {
            var deltaT = math.remap(0f, 9f, 0f, 1f, i);
            var pos = MultilerpFunction(new Vector3[] { _leftPoint.position, _rightPoint.position }, deltaT);
            AddUnit(pos);
        }
    }

    public static Vector3 MultilerpFunction(Vector3[] points, float t)
    {
        if (t >= 1)
        {
            return points[points.Length - 1];
        }
        if (t <= 0)
        {
            return points[0];
        }

        int v = Mathf.FloorToInt(t * (points.Length - 1f));

        Vector3 from = points[v];
        Vector3 to = points[v + 1];

        float m = t * (points.Length - 1f) - v;

        return Vector3.Lerp(from, to, m);
    }
}

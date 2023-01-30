using System.Numerics;
using UnityEditor;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Graph : MonoBehaviour
{
    [SerializeField]
    private Transform m_pointPrefab = default;

    [SerializeField, Range(10, 100)]
    private int m_resolution = 10;

    [SerializeField]
    private FunctionLibrary.FunctionName function = default;

    public enum TransitionMode
    {
        Cycle,
        Random
    }

    [SerializeField] 
    private TransitionMode transitionMode = TransitionMode.Cycle;

    [SerializeField, Min(0f)]
    private float functionDuration = 1f, transitionDuration = 1f;

    private Transform[] m_points;

    private float duration;

    private bool transitioning;

    private FunctionLibrary.FunctionName transitionFunction;

    void Awake()
    {
        float step = 2f / m_resolution;
        var scale = Vector3.one * step;
        m_points = new Transform[m_resolution * m_resolution];
        for (int i = 0; i < m_points.Length; i++)
        {
            Transform point = Instantiate(m_pointPrefab);
            point.localScale = scale;
            point.SetParent(transform, false);
            m_points[i] = point;
        }

    }

    void Update()
    {
        duration += Time.deltaTime;
        if (transitioning)
        {
            if (duration >= transitionDuration)
            {
                duration -= transitionDuration;
                transitioning = false;
            }
        }
        else if (duration >= functionDuration)
        {
            duration -= functionDuration;
            transitioning = true;
            transitionFunction = function;
            PickNextFunction();
        }

        if (transitioning)
        {
            UpdateFunctionTransition();
        }
        else
        {
            UpdateFunction();
        }
    }

    void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle
            ? FunctionLibrary.GetNextFunctionName(function)
            : FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }

    void UpdateFunction()
    {
        FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);
        float time = Time.time;
        float step = 2f / m_resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < m_points.Length; i++, x++)
        {
            if (x == m_resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            float u = (x + 0.5f) * step - 1f;
            m_points[i].localPosition = f(u, v, time);
        }
    }

    void UpdateFunctionTransition()
    {
        FunctionLibrary.Function 
            from = FunctionLibrary.GetFunction(transitionFunction),
            to = FunctionLibrary.GetFunction(function);
        float progress = duration / transitionDuration;
        float time = Time.time;
        float step = 2f / m_resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < m_points.Length; i++, x++)
        {
            if (x == m_resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            float u = (x + 0.5f) * step - 1f;
            m_points[i].localPosition = FunctionLibrary.Morph(u, v, time, from, to, progress);
        }
    }

}

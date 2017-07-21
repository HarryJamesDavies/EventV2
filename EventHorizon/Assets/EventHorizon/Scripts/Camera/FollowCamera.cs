using UnityEngine;
using System;
using System.Collections.Generic;

public class FollowCamera : MonoBehaviour
{
    [Serializable]
    public class Target
    {
        public Target(Transform _target, int _weighting)
        {
            name = _target.name;
            target = _target;
            weighting = _weighting;
        }

        public string name;
        public Transform target;
        [Range(3, 10)]
        public float weighting;
    };

    public Transform m_permanetTarget;
    public List<Target> m_dynamicTargets = new List<Target>();

    public float m_minDistance = 20.0f;
    public float m_maxDistance = 100.0f;
    public float m_maxZoom = 150.0f;
    public float m_minZoom = 50.0f;
    public float m_deadZone = 10.0f;
    private Vector3 m_focalPoint = Vector3.zero;

    public float m_zoomSpeed = 5.0f;

    Rigidbody2D m_rigidBody;
    Vector2 m_cameraForce = Vector2.zero;

    private Camera m_camera;

    private float m_targetZoom = 0.0f;
    private float m_furthestDistance = 0.0f;

    void Start ()
    {
        transform.position = m_permanetTarget.position;
        m_rigidBody = GetComponent<Rigidbody2D>();
        m_camera = GetComponent<Camera>();
        m_targetZoom = m_camera.orthographicSize;
    }

    public void AddTarget(Transform _target, int _weighting = 2)
    {
        m_dynamicTargets.Add(new Target(_target, _weighting));
    }

    public void RemovedTarget(Transform _target)
    {
        Target target = null;
        for (int iter = 0; iter <= m_dynamicTargets.Count - 1; iter++)
        {
            if (m_dynamicTargets[iter].target == _target)
            {
                target = m_dynamicTargets[iter];
            }
        }
        m_dynamicTargets.Remove(target);
    }

    void LateUpdate()
    {
        CalculateWeightings();
        CalculateFocalPoint();
        CalculateDistance();
        CalculateForce();
    }

    void CalculateWeightings()
    {
        List<Target> remove = new List<Target>();

        for(int iter = 0; iter <= m_dynamicTargets.Count - 1; iter++)
        {
            float distance = Vector3.Distance(m_permanetTarget.position, m_dynamicTargets[iter].target.position);
            float weighting   = (Mathf.Clamp01(distance / m_maxDistance) * 10.0f);

            if(weighting < 3.0f)
            {
                weighting = 3.0f;
            }
            else if(weighting > 8.0f)
            {
                remove.Add(m_dynamicTargets[iter]);
            }

            m_dynamicTargets[iter].weighting = weighting;
        }

        foreach(Target target in remove)
        {
            m_dynamicTargets.Remove(target);
        }

        remove.Clear();
    }

    void CalculateFocalPoint()
    {
        m_furthestDistance = 0.0f;

        foreach (Target target in m_dynamicTargets)
        {
            if (target.weighting < 10.0f)
            {
                float nextDistance = Vector3.Distance(transform.position, target.target.position);

                if (nextDistance > m_furthestDistance)
                {
                    m_furthestDistance = nextDistance;
                }
            }
        }

        m_focalPoint = m_permanetTarget.position;
        foreach (Target target in m_dynamicTargets)
        {
            if (target.weighting < 10.0f)
            {
                m_focalPoint += (target.target.position - m_focalPoint) / target.weighting;
            }
        }

        float distanceFactor = 0.0f;
        if (m_furthestDistance <= m_minDistance)
        {
            distanceFactor = m_maxDistance;
        }
        else if (m_furthestDistance >= m_maxDistance)
        {
            distanceFactor = m_maxDistance;
        }
        else
        {
            distanceFactor = m_furthestDistance / m_maxDistance;
        }

        m_focalPoint = ((m_focalPoint - m_permanetTarget.position).normalized * distanceFactor) + m_permanetTarget.position;
    }

    void CalculateDistance()
    {
        if (m_furthestDistance <= m_minDistance)
        {
            m_camera.orthographicSize = m_minZoom;
        }
        else if (m_furthestDistance >= m_maxDistance)
        {
            m_camera.orthographicSize = m_maxZoom;
        }
        else
        {
            float zoomFactor = (m_furthestDistance - m_minDistance) / (m_maxDistance - m_minDistance);
            m_camera.orthographicSize = m_minZoom + ((m_maxZoom - m_minZoom) * zoomFactor);
        }
    }

    void CalculateForce()
    {
        m_cameraForce = new Vector2(transform.position.x - m_focalPoint.x, transform.position.y - m_focalPoint.y);
        if (m_cameraForce.magnitude < m_deadZone)
        {
            m_cameraForce = Vector2.zero;
        }
        else
        {
            m_cameraForce = m_cameraForce.normalized * ((m_cameraForce.magnitude - m_deadZone) / (1 - m_deadZone));
        }
    }

    void FixedUpdate()
    {
        if(Mathf.Abs(m_cameraForce.x) > 0.0f && Mathf.Abs(m_cameraForce.y) > 0.0f)
        {
            m_rigidBody.AddForce(m_cameraForce, ForceMode2D.Impulse);
        }
    }
}

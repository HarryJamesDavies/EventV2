using UnityEngine;
using System;
using System.Collections.Generic;

public class FollowCamera : MonoBehaviour
{
    [Serializable]
    public class Target
    {
        public Target(Transform _target, float _weighting)
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

    [Serializable]
    public class GhostTarget
    {
        public GhostTarget(Vector3 _target, float _weighting)
        {
            target = _target;
            weighting = _weighting;
        }

        public Vector3 target;
        [Range(3, 10)]
        public float weighting;
    };

    public Transform m_permanetTarget;
    public List<Target> m_dynamicTargets = new List<Target>();
    public List<GhostTarget> m_ghostTargets = new List<GhostTarget>();

    public float m_minDistance = 20.0f;
    public float m_maxDistance = 100.0f;
    public float m_minZoom = 50.0f;
    public float m_maxZoom = 150.0f;
    public float m_deadZone = 10.0f;
    private Vector3 m_focalPoint = Vector3.zero;

    public float m_zoomSpeed = 5.0f;
    public AnimationCurve m_zoomRate;
    public float m_maxZoomStep = 0.1f;

    public AnimationCurve m_deacayRate;
    public float m_decayThreshold = 5.0f;

    Rigidbody2D m_rigidBody;
    Vector2 m_cameraForce = Vector2.zero;

    private Camera m_camera;
    private float m_furthestDistance = 0.0f;

    void Start ()
    {
        transform.position = m_permanetTarget.position;
        m_rigidBody = GetComponent<Rigidbody2D>();
        m_camera = GetComponent<Camera>();

        EventManager.m_instance.SubscribeToEvent(Events.Event.CAM_ADDTARGET, EvFunc_AddTarget);
        EventManager.m_instance.SubscribeToEvent(Events.Event.CAM_REMOVETARGET, EvFunc_RemoveTarget);
    }

    void OnDestroy()
    {
        EventManager.m_instance.UnsubscribeToEvent(Events.Event.CAM_ADDTARGET, EvFunc_AddTarget);
        EventManager.m_instance.UnsubscribeToEvent(Events.Event.CAM_REMOVETARGET, EvFunc_RemoveTarget);
    }

    public void EvFunc_AddTarget(object _data = null)
    {
        if (_data != null)
        {
            AddCamTargetEventData data = _data as AddCamTargetEventData;
            m_dynamicTargets.Add(new Target(data.m_target, data.m_weighting));
        }
        else
        {
            Debug.LogWarning("No cam target data!");
        }
    }

    public void EvFunc_RemoveTarget(object _data = null)
    {
        if (_data != null)
        {
            RemoveCamTargetEventData data = _data as RemoveCamTargetEventData;
            Target target = null;
            for (int iter = 0; iter <= m_dynamicTargets.Count - 1; iter++)
            {
                if (m_dynamicTargets[iter].target == data.m_target)
                {
                    target = m_dynamicTargets[iter];
                    m_ghostTargets.Add(new GhostTarget(target.target.position, target.weighting));
                    break;
                }
            }
            m_dynamicTargets.Remove(target);
        }
        else
        {
            Debug.LogWarning("No cam target data!");
        }
    }

    void LateUpdate()
    {
        //CalculateWeightings();
        CalculateDistance();
        CalculateFocalPoint();
        CalculateSize();
        CalculateForce();
    }

    //void CalculateWeightings()
    //{
    //    List<Target> remove = new List<Target>();

    //    for(int iter = 0; iter <= m_dynamicTargets.Count - 1; iter++)
    //    {
    //        float distance = Vector3.Distance(m_permanetTarget.position, m_dynamicTargets[iter].target.position);
    //        float weighting   = (Mathf.Clamp01(distance / m_maxDistance) * 10.0f);

    //        if(weighting < 3.0f)
    //        {
    //            weighting = 3.0f;
    //        }
    //        else if(weighting > 8.0f)
    //        {
    //            remove.Add(m_dynamicTargets[iter]);
    //        }

    //        m_dynamicTargets[iter].weighting = weighting;
    //    }

    //    foreach(Target target in remove)
    //    {
    //        m_dynamicTargets.Remove(target);
    //    }

    //    remove.Clear();
    //}

    void CalculateDistance()
    {
        List<Target> remove = new List<Target>();
        m_furthestDistance = 0.0f;

        foreach (Target target in m_dynamicTargets)
        {
            if (target.weighting < 10.0f)
            {
                float nextDistance = Vector3.Distance(transform.position, target.target.position);

                if (nextDistance > m_maxDistance)
                {
                    remove.Add(target);
                }
                else if (nextDistance > m_furthestDistance)
                {
                    m_furthestDistance = nextDistance;
                }
            }
        }

        foreach (Target target in remove)
        {
            m_ghostTargets.Add(new GhostTarget(target.target.position, target.weighting));
            m_dynamicTargets.Remove(target);
        }

        remove.Clear();

        foreach (GhostTarget target in m_ghostTargets)
        {
            float nextDistance = Vector3.Distance(transform.position, target.target);

            if (nextDistance > m_furthestDistance)
            {
                m_furthestDistance = nextDistance;
            }
        }
    }

    void CalculateFocalPoint()
    {
        m_focalPoint = m_permanetTarget.position;
        foreach (Target target in m_dynamicTargets)
        {
            if (target.weighting < 10.0f)
            {
                m_focalPoint += (target.target.position - m_focalPoint) / target.weighting;
            }
        }

        List<GhostTarget> remove = new List<GhostTarget>();

        for (int iter = 0; iter <= m_ghostTargets.Count - 1; iter++)
        {
            Vector3 distance = m_ghostTargets[iter].target - m_permanetTarget.position;
            if (distance.magnitude  <= m_minDistance +  m_decayThreshold)
            {
                remove.Add(m_ghostTargets[iter]);
            }
            else
            {
                m_ghostTargets[iter].target = m_permanetTarget.position + (distance.normalized * Mathf.Lerp(distance.magnitude, m_minDistance, m_deacayRate.Evaluate(distance.magnitude / m_maxDistance) * Time.deltaTime));
                m_focalPoint += (m_ghostTargets[iter].target - m_permanetTarget.position) / m_ghostTargets[iter].weighting;
            }
        }

        foreach (GhostTarget target in remove)
        {
            m_ghostTargets.Remove(target);
        }

        remove.Clear();
    }

    void CalculateSize()
    {
        if (m_furthestDistance <= m_minDistance)
        {
            m_camera.orthographicSize = Mathf.Lerp(m_camera.orthographicSize, m_minZoom, m_zoomSpeed * Time.deltaTime);
        }
        else if (m_furthestDistance >= m_maxDistance)
        {
            m_camera.orthographicSize = Mathf.Lerp(m_camera.orthographicSize, m_maxZoom, m_zoomSpeed * Time.deltaTime);
        }
        else
        {
            float distanceFactor = Mathf.Abs(m_furthestDistance / m_maxDistance);
            float targetZoom = m_minZoom + ((m_maxZoom - m_minZoom) * m_zoomRate.Evaluate(distanceFactor));
            float step = (m_zoomSpeed * Time.deltaTime) * (int)(targetZoom - m_camera.orthographicSize);
            if(step > m_maxZoomStep)
            {
                step = m_maxZoomStep;
            }

            m_camera.orthographicSize += step;
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

using UnityEngine;
using System.Collections.Generic;

public class FollowObject : MonoBehaviour
{
    public Transform m_target;
    public Transform m_center;
    public float m_deadZone = 5.0f;
    public float m_moveForce = 50.0f;
    public float m_avoidForce = 50.0f;
    public float m_radius = 100.0f;
    public int m_currentSection = 0;
    public int m_targetSection = 0;
    public int m_targetPrevSection = 0;
    public int m_difference = 0;
    public Vector2 m_forceTarget = Vector2.zero;
    public bool m_useCenterForce = true;

    Rigidbody2D m_rigidBody;
    Rigidbody2D m_centerRigidbody;
    Vector2 m_force = Vector2.zero;
    Vector2 m_prevCenterVelocity = Vector2.zero;

    public List<Transform> m_tempVisuals = new List<Transform>();

    public void Initialise(Transform _target, Transform _center, bool _useCenterForce,  float _moveForce = 5.0f, float _deadZone = 10.0f, float _avoidForce = 5.0f)
    {
        m_target = _target;
        m_center = _center;
        m_useCenterForce = _useCenterForce;
        m_moveForce = _moveForce;
        m_deadZone = _deadZone;
        m_avoidForce = _avoidForce;

        transform.position = m_target.position;
        m_rigidBody = GetComponent<Rigidbody2D>();

        if(m_useCenterForce)
        {
            m_centerRigidbody = m_center.GetComponent<Rigidbody2D>();
            m_prevCenterVelocity = m_centerRigidbody.velocity;
        }

        for(int iter = 1; iter <= 8; iter++)
        {
            m_tempVisuals.Add(new GameObject("Temp" + iter).transform);
            m_tempVisuals[m_tempVisuals.Count - 1].SetParent(transform);
        }

        CalculateForceTarget();
    }

    void SetSectionPositions()
    {
        float angle = 0.0f;
        for(int iter = 0; iter <= m_tempVisuals.Count - 1; iter++)
        {
            m_tempVisuals[iter].position = new Vector2((m_center.transform.position.x + m_radius) * Mathf.Cos(angle * Mathf.Deg2Rad), (m_center.transform.position.y + m_radius) * Mathf.Sin(angle * Mathf.Deg2Rad));
            Debug.DrawLine(m_center.position, m_tempVisuals[iter].position, Color.red);
            angle += 360.0f / m_tempVisuals.Count;
        }
    }

    Vector2 CalculateForceTarget(int _index)
    {
        float angle = (360.0f / m_tempVisuals.Count) * (_index - 1);
        return new Vector2(m_center.position.x + Mathf.Cos(angle), m_center.position.y + Mathf.Sin(angle));
    }

    int CalculateCurrentSection(Vector2 _objectPoistion)
    {
        float angle = (Mathf.Atan2(_objectPoistion.x - m_center.position.x, _objectPoistion.y - m_center.position.y) * Mathf.Rad2Deg) + 180.0f;
        return Mathf.FloorToInt(angle / (360.0f / m_tempVisuals.Count)) + 1;
    }

    void CalculateForceTarget()
    {
        SetSectionPositions();
        m_currentSection = CalculateCurrentSection(transform.position);

        int tempPrev = m_targetSection;
        m_targetSection = CalculateCurrentSection(m_target.position);

        if(tempPrev != m_targetSection)
        {
            m_targetPrevSection = tempPrev;
        }

        m_difference = CalculateSectionDifference();

        //Debug.Log("Cur: " + m_currentSection + " Tar: " + m_targetSection + " Dif: " + m_difference);

        if (m_difference > 4)
        {
            m_forceTarget = CalculateForceTarget(m_currentSection - 2);
        }
        else if (m_difference < -4)
        {
            m_forceTarget = CalculateForceTarget(m_currentSection + 2);
        }
        else
        {
            m_forceTarget = m_target.position;
        }
    }

    int CalculateSectionDifference()
    {
        int difference = m_targetSection - m_currentSection;

        if (difference > (m_tempVisuals.Count / 2))
        {
            difference = (m_currentSection - (m_targetSection - m_tempVisuals.Count)) * -1;
        }
        else if (difference < -(m_tempVisuals.Count / 2))
        {
            difference = (m_currentSection - (m_targetSection + m_tempVisuals.Count)) * -1;
        }

        return difference;
    }

    void LateUpdate()
    {
        m_prevCenterVelocity = m_centerRigidbody.velocity;

        CalculateForceTarget();

        Vector2 targetForce = new Vector2(transform.position.x - m_forceTarget.x, transform.position.y - m_forceTarget.y);
        if (targetForce.magnitude < m_deadZone)
        {
            m_force = Vector2.zero;
        }
        else
        {
            m_force = (targetForce.normalized * ((targetForce.magnitude - m_deadZone) / (1 - m_deadZone))) * m_moveForce;

            //Vector2 avoidForce = new Vector2(transform.position.x - m_avoid.position.x, transform.position.y - m_avoid.position.y);
            //if (Vector2.Distance(avoidForce, transform.position) < m_minimumAvoidDistance)
            //{
            //    m_force += avoidForce.normalized * ((1.0f - (m_minimumAvoidDistance / avoidForce.magnitude)) * m_avoidForce);
            //}
        }
    }

    void FixedUpdate()
    {
        if (Mathf.Abs(m_force.x) > 0.0f && Mathf.Abs(m_force.y) > 0.0f)
        {
            m_rigidBody.AddForce(m_force, ForceMode2D.Impulse);
        }

        Vector2 centerForce = ((m_centerRigidbody.velocity - m_prevCenterVelocity) / Time.deltaTime) * m_centerRigidbody.mass;
        m_rigidBody.AddForce(centerForce);
    }
}

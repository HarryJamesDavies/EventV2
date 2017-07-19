using UnityEngine;
using System.Collections;

public class BeamManipulator : MonoBehaviour
{
    public float m_speedForce = 1.0f;
    public float m_rotationCoefficient = 1.0f;
    public float m_rotationMinimum = 10.0f;

    public float m_objectDrag = 10.0f;
    public float m_objectAngularDrag = 1.0f;
    public float m_distanceMinLimit = 10.0f;
    public float m_distanceMaxLimit = 100.0f;

    public Ship m_ship = null;
    public FollowObject m_followObject;

    private Rigidbody2D m_rigidBody;
    private Rigidbody2D m_followRigidBody;
    private Vector3 m_force = Vector3.zero;
    private float m_torque = 0.0f;

    public bool m_contracted = false;
    public float m_contractionTimeMax = 5.0f;
    [Range(0.0f, 100.0f)]
    public float m_contractCoefficient = 50.0f;
    private float m_contractedTime = 0.0f;

    private float m_prevSign = 0.0f;

    public bool m_charged = false;
    public int m_minRotations = 2;
    public int m_maxRotations = 5;
    [Range(0.0f, 100.0f)]
    public float m_chargedCoefficient = 100.0f;
    private int m_currentRotations = 0;
    private float m_chargedMomentum = 0.0f;
    private float m_startingAngle = 0.0f;
    private bool m_onCooldown = false;
    private float m_cooldownLength = 0.1f;
    private float m_cooldownStart = 0.0f;

    void Start()
    {
        m_contracted = false;

        m_rigidBody = GetComponent<Rigidbody2D>();
        m_speedForce *= m_rigidBody.drag;

        m_followObject.SetPhysics(m_objectDrag, m_objectAngularDrag);
        m_followRigidBody = m_followObject.GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        m_followObject.SetPhysics(m_objectDrag, m_objectAngularDrag);
        m_followRigidBody = m_followObject.GetComponent<Rigidbody2D>();
    }

    void OnDisable()
    {
        m_followRigidBody = null;
        m_followObject = null;
    }

    void Update()
    {
        if (m_ship.m_inputMap)
        {
            float controllerDeadzone = 0.25f;
            float distance = Vector3.Distance(m_ship.transform.position, transform.position);
            HandleDistance(controllerDeadzone, distance);
            HandleContractionLanch(distance);
            HandleRotation(controllerDeadzone);          
        }
    }

    void LateUpdate()
    {
        HandleMomentumCharging();
    }

    void HandleDistance(float _controllerDeadZone, float _distance)
    {
        if ((_distance <= m_distanceMaxLimit) && (_distance >= m_distanceMinLimit))
        {
            if ((Input.GetAxis(m_ship.m_inputMap.GetInput("RightVertical")) > _controllerDeadZone) || (Input.GetAxis(m_ship.m_inputMap.GetInput("RightVertical")) < -_controllerDeadZone))
            {
                Vector3 displacement = (m_ship.transform.position - transform.position).normalized * Input.GetAxis(m_ship.m_inputMap.GetInput("RightVertical")) * m_speedForce;
                float nextDistance = Vector3.Distance(m_ship.transform.position, transform.position + displacement);

                if ((nextDistance <= m_distanceMaxLimit) && (nextDistance >= m_distanceMinLimit))
                {
                    transform.position += displacement;
                }
            }
        }
    }

    void HandleContractionLanch(float _distance)
    {
        if(m_contracted)
        {
            if ((_distance > m_distanceMinLimit + 5.0f) || (Input.GetAxis(m_ship.m_inputMap.GetInput("RightVertical")) < 0.9f))
            {
                m_contracted = false;
                m_contractedTime = 0.0f;
                return;
            }

            m_contractedTime += Time.deltaTime;
        }
        else if((_distance <= m_distanceMinLimit + 5.0f) && (Input.GetAxis(m_ship.m_inputMap.GetInput("RightVertical")) >= 0.9f))
        {
            m_contracted = true;
            m_contractedTime = 0.0f;
            return;
        }
    }

    public void FireContractedObject()
    {
        if (m_contractedTime > m_contractionTimeMax)
        {
            m_contractedTime = m_contractionTimeMax;
        }

        Vector2 force = (m_followObject.transform.position - m_ship.transform.position).normalized * ((m_contractedTime / m_contractionTimeMax) * (m_contractCoefficient * 100.0f));
        m_followRigidBody.AddForce(force, ForceMode2D.Impulse);

        m_contracted = false;
        m_contractedTime = 0.0f;
    }

    void HandleRotation(float _controllerDeadZone)
    {
        float currentAngle = gameObject.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 currentDirection = Vector2.zero;
        currentDirection.x = Mathf.Cos(currentAngle);
        currentDirection.y = Mathf.Sin(currentAngle);

        float targetAngle = currentAngle;         

        if (Input.GetAxis(m_ship.m_inputMap.GetInput("RightHorizontal")) > _controllerDeadZone)
        {
            targetAngle -= 90.0f;
        }
        else if (Input.GetAxis(m_ship.m_inputMap.GetInput("RightHorizontal")) < -_controllerDeadZone)
        {
            targetAngle += 90.0f;
        }

        Vector2 targetDirection = Vector2.zero;
        targetDirection.x = Mathf.Cos(targetAngle);
        targetDirection.y = Mathf.Sin(targetAngle);

        Vector3 cross = Vector3.Cross(targetDirection, currentDirection);
        float sign = Mathf.Sign(cross.z);

        float rotationAngle = Vector2.Angle(targetDirection, currentDirection);
        rotationAngle *= sign;

        m_torque = 0.0f;
        if (Mathf.Abs(rotationAngle) > m_rotationMinimum)
        {
            m_torque = -sign * Mathf.Abs(rotationAngle) * m_rotationCoefficient;
        }
    }
    
    void HandleMomentumCharging()
    {
        if (m_prevSign == m_ship.m_rotationSign)
        {
            float diff = Mathf.Abs((DifferenceDegrees(m_startingAngle, m_ship.transform.eulerAngles.z, m_ship.m_rotationSign) / 360.0f));
            m_chargedMomentum = m_currentRotations + diff + 0.01f;

            if(m_onCooldown)
            {
                if(Time.time - m_cooldownStart >= m_cooldownLength)
                {
                    m_onCooldown = false;
                }
            }

            if (!m_onCooldown && ((m_chargedMomentum - m_currentRotations) >= 1.0f))
            {
                m_currentRotations++;

                m_onCooldown = true;
                m_cooldownStart = Time.time;
            }

            if (!m_charged && (m_currentRotations >= m_minRotations))
            {
                m_charged = true;
            }
        }
        else
        {
            m_charged = false;
            m_startingAngle = m_ship.transform.eulerAngles.z + (m_ship.m_rotationSign * 10.0f);
            m_currentRotations = 0;
            m_chargedMomentum = 0.0f;
        }

        m_prevSign = m_ship.m_rotationSign;

    }

    public void FireChargedObject()
    {

        if (m_currentRotations > m_maxRotations)
        {
            m_currentRotations = m_maxRotations;
        }

        Vector2 force = m_followRigidBody.velocity.normalized * (((float)m_currentRotations / (float)m_maxRotations) * (m_chargedCoefficient * 100.0f));
        m_followRigidBody.AddForce(force, ForceMode2D.Impulse);

        m_charged = false;
        m_currentRotations = 0;
    }

    float DifferenceDegrees(float _angleA, float _angleB, float _direction = 0.0f)
    {
        float difference = _angleA - _angleB;

        if (_direction != 0.0f)
        {
            if (Mathf.Sign(difference) != _direction)
            {
                if (_direction == -1.0f)
                {
                    difference = (_angleA - (_angleB + 360.0f));
                }
                else
                {
                    difference = (difference + 360.0f);
                }
            }
        }

        return difference;
    }

    void FixedUpdate()
    {
        m_rigidBody.AddForce(m_force, ForceMode2D.Impulse);
        m_followRigidBody.AddTorque(m_torque);
    }
}
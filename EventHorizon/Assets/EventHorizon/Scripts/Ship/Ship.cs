using UnityEngine;
using System.Collections;

public class Ship : Pawn
{
    public float m_speedForce = 1.0f;
    public float m_rotationForce = 1.0f;
    public float m_velocityLimit = 500.0f;
    public float m_rotationLimit = 120.0f;

    public float m_brakingCoefficient = 1.0f;

    private Vector3 m_force = Vector3.zero;
    public float m_torque = 0.0f;

    private Rigidbody2D m_rigidBody;

    public Vector2 m_forward = new Vector2(1.0f, 0.0f);

    private float m_defaultDrag = 0.1f;
    private float m_prevRotation = 0.0f;
    public bool m_rotatingClockwise = false;
    public bool m_rotatingAntiClockwise = false;

    void Start ()
    {
        EventManager.m_instance.SubscribeToEvent(Events.Event.CO_CONTROLLERADDED, EvFunc_AssignMap);

        m_rigidBody = GetComponent<Rigidbody2D>();
        m_defaultDrag = m_rigidBody.drag;
        m_prevRotation = gameObject.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;

        m_forward = new Vector2(1.0f, 0.0f);
    }

    void OnDestroy()
    {
        EventManager.m_instance.UnsubscribeToEvent(Events.Event.CO_CONTROLLERADDED, EvFunc_AssignMap);
    }
	
    void EvFunc_AssignMap(object _data = null)
    {
        if (_data != null)
        {
            InputEnableEventData data = _data as InputEnableEventData;
            m_inputType = data.ControllerType;
            SetInputMap(KeyChainManager.InputMapManager.GetPresetMap(data.ControllerType, data.ControllerName), data.ControllerIndex);
        }
        else
        {
            Debug.Log("Can't set pawn InputMap with null data");
        }
    }

	void Update ()
    {
        if (m_inputMap)
        {
            Movement();
        }
    }

    void Movement()
    {
        float deadzone = 0.25f;

        Vector2 stickInput = new Vector2(Input.GetAxis(m_inputMap.GetInput("Horizontal")), Input.GetAxis(m_inputMap.GetInput("Vertical")));
        if (stickInput.magnitude < deadzone)
        {
            stickInput = Vector2.zero;
            //m_rigidBody.angularDrag = 0.5f;
            m_rotatingClockwise = false;
            m_rotatingAntiClockwise = false;
        }
        else
        {
            //m_rigidBody.angularDrag = 1.5f;

            stickInput = stickInput.normalized * ((stickInput.magnitude - deadzone) / (1 - deadzone));

            float rotation = Mathf.Atan2(stickInput.y, stickInput.x) * -Mathf.Rad2Deg;
            float angle = gameObject.transform.eulerAngles.z;

            if ((rotation - m_prevRotation) < 0.0f)
            {
                m_rotatingClockwise = true;
                m_rotatingAntiClockwise = false;
            }
            else if ((rotation - m_prevRotation) > 0.0f)
            {
                m_rotatingClockwise = false;
                m_rotatingAntiClockwise = true;
            }
            else
            {
                m_rotatingClockwise = false;
                m_rotatingAntiClockwise = false;
            }

            Quaternion result = Quaternion.identity;
            result.eulerAngles = new Vector3(0.0f, 0.0f, rotation);
            gameObject.transform.rotation = Quaternion.Slerp(transform.rotation, result, m_rotationForce * Time.deltaTime);

            angle = gameObject.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            m_forward.x = Mathf.Cos(angle);
            m_forward.y = Mathf.Sin(angle);
            m_forward.Normalize();

            m_prevRotation = rotation;
        }

        m_force = m_speedForce * m_forward * ((Input.GetAxis(m_inputMap.GetInput("Accelerate")) + 1.0f) * 0.5f);

        m_rigidBody.drag = (((Input.GetAxis(m_inputMap.GetInput("Deccelerate")) + 1.0f) * 0.5f) * m_brakingCoefficient) + m_defaultDrag;        
    }

    void FixedUpdate()
    {
        m_rigidBody.AddForce(m_force, ForceMode2D.Impulse);
    }
}

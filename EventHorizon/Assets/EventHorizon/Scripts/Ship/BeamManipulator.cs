using UnityEngine;
using System.Collections;

public class BeamManipulator : MonoBehaviour
{
    public float m_speedForce = 10.0f;
    public float m_rotationForce = 50.0f;

    public float m_velocityLimit = 500.0f;
    public float m_distanceLimit = 40.0f;
    public Ship m_ship = null;

    private Vector2 m_forward = new Vector2(0.0f, -1.0f);
    private Rigidbody2D m_rigidBody;
    private Vector3 m_force = Vector3.zero;

    void Start()
    {
        m_rigidBody = GetComponent<Rigidbody2D>();
        m_speedForce *= GetComponent<Rigidbody2D>().drag;
    }

        void Update()
    {
        if (m_ship.m_inputMap)
        {
            float deadzone = 0.25f;
            Vector2 stickInput = new Vector2(Input.GetAxis(m_ship.m_inputMap.GetInput("RightHorizontal")), Input.GetAxis(m_ship.m_inputMap.GetInput("RightVertical")));
            if (stickInput.magnitude < deadzone)
            {
                stickInput = Vector2.zero;
                m_forward = stickInput;
            }
            else
            {
                stickInput = stickInput.normalized * ((stickInput.magnitude - deadzone) / (1 - deadzone));
                m_forward = stickInput.normalized;
                m_forward.y = -m_forward.y;
            }

            m_force = m_speedForce * m_forward;

            if(Input.GetButton(m_ship.m_inputMap.GetInput("Clockwise")))
            {
                Quaternion result = Quaternion.identity;
                result.eulerAngles = new Vector3(0.0f, 0.0f, (transform.rotation.eulerAngles.z + 90.0f));
                gameObject.transform.rotation = Quaternion.Lerp(transform.rotation, result, Time.deltaTime * (m_rotationForce / m_rigidBody.mass));
            }
            else if (Input.GetButton(m_ship.m_inputMap.GetInput("Anti-Clockwise")))
            {
                Quaternion result = Quaternion.identity;
                result.eulerAngles = new Vector3(0.0f, 0.0f, (transform.rotation.eulerAngles.z - 90.0f));
                gameObject.transform.rotation = Quaternion.Lerp(transform.rotation, result, Time.deltaTime * (m_rotationForce / m_rigidBody.mass));
            }
        }
    }

    void FixedUpdate()
    {
        m_rigidBody.AddForce(m_force, ForceMode2D.Impulse);
    }
}
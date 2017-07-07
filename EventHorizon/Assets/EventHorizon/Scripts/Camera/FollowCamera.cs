using UnityEngine;
using System.Collections;

public class FollowCamera : MonoBehaviour
{
    public Transform m_target;
    public float m_deadZone = 10.0f;

    [Range(0.0f, 10.0f)]
    public float m_drag = 0.2f;

    Rigidbody2D m_RD;
    Vector2 m_cameraForce = Vector2.zero;

    // Use this for initialization
    void Start ()
    {
        transform.position = m_target.position;
        m_RD = GetComponent<Rigidbody2D>();
	}

    // Update is called once per frame
    void Update()
    {
        m_cameraForce = new Vector2(transform.position.x - m_target.position.x, transform.position.y -  m_target.position.y);
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
            m_RD.AddForce(m_cameraForce, ForceMode2D.Impulse);
        }
    }
}

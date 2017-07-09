using UnityEngine;
using System.Collections;

public class TractorBeamProjectile : MonoBehaviour
{
    private float m_maxDistance;
    private float m_speed;
    private Transform m_origin;
    private Vector2 m_spawnPoint;
    private Vector2 m_direction;
    private Rigidbody2D m_rigidBody;
    private TractorBeamManager m_manager;
    private bool m_travelling = true;
    private bool m_returning = false;

    private TPLineRenderer m_render;

    public void Initialise(TractorBeamManager _manager, Transform _origin, Vector2 _direction, float _speed, float _maxDistance)
    {
        m_origin = _origin;
        m_spawnPoint = m_origin.position;
        m_direction = _direction.normalized;
        m_speed = _speed;
        m_maxDistance = _maxDistance;
        m_manager = _manager;

        m_render = GetComponent<TPLineRenderer>();
        Vector3[] positions = { m_origin.transform.position, transform.position };
        m_render.SetPositions(positions);
    }

    void Start()
    {
        m_rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (m_travelling)
        {
            if (m_returning)
            {
                if (Vector3.Distance(transform.position, m_origin.position) <= 5.0f)
                {
                    m_manager.BeamReturned();
                    Destroy(gameObject);
                }
            }
            else
            {
                if (Vector3.Distance(transform.position, m_spawnPoint) >= m_maxDistance)
                {
                    m_returning = true;
                }
            }

            Vector3[] positions = { m_origin.transform.position, transform.position };
            m_render.SetPositions(positions);
        }
    }

    void LateUpdate()
    {
        if (m_returning)
        {
            m_direction = (m_origin.position - transform.position).normalized;
        }
    }

    void FixedUpdate()
    {
        if (m_travelling)
        {         
            m_rigidBody.AddForce(m_direction * m_speed, ForceMode2D.Impulse);
        }
    }

    void OnCollisionEnter2D(Collision2D _collision)
    {
        if (_collision.gameObject.layer == 10)
        {
            m_travelling = false;
            m_rigidBody.velocity = Vector2.zero;
            m_rigidBody.angularVelocity = 0.0f;
            transform.position = _collision.transform.position;
            _collision.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            _collision.gameObject.GetComponent<Rigidbody2D>().angularVelocity = 0.0f;
            _collision.gameObject.layer = 11;
            gameObject.GetComponent<CircleCollider2D>().enabled = false;
            m_manager.CreateBeam(_collision.gameObject, true);
            Destroy(gameObject);
        }
    }
}

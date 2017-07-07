using UnityEngine;
using System.Collections;

public class TractorBeamProjectile : MonoBehaviour
{
    private float m_maxDistance;
    private float m_speed;
    private float m_remainingLife;
    private Vector2 m_spawnPoint;
    private Vector2 m_direction;
    private Rigidbody2D m_rigidBody;
    private TractorBeamManager m_manager;
    private bool m_travelling = true;

    public void Initialise(TractorBeamManager _manager, Vector2 _spawnPoint, Vector2 _direction, float _speed, float _remainingLife, float _maxDistance)
    {
        m_spawnPoint = _spawnPoint;
        m_direction = _direction;
        m_speed = _speed;
        m_remainingLife = _remainingLife;
        m_maxDistance = _maxDistance;
        m_manager = _manager;
    }

    void Start()
    {
        m_rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (m_travelling)
        {
            m_remainingLife -= Time.deltaTime;
            if ((Vector3.Distance(transform.position, m_spawnPoint) >= m_maxDistance) || (m_remainingLife <= 0.0f))
            {
                m_manager.m_beamActive = false;
                Destroy(gameObject);
            }
        }
    }

    void FixedUpdate()
    {
        if (m_travelling)
        {
            m_rigidBody.AddForce(m_direction.normalized * m_speed, ForceMode2D.Impulse);
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
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            m_manager.CreateBeam(_collision.gameObject, true);
            Destroy(gameObject);
        }
    }
}

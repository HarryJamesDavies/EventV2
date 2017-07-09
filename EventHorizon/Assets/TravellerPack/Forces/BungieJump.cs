using UnityEngine;
using System.Collections;

public class BungieJump : MonoBehaviour
{
    public float m_retractDistance;
    private float m_intialMagnitude;
    public float m_currentMagnitude;
    public Vector3 m_force;

    private Vector3 m_initialDirection = Vector3.zero;
    public bool m_useInitialDirection = true;

    [Range(1.0f, 100.0f)]
    public float m_decayRate;
    public bool m_decayOnRebound = true;
    public bool m_decayRelativeToCurrent = true;
    public GameObject m_anchor;

    public bool m_bungieFinished = false;
    private bool m_retracting = false;

    private Rigidbody2D m_rigidBody;

	// Use this for initialization
	void Start ()
    {
        m_rigidBody = GetComponent<Rigidbody2D>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (!m_retracting)
        {
            if (Vector3.Distance(transform.position, m_anchor.transform.position) >= m_retractDistance)
            {
                m_retracting = true;

                if(m_decayOnRebound)
                {
                    CalculateDecay();
                }
            }
        }
        else
        {
            if (Vector3.Distance(transform.position, m_anchor.transform.position) <= 0.0f)
            {
                m_retracting = false;
                CalculateDecay();
            }
        }

        CalculateForce();

        if (m_force == Vector3.zero)
        {
            m_bungieFinished = true;
        }
    }

    void FixedUpdate()
    {
        if(!m_bungieFinished)
        {
            m_rigidBody.AddForce(m_force, ForceMode2D.Impulse);
        }
    }

    void CalculateDecay()
    {
        if(m_decayRelativeToCurrent)
        {
            m_currentMagnitude -= (m_currentMagnitude / (m_decayRate / 100.0f));
        }
        else
        {
            m_currentMagnitude -= (m_intialMagnitude / (m_decayRate / 100.0f));
        }

        if(m_currentMagnitude < 0.0f)
        {
            m_currentMagnitude = 0.0f;
        }
    }

    void CalculateForce()
    {       

        if (m_useInitialDirection)
        {
            if(m_retracting)
            {
                m_force = -m_initialDirection * m_currentMagnitude;
            }
            else
            {
                m_force = m_initialDirection * m_currentMagnitude;
            }
        }
        else
        {
            Vector3 direction = Vector3.zero;

            if (m_retracting)
            {
                direction = (m_anchor.transform.position - transform.position).normalized;
            }
            else
            {
                direction = (transform.position - m_anchor.transform.position).normalized;
            }

            m_force = direction * m_currentMagnitude;
        }
    }

    public void SetBungie(GameObject _anchor, float _retractDistance, float _decayRate)
    {
        m_anchor = _anchor;
        m_retractDistance = _retractDistance;
        m_decayRate = _decayRate;
    }

    public void FireBungie(float _initialForce)
    {
        m_intialMagnitude = _initialForce;
        m_currentMagnitude = m_intialMagnitude;
        m_initialDirection = (transform.position - m_anchor.transform.position).normalized;
    }
}

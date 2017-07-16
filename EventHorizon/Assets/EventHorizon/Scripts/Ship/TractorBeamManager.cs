using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TractorBeamManager : MonoBehaviour
{
    public float m_beamSpeed = 10.0f;
    public float m_beamMaxDistance = 20.0f;
    public float m_offset = 10.0f;
    public GameObject m_manipulatorObject;
    public GameObject m_manipulatorBeam;
    public GameObject m_trailingBeam;
    public GameObject m_projectile;
    public Ship m_ship;

    private int m_currentBeam = -1;
    private List<SWChain> m_tractorBeams = new List<SWChain>();

    private bool m_beamActive = false;

    public Color m_highlightStart;
    public Color m_highlightEnd;

    private int m_currentStorageBeam = 0;
    private bool m_poolInitialised = false;

    void Update ()
    {
        if (m_ship.m_inputMap)
        {
            if (Input.GetButtonDown(m_ship.m_inputMap.GetInput("Attach")) && !m_beamActive)
            {
                m_beamActive = true;
                Vector3 forward = (m_ship.m_forward * m_offset);
                GameObject projectile = Instantiate(m_projectile, (m_ship.transform.position + forward), Quaternion.identity) as GameObject;
                projectile.GetComponent<TractorBeamProjectile>().Initialise(this, m_ship.transform, m_ship.m_forward, m_beamSpeed, m_beamMaxDistance);
            }
            else if (Input.GetButtonDown(m_ship.m_inputMap.GetInput("Unattach")))
            {
                if (m_currentBeam != -1)
                {
                    m_tractorBeams[m_currentBeam].m_target.layer = 10;
                    m_tractorBeams[m_currentBeam].ResetMass();
                    RemoveBeam(m_tractorBeams[m_currentBeam]);
                    ResetCurrentBeam();
                }
            }

            if (m_poolInitialised)
            {
                PoolingStorageBeam();
            }
            else if (Input.GetButtonDown(m_ship.m_inputMap.GetInput("Store")))
            {
                if (m_currentBeam == -1 && m_tractorBeams.Count != 0)
                {
                    if (m_tractorBeams.Count == 1)
                    {
                        UnstoreBeam(0);
                        m_currentBeam = 0;
                    }
                    else
                    {
                        if (!m_poolInitialised)
                        {
                            m_beamActive = true;
                            m_poolInitialised = true;
                            m_currentStorageBeam = 0;
                            m_tractorBeams[m_currentStorageBeam].gameObject.GetComponent<TPLineRenderer>().SetColours(m_highlightStart, m_highlightEnd);
                        }
                    }
                }
                else if (m_currentBeam != -1)
                {
                    StoreBeam(m_currentBeam);
                    ResetCurrentBeam();
                }
            }
        }
	}

    void LateUpdate()
    {
        CheckForSnappedChains();
    }

    public void BeamReturned()
    {
        m_beamActive = false;
    }

    void PoolingStorageBeam()
    {   
        if(Input.GetButtonDown(m_ship.m_inputMap.GetInput("Unstore")))
        {
            UnstoreBeam(m_currentStorageBeam);
            m_poolInitialised = false;
            return;
        }

        if (Input.GetButtonDown(m_ship.m_inputMap.GetInput("Store")))
        {
            m_tractorBeams[m_currentStorageBeam].gameObject.GetComponent<TPLineRenderer>().RevertColours();
            m_currentStorageBeam++;
            if(m_currentStorageBeam == m_tractorBeams.Count)
            {
                m_currentStorageBeam = 0;
            }
            m_tractorBeams[m_currentStorageBeam].gameObject.GetComponent<TPLineRenderer>().SetColours(m_highlightStart, m_highlightEnd);

        }
    }

    void CheckForSnappedChains()
    {
        List<int> toRemove = new List<int>();

        for (int iter = 0; iter <= m_tractorBeams.Count - 1; iter++)
        {
            if (m_tractorBeams[iter].m_meshHasSnapped)
            {
                toRemove.Add(iter);
            }
        }

        foreach (int chainIndex in toRemove)
        {
            RemoveBeam(m_tractorBeams[chainIndex]);
            if (chainIndex == m_currentBeam)
            {
                ResetCurrentBeam();
            }
        }

        toRemove.Clear();
    }

    public void ResetCurrentBeam()
    {
        m_currentBeam = -1;
        m_beamActive = false;
    }

    public void RemoveBeam(SWChain _beam)
    {
        if (_beam.m_anchor.GetComponent<SWMass>().RemoveChainID(_beam.m_meshID))
        {
            Destroy(_beam.m_anchor.GetComponent<SWMass>());
        }

        if (_beam.m_target.GetComponent<SWMass>().RemoveChainID(_beam.m_meshID))
        {
            _beam.m_target.layer = 10;
            Destroy(_beam.m_target.GetComponent<FollowObject>());
            m_manipulatorObject.GetComponent<BeamManipulator>().enabled = false;
            //Destroy(_beam.m_target.GetComponent<BeamManipulator>());
            Destroy(_beam.m_target.GetComponent<SWMass>());
        }

        Destroy(_beam.gameObject);
        m_tractorBeams.Remove(_beam);
    }

    public void CreateBeam(GameObject _target, bool _manipulate = false, float _replacementMass = 0.0f)
    {
        GameObject beam = null;
        if (_manipulate)
        {
            beam = Instantiate(m_manipulatorBeam, m_ship.transform.position, Quaternion.identity) as GameObject;
            _target.AddComponent<FollowObject>().Initialise(m_manipulatorObject.transform, m_ship.transform, true);
            m_manipulatorObject.GetComponent<BeamManipulator>().m_ship = m_ship;
            m_manipulatorObject.GetComponent<BeamManipulator>().enabled = true;
            //_target.AddComponent<BeamManipulator>().m_ship = m_ship;

            m_currentBeam = m_tractorBeams.Count;
            if(m_poolInitialised)
            {
                m_currentBeam = m_tractorBeams.Count - 1;
            }
        }
        else
        {
            beam = Instantiate(m_trailingBeam, m_ship.transform.position, Quaternion.identity) as GameObject;
        }

        if(_replacementMass != 0.0f)
        {
            _target.GetComponent<Rigidbody2D>().mass = _replacementMass;
        }

        beam.transform.SetParent(transform);
        beam.GetComponent<SWChain>().m_anchor = m_ship.gameObject;
        beam.GetComponent<SWChain>().m_target = _target;
        beam.GetComponent<SWChain>().enabled = true;
        m_tractorBeams.Add(beam.GetComponent<SWChain>());
    }

    public void StoreBeam(int _beamIndex)
    {
        m_manipulatorObject.GetComponent<BeamManipulator>().enabled = false;
        Destroy(m_tractorBeams[_beamIndex].m_target.GetComponent<FollowObject>());
        //Destroy(m_tractorBeams[_beamIndex].m_target.GetComponent<BeamManipulator>());
        CreateBeam(m_tractorBeams[_beamIndex].m_target, false, m_tractorBeams[_beamIndex].m_startingTargetMass);
        RemoveBeam(m_tractorBeams[_beamIndex]);
    }

    void UnstoreBeam(int _beamIndex)
    {
        CreateBeam(m_tractorBeams[_beamIndex].m_target, true, m_tractorBeams[_beamIndex].m_startingTargetMass);
        RemoveBeam(m_tractorBeams[_beamIndex]);
        m_beamActive = true;
    }
}

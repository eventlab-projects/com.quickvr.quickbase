using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class Gun : MonoBehaviour
{

    #region PUBLIC ATTRIBUTES

    public float _speed = 40.0f;
    public GameObject _pfBullet = null;
    public Transform _barrel = null;
    public AudioClip _audioClip;

    #endregion

    #region PROTECTED ATTRIBUTES

    public AudioSource _audioSource
    {
        get
        {
            if (!m_Auciosource)
            {
                if (!gameObject.GetComponent<AudioSource>())
                {
                    gameObject.AddComponent<AudioSource>();
                }
                m_Auciosource = gameObject.GetComponent<AudioSource>();
            }

            return m_Auciosource;
        }
    }
    protected AudioSource m_Auciosource = null;

    #endregion

    #region GET AND SET

    public virtual void Shoot()
    {
        GameObject goBullet = Instantiate(_pfBullet, _barrel.position, _barrel.rotation);
        goBullet.GetComponent<Rigidbody>().velocity = _speed * _barrel.forward;
        _audioSource.PlayOneShot(_audioClip);
        Destroy(goBullet, 2);
    }

    #endregion
    
}

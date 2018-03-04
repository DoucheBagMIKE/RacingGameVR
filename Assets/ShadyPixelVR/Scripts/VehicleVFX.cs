using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehicleVFX : MonoBehaviour {

    public ParticleSystem boostTrail;
    public GameObject speedTrailObject;
    public ParticleSystem collisionSparks;

    public float minTrailSpeed = 20f;
    float thrustMaxLifetime;

    ParticleSystem.MainModule thrustPar;
    VehicleMovement movement;
    PlayerInput input;

    // Use this for initialization
    void Start () {

        movement = GetComponent<VehicleMovement>();

        speedTrailObject.SetActive(false);
        thrustPar = boostTrail.main;
        thrustMaxLifetime = thrustPar.startLifetime.constant;
	}
	
	// Update is called once per frame
	void Update () {

        //update speed lines
        if(movement.speed >= minTrailSpeed)
        {
            speedTrailObject.SetActive(true);
        }
        else
            speedTrailObject.SetActive(false);

        //update boost trail


    }

    void UpdateBoostParticle()
    {
        float currentLifeTime = thrustMaxLifetime * input.thruster;

        if(currentLifeTime > 0)
        {
            boostTrail.Play();
            thrustPar.startLifetime = currentLifeTime;
        }
        else
        {
            boostTrail.Stop();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.layer != LayerMask.NameToLayer("Walls"))
        {
            return;
        }

        collisionSparks.transform.position = collision.contacts[0].point;
        collisionSparks.Play(true);
    }

    void OnCollisionExit(Collision collision)
    {
        collisionSparks.Stop(true);
    }
}

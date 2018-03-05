using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VehicleAVFX : MonoBehaviour {

    [Header("AVFX References")]
    [Tooltip("Boost trail particle system.")]
    public ParticleSystem boostTrail;
    [Tooltip("Speed trail object")]
    public GameObject speedTrailObject;
    [Tooltip("Collision sparks particle system.")]
    public ParticleSystem collisionSparks;
    [Tooltip("Engine audio source.")]
    public AudioSource engineAudio;

    [Header("Variables")]
    [Tooltip("minimum speed required to activate the speed trails.")]
    public float minTrailSpeed = 20f;

    [Header("Audio")]
    [Tooltip("min volume.")]
    public float minEngineVolume = 0f;
    [Tooltip("max volume.")]
    public float maxEngineVolume = 0.6f;
    [Tooltip("min pitch.")]
    public float minEnginePitch = 0.3f;
    [Tooltip("max pitch.")]
    public float maxEnginePitch = 0.8f;

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

        //Get the percentage of speed the ship is traveling
        float speedPercent = movement.GetSpeedPercentage();

        //If we have an audio source for the engine sounds...
        if (engineAudio != null)
        {
            //...modify the volume and pitch based on the speed of the ship
            engineAudio.volume = Mathf.Lerp(minEngineVolume, maxEngineVolume, speedPercent);
            engineAudio.pitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, speedPercent);
        }


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

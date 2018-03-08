using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPad : MonoBehaviour
{
    public float boostTime;
    public float terminalMod;
    public float driveForceMod;

    //make this static if you dont want boostpads to add up...
    List<VehicleMovement> trackedRacers;

    private void Start()
    {
        trackedRacers = new List<VehicleMovement>();
    }
    private void OnTriggerEnter(Collider other)
    {
        VehicleMovement v = other.transform.root.gameObject.GetComponent<VehicleMovement>();
        if (v != null && trackedRacers.Contains(v) == false)
        {
            StartCoroutine(BoostCar(boostTime, terminalMod, driveForceMod, v));
        }
    }

    IEnumerator BoostCar(float boostTime, float terminalVelocityMod, float carAccMod, VehicleMovement movement) 
    {
        trackedRacers.Add(movement);
        movement.terminalVelocity += terminalVelocityMod;
        movement.driveForce += carAccMod;

        yield return new WaitForSeconds(boostTime);

        trackedRacers.Remove(movement);
        movement.terminalVelocity -= terminalVelocityMod;
        movement.driveForce -= carAccMod;
    }
}
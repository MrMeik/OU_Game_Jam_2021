using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoilsController : MonoBehaviour
{
    public bool StartState = false;

    [SerializeField]
    private GameObject Coil1;
    [SerializeField]
    private GameObject Coil2;

    [SerializeField]
    private ParticleSystem CoilPS1;
    [SerializeField]
    private ParticleSystem CoilPS2;

    [SerializeField]
    private AudioClip onEnableClip;
    [SerializeField]
    private AudioClip onDisableClip;

    private AudioSource source;
    private bool StillStart = true;

    private BoxCollider boxCollider;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        CreateCenteredBoxCollider();
        if(Coil1.transform.localPosition.x == Coil2.transform.localPosition.x) SetupXAligned();
        else if(Coil1.transform.localPosition.z == Coil2.transform.localPosition.z) SetupZAligned();
        else throw new System.Exception("Coils not aligned");
        if (StartState is false) TurnOffGate();
        else TurnOnGate();

        StillStart = false;
    }

    private void SetupXAligned()
    {
        float distance = Vector3.Distance(Coil1.transform.localPosition, Coil2.transform.localPosition);
        boxCollider.size = new Vector3(0.25f, 2, distance);
    }

    private void SetupZAligned()
    {
        float distance = Vector3.Distance(Coil1.transform.localPosition, Coil2.transform.localPosition);
        boxCollider.size = new Vector3(distance, 2, 0.25f);
    }

    private void CreateCenteredBoxCollider()
    {
        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.center = (Coil1.transform.localPosition + Coil2.transform.localPosition) / 2f;
    }

    public void TurnOnGate()
    {
        CoilPS1.Play();
        CoilPS2.Play();
        boxCollider.enabled = true;
        if (StillStart is false) source.PlayOneShot(onEnableClip);
    }

    public void TurnOffGate()
    {
        CoilPS1.Stop();
        CoilPS2.Stop();
        boxCollider.enabled = false;
        if (StillStart is false) source.PlayOneShot(onDisableClip);
    }
}

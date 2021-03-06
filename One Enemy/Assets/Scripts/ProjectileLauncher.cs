using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    public bool FireOnStart = false;
    public bool LoopFire = false;
    public float RefireRate = 0.05f;
    public bool MegaFire = false;
    public bool SuperMegaFire = false;

    [SerializeField]
    private GameObject projectilePrefab;
    [SerializeField]
    private GameObject ejectionPoint;
    [SerializeField]
    private Collider[] sourceColliders;
    [SerializeField]
    private MovingObject movementSource;
    [SerializeField]
    private ParticleSystem extraPizzazz;

    private AudioSource source;

    private int fireId = -1;
    private bool canFireAgain = true;

    public void Start()
    {
        source = GetComponent<AudioSource>();
        if(FireOnStart) Fire();
    }

    public void HaltFire()
    {
        if (fireId != -1)
        {
            var descr = LeanTween.descr(fireId);
            if(descr == null)
            {
                canFireAgain = true;
            }
            else
            {
                float timeLeft = descr.time - descr.passed;

                if (timeLeft == 0) canFireAgain = true;
                else LeanTween.delayedCall(timeLeft, () => canFireAgain = true);
                LeanTween.cancel(fireId);
            }
            fireId = -1;
        }
    }

    public void Fire(bool checkCanFire = true)
    {
        if (checkCanFire && canFireAgain is false) return;
        HaltFire();

        source.Play();
        var newObj = Instantiate(projectilePrefab, ejectionPoint.transform.position, this.transform.rotation, BulletCollector.Instance.transform);
        var projectile = newObj.GetComponent<Projectile>();
        projectile.MegaBullet = MegaFire;
        if (sourceColliders.Length != 0) foreach (var collider in sourceColliders) projectile.IgnoreCollision(collider);
        if (SuperMegaFire && extraPizzazz != null)
        {
            extraPizzazz.Play();
            projectile.FlightSpeed *= 2;
            projectile.IgnoreShields = true;
            projectile.Damage *= 3;
        }
        else if (MegaFire) projectile.FlightSpeed = (int)(projectile.FlightSpeed * 1.5f);

        if (movementSource != null)
        {
            var sourceVelocity = movementSource.GetVelocity();
            float deltaSpeed = Vector3.Dot(transform.forward, sourceVelocity);
            if(deltaSpeed > 0) projectile.FlightSpeed += Mathf.RoundToInt(deltaSpeed);
        }

        if(LoopFire) fireId = LeanTween.delayedCall(RefireRate, () => Fire(false)).id;
        canFireAgain = false;
    }
}

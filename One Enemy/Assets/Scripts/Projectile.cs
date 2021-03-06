using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int FlightSpeed = 10;
    public int MaxLifeTime = 3;
    public int Damage = 10;
    public bool MegaBullet = false;
    public bool IgnoreShields = false;

    private Vector3 movementDirection;
    private int killId = -1;
    private Rigidbody projRB;

    private List<Collider> ignoredColliders = new List<Collider>();
    private Collider ownCollider;

    [SerializeField]
    private GameObject Visuals;
    [SerializeField]
    private ParticleSystem Trails;
    [SerializeField]
    private GameObject Explosion;

    [SerializeField]
    private AudioClip WallHit;
    [SerializeField]
    private AudioClip PlayerHit;
    [SerializeField]
    private AudioClip EnemyHit;
    [SerializeField]
    private AudioClip ShieldHit;

    private AudioSource source;

    public Vector3 MovementDirection => movementDirection;

    void Start()
    {
        movementDirection = transform.forward;
        projRB = GetComponent<Rigidbody>();
        ownCollider = GetComponent<Collider>();
        foreach (Collider col in ignoredColliders) Physics.IgnoreCollision(ownCollider, col);
        ResetKillTimer();
        source = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        projRB.MovePosition(transform.position + movementDirection * FlightSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (projRB == null)
        {
            LeanTween.cancel(killId);
            Destroy(gameObject);
            return;
        }
        var collider = collision.collider;
        if (MegaBullet && collider.CompareTag("Coil"))
        {
            collider.GetComponent<CoilsController>().TurnOffGate();
            BlowUp(AudioType.Enemy);
        }
        if (collider.CompareTag("Shield"))
        {
            if (IgnoreShields)
            {
                Physics.IgnoreCollision(ownCollider, collider);
                return;
            }
            var shield = collider.GetComponent<Shield>();
            if (shield.IsOn())
            {
                if (shield.IsEngaging())
                {
                    ClearCollisions();
                    IgnoreCollision(collider);
                    ReflectAcross(collision.GetContact(0).normal);
                    source.PlayOneShot(ShieldHit);
                    source.volume = .8f;
                    FlightSpeed += (int)(FlightSpeed * 0.5f);
                }
                else BlowUp();
            }
        }
        else if (collider.CompareTag("Player") || collider.CompareTag("Enemy"))
        {
            var hurtable = collider.GetComponent<HurtableObject>();
            hurtable.ModifyHealth(-Damage);
            if (collider.CompareTag("Player"))
            {
                BlowUp(AudioType.Player, Mathf.Lerp(3f, 5f, 1f - ((float)hurtable.CurrentHealth / hurtable.MaxHealth)));
            }
            else
            {
                BlowUp(AudioType.Enemy, Mathf.Lerp(3f, 5f, 1f - ((float)hurtable.CurrentHealth / hurtable.MaxHealth)));
                collider.GetComponent<Turret>().StopShielding();
            }
        }
        else BlowUp();
    }

    public bool IsIgnoringCollision(Collider collider) => ignoredColliders.Contains(collider);

    public void IgnoreCollision(Collider collider)
    {
        ignoredColliders.Add(collider);
        if(ownCollider != null) Physics.IgnoreCollision(ownCollider, collider);
    }

    public void ClearCollisions()
    {
        foreach (var collider in ignoredColliders)
            Physics.IgnoreCollision(ownCollider, collider, false);
        ignoredColliders.Clear();
    }

    private void ResetKillTimer()
    {
        LeanTween.cancel(killId);
        killId = LeanTween.delayedCall(MaxLifeTime, () => BlowUp()).id;
    }

    private void BlowUp(AudioType clip = AudioType.Wall, float volumeScaler = 1f)
    {
        if (projRB == null) return;
        //Play Destroy animation
        LeanTween.cancel(killId);
        projRB.velocity = Vector3.zero;
        FlightSpeed = 0;
        ownCollider.enabled = false;
        Trails.Stop();
        LeanTween.alpha(Visuals, 0f, .25f);
        Explosion.SetActive(true);
        var hitClip = GetClip(clip);
        source.volume = .2f;
        source.volume *= volumeScaler;
        if(source.enabled is true) source.PlayOneShot(hitClip);
        Destroy(gameObject, hitClip.length);
    }

    private void ReflectAcross(Vector3 normal)
    {
        ResetKillTimer();
        movementDirection = Vector3.Reflect(movementDirection, -normal);
    }

    private AudioClip GetClip(AudioType type) => type switch
    {
        AudioType.Enemy => EnemyHit,
        AudioType.Player => PlayerHit,
        _ => WallHit
    };

    private enum AudioType
    {
        Player,
        Enemy,
        Wall
    }
}

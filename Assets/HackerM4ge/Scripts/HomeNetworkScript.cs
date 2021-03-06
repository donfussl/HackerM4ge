using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeNetworkScript : MonoBehaviour
{
    public GameObject HealthManagerObject;
    private HealthManagerScript healthManager;
    private AudioSource damageSoundSource;

    private int health = 50;

    // Use this for initialization
    void Start ()
    {
        this.damageSoundSource = gameObject.GetComponent<AudioSource> ();

        this.healthManager = HealthManagerObject.GetComponent<HealthManagerScript> ();
        this.healthManager.RegisterNetwork (gameObject);
    }
	
    // Update is called once per frame
    void Update ()
    {
		
    }

    public void Damage(int damage)
    {
        this.health -= damage;
        if (this.health > 0) {
            PlayDamageSound ();
            this.healthManager.DamageFlash ();
            return;
        }
        PlayDeathSound ();
        this.healthManager.LeaveNetwork (gameObject);
        Destroy (gameObject, 1f);
    }

    private void PlayDamageSound(){
        this.damageSoundSource.Play ();
    }

    private void PlayDeathSound(){
        // TODO
    }
}

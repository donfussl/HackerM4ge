using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSoundObjectScript : MonoBehaviour {

    // Use this for initialization
    void Start () {
        
    }

    // Update is called once per frame
    void Update () {
		
    }

    public void FadeOut(){
        StartCoroutine (AudioFadeOut.FadeOut (gameObject.GetComponent<AudioSource>(), 1f, gameObject));
    }
}

﻿using UnityEngine;
using System.Collections;

public class WandController : MonoBehaviour {

  const int up = 4;
  const int right = 1;
  const int left = 3;
  const int down = 2;
  const int noDirection = 0;

  public Transform tipOfWand;
  
  public AudioSource teleportSound;
  
  private GameObject teleportLine;
  
	// buttons
	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
	private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
	private Valve.VR.EVRButtonId menuButton = Valve.VR.EVRButtonId.k_EButton_ApplicationMenu;
	private Valve.VR.EVRButtonId touchpad = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;
	
	// controller initalize
	private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index);}}
	private SteamVR_TrackedObject trackedObj;
  
  
	// Use this for initialization
	void Start () {
    trackedObj = GetComponent<SteamVR_TrackedObject>();
	}
	
	// Update is called once per frame
	void Update () { 
    showTeleportDirection();
    teleport();
	}
  // renderer.material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
  void teleport() {
    if (controller.GetPressUp(touchpad)){
      int direction = GetDirectionOfTouchpad();
      if (direction == up){
        GameObject nextPlatform = getNextPlatform();
        if (nextPlatform != null) {
          CameraComponent cameraComponent = transform.parent.gameObject.GetComponent<CameraComponent>();
          transform.parent.gameObject.transform.position += nextPlatform.transform.position-cameraComponent.currentPlatformTransform.position;
          cameraComponent.currentPlatformTransform = nextPlatform.transform;
          teleportSound.Play();
        }
      }
    }
  }
  
  GameObject getNextPlatform() {
    RaycastHit hitObject;
    Ray ray = new Ray(transform.position, tipOfWand.position-transform.position);
    if (Physics.Raycast(ray, out hitObject))
    {
      if (hitObject.transform.parent != null){
        GameObject nextPlatform = hitObject.transform.parent.gameObject;          
        if (hitObject.collider.isTrigger && nextPlatform.GetComponent<PlatformComponent>() != null){
          return nextPlatform;
        }
      }
    }
    return null;
  }
  
  void showTeleportDirection() {
    if (controller.GetPressDown(touchpad)){
      int direction = GetDirectionOfTouchpad();
      
      teleportLine = new GameObject();
      teleportLine.transform.position = transform.position;
      teleportLine.transform.SetParent(transform);
      teleportLine.AddComponent<LineRenderer>();
      LineRenderer lineRenderer = teleportLine.GetComponent<LineRenderer>();
      lineRenderer.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
      lineRenderer.SetColors(Color.black, Color.blue);
      lineRenderer.SetWidth(0.01f, 0.01f);
      Vector3 wandDirection = tipOfWand.position - transform.position;
      
      lineRenderer.SetPosition(0, transform.position);
      lineRenderer.SetPosition(1, transform.position + 20*wandDirection);
    }
    
    if (controller.GetPress(touchpad)) {
      Vector3 wandDirection = tipOfWand.position - transform.position;
      
      LineRenderer lineRenderer = teleportLine.GetComponent<LineRenderer>();
      lineRenderer.SetPosition(0, transform.position);
      lineRenderer.SetPosition(1, transform.position + 20*wandDirection);
    }
    
    if (controller.GetPressUp(touchpad)) {
      Destroy(teleportLine);
      teleportLine = null;
    }
  }
  
  // Returns direction of the touchpad when pressed, Tobi(s füße) stinkt.
	private int GetDirectionOfTouchpad(){
		Vector2 axes = controller.GetAxis ();
		if (axes [0] < -0.7)
			return left;
		if (axes [0] > 0.7)
			return right;
		if (axes [1] < -0.7)
			return down;
		if (axes [1] > 0.7)
			return up;
		return noDirection;
	}
  
}

using UnityEngine;
using System;

using TWandAction = Union2<WandAction.Drain, WandAction.Vibrate>;

public class MeteorSpell : Spell
{

    public Material Thumbnail;

    private GameObject previewSpherePrefab;
    private GameObject previewSphere;
    private GameObject meteorPrefab;
    private LayerMask raycastLayerMask;

    private SpellSelectState spellSelectState = SpellSelectState.Aiming;

    private float draggingStartPosition;

    private MeteorController meteorToDragScript;

    private enum SpellSelectState
    {
        Aiming = 0,
        MeteorSpawned = 1,
        Dragging = 2,
    };

    public MeteorSpell ()
    {
        this.Thumbnail = Resources.Load ("MeteorThumbnailMaterial") as Material;
        this.previewSpherePrefab = Resources.Load ("MeteorSpellPrefabs/MeteorPreviewPrefab") as GameObject;
        this.raycastLayerMask = LayerMask.GetMask ("Surfaces");
        this.meteorPrefab = Resources.Load ("MeteorSpellPrefabs/Meteor2") as GameObject;
    }

    TWandAction[] Spell.UpdateSpell(ControllerBridge rightController, ControllerBridge leftController)
    {
        TriggerState rightTriggerState = rightController.GetTriggerState();
        Vector2 rightTouchpadAxis = rightController.GetTouchpadAxis();
        Vector3 rightControllerPosition = rightController.GetPosition();
        Vector3 rightControllerDirection = rightController.GetDirection();
        Vector3 rightControllerVelocity = rightController.GetVelocity();

        TriggerState leftTriggerState = leftController != null ? leftController.GetTriggerState() : null;
        Vector2 leftTouchpadAxis = leftController != null ? leftController.GetTouchpadAxis() : Vector2.zero;
        Vector3? leftControllerPosition = leftController != null ? leftController.GetPosition() : null as Vector3?;
        Vector3? leftControllerDirection = leftController != null ? leftController.GetDirection() : null as Vector3?;
        Vector3? leftControllerVelocity = leftController != null ? leftController.GetVelocity() : null as Vector3?;

        UpdateSpellSelectState (rightTriggerState, leftTriggerState);

        switch (spellSelectState)
        {
        case SpellSelectState.Aiming:
            if (rightTriggerState.press) {
                UpdateAiming (rightControllerPosition, rightControllerDirection);
            }            
            if (this.previewSphere && rightTriggerState.up) {
                spellSelectState = SpellSelectState.MeteorSpawned;
                meteorToDragScript = SpawnMeteorToPosition (previewSphere.transform.position).GetComponent<MeteorController>();
            }
            break;

        case SpellSelectState.MeteorSpawned:
            if (rightTriggerState.press && leftTriggerState.press) {
                spellSelectState = SpellSelectState.Dragging;
                meteorToDragScript.SetTargetArea (previewSphere.transform.position);
                UnityEngine.Object.Destroy (this.previewSphere);
                //TODO check ob linker controller ueberhaupt da ist
                draggingStartPosition = (rightControllerPosition.y + leftControllerPosition.Value.y) / 2f;
            }
            break;

        case SpellSelectState.Dragging:
            if (!rightTriggerState.press && !leftTriggerState.press) {
                float draggingEndPosition = (rightControllerPosition.y + leftControllerPosition.Value.y) / 2f;
                float dragLength = Mathf.Max(draggingStartPosition - draggingEndPosition, 0f);
                spellSelectState = SpellSelectState.Aiming;
                if (dragLength <= 0.1f) {
                    meteorToDragScript.DestroyMeteor ();
                } else {
                    meteorToDragScript.SetSpeed (dragLength * 25f);
                    meteorToDragScript.StartFalling ();
                }
            }
            break;
        }

        TWandAction[] actions = { };
        return actions;
    }

    private void UpdateAiming (Vector3 wandPosition, Vector3 wandDirection)
    {
        RaycastHit hitObject;
        Ray ray = new Ray (wandPosition, wandDirection);
        if (Physics.Raycast (ray, out hitObject, Mathf.Infinity, this.raycastLayerMask)) {
            if (this.previewSphere)
                this.previewSphere.transform.position = hitObject.point;
            else {
                this.previewSphere = GameObject.Instantiate (this.previewSpherePrefab, hitObject.point, new Quaternion ());
            }
        } else {
            if (this.previewSphere)
                UnityEngine.Object.Destroy (this.previewSphere);
        }
    }

    private GameObject SpawnMeteorToPosition (Vector3 targetArea)
    {
        // instantiate meteor
        GameObject actualMeteor = GameObject.Instantiate (
                                  this.meteorPrefab, targetArea + new Vector3 (
                                  UnityEngine.Random.Range (-15f, 15f),
                                  20,
                                  UnityEngine.Random.Range (-15f, 15f)),
                                  new Quaternion ());
        return actualMeteor;
    }

    Material Spell.GetThumbnail ()
    {
        return this.Thumbnail;
    }

    string Spell.GetName ()
    {
        return "Meteor";
    }

    TWandAction[] Spell.Select ()
    {
        TWandAction[] actions = {
            new Union2<WandAction.Drain, WandAction.Vibrate>.Case2(new WandAction.Vibrate(500)),
        };
        return actions;
    }

    TWandAction[] Spell.Deselect ()
    {
        TWandAction[] actions = { };
        return actions;
    }

    private void UpdateSpellSelectState(TriggerState rightTriggerState, TriggerState leftTriggerState){
        switch (spellSelectState) {
        case SpellSelectState.Aiming:

            break;
        case SpellSelectState.MeteorSpawned:
 
            break;
        case SpellSelectState.Dragging:
            break;
        }
    }
}

using System;
using UnityEngine;

using TWandAction = Union2<WandAction.Drain, WandAction.Vibrate>;

class LasertrapSpell : MonoBehaviour, Spell
{
    const float minimalDistance = 0.2f;
    const float maxSpeed = 15f;
    const float lifetime = 20f;

    private UnityEngine.Object laserSourcePrefab;
    private Material laserBeamHazardMaterial;
    private LayerMask surfaceLayer;

    private GameObject trapSource;
    private GameObject trapTarget;
    private float trapSourceDistance;

    /*
     * TODO:
     * - Eine Source pro Controller setzen
     * - Nach dem Setzen nicht direkt den Laser einschalten, sondern Traps erst noch "werfen"
     * - Trap-Positionen beim Setzen besser sichtbar machen: Strahl in den Himmel z.B.?
     * - Schadenssschema �ndern: Gesamtschaden statt Lebensdauer beschr�nken, daf�r mehr DPS machen (evtl. einfach unbegrenzt, zumindest aber genug um normale Gegner zu t�ten bevor sie durchlaufen k�nnen)
     * - Traps w�hrend das Platzierens, d.h. vor dem "werfen", halb-transparent machen
     */

    public LasertrapSpell()
    {
        laserSourcePrefab = Resources.Load("LaserTrapSpellPrefabs/LaserSource");
        laserBeamHazardMaterial = Resources.Load<Material>("LaserTrapSpellPrefabs/LaserBeamHazard");
        surfaceLayer = LayerMask.GetMask("Surfaces");

        trapSourceDistance = minimalDistance;
    }

    private void Init()
    {
        trapSource = Instantiate(laserSourcePrefab) as GameObject;
        trapTarget = null;
    }

    string Spell.GetName()
    {
        return GetType().Name;
    }

    Material Spell.GetThumbnail()
    {
        return laserBeamHazardMaterial;
    }

    TWandAction[] Spell.UpdateSpell(TriggerState rightTriggerState, Vector2 rightTouchpadAxis, Vector3 rightControllerPosition, Vector3 rightControllerDirection,
        TriggerState leftTriggerState, Vector2 leftTouchpadAxis, Vector3? leftControllerPosition, Vector3? leftControllerDirection)
    {
        if (trapTarget == null)
        {
            UpdateTrapSource(ref trapSource, rightTouchpadAxis, rightControllerPosition, rightControllerDirection);
        }
        else if (trapTarget != null)
        {
            UpdateTrapSource(ref trapTarget, rightTouchpadAxis, rightControllerPosition, rightControllerDirection);
        }

        if (rightTriggerState.down && trapTarget == null && NextSurfacePosition(rightControllerPosition, rightControllerDirection) != null)
        {
            trapTarget = Instantiate(Resources.Load("LaserTrapSpellPrefabs/LaserSource")) as GameObject;
        }
        else if (rightTriggerState.down && trapTarget != null && NextSurfacePosition(rightControllerPosition, rightControllerDirection) != null)
        {
            GameObject laser = CreateLaser(trapSource.transform.position, trapTarget.transform.position);
            Destroy(trapSource, lifetime);
            Destroy(trapTarget, lifetime);
            Destroy(laser, lifetime);

            Init();
        }

        TWandAction[] actions = { };
        return actions;
    }

    private void UpdateTrapSource(ref GameObject trapSource, Vector2 touchpadAxis, Vector3 wandPosition, Vector3 wandDirection)
    {
        UpdateTrapSourcePosition(ref trapSource, wandPosition, wandDirection);
    }

    private void UpdateTrapSourcePosition(ref GameObject trapSource, Vector3 wandPosition, Vector3 wandDirection)
    {
        Vector3? surfacePosition = NextSurfacePosition(wandPosition, wandDirection);
        if (surfacePosition != null)
        {
            trapSource.transform.position = surfacePosition.Value + Vector3.up * 0.3f;
        }
        else
        {
            trapSource.transform.position = wandPosition + wandDirection * minimalDistance;
        }
    }

    private Vector3? NextSurfacePosition(Vector3 wandPosition, Vector3 wandDirection)
    {
        RaycastHit hitObject;
        Ray ray = new Ray(wandPosition, wandDirection);

        if (! Physics.Raycast(ray, out hitObject, Mathf.Infinity, surfaceLayer))
        {
            return null;
        }

        return wandPosition + wandDirection * hitObject.distance;
    }

    private static GameObject CreateLaser(Vector3 from, Vector3 to)
    {
        GameObject laser = new GameObject();
        laser.AddComponent<LineRenderer>();
        laser.GetComponent<LineRenderer>().numPositions = 2;
        laser.GetComponent<LineRenderer>().SetPosition(0, from);
        laser.GetComponent<LineRenderer>().SetPosition(1, to);
        laser.GetComponent<LineRenderer>().startWidth = 0.01f;
        laser.GetComponent<LineRenderer>().endWidth = 0.01f;
        laser.GetComponent<LineRenderer>().startColor = Color.red;
        laser.GetComponent<LineRenderer>().endColor = Color.red;
        laser.GetComponent<LineRenderer>().material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));

        laser.AddComponent<LaserRay>();
        laser.GetComponent<LaserRay>().from = from;
        laser.GetComponent<LaserRay>().to = to;

        return laser;
    }

    TWandAction[] Spell.Select()
    {
        Init();
        TWandAction[] actions = {
            new Union2<WandAction.Drain, WandAction.Vibrate>.Case2(new WandAction.Vibrate(500)),
        };
        return actions;
    }

    TWandAction[] Spell.Deselect()
    {
        Destroy(trapSource);
        Destroy(trapTarget);
        trapSource = null;
        trapTarget = null;

        TWandAction[] actions = { };
        return actions;
    }
}

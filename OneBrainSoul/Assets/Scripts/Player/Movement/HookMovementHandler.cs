using System.Collections;
using System.Collections.Generic;
using System.Xml.Xsl;
using Unity.Burst;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class HookMovementHandler : MovementHandler
{
    public static float speed = 120f;
    public static float jumpStrength = 14f;
    public static float endDistance = 1.5f;
    public static float speedFalloff = 77.5f;
    public static float speedFalloffPower = 3f;
    public static float bobbingStrength = .6f;
    public static float snapDownwardsStrength = 6f;

    public static float delay = 1f;
    public static float delayHitImpact = 0.025f;

    public static int lineVertices = 10;

    Vector3 startPos;
    Vector3 endPos;
    Vector3 endVisualPos;
    Vector3 lineStartPos;
    bool snap;
    float hookDistance;
    float delayTime = 0f;
    float bobbingCycle = 0f;
    Vector3 movementDirection = Vector3.zero;
    LineRenderer line;

    public HookMovementHandler(PlayerCharacterController player, Vector3 startPos, Vector3 endPos, Vector3 endVisualPos, bool snap)
    {
        this.startPos = startPos;
        this.endPos = endPos;
        this.endVisualPos = endVisualPos;
        this.snap = snap;
        hookDistance = Vector3.Distance(this.endPos, this.startPos);
        line = player.GetComponent<LineRenderer>();
        line.enabled = true;
        line.positionCount = lineVertices;
        player.canSwitch = false;
    }

    public void VisualUpdate(PlayerCharacterController player)
    {
        lineStartPos = player.transform.position + new Vector3(0f, 1f, 0f) + player.orientation.right * 2f;
        if (delayTime < delay + delayHitImpact)
        {
            if (delayTime < delay)
            {
                line.SetPosition(0, lineStartPos);
                for (int i = 1; i < line.positionCount; i++)
                {
                    float lp = ((float)i / line.positionCount);
                    line.SetPosition(i, lineStartPos + (endVisualPos - lineStartPos) * Mathf.Pow(delayTime / delay, 2f) * lp);
                }
            }
        }
    }

    public void Move(PlayerCharacterController player)
    {
        float distanceToTarget = Vector3.Distance(endPos, player.transform.position);

        Vector3 prevLineStartPos = lineStartPos;
        lineStartPos = player.transform.position + new Vector3(0f, 1f, 0f) + player.orientation.right * 2f;
        //Throw hook
        if (delayTime < delay + delayHitImpact)
        {
            if (delayTime < delay)
            {
                line.SetPosition(0, lineStartPos);
                for (int i = 1; i < line.positionCount; i++)
                {
                    float lp = ((float)i / line.positionCount);
                    Vector3 lineStartPosInterpolated = Vector3.Lerp(lineStartPos, prevLineStartPos, lp);
                    line.SetPosition(i, lineStartPosInterpolated + (endVisualPos - lineStartPosInterpolated) * Mathf.Pow(delayTime / delay, 2f) * lp);
                }
                //line.SetPosition(1, lineStartPos + (endVisualPos - lineStartPos) * Mathf.Pow(delayTime / delay, 2f));

                if (delayTime + Time.fixedDeltaTime >= delay)
                {
                    //sound
                }
            }
            else
            {
                if (delayTime + Time.fixedDeltaTime >= delay + delayHitImpact)
                {
                    startPos = player.transform.position;
                    movementDirection = (endPos - startPos).normalized;
                    hookDistance = Vector3.Distance(endPos, startPos);
                    player.cam.FovWarp(2f * 60f / distanceToTarget, 2.5f);
                    player.rb.AddForce(Mathf.Max(0f, -player.rb.velocity.y) * Vector3.up, ForceMode.VelocityChange);
                }
            }

            delayTime += Time.fixedDeltaTime;
            return;
        }

        line.SetPosition(0, lineStartPos);
        for (int i = 1; i < line.positionCount; i++)
        {
            line.SetPosition(i, endVisualPos + (lineStartPos - endVisualPos) * ((float)i / line.positionCount));
        }

        float progress = 1f - Mathf.Min(1f, distanceToTarget / hookDistance);
        float speedWithFalloff = speed - Mathf.Pow(progress, 1f / speedFalloffPower) * speedFalloff;
        player.rb.velocity = movementDirection * speedWithFalloff;

        bobbingCycle += Time.fixedDeltaTime * player.rb.velocity.magnitude / 2f;
        player.rb.AddTorque(Quaternion.Euler(0, 90, 0) * new Vector3(movementDirection.x, 0f, movementDirection.z).normalized * player.rb.velocity.magnitude * bobbingStrength, ForceMode.Acceleration);
        float sidewaysBobbingMagnitude = Mathf.Sin(bobbingCycle) * player.rb.velocity.magnitude * bobbingStrength / 8f;
        //player.rb.AddTorque(movementDirection * sidewaysBobbingMagnitude, ForceMode.Acceleration);

        //End
        if (distanceToTarget < endDistance || distanceToTarget > hookDistance * 2f)
        {
            if (snap)
            {
                Vector3 dir = (endPos - startPos).normalized;
                dir.y *= -1f;
                Vector3 snapDir = (Vector3.down + dir.normalized).normalized;
                Vector3 snapForce = snapDir * snapDownwardsStrength * player.rb.velocity.magnitude * Mathf.Max(0f, Vector3.Dot(player.rb.velocity, Vector3.up));
                player.rb.AddForce(snapForce);
            }

            Exit(player);
        }

        RaycastHit hit;
        var playerCollider = player.capsuleCollider;
        Vector3 p1 = player.transform.position + playerCollider.center + Vector3.up * -playerCollider.height * 0.5f;
        Vector3 p2 = p1 + Vector3.up * playerCollider.height;
        if (Physics.CapsuleCast(p1, p2, playerCollider.radius, player.rb.velocity.normalized, out hit, player.rb.velocity.magnitude * 0.04f, 127, QueryTriggerInteraction.Ignore))
        {
            Exit(player);
        }

        // Jump and switch state to airborne
        if (player.jumpInput)
        {
            Vector3 velocity = player.rb.velocity;
            velocity.y = jumpStrength;
            player.rb.velocity = velocity;
            Exit(player);

            return;
        }

    }

    private void Exit(PlayerCharacterController player)
    {
        player.canSwitch = true;
        line.enabled = false;

        if (!player.onGround)
        {
            player.movementHandler = new AirborneMovementHandler();
        }
        else
        {
            player.movementHandler = new GroundedMovementHandler();
        }
    }

    public bool ShouldGravityApply(PlayerCharacterController player)
    {
        return delayTime < delay;
    }

    public bool ShouldHoverApply(PlayerCharacterController player)
    {
        return false;
    }
}

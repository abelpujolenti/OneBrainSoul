using Managers;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class WallClimbMovementHandler : MovementHandler
{
    public static float runSpeed = 450f;
    public static float strafeSpeed = 30f;
    public static float torqueStrength = 7f;
    public static float jumpStrength = 14f;
    public static float verticalDrag = 30f;
    public static float horizontalDrag = 7f;
    public static float bobbingStrength = 5f;
    public static float bonusSpeedDuration = .2f;
    public static float bonusSpeed = 180f;

    public static float viewAngleThreshold = 0.85f;
    public static float moveAngleThreshold = 0.4f;
    public static float climbDuration = 1f;

    private float climbTime = 0f;
    private float bobbingCycle = 0f;

    public void Move(PlayerCharacterController player)
    {
        float additionalSpeed = Mathf.Lerp(bonusSpeed, 0f, Mathf.Clamp01(climbTime / bonusSpeedDuration));
        player.rb.AddForce(Vector3.up * (runSpeed + additionalSpeed), ForceMode.Acceleration);

        Vector3 direction = (player.orientation.right * player.xInput + player.orientation.forward * player.yInput).normalized;
        Vector3 directionStrafe = (player.orientation.right * player.xInput).normalized;

        player.rb.AddForce(-new Vector3(0f, player.rb.velocity.y, 0f) * verticalDrag, ForceMode.Acceleration);
        player.rb.AddForce(-new Vector3(player.rb.velocity.x, 0f, player.rb.velocity.z) * horizontalDrag, ForceMode.Acceleration);

        player.rb.AddForce(directionStrafe * strafeSpeed * player.moveSpeedMultiplier, ForceMode.Acceleration);
        
        Vector3 horizontalVelocity = player.rb.velocity;
        horizontalVelocity.y = 0;
        player.rb.AddTorque(Quaternion.Euler(0, 90, 0) * horizontalVelocity * torqueStrength, ForceMode.Acceleration);

        bobbingCycle += Time.fixedDeltaTime * player.rb.velocity.magnitude * 0.75f;
        float sidewaysBobbingMagnitude = Mathf.Sin(bobbingCycle) * player.rb.velocity.magnitude * bobbingStrength / 8f;
        player.rb.AddTorque(player.orientation.forward * sidewaysBobbingMagnitude, ForceMode.Acceleration);

        if (!player.jumpInput)
        {
            player.movementHandler = new AirborneMovementHandler();
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(player.transform.position + new Vector3(0f, 1.9f, 0f), player.orientation.forward, out hit, 1f, GameManager.Instance.GetRaycastLayersWithoutAlly(), QueryTriggerInteraction.Ignore))
        {
            float viewDot = Vector3.Dot(hit.normal, player.orientation.forward);
            float moveDot = Vector3.Dot(hit.normal, player.orientation.right * player.xInput + player.orientation.forward * player.yInput);
            if (viewDot >= -viewAngleThreshold || moveDot >= -moveAngleThreshold)
            {
                player.movementHandler = new AirborneMovementHandler();
            }
        }
        else
        {
            player.movementHandler = new AirborneMovementHandler();
        }

        if (climbTime > climbDuration)
        {
            player.movementHandler = new AirborneMovementHandler();
        }

        climbTime += Time.fixedDeltaTime;
    }

    public bool ShouldGravityApply(PlayerCharacterController player)
    {
        return false;
    }

    public bool ShouldHoverApply(PlayerCharacterController player)
    {
        return true;
    }
}

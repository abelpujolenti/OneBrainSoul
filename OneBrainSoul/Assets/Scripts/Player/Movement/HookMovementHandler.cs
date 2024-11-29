using UnityEngine;

public class HookMovementHandler : MovementHandler
{
    public static float speed = 110f;
    public static float jumpStrength = 14f;
    public static float endDistance = 1.5f;
    public static float speedFalloff = 77.5f;
    public static float speedFalloffPower = 3f;
    public static float bobbingStrength = .6f;

    public static float delay = 0.18f;
    public static float delayHitImpact = 0.025f;

    public Vector3 startPos;
    public Vector3 endPos;
    float hookDistance;
    float delayTime = 0f;
    float bobbingCycle = 0f;
    public Vector3 movementDirection = Vector3.zero;
    LineRenderer line;

    public HookMovementHandler(PlayerCharacterController player, Vector3 startPos, Vector3 endPos)
    {
        this.startPos = startPos;
        this.endPos = endPos;
        hookDistance = Vector3.Distance(endPos, startPos);
        movementDirection = (endPos - startPos).normalized;
        line = player.GetComponent<LineRenderer>();
        line.enabled = true;
    }

    public void Move(PlayerCharacterController player)
    {
        float distanceToTarget = Vector3.Distance(endPos, player.transform.position);

        Vector3 lineStartPos = player.transform.position + new Vector3(0f, 1f, 0f) + player.orientation.right * 2f;
        //Throw hook
        if (delayTime < delay + delayHitImpact)
        {
            player.rb.velocity = Vector3.zero;
            if (delayTime < delay)
            {
                line.SetPosition(1, lineStartPos + (endPos - lineStartPos) * Mathf.Pow(delayTime / delay, 2f));
                line.SetPosition(0, lineStartPos);

                if (delayTime + Time.fixedDeltaTime >= delay)
                {
                    player.cam.FovWarp(2f * 60f / distanceToTarget , 2.5f);
                    //sound
                }
            }

            delayTime += Time.fixedDeltaTime;
            return;
        }

        line.SetPosition(0, lineStartPos);

        // Grind dat beam
        float progress = 1f - Mathf.Min(1f, distanceToTarget / hookDistance);
        float speedWithFalloff = speed - Mathf.Pow(progress, 1f / speedFalloffPower) * speedFalloff;
        player.rb.velocity = movementDirection * speedWithFalloff;

        bobbingCycle += Time.fixedDeltaTime * player.rb.velocity.magnitude / 2f;
        player.rb.AddTorque(Quaternion.Euler(0, 90, 0) * new Vector3(movementDirection.x, 0f, movementDirection.z).normalized * player.rb.velocity.magnitude * bobbingStrength, ForceMode.Acceleration);
        float sidewaysBobbingMagnitude = Mathf.Sin(bobbingCycle) * player.rb.velocity.magnitude * bobbingStrength / 8f;
        //player.rb.AddTorque(movementDirection * sidewaysBobbingMagnitude, ForceMode.Acceleration);

        //End
        if (distanceToTarget < endDistance || distanceToTarget > hookDistance * 1.1f)
        {
            Exit(player);
        }

        RaycastHit hit;
        var playerCollider = player.GetComponent<CapsuleCollider>();
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
        return false;
    }

    public bool ShouldHoverApply(PlayerCharacterController player)
    {
        return false;
    }
}

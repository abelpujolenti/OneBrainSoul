using UnityEngine;

public class ChargeMovementHandler : MovementHandler
{
    public static float chargeSpeed = 1000f;
    public static float chargeSpeedFalloff = 575f;
    public static float chargeSpeedFalloffPower = 4f;
    public static float swerveSpeed = 280f;
    public static float horizontalDrag = 15f;
    public static float bobbingStrength = 0.6f;
    public static float duration = .7f;
    public static float bounceStrength = 1900f;
    public static float bounceVerticalRatio = .45f;

    private float chargeTime = 0f;
    private float bobbingCycle = 0f;
    Vector3 chargeDirection;

    public ChargeMovementHandler(PlayerCharacterController player, Vector3 chargeDirection)
    {
        this.chargeDirection = chargeDirection;
        player.cam.FovWarp(1f, 2f);
        player.urpManager.ChargeRunEffect(duration);
    }

    public void Move(PlayerCharacterController player)
    {

        Vector3 swerveDirection = (player.orientation.right * player.xInput).normalized;
        swerveDirection = Quaternion.FromToRotation(Vector3.up, player.groundHit.normal) * swerveDirection;
        player.rb.AddForce(swerveDirection * swerveSpeed, ForceMode.Acceleration);

        bobbingCycle += Time.fixedDeltaTime * player.rb.velocity.magnitude / 2f;
        player.rb.AddTorque(Quaternion.Euler(0, 90, 0) * player.rb.velocity * bobbingStrength, ForceMode.Acceleration);
        float sidewaysBobbingMagnitude = Mathf.Sin(bobbingCycle) * player.rb.velocity.magnitude * bobbingStrength / 8f;
        player.rb.AddTorque(chargeDirection * sidewaysBobbingMagnitude, ForceMode.Acceleration);

        Vector3 chargeForwardDirection = Quaternion.FromToRotation(Vector3.up, player.groundHit.normal) * chargeDirection;
        float chargeSpeedWithFalloff = chargeSpeed - Mathf.Pow(chargeTime / duration, 1f / chargeSpeedFalloffPower) * chargeSpeedFalloff;
        player.rb.AddForce(chargeForwardDirection * chargeSpeedWithFalloff, ForceMode.Acceleration);

        chargeTime += Time.fixedDeltaTime;
        if (!player.onGround)
        {
            Exit(player);
            return;
        }
        else if (chargeTime >= duration)
        {
            Exit(player);
            return;
        }

        // Drag, opposite to current horizontal velocity
        Vector3 horizontalVelocity = player.rb.velocity;
        horizontalVelocity.y = 0;
        player.rb.AddForce(-horizontalVelocity * horizontalDrag, ForceMode.Acceleration);

        RaycastHit hit;
        if (Physics.Raycast(player.transform.position, chargeDirection, out hit, player.rb.velocity.magnitude * 0.04f, 127, QueryTriggerInteraction.Ignore))
        {
            DestructibleTerrain destructibleTerrain = hit.collider.GetComponent<DestructibleTerrain>();
            bool hitTerrain = destructibleTerrain != null;
            if (hitTerrain)
            {
                destructibleTerrain.Break(hit.point);
            }
            DamageTakingEntity entity = hit.collider.GetComponent<DamageTakingEntity>();
            if (entity != null)
            {
                entity.Damage(player, hit.point);
            }

            bool damaged = hitTerrain || entity != null;
            Collide(player, hit.normal, damaged);
            //Exit(player);
            return;
        }
    }

    private void Collide(PlayerCharacterController player, Vector3 normal, bool damaged)
    {
        player.urpManager.ChargeCollideEffect((damaged ? .12f : .065f) + .3f);

        player.cam.StopFovWarp();
        player.hitstop.Add(damaged ? .12f : .07f);
        player.cam.ScreenShake(damaged ? .12f : .07f, damaged ? .6f : .25f);

        player.hitstop.AddAftershock(damaged ? .23f : .2f);
        player.rb.velocity = Vector3.zero;
        player.rb.AddForce((new Vector3(normal.x, Mathf.Max(0f, normal.y), normal.z).normalized + new Vector3(0f, bounceVerticalRatio * (damaged ? 1.2f : 1f), 0f)).normalized * bounceStrength * (damaged ? 1.4f : 1f) , ForceMode.Acceleration);
        
        player.movementHandler = new AirborneMovementHandler();
        (player.movementHandler as AirborneMovementHandler).horizontalDrag = 5f;
    }

    private void Exit(PlayerCharacterController player)
    {
        if (!player.onGround)
        {
            player.movementHandler = new AirborneMovementHandler();
        }
        else
        {
            player.movementHandler = new GroundedMovementHandler();
        }
    }
}

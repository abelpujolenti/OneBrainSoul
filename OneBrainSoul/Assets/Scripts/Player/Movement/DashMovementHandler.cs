using UnityEngine;

public class DashMovementHandler : MovementHandler
{
    public static float dashSpeed = 2700f;
    public static float dashSpeedFalloff = 800f;
    public static float dashSpeedFalloffPower = 2f;
    public static float horizontalDrag = 35f;
    public static float horizontalAirDrag = 25f;
    public static float bobbingStrength = 0.6f;
    public static float duration = .09f;

    private float chargeTime = 0f;
    private float bobbingCycle = 0f;
    Vector3 dashDirection;

    public DashMovementHandler(PlayerCharacterController player, Vector3 dashDirection)
    {
        this.dashDirection = dashDirection;
        player.cam.FovWarp(.8f / duration, 1.22f);
        //PostProcessingManager.Instance.ChargeRunEffect(duration);
        player.canSwitch = false;
    }

    public void Move(PlayerCharacterController player)
    {
        bobbingCycle += Time.fixedDeltaTime * player.rb.velocity.magnitude / 2f;
        player.rb.AddTorque(Quaternion.Euler(0, 90, 0) * player.rb.velocity * bobbingStrength, ForceMode.Acceleration);

        Vector3 dashForwardDirection = Quaternion.FromToRotation(Vector3.up, player.groundHit.normal) * dashDirection;
        float chargeSpeedWithFalloff = dashSpeed - Mathf.Pow(chargeTime / duration, 1f / dashSpeedFalloffPower) * dashSpeedFalloff;
        player.rb.AddForce(dashForwardDirection * chargeSpeedWithFalloff, ForceMode.Acceleration);

        chargeTime += Time.fixedDeltaTime;
        if (chargeTime >= duration)
        {
            Exit(player);
            return;
        }

        // Drag, opposite to current horizontal velocity
        Vector3 horizontalVelocity = player.rb.velocity;
        horizontalVelocity.y = 0;
        player.rb.AddForce(-horizontalVelocity * (player.onGround ? horizontalDrag : horizontalAirDrag), ForceMode.Acceleration);

        RaycastHit hit;
        if (Physics.Raycast(player.transform.position, dashDirection, out hit, player.rb.velocity.magnitude * 0.04f, 127, QueryTriggerInteraction.Ignore))
        {
            DamageTakingEntity entity = hit.collider.GetComponent<DamageTakingEntity>();
            if (entity != null)
            {
                entity.Damage(player, hit.point, 1);
            }

            bool damaged = entity != null;
            Exit(player);
            return;
        }
    }

    private void Exit(PlayerCharacterController player)
    {
        player.canSwitch = true;

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

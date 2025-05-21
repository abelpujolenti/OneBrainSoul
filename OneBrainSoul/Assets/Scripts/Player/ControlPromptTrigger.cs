using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Player.Movement;

public class ControlPromptTrigger : MonoBehaviour
{
    [SerializeField] private string messageId;
    private static Dictionary<string, Message> messages = new Dictionary<string, Message>() {
        ["dashToAttack"] = new Message() { 
            duration=0.5f, 
            exitState=new DashMovementHandler(), 
            text= "<color=#77f0d8><size=43><b>[Damage enemies by dashing into them]</b>"
        },
        ["doubleJump"] = new Message() { 
            duration=2f, 
            exitState=new AirborneMovementHandler(), 
            text= "<color=#77f0d8><size=43><b>[Space] Mid-air</b> <color=white> <size=40>Double Jump"
        },
    };

    struct Message
    {
        public string text;
        public IMovementHandler exitState;
        public float duration;
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerCharacterController>();
        if (player == null) return;
        Message message = messages[messageId];
        StartCoroutine(player.SetControlText(message.text, message.duration, message.exitState));
    }
}

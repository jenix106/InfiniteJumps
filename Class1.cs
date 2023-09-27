using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace InfiniteJumps
{
    public class JumpManager : ThunderScript
    {
        [ModOption(name: "Enable/Disable", tooltip: "Enables/disables the mod", valueSourceName: nameof(booleanOption), defaultValueIndex = 0)]
        public static bool Enabled = true;
        [ModOption(name: "Infinite Jumps", tooltip: "Enables/disables infinite jumping", valueSourceName: nameof(booleanOption), defaultValueIndex = 0)]
        public static bool Infinite = true;
        [ModOption(name: "Extra Jumps", tooltip: "Limits the amount of extra jumps you can make", valueSourceName: nameof(intValues), defaultValueIndex = 0)]
        public static int JumpLimit = 0;
        public float jumpTime = 0;
        public Vector3 jumpForce; 
        float jumpForceMultiplier = 1;
        public int jumps = 0;
        public bool isJumping = false;
        public static ModOptionBool[] booleanOption =
        {
            new ModOptionBool("Enabled", true),
            new ModOptionBool("Disabled", false)
        };
        public static ModOptionInt[] intValues()
        {
            ModOptionInt[] modOptionInts = new ModOptionInt[1001];
            int num = 0;
            for (int i = 0; i < modOptionInts.Length; ++i)
            {
                modOptionInts[i] = new ModOptionInt(num.ToString("0"), num);
                num += 1;
            }
            return modOptionInts;
        }
        public override void ScriptEnable()
        {
            base.ScriptEnable();
            PlayerControl.local.OnJumpButtonEvent += Local_OnJumpButtonEvent;
        }

        private void Local_OnJumpButtonEvent(bool active, EventTime eventTime)
        {
            isJumping = active;
            if (active && eventTime == EventTime.OnStart && !Player.local.locomotion.isGrounded && Enabled && (jumps < JumpLimit || Infinite))
            {
                jumpTime = Player.local.locomotion.jumpMaxDuration;
                jumpForce = new Vector3(0, Player.local.locomotion.jumpGroundForce * Player.local.locomotion.jumpClimbVerticalMultiplier * Mathf.InverseLerp(Player.local.locomotion.jumpClimbVerticalMaxVelocityRatio, 0, Player.local.locomotion.rb.velocity.y), 0);
                jumpForceMultiplier = 1;
                foreach (Locomotion.SpeedModifier speedModifier in Player.local.locomotion.speedModifiers)
                {
                    jumpForceMultiplier *= speedModifier.jumpForceMultiplier;
                }
                Player.local.locomotion.rb.velocity = new Vector3(Player.local.locomotion.rb.velocity.x, Mathf.Max(Player.local.locomotion.rb.velocity.y, 0), Player.local.locomotion.rb.velocity.z); 
                if (Player.local.creature.data.jumpEffectData != null)
                {
                    EffectInstance effectInstance = Player.local.creature.data.jumpEffectData.Spawn(Player.local.creature.transform);
                    effectInstance.source = Player.currentCreature;
                    effectInstance.Play();
                }
                ++jumps;
            }
        }
        public override void ScriptUpdate()
        {
            base.ScriptUpdate();
            if(jumpTime > 0 && isJumping)
            {
                Player.local.locomotion.rb.AddForce(new Vector3(
                    Utils.CalculateRatio(jumpTime, 0.0f, Player.local.locomotion.jumpMaxDuration, 0.0f, jumpForce.x),
                    Utils.CalculateRatio(jumpTime, 0.0f, Player.local.locomotion.jumpMaxDuration, 0.0f, jumpForce.y),
                    Utils.CalculateRatio(jumpTime, 0.0f, Player.local.locomotion.jumpMaxDuration, 0.0f, jumpForce.z)) *
                    TimeManager.GetTimeStepMultiplier() * jumpForceMultiplier, ForceMode.VelocityChange);
                jumpTime -= Time.deltaTime;
            }
            else
            {
                jumpTime = 0;
            }
            if (Player.local?.locomotion != null && Player.local.locomotion.isGrounded) jumps = 0;
        }
    }
}

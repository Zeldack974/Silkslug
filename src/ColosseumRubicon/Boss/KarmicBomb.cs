using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Silkslug.ColosseumRubicon.Boss
{
    public class KarmicBomb : UpdatableAndDeletable
    {
        public KarmicBomb(HellKnight parent, Vector2 position, float diameter, int lifeTime, bool simulMain)
        {
            this.pos = position;
            this.parent = parent;
            this.life = lifeTime;
            this.lifeTime = lifeTime;
            this.simulMain = simulMain;
            this.diameter = diameter;
            this.karmaSymbol = new KarmaVectorX(this.pos, diameter * 1.25f, 5f, 0f);
            this.parent.room.AddObject(this.karmaSymbol);
            this.wasPlayerAlreadyDead = this.parent.player.dead;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            float perc = (float)this.life / (float)this.lifeTime;
            this.karmaSymbol.alpha = Custom.LerpSinEaseInOut(1f, 0f, perc);
            this.karmaSymbol.diameter = Custom.LerpExpEaseInOut(this.diameter, this.diameter * 1.25f, perc);
            this.life--;
            if (this.life == -1 && !base.slatedForDeletetion)
            {
                base.slatedForDeletetion = true;
                this.karmaSymbol.slatedForDeletetion = true;
                for (int i = 0; i < 5; i++)
                {
                    this.room.AddObject(new Spark(this.pos, Custom.RNV() * UnityEngine.Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
                }
                if (Custom.Dist(this.parent.player.firstChunk.pos, this.pos) <= this.diameter / 2f)
                {
                    this.parent.player.Die();
                    BodyChunk[] bodyChunks = this.parent.player.bodyChunks;
                    for (int j = 0; j < bodyChunks.Length; j++)
                    {
                        bodyChunks[j].vel += Custom.RNV() * 36f;
                    }
                    if (this.simulMain && !this.wasPlayerAlreadyDead)
                    {
                        //if (this.parent.player.dead)
                        //{
                        this.room.PlaySound(SoundID.Firecracker_Bang, this.pos, 0.5f, 0.75f + UnityEngine.Random.value);
                        this.room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, this.pos, 0.5f, 0.5f + UnityEngine.Random.value * 0.5f);
                        //}
                        //this.room.PlaySound(SoundID.Snail_Pop, this.pos, 0.75f, 1.5f + UnityEngine.Random.value);
                    }
                }
                else
                {
                    this.room.PlaySound(SoundID.Snail_Pop, this.pos, 0.75f, 1.5f + UnityEngine.Random.value);
                }

            }
        }

        public Vector2 pos;

        public bool wasPlayerAlreadyDead;

        public KarmaVectorX karmaSymbol;

        public HellKnight parent;

        public int life;

        public int lifeTime;

        public float diameter;

        public bool simulMain;
    }
}

using UnityEngine;
using RWCustom;
using System.Collections.Generic;
using System;

namespace Silkslug.ColosseumRubicon.Boss.Halo
{
    /**
        Copyright (c) 2023 Maxi Mol & gruntonium
    **/

    //basically a tweaked copy from the game (TempleGuardGraphics.Halo)
    public class TempleGuardHalo : BaseHalo
    {
        public int[][] glyphs = new int[4][];
        public float[,] lines = new float[40, 4];
        public float[,] smallCircles = new float[10, 5];
        public int circles = 7;
        public int firstSwapperSprite;
        public int firstLineSprite;
        public int firstSmallCircleSprite;
        public bool[][] dirtyGlyphs;
        public float[][,] glyphPositions;
        public GlyphSwapper[] swappers = new GlyphSwapper[3];
        public float[,] rotation;
        public float[,] rad;
        public float savDisruption;
        public float activity;
        public float slowRingsActive = 1f; //changed to 1f to gradually build halo roughly after flashAmount reached 0f
        public float lastSlowRingsActive;
        public int ringsActive = 4;
        public bool firstUpdate = true;
        public bool deactivated = false;
        public List<EntityID> reactedToCritters = new List<EntityID>();

        //added to original
        public float telekinesis = 0.3f;
        public float lastTelekin = 0.3f; //TODO track telekinesis
        public float stress = 0.5f;
        public float flashAmount = 1f; //start value when created
        public float haloLerpSpeed = 1f; //changed from 0.1f
        public float distAboveHead = 110f; //changed from 200f
        public Vector2? swapperPos = null;


        public TempleGuardHalo(HellKnight owner) : base(owner)
        {
            Plugin.Log("creating TempleGuardHalo");
            firstSprite = 0;
            rad = new float[2, 3];
            rad[0, 0] = 0f;
            rad[0, 1] = 0f;
            rad[0, 2] = 0f;
            rad[1, 0] = 1f;
            rad[1, 1] = 1f;
            rad[1, 2] = 1f;
            dirtyGlyphs = new bool[glyphs.Length][];
            glyphPositions = new float[glyphs.Length][,];
            for (int i = 0; i < glyphs.Length; i++)
            {
                glyphs[i] = new int[(int)(CircumferenceAtCircle(i * 2, 1f, 0f) / 15f)];
                dirtyGlyphs[i] = new bool[glyphs[i].Length];
                glyphPositions[i] = new float[glyphs[i].Length, 3];
                for (int j = 0; j < glyphs[i].Length; j++)
                    glyphs[i][j] = UnityEngine.Random.value < 0.033333335f ? -1 : UnityEngine.Random.Range(0, 7);
            }
            rotation = new float[circles, 2];
            for (int k = 0; k < rotation.GetLength(0); k++)
            {
                rotation[k, 0] = UnityEngine.Random.value;
                rotation[k, 1] = rotation[k, 0];
            }
            totalSprites = circles;
            for (int l = 0; l < glyphs.Length; l++)
                totalSprites += glyphs[l].Length;
            firstSwapperSprite = firstSprite + totalSprites;
            for (int m = 0; m < swappers.Length; m++)
                swappers[m] = new GlyphSwapper(this);
            totalSprites += swappers.Length * 3;
            firstLineSprite = totalSprites;
            for (int n = 0; n < lines.GetLength(0); n++)
            {
                lines[n, 0] = UnityEngine.Random.value;
                lines[n, 1] = lines[n, 0];
                lines[n, 2] = UnityEngine.Random.Range(0, 3);
                lines[n, 3] = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value);
            }
            totalSprites += lines.GetLength(0);
            firstSmallCircleSprite = totalSprites;
            for (int num = 0; num < smallCircles.GetLength(0); num++)
            {
                smallCircles[num, 0] = UnityEngine.Random.value;
                smallCircles[num, 1] = smallCircles[num, 0];
                smallCircles[num, 2] = UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, 6));
                smallCircles[num, 3] = UnityEngine.Random.Range((int)smallCircles[num, 2] + 1, 7);
                smallCircles[num, 4] = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value);
            }
            totalSprites += smallCircles.GetLength(0);
        }


        public override void Update(bool eu)
        {
            //destroy and return if owner is deleted or moves to another room
            if (owner.slatedForDeletetion != false || room != owner.room) //remove halo when mousedrag is not active
            {
                ringsActive = 2;
                if (slowRingsActive <= 2f && flashAmount < 1f)
                    flashAmount += 1f / 10f;
                if (flashAmount >= 1f)
                {
                    Destroy();
                    deactivated = true;
                    room?.RemoveObject(this);
                    RemoveFromRoom();
                    return;
                }
            }
            else
            {
                ringsActive = 4; //final amount of rings active
                if (flashAmount > 0f)
                    flashAmount -= 1f / 10f;
            }
            flashAmount = Mathf.Clamp(flashAmount, 0f, 1f); //keep flashAmount a value from 0f to 1f

            lastPos = pos;
            Vector2 vector = overridePos ?? owner.pos;
            //if (HaloManager.colorType == Options.ColorTypes.Black)
            //    vector += new Vector2(-7.5f, 15.5f); //adjust to background
            //if (Options.haloOffset.Value) {
            //    Vector2 headDir = new Vector2(0f, -1f);
            //    //Vector2 headDir = Custom.DirVec(owner.pos, (owner.owner.owner as Creature).mainBodyChunk.pos);
            //    vector -= headDir * Mathf.Lerp(distAboveHead, this.RadAtCircle(2f + this.slowRingsActive * 2f, 1f, 0f), 0.5f);
            //    //Vector2 vector = this.owner.guard.mainBodyChunk.pos - this.owner.guard.StoneDir * Mathf.Lerp(200f, this.RadAtCircle(2f + this.slowRingsActive * 2f, 1f, 0f), 0.5f);
            //}
            pos += Vector2.ClampMagnitude(vector - pos, 10f);
            pos = Vector2.Lerp(pos, vector, haloLerpSpeed);
            if (firstUpdate)
            {
                pos = vector;
                lastPos = pos;
                firstUpdate = false;
            }
            radius = RadAtCircle(ringsActive, 0f, 0f); //"radius" is unused for TempleGuardHalo

            swapperPos = connectionPos; //swapperPos isn't reset after Update(), quick fix for LightningType TempleGuard

            for (int j = 0; j < swappers.Length; j++) //moved before base.Update so swappers can read connectionPos
                swappers[j].Update();

            base.Update(eu); //lightning bolts and color cycle

            //deactivated = owner.dead;

            if (activity > stress)
            {
                activity = Mathf.Max(stress - 0.0033333334f, stress);
            }
            else
            {
                activity = stress;
            }
            /*if (UnityEngine.Random.value < 0.01f)
                this.ringsActive = Custom.IntClamp((int)Mathf.Lerp(2f, 9f, Mathf.Pow(stress, 0.5f)), 2, 4);*/
            lastSlowRingsActive = slowRingsActive;
            if (slowRingsActive < ringsActive)
            {
                slowRingsActive = Mathf.Min(ringsActive, slowRingsActive + 0.1f);
            }
            else
            {
                slowRingsActive = Mathf.Max(ringsActive, slowRingsActive - 0.05f);
            }
            savDisruption = Mathf.InverseLerp(10f, 150f, Vector2.Distance(pos, vector));
            for (int i = 0; i < rotation.GetLength(0); i++)
            {
                rotation[i, 1] = rotation[i, 0];
                rotation[i, 0] += 0.2f / Mathf.Max(1f, CircumferenceAtCircle(i, 1f, savDisruption))
                    * (i % 2 == 0 ? -1f : 1f) * Mathf.Lerp(Speed, 3f, telekinesis);
            }
            for (int k = 0; k < lines.GetLength(0); k++)
            {
                lines[k, 1] = lines[k, 0];
                lines[k, 0] += 0.008333334f * lines[k, 3] * Speed;
            }
            for (int l = 0; l < smallCircles.GetLength(0); l++)
            {
                smallCircles[l, 1] = smallCircles[l, 0];
                smallCircles[l, 0] += 0.004166667f * smallCircles[l, 4] * Speed;
            }
            for (int m = 0; m < glyphs.Length; m++)
            {
                for (int n = 0; n < glyphs[m].Length; n++)
                {
                    glyphPositions[m][n, 1] = glyphPositions[m][n, 0];
                    if (UnityEngine.Random.value < Speed / 160f)
                    {
                        if (UnityEngine.Random.value < 0.033333335f && glyphPositions[m][n, 0] == 0f && glyphs[m][n] > -1)
                        {
                            if (m == glyphs.Length - 1)
                            {
                                glyphPositions[m][n, 0] = -1f;
                            }
                            else if (m == glyphs.Length - 2 && ringsActive == 4)
                            {
                                glyphPositions[m][n, 0] = -3f;
                            }
                        }
                        else
                        {
                            glyphPositions[m][n, 0] = UnityEngine.Random.value < 0.05f ? 1f : 0f;
                        }
                    }
                    if (glyphPositions[m][n, 0] == 1f && glyphs[m][n] == -1)
                    {
                        glyphs[m][n] = UnityEngine.Random.Range(0, 7);
                        dirtyGlyphs[m][n] = true;
                    }
                    if (glyphPositions[m][n, 2] > 0f && glyphs[m][n] > -1)
                    {
                        glyphPositions[m][n, 2] -= 0.05f;
                        glyphs[m][n] = UnityEngine.Random.Range(0, 7);
                        dirtyGlyphs[m][n] = true;
                    }
                }
            }
            for (int num = 0; num < smallCircles.GetLength(0); num++)
            {
                if (UnityEngine.Random.value < Speed / 120f && smallCircles[num, 3] < ringsActive * 2)
                {
                    float num2 = RadAtCircle(smallCircles[num, 2] - 0.5f, 1f, savDisruption);
                    float num3 = RadAtCircle(smallCircles[num, 3] - 0.5f, 1f, savDisruption);
                    Vector2 p = Custom.DegToVec(smallCircles[num, 0] * 360f) * Mathf.Lerp(num2, num3, 0.5f);
                    for (int num4 = 0; num4 < glyphs.Length; num4++)
                        for (int num5 = 0; num5 < glyphs[num4].Length; num5++)
                            if (Custom.DistLess(p, GlyphPos(num4, num5, 1f), (num3 - num2) / 2f))
                                glyphPositions[num4][num5, 2] = 1f;
                }
            }
            int num6 = 0;
            for (int num7 = 0; num7 < glyphs[0].Length; num7++)
                if (glyphPositions[0][num7, 0] == 1f)
                    num6++;
            if (num6 > 1)
                for (int num8 = 0; num8 < glyphs[0].Length; num8++)
                    glyphPositions[0][num8, 0] = 0f;
            for (int num9 = 0; num9 < 2; num9++)
            {
                rad[num9, 1] = rad[num9, 0];
                if (rad[num9, 0] < rad[num9, 2])
                {
                    rad[num9, 0] = Mathf.Min(rad[num9, 2], rad[num9, 0] + (num9 == 0 ? 0.15f : 0.0035714286f));
                }
                else
                {
                    rad[num9, 0] = Mathf.Max(rad[num9, 2], rad[num9, 0] - (num9 == 0 ? 0.15f : 0.0035714286f));
                }
                rad[num9, 0] = Mathf.Lerp(rad[num9, 0], rad[num9, 2], 0.01f);
            }
            if (UnityEngine.Random.value < Speed / 120f)
            {
                rad[0, 2] = UnityEngine.Random.value > activity ? 0f : UnityEngine.Random.Range(-1, 3) * 20f;
                rad[1, 2] = UnityEngine.Random.value < 1f / Mathf.Lerp(1f, 5f, activity) ? 1f : Mathf.Lerp(0.75f, 1.25f, UnityEngine.Random.value);
            }
        }


        public float Circumference(float rad)
        {
            return 2f * rad * 3.1415927f;
        }


        public float RadAtCircle(float circle, float timeStacker, float disruption)
        {
            // 20f
            return ((circle + 1f) * 35f + Mathf.Lerp(rad[0, 1], rad[0, 0], timeStacker) * (1f - Mathf.Lerp(lastTelekin, telekinesis, timeStacker)))
                * Mathf.Lerp(Mathf.Lerp(rad[1, 1], rad[1, 0], timeStacker), 0.7f, Mathf.Lerp(lastTelekin, telekinesis, timeStacker))
                * Mathf.Lerp(1f, UnityEngine.Random.value * disruption, Mathf.Pow(disruption, 2f));
        }


        public float CircumferenceAtCircle(float circle, float timeStacker, float disruption)
        {
            return Circumference(RadAtCircle(circle, timeStacker, disruption));
        }


        public float Speed
        {
            get
            {
                float b = 1.8f;
                /*if (this.owner.guard.AI.focusCreature != null && this.owner.guard.AI.FocusCreatureMovingTowardsProtectExit 
                    && this.owner.guard.AI.focusCreature.VisualContact && this.owner.guard.AI.focusCreature.representedCreature.realizedCreature != null)
                {
                    b = Custom.LerpMap(Vector2.Distance(this.owner.guard.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.lastPos, 
                        this.owner.guard.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos), 1.5f, 5f, 1.2f, 3f);
                }*/
                return Mathf.Lerp(0.2f, b, activity);
            }
        }


        public void ReactToCreature(bool firstSpot, Tracker.CreatureRepresentation creatureRep)
        {
            if (false /*Mathf.Abs(this.owner.guard.mainBodyChunk.pos.x - this.owner.guard.room.MiddleOfTile(creatureRep.BestGuessForPosition()).x) < 300f*/
                && !reactedToCritters.Contains(creatureRep.representedCreature.ID))
            {
                ringsActive = Math.Max(ringsActive, UnityEngine.Random.Range(3, 5));
                rad[0, 2] = UnityEngine.Random.value > activity ? 0f : UnityEngine.Random.Range(-1, 3) * 20f;
                rad[1, 2] = UnityEngine.Random.value < 1f / Mathf.Lerp(1f, 5f, activity) ? 1f : Mathf.Lerp(0.75f, 1.25f, UnityEngine.Random.value);
                reactedToCritters.Add(creatureRep.representedCreature.ID);
                for (int i = 0; i < (int)Custom.LerpMap(creatureRep.representedCreature.realizedCreature.TotalMass, 0.2f, 2f, 4f, 100f); i++)
                {
                    int num = UnityEngine.Random.Range(0, glyphs.Length);
                    int num2 = UnityEngine.Random.Range(0, glyphs[num].Length);
                    glyphs[num][num2] = -1;
                    dirtyGlyphs[num][num2] = true;
                }
                activity = Mathf.Min(1f, activity + 0.2f);
                return;
            }
            for (int j = 0; j < (int)Custom.LerpMap(creatureRep.representedCreature.realizedCreature.TotalMass, 0.2f, 2f, 2f, 11 * ringsActive); j++)
            {
                int num3 = UnityEngine.Random.Range(0, ringsActive);
                int num4 = UnityEngine.Random.Range(0, glyphs[num3].Length);
                glyphs[num3][num4] = UnityEngine.Random.Range(0, 7);
                dirtyGlyphs[num3][num4] = true;
                if (UnityEngine.Random.value < 0.5f)
                    glyphPositions[num3][num4, 2] = 1f;
            }
        }


        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[totalSprites]; //added initializer, because templeguard sprite leaser does not exist
            for (int i = 0; i < circles; i++)
            {
                sLeaser.sprites[firstSprite + i] = new FSprite("Futile_White", true);
                sLeaser.sprites[firstSprite + i].color = color;
                sLeaser.sprites[firstSprite + i].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
            }
            int num = circles;
            for (int j = 0; j < glyphs.Length; j++)
            {
                for (int k = 0; k < glyphs[j].Length; k++)
                {
                    sLeaser.sprites[firstSprite + num] = new FSprite("haloGlyph" + glyphs[j][k].ToString(), true);
                    sLeaser.sprites[firstSprite + num].color = color;
                    num++;
                }
            }
            for (int l = 0; l < swappers.Length; l++)
                swappers[l].InitiateSprites(firstSwapperSprite + l * 3, sLeaser, rCam);
            for (int m = 0; m < lines.GetLength(0); m++)
            {
                sLeaser.sprites[firstSprite + firstLineSprite + m] = new FSprite("pixel", true);
                sLeaser.sprites[firstSprite + firstLineSprite + m].color = color;
            }
            for (int n = 0; n < smallCircles.GetLength(0); n++)
            {
                sLeaser.sprites[firstSprite + firstSmallCircleSprite + n] = new FSprite("Futile_White", true);
                sLeaser.sprites[firstSprite + firstSmallCircleSprite + n].color = color;
                sLeaser.sprites[firstSprite + firstSmallCircleSprite + n].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
            }
            AddToContainer(sLeaser, rCam, null); //added
        }


        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 headPos = overridePos ?? owner.pos; //replaced parameter with fixed variable
            Vector2 headDir = new Vector2(); //replaced parameter with fixed value
                                             //if (Options.haloOffset.Value && (owner.owner?.owner as Creature)?.mainBodyChunk != null)
                                             //    headDir = new Vector2(0f, -1f);
                                             //headDir = Custom.DirVec(owner.pos, (owner.owner.owner as Creature).mainBodyChunk.pos);
            headPos -= new Vector2(0f, 200f) * flashAmount; //makes halo flash

            //set color of all sprites
            Color curColor = Color.Lerp(prevColor, color, timeStacker);
            for (int i = firstSprite; i < firstSprite + totalSprites; i++)
                sLeaser.sprites[i].color = curColor;

            Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
            float num = Mathf.InverseLerp(10f, 150f, Vector2.Distance(vector, headPos - headDir
                * Mathf.Lerp(distAboveHead, RadAtCircle(2f + slowRingsActive * 2f, timeStacker, 0f), 0.5f)));
            int num2 = Custom.IntClamp((int)(Mathf.Lerp(lastSlowRingsActive, slowRingsActive, timeStacker)
                + Mathf.Lerp(-0.4f, 0.4f, UnityEngine.Random.value) * Mathf.InverseLerp(0.01f, 0.1f, Mathf.Abs(lastSlowRingsActive - slowRingsActive))), 2, 4);
            if (UnityEngine.Random.value < num || deactivated)
            {
                for (int i = firstSprite; i < firstSprite + totalSprites; i++)
                    sLeaser.sprites[i].isVisible = false;
                return;
            }
            for (int j = firstSprite; j < firstSprite + totalSprites; j++)
                sLeaser.sprites[j].isVisible = true;
            for (int k = 0; k < circles; k++)
            {
                sLeaser.sprites[firstSprite + k].x = vector.x - camPos.x;
                sLeaser.sprites[firstSprite + k].y = vector.y - camPos.y;
                float num3 = RadAtCircle(k - 0.5f, timeStacker, num);
                sLeaser.sprites[firstSprite + k].scale = num3 / 8f;
                sLeaser.sprites[firstSprite + k].alpha = 1f / num3;
                sLeaser.sprites[firstSprite + k].isVisible = k < num2 * 2;
            }
            int num4 = circles;
            for (int l = 0; l < glyphs.Length; l++)
            {
                for (int m = 0; m < glyphs[l].Length; m++)
                {
                    Vector2 vector2 = vector + GlyphPos(l, m, timeStacker);
                    sLeaser.sprites[firstSprite + num4].x = vector2.x - camPos.x;
                    sLeaser.sprites[firstSprite + num4].y = vector2.y - camPos.y;
                    if (dirtyGlyphs[l][m])
                    {
                        sLeaser.sprites[firstSprite + num4].element = Futile.atlasManager.GetElementWithName("haloGlyph" + glyphs[l][m].ToString());
                        dirtyGlyphs[l][m] = false;
                    }
                    sLeaser.sprites[firstSprite + num4].isVisible = UnityEngine.Random.value > num && l < num2;
                    if (glyphs[l][m] == -1 || l == 0 && glyphPositions[l][m, 0] == 1f)
                    {
                        sLeaser.sprites[firstSprite + num4].rotation = 0f;
                    }
                    else
                    {
                        sLeaser.sprites[firstSprite + num4].rotation = (m / (float)glyphs[l].Length
                            + Mathf.Lerp(rotation[l, 1], rotation[l, 0], timeStacker)) * 360f;
                    }
                    num4++;
                }
            }
            for (int n = 0; n < swappers.Length; n++)
                swappers[n].DrawSprites(firstSwapperSprite + n * 3, sLeaser, rCam, timeStacker, camPos, vector);
            for (int num5 = 0; num5 < lines.GetLength(0); num5++)
            {
                float num6 = Mathf.Lerp(lines[num5, 1], lines[num5, 0], timeStacker);
                Vector2 vector3 = Custom.DegToVec(num6 * 360f) * RadAtCircle(lines[num5, 2] * 2f + 1f, timeStacker, num) + vector;
                sLeaser.sprites[firstSprite + firstLineSprite + num5].isVisible = lines[num5, 2] < num2 - 1;
                if (UnityEngine.Random.value > num || UnityEngine.Random.value > 0.25f)
                {
                    sLeaser.sprites[firstSprite + firstLineSprite + num5].rotation = num6 * 360f;
                    sLeaser.sprites[firstSprite + firstLineSprite + num5].scaleY = RadAtCircle(lines[num5, 2] - 0.5f, timeStacker, num)
                        - RadAtCircle(lines[num5, 2] + 0.5f, timeStacker, num);
                }
                else
                {
                    vector3 = Vector2.Lerp(vector3, headPos, 0.4f);
                    sLeaser.sprites[firstSprite + firstLineSprite + num5].rotation = Custom.AimFromOneVectorToAnother(vector3, headPos);
                    sLeaser.sprites[firstSprite + firstLineSprite + num5].scaleY = Vector2.Distance(vector3, headPos) * 1.5f * UnityEngine.Random.value;
                }
                sLeaser.sprites[firstSprite + firstLineSprite + num5].x = vector3.x - camPos.x;
                sLeaser.sprites[firstSprite + firstLineSprite + num5].y = vector3.y - camPos.y;
            }
            for (int num7 = 0; num7 < smallCircles.GetLength(0); num7++)
            {
                float num8 = Mathf.Lerp(smallCircles[num7, 1], smallCircles[num7, 0], timeStacker);
                float num9 = RadAtCircle(smallCircles[num7, 2] - 0.5f, timeStacker, num);
                float num10 = RadAtCircle(smallCircles[num7, 3] - 0.5f, timeStacker, num);
                sLeaser.sprites[firstSprite + firstSmallCircleSprite + num7].isVisible = smallCircles[num7, 3] < num2 * 2;
                Vector2 vector4 = Custom.DegToVec(num8 * 360f) * Mathf.Lerp(num9, num10, 0.5f) + vector;
                sLeaser.sprites[firstSprite + firstSmallCircleSprite + num7].x = vector4.x - camPos.x;
                sLeaser.sprites[firstSprite + firstSmallCircleSprite + num7].y = vector4.y - camPos.y;
                sLeaser.sprites[firstSprite + firstSmallCircleSprite + num7].scale = (num10 - num9) / 16f;
                sLeaser.sprites[firstSprite + firstSmallCircleSprite + num7].alpha = 2f / (num10 - num9);
            }
        }


        public Vector2 GlyphPos(int circle, int glyph, float timeStacker)
        {
            //added to hide a connection position for a cursor behind the grabbed object
            //if (HaloManager.lightningType == Options.LightningTypes.TempleGuard && circle == 0 && glyph == 0 && swapperPos != null)
            //    return swapperPos.Value - this.pos;

            if (circle * 2f - Mathf.Lerp(glyphPositions[circle][glyph, 1], glyphPositions[circle][glyph, 0], timeStacker) < 0f)
                return new Vector2(0f, 0f);
            float num = Mathf.Lerp(rotation[circle, 1], rotation[circle, 0], timeStacker);
            return Custom.DegToVec((glyph / (float)glyphs[circle].Length + num) * 360f)
                * RadAtCircle(circle * 2f - Mathf.Lerp(glyphPositions[circle][glyph, 1],
                glyphPositions[circle][glyph, 0], timeStacker), timeStacker, savDisruption);
        }


        public class GlyphSwapper
        {
            public TempleGuardHalo halo;
            public Cursor[] cursors;
            public int counter;
            public int switchAt;


            public GlyphSwapper(TempleGuardHalo halo)
            {
                this.halo = halo;
                cursors = new Cursor[2];
                for (int i = 0; i < cursors.Length; i++)
                    cursors[i] = new Cursor(this, i);
            }


            public void Update()
            {
                if (counter > 0)
                    counter--;
                if (counter == switchAt)
                {
                    int num = halo.glyphs[cursors[0].pos.x][cursors[0].pos.y];
                    int num2 = halo.glyphs[cursors[1].pos.x][cursors[1].pos.y];
                    if (num == -1 && num2 == -1)
                    {
                        num = UnityEngine.Random.Range(0, 7);
                        num2 = UnityEngine.Random.Range(0, 7);
                    }
                    else if (num == num2)
                    {
                        num = -1;
                        num2 = -1;
                    }
                    else if (num == -1)
                    {
                        num = num2;
                    }
                    else if (num2 == -1)
                    {
                        num2 = num;
                    }
                    halo.glyphs[cursors[0].pos.x][cursors[0].pos.y] = num2;
                    halo.glyphs[cursors[1].pos.x][cursors[1].pos.y] = num;
                    halo.dirtyGlyphs[cursors[0].pos.x][cursors[0].pos.y] = true;
                    halo.dirtyGlyphs[cursors[1].pos.x][cursors[1].pos.y] = true;
                }
                for (int i = 0; i < cursors.Length; i++)
                    cursors[i].Update();
            }


            public void InitiateSprites(int frst, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites[frst + 2] = new FSprite("pixel", true);
                sLeaser.sprites[frst + 2].color = halo.color;
                sLeaser.sprites[frst + 2].anchorY = 0f;
                for (int i = 0; i < 2; i++)
                {
                    sLeaser.sprites[frst + i] = new FSprite("Futile_White", true);
                    sLeaser.sprites[frst + i].scale = 1.25f;
                    sLeaser.sprites[frst + i].color = halo.color;
                    sLeaser.sprites[frst + i].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
                    sLeaser.sprites[frst + i].alpha = 0.1f;
                }
            }


            public void DrawSprites(int frst, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 haloPos)
            {
                Vector2 vector = cursors[0].CursorPos(timeStacker) + haloPos;
                Vector2 vector2 = cursors[1].CursorPos(timeStacker) + haloPos;
                sLeaser.sprites[frst].x = vector.x - camPos.x;
                sLeaser.sprites[frst].y = vector.y - camPos.y;
                sLeaser.sprites[frst + 1].x = vector2.x - camPos.x;
                sLeaser.sprites[frst + 1].y = vector2.y - camPos.y;
                sLeaser.sprites[frst + 2].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
                sLeaser.sprites[frst + 2].scaleY = Vector2.Distance(vector, vector2) - 20f;
                vector += Custom.DirVec(vector, vector2) * 10f;
                sLeaser.sprites[frst + 2].x = vector.x - camPos.x;
                sLeaser.sprites[frst + 2].y = vector.y - camPos.y;
            }


            public class Cursor
            {
                public GlyphSwapper owner;
                public IntVector2 pos;
                public IntVector2 nextPos;
                public float prog;
                public float lastProg;
                public int num;


                public Cursor(GlyphSwapper owner, int num)
                {
                    this.owner = owner;
                    this.num = num;
                    pos = RandomGlyphPos();
                    nextPos = pos;
                    prog = 1f;
                    lastProg = 1f;
                }


                public void Update()
                {
                    lastProg = prog;
                    if (nextPos == pos)
                    {
                        if (UnityEngine.Random.value < owner.halo.Speed / 10f && owner.halo.glyphPositions[pos.x][pos.y, 0] == 1f && pos.x > 0)
                        {
                            owner.halo.glyphs[pos.x][pos.y] = UnityEngine.Random.Range(0, 7);
                            owner.halo.dirtyGlyphs[pos.x][pos.y] = true;
                        }
                        if (owner.counter == 0 && UnityEngine.Random.value < owner.halo.Speed / 40f
                            && owner.cursors[1 - num].prog == 1f || pos.x >= owner.halo.ringsActive)
                        {
                            nextPos = RandomGlyphPos();
                            lastProg = 0f;
                            prog = 0f;
                            return;
                        }
                    }
                    else
                    {
                        prog += 5f * Mathf.Lerp(owner.halo.Speed, 1f, 0.7f) / Mathf.Max(1f, Vector2.Distance(owner.halo.GlyphPos(pos.x, pos.y, 1f),
                            owner.halo.GlyphPos(nextPos.x, nextPos.y, 1f)));
                        if (prog >= 1f)
                        {
                            pos = nextPos;
                            prog = 1f;
                            owner.counter = (int)(Mathf.Lerp(10f, 70f, UnityEngine.Random.value) / owner.halo.Speed);
                            owner.switchAt = owner.counter / 2;
                            if (UnityEngine.Random.value < 0.5f && owner.halo.glyphs[pos.x][pos.y] > -1 && pos.y > 0)
                                owner.halo.glyphPositions[pos.x][pos.y, 0] = 1f - owner.halo.glyphPositions[pos.x][pos.y, 0];
                        }
                    }
                }


                public Vector2 CursorPos(float timeStacker)
                {
                    Vector2 a = Vector2.Lerp(owner.halo.GlyphPos(pos.x, pos.y, timeStacker),
                        owner.halo.GlyphPos(nextPos.x, nextPos.y, timeStacker), Mathf.Lerp(lastProg, prog, timeStacker));
                    Vector2 b = Vector3.Slerp(owner.halo.GlyphPos(pos.x, pos.y, timeStacker),
                        owner.halo.GlyphPos(nextPos.x, nextPos.y, timeStacker), Mathf.Lerp(lastProg, prog, timeStacker));
                    return Vector2.Lerp(a, b, 0.5f);
                }


                public IntVector2 RandomGlyphPos()
                {
                    IntVector2 intVector = new IntVector2(0, 0);

                    //added force first glyph
                    //if (HaloManager.lightningType == Options.LightningTypes.TempleGuard && owner.halo.swapperPos != null && num == 0)
                    //    return intVector;

                    intVector.x = UnityEngine.Random.Range(0, owner.halo.ringsActive);
                    intVector.y = UnityEngine.Random.Range(0, owner.halo.glyphs[intVector.x].Length);
                    return intVector;
                }
            }
        }
    }
}

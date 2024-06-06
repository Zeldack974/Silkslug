using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pom.Pom;
using RegionKit;
using RWCustom;
using UnityEngine;
using RegionKit.Modules.Objects;
using System.CodeDom;
using EffExt;

namespace Silkslug
{
    public static class DevtoolObjects
    {
        public static void Register()
        {
            //RegisterManagedObject<V1.RoomParticleSystem, V1.WholeScreenSpawnerData, ManagedRepresentation>("WholeScreenSpawner", PARTICLES_POM_CATEGORY);
            //RegisterEmptyObjectType<SawBladeData, ManagedRepresentation>("SawBlade", "Gameplay");
            RegisterManagedObject(new ManagedObjectType("SawBlade", "Gameplay", typeof(SawBlade), typeof(SawBladeData), typeof(ManagedRepresentation)));
            RegisterManagedObject(new ManagedObjectType("MovingPlatform", "Gameplay", typeof(MovingPlatform), typeof(PlatformData), typeof(ManagedRepresentation)));
            RegisterManagedObject(new ManagedObjectType("FallingBlock", "Gameplay", typeof(FallingBlock), typeof(FallingBlockData), typeof(ManagedRepresentation)));

            EffectDefinitionBuilder builder = new EffectDefinitionBuilder("FallingBlocksSettings");

            builder
                .AddFloatField("fallTime", 0.025f, 10f, 0.1f, 2f, "Falling time")
                .AddFloatField("respawnTime", 0.025f, 40f, 0.1f, 6f, "Respawn time")
                .SetEffectInitializer(FallingBlocksSettingsInitializer)
                .SetCategory("Gameplay")
                .Register();

        }

        public static void FallingBlocksSettingsInitializer(Room room, EffectExtraData data, bool firstTimeRealized)
        {
            FallingBlock.fallTicks = (int)(40f * data.GetFloat("fallTime"));
            FallingBlock.respawnTicks = (int)(40f * data.GetFloat("respawnTime"));
        }


        public class FallingBlockData : ManagedData
        {
            [IntVector2Field("scaleVec", 3, 1, IntVector2Field.IntVectorReprType.rect)]
            public IntVector2 scaleVec;
            public FallingBlockData(PlacedObject owner) : base(owner, null)
            {
            }
        }


        public class PlatformData : ManagedData
        {
            [FloatField("speed", 0f, 100, 20f, displayName: "Speed")]
            internal float speed;

            [FloatField("wait", 0f, 5f, 0.5f, displayName: "Wait")]
            internal float wait;

            [IntVector2Field("vec", 0, 2)]
            internal IntVector2 Vec;

            [IntVector2Field("scaleVec", 3, 1, IntVector2Field.IntVectorReprType.rect)]
            public IntVector2 scaleVec;
            public PlatformData(PlacedObject owner) : base(owner, null)
            {
            }
        }

        public class SawBladeData : ManagedData
        {
            [FloatField("speed", 0f, 100, 20f, displayName: "Speed")]
            internal float speed;

            [FloatField("wait", 0f, 5f, 0.5f, displayName: "Wait")]
            internal float wait;

            [Vector2Field("radVec", 70f * 0.75f, 0f, Vector2Field.VectorReprType.circle)]
            public Vector2 radVec;

            [Vector2Field("vec", 0f, 100f)]
            internal Vector2 Vec;

            public SawBladeData(PlacedObject owner) : base(owner, null)
            {
            }
        }
    }
}

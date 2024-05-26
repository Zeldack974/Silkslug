using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pom.Pom;
using RegionKit;

namespace Silkslug
{
    public static class DevtoolObjects
    {
        public static void Register()
        {
            //RegisterManagedObject<V1.RoomParticleSystem, V1.WholeScreenSpawnerData, ManagedRepresentation>("WholeScreenSpawner", PARTICLES_POM_CATEGORY);
            RegisterEmptyObjectType<SawBladeData, ManagedRepresentation>("SawBlade", "Gameplay");

        }

        public class SawBladeData : ManagedData
        {
            [IntegerField("fadeInFluke", 0, 400, 0, ManagedFieldWithPanel.ControlType.text, displayName: "Fade-in fluke")]
            public int fadeInFluke;

            [FloatField("float", 0f, 10f, 0f)]
            internal float Speed;

            public SawBladeData(PlacedObject owner) : base(owner, null)
            {
            }
        }
    }
}

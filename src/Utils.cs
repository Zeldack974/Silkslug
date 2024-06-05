using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL.MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Silkslug
{
    public class Utils
    {
        public class BoxCollisionResult
        {
            public BoxCollisionResult() { }
            public Vector2 correctionVec;
            public Vector2 center;
            public bool collide;
        }

        public static BoxCollisionResult PointInBox(Vector2 point, Vector2 min, Vector2 max, float rad = 0)
        {
            Vector2 boxCenter = (max + min) / 2f;
            min -= Vector2.one * rad;
            max += Vector2.one * rad;

            Vector2 distMax = (max - point);
            Vector2 distMin = (min - point);
            float cx = distMin.x;
            if (Math.Abs(cx) > Math.Abs(distMax.x))
                cx = distMax.x;

            float cy = distMin.y;
            if (Math.Abs(cy) > Math.Abs(distMax.y))
                cy = distMax.y;

            return new BoxCollisionResult()
            {
                center = boxCenter,
                correctionVec = new Vector2(cx, cy),
                collide = (point.x >= min.x) && (point.y >= min.y) && (point.x <= max.x) && (point.y <= max.y)
            };
        }
    }
}

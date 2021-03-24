using System;
using TaleWorlds.Library;

namespace TouhouAnalepsia
{
    class TAMath
    {
        public static double R2D(double rad)
        {
            return (180 / Math.PI) * rad;
        }
        public static double D2R(double angle)
        {
            return (Math.PI / 180) * angle;
        }

        // I don't know how this works, copied from MBShotgun
        // returns an orientation based on an eye position and the position 
        //  of the object we are looking at 
        public static Mat3 LookAtWorld(in Vec3 At, in Vec3 Eye)
        {
            Vec3 vec = (At - Eye).NormalizedCopy();
            Vec3 vec2 = Vec3.CrossProduct(vec, Vec3.Up).NormalizedCopy();
            Vec3 vec3 = Vec3.CrossProduct(vec2, vec);
            vec2.w = -Vec3.DotProduct(vec2, Eye);
            vec.w = -Vec3.DotProduct(vec, Eye);
            vec3.w = -Vec3.DotProduct(vec3, Eye);
            return new Mat3(vec2, vec, vec3);
        }
    }
}

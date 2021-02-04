using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCAD
{

    class ActorTransform
    {
        /// <param name="lookDir">viewing direction</param>
        /// <param name="upDir">up direction </param>
        public static Matrix GetMatrix(Vector3 pos, Vector3 lookDir,   Vector3 upDir)
        {
            lookDir.Normalize();
            upDir.Normalize();

            Matrix m = new Matrix();
            Vector3 vXAxis = Vector3.Cross(upDir, lookDir);

            vXAxis.Normalize();

            m.M11 = vXAxis.X;
            m.M12 = vXAxis.Y;
            m.M13 = vXAxis.Z;
            m.M14 = 0;

            m.M21 = upDir.X;
            m.M22 = upDir.Y;
            m.M23 = upDir.Z;
            m.M24 = 0;

            m.M31 = lookDir.X;
            m.M32 = lookDir.Y;
            m.M33 = lookDir.Z;
            m.M34 = 0;

            m.M41 = pos.X;
            m.M42 = pos.Y;
            m.M43 = pos.Z;
            m.M44 = 1.0f;
            return m;
        }
    }
}


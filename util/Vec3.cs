using System;
using Godot;
using Godot.Collections;

namespace ShopIsDone.Utils
{
    /// <summary>
    /// Adds more constants for Vector3.
    /// </summary>
    /// <remarks>
    /// Also includes all of Godot's Vector3 constants.
    /// </remarks>
    public static partial class Vec3
    {
        // A point very far off the tilemap
        public static Vector3 FarOffPoint = new Vector3(1000000, 1000000, 1000000);

        public static readonly Vector3 Zero = new Vector3(0, 0, 0);
        public static readonly Vector3 One = new Vector3(1, 1, 1);
        public static readonly Vector3 Right = new Vector3(1, 0, 0);
        public static readonly Vector3 BackRight = new Vector3(1, 0, 1);
        public static readonly Vector3 Back = new Vector3(0, 0, 1);
        public static readonly Vector3 BackLeft = new Vector3(-1, 0, 1);
        public static readonly Vector3 Left = new Vector3(-1, 0, 0);
        public static readonly Vector3 ForwardLeft = new Vector3(-1, 0, -1);
        public static readonly Vector3 Forward = new Vector3(0, 0, -1);
        public static readonly Vector3 ForwardRight = new Vector3(1, 0, -1);

        public static readonly Array<Vector3> DirCardinal = new Array<Vector3>{
            Right,
            Back,
            Left,
            Forward,
        };

        public static readonly Array<Vector3> Dir = new Array<Vector3>{
            Right,
            BackRight,
            Back,
            BackLeft,
            Left,
            ForwardLeft,
            Forward,
            ForwardRight,
        };

        public static readonly Vector3 RightNorm = new Vector3(1, 0, 0);
        public static readonly Vector3 BackRightNorm = new Vector3(0.7071067811865475244f, 0, 0.7071067811865475244f);
        public static readonly Vector3 BackNorm = new Vector3(0, 0, 1);
        public static readonly Vector3 BackLeftNorm = new Vector3(-0.7071067811865475244f, 0, 0.7071067811865475244f);
        public static readonly Vector3 LeftNorm = new Vector3(-1, 0, 0);
        public static readonly Vector3 ForwardLeftNorm = new Vector3(-0.7071067811865475244f, 0, -0.7071067811865475244f);
        public static readonly Vector3 ForwardNorm = new Vector3(0, 0, -1);
        public static readonly Vector3 ForwardRightNorm = new Vector3(0.7071067811865475244f, 0, -0.7071067811865475244f);

        public static readonly Array<Vector3> DirNorm = new Array<Vector3>{
            RightNorm,
            BackRightNorm,
            BackNorm,
            BackLeftNorm,
            LeftNorm,
            ForwardLeftNorm,
            ForwardNorm,
            ForwardRightNorm,
        };

        public static readonly Vector3 E = new Vector3(1, 0, 0);
        public static readonly Vector3 SE = new Vector3(1, 0, 1);
        public static readonly Vector3 S = new Vector3(0, 0, 1);
        public static readonly Vector3 SW = new Vector3(-1, 0, 1);
        public static readonly Vector3 W = new Vector3(-1, 0, 0);
        public static readonly Vector3 NW = new Vector3(-1, 0, -1);
        public static readonly Vector3 N = new Vector3(0, 0, -1);
        public static readonly Vector3 NE = new Vector3(1, 0, -1);

        public static readonly Vector3 ENorm = new Vector3(1, 0, 0);
        public static readonly Vector3 SENorm = new Vector3(0.7071067811865475244f, 0, 0.7071067811865475244f);
        public static readonly Vector3 SNorm = new Vector3(0, 0, 1);
        public static readonly Vector3 SWNorm = new Vector3(-0.7071067811865475244f, 0, 0.7071067811865475244f);
        public static readonly Vector3 WNorm = new Vector3(-1, 0, 0);
        public static readonly Vector3 NWNorm = new Vector3(-0.7071067811865475244f, 0, -0.7071067811865475244f);
        public static readonly Vector3 NNorm = new Vector3(0, 0, -1);
        public static readonly Vector3 NENorm = new Vector3(0.7071067811865475244f, 0, -0.7071067811865475244f);

        // These are always normalized, because tan(22.5 degrees) is not rational.
        public static readonly Vector3 SEE = new Vector3(0.9238795325112867561f, 0, 0.3826834323650897717f);
        public static readonly Vector3 SSE = new Vector3(0.3826834323650897717f, 0, 0.9238795325112867561f);
        public static readonly Vector3 SSW = new Vector3(-0.3826834323650897717f, 0, 0.9238795325112867561f);
        public static readonly Vector3 SWW = new Vector3(-0.9238795325112867561f, 0, 0.3826834323650897717f);
        public static readonly Vector3 NWW = new Vector3(-0.9238795325112867561f, 0, -0.3826834323650897717f);
        public static readonly Vector3 NNW = new Vector3(-0.3826834323650897717f, 0, -0.9238795325112867561f);
        public static readonly Vector3 NNE = new Vector3(0.3826834323650897717f, 0, -0.9238795325112867561f);
        public static readonly Vector3 NEE = new Vector3(0.9238795325112867561f, 0, -0.3826834323650897717f);

        public static readonly Array<Vector3> Dir16 = new Array<Vector3>{
            ENorm,
            SEE,
            SENorm,
            SSE,
            SNorm,
            SSW,
            SWNorm,
            SWW,
            WNorm,
            NWW,
            NWNorm,
            NNW,
            NNorm,
            NNE,
            NENorm,
            NEE,
        };

        public static readonly Vector3 Up = new Vector3(0, 1, 0);
        public static readonly Vector3 UpRight = new Vector3(1, 1, 0);
        public static readonly Vector3 UpBackRight = new Vector3(1, 1, 1);
        public static readonly Vector3 UpBack = new Vector3(0, 1, 1);
        public static readonly Vector3 UpBackLeft = new Vector3(-1, 1, 1);
        public static readonly Vector3 UpLeft = new Vector3(-1, 1, 0);
        public static readonly Vector3 UpForwardLeft = new Vector3(-1, 1, -1);
        public static readonly Vector3 UpForward = new Vector3(0, 1, -1);
        public static readonly Vector3 UpForwardRight = new Vector3(1, 1, -1);

        public static readonly Vector3 Down = new Vector3(0, -1, 0);
        public static readonly Vector3 DownRight = new Vector3(1, -1, 0);
        public static readonly Vector3 DownBackRight = new Vector3(1, -1, 1);
        public static readonly Vector3 DownBack = new Vector3(0, -1, 1);
        public static readonly Vector3 DownBackLeft = new Vector3(-1, -1, 1);
        public static readonly Vector3 DownLeft = new Vector3(-1, -1, 0);
        public static readonly Vector3 DownForwardLeft = new Vector3(-1, -1, -1);
        public static readonly Vector3 DownForward = new Vector3(0, -1, -1);
        public static readonly Vector3 DownForwardRight = new Vector3(1, -1, -1);

        public static readonly Vector3 UpNorm = new Vector3(0, 1, 0);
        public static readonly Vector3 UpRightNorm = new Vector3(0.7071067811865475244f, 0.7071067811865475244f, 0);
        public static readonly Vector3 UpBackRightNorm = new Vector3(0.5773502691896257645f, 0.5773502691896257645f, 0.5773502691896257645f);
        public static readonly Vector3 UpBackNorm = new Vector3(0, 0.7071067811865475244f, 0.7071067811865475244f);
        public static readonly Vector3 UpBackLeftNorm = new Vector3(-0.5773502691896257645f, 0.5773502691896257645f, 0.5773502691896257645f);
        public static readonly Vector3 UpLeftNorm = new Vector3(-0.7071067811865475244f, 0.7071067811865475244f, 0);
        public static readonly Vector3 UpForwardLeftNorm = new Vector3(-0.5773502691896257645f, 0.5773502691896257645f, -0.5773502691896257645f);
        public static readonly Vector3 UpForwardNorm = new Vector3(0, 0.7071067811865475244f, -0.7071067811865475244f);
        public static readonly Vector3 UpForwardRightNorm = new Vector3(0.5773502691896257645f, 0.5773502691896257645f, -0.5773502691896257645f);

        public static readonly Vector3 DownNorm = new Vector3(0, -1, 0);
        public static readonly Vector3 DownRightNorm = new Vector3(0.7071067811865475244f, -0.7071067811865475244f, 0);
        public static readonly Vector3 DownBackRightNorm = new Vector3(0.5773502691896257645f, -0.5773502691896257645f, 0.5773502691896257645f);
        public static readonly Vector3 DownBackNorm = new Vector3(0, -0.7071067811865475244f, 0.7071067811865475244f);
        public static readonly Vector3 DownBackLeftNorm = new Vector3(-0.5773502691896257645f, -0.5773502691896257645f, 0.5773502691896257645f);
        public static readonly Vector3 DownLeftNorm = new Vector3(-0.7071067811865475244f, -0.7071067811865475244f, 0);
        public static readonly Vector3 DownForwardLeftNorm = new Vector3(-0.5773502691896257645f, -0.5773502691896257645f, -0.5773502691896257645f);
        public static readonly Vector3 DownForwardNorm = new Vector3(0, -0.7071067811865475244f, -0.7071067811865475244f);
        public static readonly Vector3 DownForwardRightNorm = new Vector3(0.5773502691896257645f, -0.5773502691896257645f, -0.5773502691896257645f);


        // NB: I am so fucking stupid that I needed to do this. I am so fucking stupid.
        // Don't do what I did. Learn vector math. Learn linear algebra. Don't forget it
        // all the second you leave college. Don't be such a moron that you have to mentally
        // rotate and then draw on paper each individual of all 16 cases here to get the
        // answer. Please just learn math next time
        public static Vector3 TransformFacingDir(Vector3 facingDir, Vector3 viewedDir)
        {
            if (viewedDir == ForwardRight)
            {
                if (facingDir == Vector3.Left)
                {
                    return BackLeft;
                }
                else if (facingDir == Vector3.Right)
                {
                    return ForwardRight;
                }
                else if (facingDir == Vector3.Forward)
                {
                    return ForwardLeft;
                }
                else if (facingDir == Vector3.Back)
                {
                    return BackRight;
                }
                else
                {
                    throw new NotImplementedException($"Given facing direction {facingDir} not one of four allowed cases!");
                }
            }
            else if (viewedDir == ForwardLeft)
            {
                if (facingDir == Vector3.Left)
                {
                    return ForwardLeft;
                }
                else if (facingDir == Vector3.Right)
                {
                    return BackRight;
                }
                else if (facingDir == Vector3.Forward)
                {
                    return ForwardRight;
                }
                else if (facingDir == Vector3.Back)
                {
                    return BackLeft;
                }
                else
                {
                    throw new NotImplementedException($"Given facing direction {facingDir} not one of four allowed cases!");
                }
            }
            else if (viewedDir == BackRight)
            {
                if (facingDir == Vector3.Left)
                {
                    return BackRight;
                }
                else if (facingDir == Vector3.Right)
                {
                    return ForwardLeft;
                }
                else if (facingDir == Vector3.Forward)
                {
                    return BackLeft;
                }
                else if (facingDir == Vector3.Back)
                {
                    return ForwardRight;
                }
                else
                {
                    throw new NotImplementedException($"Given facing direction {facingDir} not one of four allowed cases!");
                }
            }
            else if (viewedDir == BackLeft)
            {
                if (facingDir == Vector3.Left)
                {
                    return ForwardRight;
                }
                else if (facingDir == Vector3.Right)
                {
                    return BackLeft;
                }
                else if (facingDir == Vector3.Forward)
                {
                    return BackRight;
                }
                else if (facingDir == Vector3.Back)
                {
                    return ForwardLeft;
                }
                else
                {
                    throw new NotImplementedException($"Given facing direction {facingDir} not one of four allowed cases!");
                }
            }
            else
            {
                throw new NotImplementedException($"Given viewed direction {viewedDir} not one of four allowed cases!");
            }
        }

        public static string DirToString(Vector3 dir)
        {
            if (dir == Forward)
            {
                return nameof(Forward);
            }
            else if (dir == Left)
            {
                return nameof(Left);
            }
            else if (dir == Right)
            {
                return nameof(Right);
            }
            else if (dir == Back)
            {
                return nameof(Back);
            }
            if (dir == DownRight)
            {
                return nameof(DownRight);
            }
            else if (dir == DownLeft)
            {
                return nameof(DownLeft);
            }
            else if (dir == UpRight)
            {
                return nameof(UpRight);
            }
            else if (dir == UpLeft)
            {
                return nameof(UpLeft);
            }
            else if (dir == ForwardLeft)
            {
                return nameof(ForwardLeft);
            }
            else if (dir == ForwardRight)
            {
                return nameof(ForwardRight);
            }
            else if (dir == BackLeft)
            {
                return nameof(BackLeft);
            }
            else if (dir == BackRight)
            {
                return nameof(BackRight);
            }
            else
            {
                throw new NotImplementedException($"Given direction {dir} not found in DirToString!");
            }
        }


        public static Vector3 YRotDegToFacingDir(float yRotDeg)
        {
            // Convert to 0-360 range
            var deg = Mathf.RoundToInt(yRotDeg) % 360;
            // Convert to positive
            if (Mathf.Sign(deg) == -1) deg = 360 + deg;

            // Go through cases
            if (deg == 0)
            {
                return Vector3.Forward;
            }
            else if (deg == 180)
            {
                return Vector3.Back;
            }
            else if (deg == 270)
            {
                return Vector3.Left;
            }
            else if (deg == 90)
            {
                return Vector3.Right;
            }
            else
            {
                throw new NotImplementedException($"Given angle {deg} not one of four allowed cases!");
            }
        }

        public static Vector3 YRotRadToFacingDir(float yRotRad)
        {
            return YRotDegToFacingDir(Mathf.RadToDeg(yRotRad));
        }

        public static float FacingDirToYDeg(Vector3 dir)
        {
            if (dir == Vector3.Forward)
            {
                return 0;
            }
            else if (dir == Vector3.Back)
            {
                return 180;
            }
            else if (dir == Vector3.Left)
            {
                return 270;
            }
            else if (dir == Vector3.Right)
            {
                return 90;
            }
            else
            {
                throw new NotImplementedException($"Given direction {dir} not one of four allowed cases!");
            }
        }

        public static float FacingDirToYRad(Vector3 dir)
        {
            return Mathf.DegToRad(FacingDirToYDeg(dir));
        }
    }
}
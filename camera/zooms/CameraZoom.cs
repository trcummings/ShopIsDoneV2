using Godot;

namespace ShopIsDone.Cameras.Zooms
{
    // This is for handling "Positional Zoom", i.e moving the camera physically
    // closer
    public partial class CameraZoom : Node
    {
        [Signal]
        public delegate void ZoomChangedEventHandler(float zoom);

        [Export]
        protected Camera3D _Camera;

        public float ZoomAmount { get { return GetZoom(); } }

        public virtual void UpdateZoom(double delta)
        {

        }

        public virtual void SyncClone(Camera3D clone)
        {

        }

        public virtual void ZoomIn()
        {
            //
        }

        public virtual void ZoomOut()
        {
            //
        }

        public virtual float GetZoom()
        {
            return 0;
        }

        public virtual void SetZoom(float zoomAmount)
        {
            //
        }

        public virtual void ForceZoom(float zoomAmount)
        {
            //
        }
    }
}


using System;
using System.Threading.Tasks;
using Godot;

namespace ShopIsDone.Models
{
	public interface IModel
	{
        void SetFacingDir(Vector3 facingDir);

        void Init();

        string GetDefaultAnimationName();

        string TransformAnimName(string animName);

        Task PerformAnimation(string animName, bool advance = false);

        void Show();

        void Hide();

        void PauseAnimation();

        void UnpauseAnimation();
    }
}


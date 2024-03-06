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

        Task PerformAnimation(string animName);

        string GetCurrentAnimation();

        void Show();

        void Hide();

        void PauseAnimation();

        void UnpauseAnimation();
    }
}


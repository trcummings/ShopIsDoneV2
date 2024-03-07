using System;
using System.Threading.Tasks;
using Godot;
using ShopIsDone.Core;
using ShopIsDone.Utils.Commands;

namespace ShopIsDone.Models
{
	public partial class ModelComponent : NodeComponent
    {
        [Export]
        private NodePath _ModelPath;

        protected IModel _Model;
        public IModel Model
        {
            get { return _Model; }
            set { SetModel(value); }
        }

        public override void _Ready()
        {
            base._Ready();
            _Model = GetNode<IModel>(_ModelPath);
        }

        public override void Init()
        {
            // Set render model
            SetModel(_Model);
        }

        public void SetModel(IModel newModel)
        {
            // Hide
            _Model?.Hide();
            // Set new model
            _Model = newModel;
            // Play default function
            // FIXME: This needs some kind of connection to state handler to
            // drive any idle behavior
            //_ = _Model.PerformAnimation(_Model.GetDefaultAnimationName());
            // Initialize model and set proper facing dir
            _Model.Init();
            Entity.FacingDirection = Entity.FacingDirection;
            // Show
            _Model.Show();
        }

        public async Task PerformActionAsync(string actionName)
        {
            await _Model.PerformAnimation(actionName);
        }

        public virtual Command RunPerformAction(string actionName)
        {
            return new AsyncCommand(async () => await _Model.PerformAnimation(actionName));
        }

        public void PauseAction()
        {
            _Model.PauseAnimation();
        }

        public void UnpauseAction()
        {
            _Model.UnpauseAnimation();
        }
    }
}


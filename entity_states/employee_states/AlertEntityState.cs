﻿using System;
using Godot;
using ShopIsDone.Models;

namespace ShopIsDone.EntityStates.EmployeeStates
{
    public partial class AlertEntityState : EntityState
    {
        [Export]
        public NodePath ModelPath;
        private IModel _Model;

        public override void _Ready()
        {
            base._Ready();
            _Model = GetNode<IModel>(ModelPath);
        }

        public async override void Enter()
        {
            await _Model.PerformAnimation("alert");
            base.Enter();
        }

        public override bool IsInArena()
        {
            return true;
        }

        public override bool CanAct()
        {
            return true;
        }
    }
}
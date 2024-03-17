using Godot;
using System;
using System.Linq;
using ShopIsDone.Core;
using ShopIsDone.Microgames;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Microgames.Outcomes;
using Godot.Collections;
using ShopIsDone.Utils.Extensions;
using ShopIsDone.Utils.DependencyInjection;
using ShopIsDone.Tiles;
using ShopIsDone.Widgets;
using ShopIsDone.Cameras;

namespace ShopIsDone.Tasks
{
    // This is a component that represents a task attached to an entity
    public partial class TaskComponent : NodeComponent, IOutcomeHandler, IEntityActiveHandler, IDamageTarget, IHoverableComponent
    {
        [Signal]
        public delegate void TaskProgressedEventHandler(Error status);

        [Signal]
        public delegate void TaskFinishedEventHandler();

        [Signal]
        public delegate void TaskHealthInitializedEventHandler(int current, int total, float percent);

        [Signal]
        public delegate void TaskHealthDamagedEventHandler(int amount, int current, int total, float percent);

        [Signal]
        public delegate void TaskProgressBeganEventHandler(float duration);

        [Signal]
        public delegate void TaskProgressEndedEventHandler(float duration);

        [Signal]
        public delegate void TaskProgressFailedEventHandler();

        // State variables
        [Export]
        public int TaskHealth = 0;

        // How much damage the task does to whoever engages with it
        [Export]
        public int TaskDamage = 0;

        // Task handler for each entity currently working on the task
        private Array<TaskHandler> _Operators = new Array<TaskHandler>();
        public Array<TaskHandler> Operators { get { return _Operators; } }

        [Export]
        public int MaxTaskHealth = 1;

        // Task Requirements
        [Export]
        public int OperatorsRequired = 1;

        [Export]
        public int OperatorsAllowed = 1;

        // Task cost
        [Export]
        public int APCostPerTurn = 1;

        [Export]
        public int ExcessAPGranted = 1;

        // Cosmetics
        [Export(PropertyHint.MultilineText)]
        public string TaskDescription = "";

        // Nodes
        [Export]
        private MicrogameHandler _MicrogameHandler;

        [Export]
        private NodePath _TaskCompletionHandlerPath;
        private TaskCompletionHandler _TaskCompletionHandler;

        [Export]
        private NodePath _TaskProgressHandlerPath;
        private ITaskProgressHandler _TaskProgressHandler;

        [Export]
        private ProgressBar3D _ProgressBar3D;

        [Export]
        private SelectTaskHandler _SelectTaskHandler;

        [Inject]
        private ScreenshakeService _Screenshake;

        [Inject]
        private EntityWidgetService _WidgetService;

        // Tiles that that an task handler can stand on and start a task on
        protected Array<TaskTile> _TaskTiles = new Array<TaskTile>();

        public override void _Ready()
        {
            _TaskProgressHandler = GetNode<ITaskProgressHandler>(_TaskProgressHandlerPath);
            _TaskCompletionHandler = GetNode<TaskCompletionHandler>(_TaskCompletionHandlerPath);

            // Ready interaction tiles
            _TaskTiles = GetChildren().OfType<TaskTile>().ToGodotArray();
            foreach (var tile in _TaskTiles) tile.Hide();

            TaskHealthDamaged += (int amount, int _current, int _total, float _percent) =>
            {
                _WidgetService.PopupNumber(Entity.WidgetPoint, amount, Colors.Red, Colors.Black);
            };
        }

        public override void Init()
        {
            base.Init();
            // Inject
            InjectionProvider.Inject(this);
            // Register tiles with task
            foreach (var iTile in _TaskTiles)  iTile.Task = this;
            // Init finished handler
            InjectionProvider.Inject(_TaskProgressHandler as Node);
            // Init task health
            var percent = TaskHealth / (float)MaxTaskHealth * 100;
            _ProgressBar3D.BarValue = percent;
            EmitSignal(nameof(TaskHealthInitialized), TaskHealth, MaxTaskHealth, percent);
        }

        public bool IsHoverableOnTile(Tile tile)
        {
            return _TaskTiles.Any(tt => tt.Tile == tile);
        }

        public Tile GetClosestTaskTile(Vector3 pos)
        {
            return _TaskTiles
                .OrderBy(iTile => iTile.GlobalPosition.DistanceTo(pos))
                .Select(iTile => iTile.Tile)
                .FirstOrDefault();
        }

        public Array<Tile> GetSelectTiles()
        {
            return _SelectTaskHandler
                .SelectableTiles
                .Select(t => t.Tile)
                .ToGodotArray();
        }

        public bool IsTaskComplete()
        {
            return TaskHealth <= 0;
        }

        // Is Entity Active
        public bool IsActive()
        {
            return !IsTaskComplete();
        }

        public bool IsInArena()
        {
            return !IsTaskComplete();
        }

        // To be called when the task is completed
        public Command CompleteTask()
        {
            // Return commands
            return new SeriesCommand(
                // Reward them
                RewardOperators(),
                // Free operators from task
                FinishOperators(),
                // Run completion hook
                _TaskCompletionHandler.OnTaskCompleted(this),
                // Emit finished signal
                new ActionCommand(() => {
                    EmitSignal(nameof(TaskFinished));
                })
            );
        }

        public Command ReadyOperators()
        {
            // TODO: Dispatch some sort of event to have the popups show up?
            return new Command();
        }

        // To be called when the task is meant to be progressed, even if it is in
        // an invalid state
        public Command ProgressTask()
        {
            return OperatorsCanPayEntryFeeCheck(
                HasRequiredOperatorsCheck(
                    CanTaskBeProgressedCheck(
                        // Progress the task normally
                        new SeriesCommand(
                            // Spend operator AP on task
                            new SeriesCommand(_Operators.Select(op =>
                                new SeriesCommand(
                                    new ActionCommand(op.CommitAPToTask),
                                    new WaitForCommand(Entity, 0.1f)
                            )).ToArray()),
                            new WaitForCommand(Entity, 0.25f),
                            // Run Microgame
                            _MicrogameHandler.RunMicrogame(new MicrogamePayload()
                            {
                                Targets = _Operators.Select(o => o.GetComponent<IOutcomeHandler>()).ToArray()
                            })
                        )
                    )
                )
            );
        }

        #region IOutcomeHandler and IDamageTarget
        // We are our own damage target
        public IDamageTarget DamageTarget { get { return this; } }

        public Command InflictDamage(IDamageTarget target, MicrogamePayload outcomePayload)
        {
            return target.ReceiveDamage(GetDamage(outcomePayload));
        }

        public Command ReceiveDamage(DamagePayload damage)
        {
            return new ConditionalCommand(
                // Only run this if the task actually received damage
                () => damage.Damage > 0,
                new SeriesCommand(
                    // Tick down the task by the damage payload
                    DamageTaskHealth(damage.Damage),
                    // Wait a moment for the result to have impact
                    new WaitForCommand(Entity, 0.25F),
                    // Run progress hook
                    _TaskProgressHandler.OnProgressTask(this)
                )
            );
        }

        public Command BeforeOutcomeResolution(MicrogamePayload payload)
        {
            // Only show the health bar if we lost
            return new ConditionalCommand(
                payload.LostMicrogame,
                new SeriesCommand(
                    // Show progress bar
                    new ActionCommand(() =>
                    {
                        EmitSignal(nameof(TaskProgressBegan), 0.25f);
                    }),
                    // Wait for bar
                    new WaitForCommand(Entity, 0.3f)
                )
            );
        }

        public Command AfterOutcomeResolution(MicrogamePayload payload)
        {
            // Only need to hide the health bar if we lost
            return new ConditionalCommand(
                payload.LostMicrogame,
                new SeriesCommand(
                    // Hide progress bar
                    new ActionCommand(() =>
                    {
                        EmitSignal(nameof(TaskProgressEnded), 0.25f);
                    }),
                    // Wait for bar
                    new WaitForCommand(Entity, 0.3f)
                )
            );
        }

        public DamagePayload GetDamage(MicrogamePayload outcomePayload)
        {
            return new DamagePayload()
            {
                Damage = outcomePayload.WonMicrogame() ? TaskDamage : 0,
                Source = Entity
            };
        }
        #endregion

        public Command DamageTaskHealth(int amount)
        {
            return new ActionCommand(() =>
            {
                TaskHealth = Mathf.Max(TaskHealth - amount, 0);

                // Emit Signal with damage amount
                var percent = TaskHealth / (float)MaxTaskHealth * 100;
                _ProgressBar3D.TweenValue(percent);
                EmitSignal(nameof(TaskHealthDamaged), amount, TaskHealth, MaxTaskHealth, percent);

                // Shake screen
                _Screenshake.Shake(ScreenshakeHandler.ShakePayload.ShakeSizes.Mild);
            });
        }

        public Vector3 GetOperatorCenter()
        {
            if (_Operators.Count == 0) return Vector3.Zero;

            // Return the central position of all the operators in space
            return _Operators.Aggregate(Vector3.Zero, (acc, op) => op.Entity.GlobalPosition + acc) / _Operators.Count;
        }

        public bool HasRequiredOperators()
        {
            return _Operators.Count >= OperatorsRequired;
        }

        public bool CanRegisterOperator()
        {
            return _Operators.Count <= OperatorsAllowed;
        }

        public void RegisterOperator(TaskHandler taskHandler)
        {
            // Only register the operator if we're allowed to
            if (CanRegisterOperator()) _Operators.Add(taskHandler);
        }

        public void DeregisterOperator(TaskHandler taskHandler)
        {
            _Operators.Remove(taskHandler);
        }

        // Grant all operators the AP excess from the task
        private Command RewardOperators()
        {
            return new SeriesCommand(
                _Operators.Select(op => new SeriesCommand(
                    new WaitForCommand(op.Entity, 0.15f),
                    new ActionCommand(() => op.RewardTaskCompletion(ExcessAPGranted))
                )
            ).ToArray());
        }

        private Command FinishOperators()
        {
            return new SeriesCommand(_Operators.Select(op => op.StopDoingTask()).ToArray());
        }

        // Task progression checks
        private Command HasRequiredOperatorsCheck(Command next)
        {
            return new IfElseCommand(
                // If we don't have the required number of operators
                () => !HasRequiredOperators(),
                // Run failure hook
                new SeriesCommand(
                    // Show "not enough operators" pop up
                    new ActionCommand(() =>
                    {
                        EmitSignal(nameof(TaskProgressFailed));
                    }),
                    // Run failure hook
                    FinishOperators()
                ),
                // Otherwise, progress normally
                next
            );
        }

        private Command CanTaskBeProgressedCheck(Command next)
        {
            // Delegate progression checking down to the task's progression handler
            return new IfElseCommand(
                () => !_TaskProgressHandler.CanProgressTask(this),
                // Fail to progress
                _TaskProgressHandler.OnFailToProgress(this),
                // Progress
                next
            );
        }

        private Command OperatorsCanPayEntryFeeCheck(Command next)
        {
            return new SeriesCommand(
                // Loop through each operator and make sure they can pay the AP
                new SeriesCommand(_Operators.Select(op => new ConditionalCommand(
                    () => !op.CanProgressTask(),
                    // If they can't, make them stop the task and show a "not
                    // enough AP popup"
                    new SeriesCommand(
                        op.StopDoingTask(),
                        // TODO: Have pawn look disappointed
                        new Command()
                    )
                )).ToArray()),
                next
            );
        }

    }
}

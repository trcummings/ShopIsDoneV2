using Godot;
using System;
using System.Linq;
using ShopIsDone.Core;
using ShopIsDone.Microgames;
using ShopIsDone.Utils.Positioning;
using ShopIsDone.Utils.Commands;
using ShopIsDone.Microgames.Outcomes;
using Godot.Collections;

namespace ShopIsDone.Tasks
{
    // This is a component that represents a task attached to an entity
    public partial class TaskComponent : NodeComponent, IOutcomeHandler, IEntityActiveHandler
    {
        [Signal]
        public delegate void TaskProgressedEventHandler(Error status);

        [Signal]
        public delegate void TaskFinishedEventHandler();

        [Signal]
        public delegate void TaskHealthDamagedEventHandler(int amount, int current, int total);

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
        private TaskCompletionHandler _TaskCompletionHandler;

        [Export]
        private NodePath _TaskProgressHandlerPath;
        private IOnTaskProgressComponent _TaskProgressHandler;

        public override void _Ready()
        {
            _TaskProgressHandler = GetNode<IOnTaskProgressComponent>(_TaskProgressHandlerPath);
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

        public Command HandleOutcome(MicrogamePayload payload)
        {
            return new IfElseCommand(
                // Win check
                () => payload.Outcome == Microgame.Outcomes.Win,
                // If the player wins, damage the task
                RunTaskProgress(payload.Targets.Select(t => t.GetDamage().Damage).ToArray()),
                // On Lose
                new Command()
            );
        }

        public Command RunTaskProgress(int[] damage)
        {
            return new SeriesCommand(
                // Show progress bar
                new ActionCommand(() =>
                {
                    EmitSignal(nameof(TaskProgressBegan), 0.25f);
                }),
                // Wait for bar
                new WaitForCommand(Entity, 0.3f),
                // Progress the task
                new SeriesCommand(damage.Select(d => new SeriesCommand(
                    // TODO: Resolve any penalties the task might incur

                    // Tick down the task by the damage payload
                    DamageTaskHealth(d),
                    // Wait a moment for the result to have impact
                    new WaitForCommand(Entity, 0.25F)
                )).ToArray()),
                // Run progress hook
                _TaskProgressHandler.OnProgressTask(this),
                // Hide progress bar
                new ActionCommand(() =>
                {
                    EmitSignal(nameof(TaskProgressEnded), 0.25f);
                }),
                // Wait for bar
                new WaitForCommand(Entity, 0.3f)
            );
        }

        public DamagePayload GetDamage()
        {
            return new DamagePayload()
            {
                Health = TaskHealth,
                Defense = 0,
                DrainDefense = 0,
                Damage = TaskDamage,
                Drain = 0,
                Piercing = 0
            };
        }

        public Command DamageTaskHealth(int amount)
        {
            return new ActionCommand(() =>
            {
                TaskHealth = Mathf.Max(TaskHealth - amount, 0);

                // Emit Signal with damage amount
                EmitSignal(nameof(TaskHealthDamaged), amount, TaskHealth, MaxTaskHealth);
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

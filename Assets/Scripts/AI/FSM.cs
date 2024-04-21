using System.Collections.Generic;
using System.Linq;

// Defer function to trigger activation condition
// Returns true when transition can fire
public delegate bool FSMCondition();

// Defer function to perform action
public delegate void FSMAction();

public class FSMTransition
{
    // The method to evaluate if the transition is ready to fire
    public readonly FSMCondition Condition;

    // A list of actions to perform when this transition fires
    private readonly List<FSMAction> _actions = new();

    public FSMTransition(FSMCondition condition, FSMAction[] actions = null)
    {
        Condition = condition;
        if (actions != null) _actions.AddRange(actions);
    }

    // Call all  actions
    public void Fire()
    {
        foreach (FSMAction action in _actions) action();
    }
}

public class FSMState
{
    // Arrays of actions to perform based on transitions fire (or not)
    // Getters and setters are preferable, but we want to keep the source clean
    public readonly List<FSMAction> EnterActions = new();
    public readonly List<FSMAction> StayActions = new();
    public readonly List<FSMAction> ExitActions = new();

    // A dictionary of transitions and the states they are leading to
    private readonly Dictionary<FSMTransition, FSMState> _links = new();

    public void AddTransition(FSMTransition transition, FSMState target)
    {
        _links[transition] = target;
    }

    public FSMTransition VerifyTransitions()
    {
        return _links.Keys.FirstOrDefault(t => t.Condition());
    }

    public FSMState NextState(FSMTransition t)
    {
        return _links[t];
    }

    // These methods will perform the actions in each list
    public void Enter()
    {
        foreach (FSMAction a in EnterActions) a();
    }

    public void Stay()
    {
        foreach (FSMAction a in StayActions) a();
    }

    public void Exit()
    {
        foreach (FSMAction a in ExitActions) a();
    }
}

public class FSM
{
    // Current state
    public FSMState Current { get; private set; }

    public FSM(FSMState state)
    {
        Current = state;
        Current.Enter();
    }

    // Examine transitions leading out from the current state
    // If a condition is activated, then:
    // (1) Execute actions associated to exit from the current state
    // (2) Execute actions associated to the firing transition
    // (3) Retrieve the new state and set is as the current one
    // (4) Execute actions associated to entering the new current state
    // Otherwise, if no condition is activated,
    // (5) Execute actions associated to staying into the current state

    public void Update()
    {
        // NOTE: this is NOT a MonoBehaviour
        FSMTransition transition = Current.VerifyTransitions();
        if (transition != null)
        {
            Current.Exit(); // 1
            transition.Fire(); // 2
            Current = Current.NextState(transition); // 3
            Current.Enter(); // 4
        }
        else
        {
            Current.Stay(); // 5
        }
    }
}
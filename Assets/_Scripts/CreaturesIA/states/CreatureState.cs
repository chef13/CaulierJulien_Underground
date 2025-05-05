public abstract class CreatureState
{
    protected CreatureAI creature;

    public CreatureState(CreatureAI creature)
    {
        this.creature = creature;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
}
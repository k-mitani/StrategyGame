using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ManagementSequence
{
    public Character actor;

    public void Do()
    {

    }
}

public class ManagementActionContext
{
    public Character Actor { get; set; }
    public Castle Location { get; set; }

    public CellManager CellManager { get; set; }
}

public abstract class ManagementAction
{
    public ManagementActionContext Context { get; set; }
    protected Character Actor => Context.Actor;
    protected Castle Location => Context.Location;

    public abstract int Cost { get; }

    protected bool HasEnoughPoint()
    {
        return Actor.commandPoints >= Cost;
    }

    public abstract bool CanDo();
    public abstract void Do();

    public void PayCost()
    {
        Actor.commandPoints -= Cost;
    }

    public bool TryDoAndPayCost()
    {
        if (!CanDo()) return false;
        Do();
        PayCost();
        return true;
    }
}

public class MarchAction : ManagementAction
{
    public Character Commander { get; set; }
    public MarchActionTarget Target { get; set; }

    public override int Cost { get; } = 5;

    public override bool CanDo() =>
        HasEnoughPoint() &&
        // 出撃者が城にいる。
        Commander.IsInCastle &&
        // 城内に2人以上待機中の人材がいる。
        Location.Characters.Count(c => c.IsInCastle) > 1 &&
        // 城に敵軍が隣接していない。
        Context.CellManager.GetNeighbors(Location.Cell)
            .All(c => c.force?.Country.Equals(Location.Country) ?? true) &&
        // 隣接位置に空きがある。
        Context.CellManager.GetNeighbors(Location.Cell)
            .Any(c => c.force == null);

    public override void Do()
    {
        var location = Context.CellManager.GetNeighbors(Location.Cell)
            .Where(c => c.force == null)
            .OrderBy(c => c.Distance(Target.Location))
            .First();

        var force = new Force()
        {
            Location = location,
            Commander = Commander,
            Target = Target,
        };
        location.force = force;
        Commander.State = Character.CharacterState.Marching;
    }

    public abstract class MarchActionTarget
    {
        public abstract Cell Location { get; }
    }
    public class LocationTarget : MarchActionTarget
    {
        public Cell Target { get; set; }
        public override Cell Location => Target;
    }
    public class ForceTarget : MarchActionTarget
    {
        public Force Target { get; set; }
        public override Cell Location => Target.Location;
    }
}

public class PunishAction : ManagementAction
{
    public Character Target { get; set; }
    public override int Cost { get; } = 9;
    public override bool CanDo() =>
        HasEnoughPoint() &&
        // 行動者が城にいる。
        Actor.IsInCastle &&
        // 対象が城にいる。
        Target.IsInCastle;

    public override void Do()
    {
        TODO.Instance.StartBattle(Actor, Target);
    }
}
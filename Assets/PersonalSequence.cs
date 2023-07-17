using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class PersonalSequence
{
    private class PersonalActions
    {
        public RecruitSoldierAction RecruitSoldier { get; } = new();
        public TrainSoldierAction TrainSoldier { get; } = new();
        public ShuffleFormationAction ShuffleFormation { get; } = new();
        public ImproveAgricultureAction ImproveAgriculture { get; } = new();
        public ImproveCommerceAction ImproveCommerce { get; } = new();
        public ImproveFortressAction ImproveFortress { get; } = new();

        public void SetContext(PersonalActionContext context)
        {
            RecruitSoldier.Context = context;
            TrainSoldier.Context = context;
            ShuffleFormation.Context = context;
            ImproveAgriculture.Context = context;
            ImproveCommerce.Context = context;
            ImproveFortress.Context = context;
        }
    }

    public Character actor;
    public void Do()
    {
        // 前準備を行う。
        var (context, actions) = Prepare();

        while (true)
        {
            // 兵士が雇えるなら雇う。
            if (actions.RecruitSoldier.TryDoAndPayCost())
            {
                continue;
            }
            // 軍務を行う。
            var doMilitaryProb = actor.martial / (actor.martial + actor.stewardship);
            var doMilitary = Random.value < doMilitaryProb;
            if (doMilitary)
            {
                if (actions.TrainSoldier.TryDoAndPayCost())
                {
                    continue;
                }
            }
            // 政務を行う。
            else
            {
                var doAgriProb = 1f / 3f;
                var doAgri = Random.value < doAgriProb;
                var doCommerceProb = 1f / 2f;
                var doCommerce = Random.value < doCommerceProb;
                if (doAgri)
                {
                    if (actions.ImproveAgriculture.TryDoAndPayCost())
                    {
                        continue;
                    }
                }
                else if (doCommerce)
                {
                    if (actions.ImproveCommerce.TryDoAndPayCost())
                    {
                        continue;
                    }
                }
                else
                {
                    if (actions.ImproveFortress.TryDoAndPayCost())
                    {
                        continue;
                    }
                }
            }
            // 何もできなかったら終了。
            break;
        }
    }

    private (PersonalActionContext, PersonalActions) Prepare()
    {
        var context = new PersonalActionContext()
        {
            Actor = actor,
            Location = actor.location,
        };
        var actions = new PersonalActions();
        actions.SetContext(context);
        return (context, actions);
    }
}

public class PersonalActionContext
{
    public Character Actor { get; set; }
    public Castle Location { get; set; }
}

public abstract class PersonalAction
{
    public PersonalActionContext Context { get; set; }
    protected Character Actor => Context.Actor;
    protected Castle Location => Context.Location;

    public abstract int Cost { get; }

    protected bool HasEnoughMoney()
    {
        return Actor.money >= Cost;
    }


    public abstract bool CanDo();
    public abstract void Do();

    public void PayCost()
    {
        Actor.money -= Cost;
    }

    public bool TryDoAndPayCost()
    {
        if (!CanDo()) return false;
        Do();
        PayCost();
        return true;
    }
}

/// <summary>
/// 兵を雇う。
/// </summary>
public class RecruitSoldierAction : PersonalAction
{
    public override int Cost { get; } = 4;

    public override bool CanDo()
    {
        return HasEnoughMoney() && Actor.formation.HasEmptySlot;
    }

    public override void Do()
    {
        Actor.formation.AddRecruitSoldier();
        Actor.AddContribution(Cost / 3f);
    }
}

/// <summary>
/// 兵を訓練する。
/// </summary>
public class TrainSoldierAction : PersonalAction
{
    public override int Cost { get; } = 3;

    public override bool CanDo()
    {
        return HasEnoughMoney() && Actor.formation.HasAnySoldier;
    }

    public override void Do()
    {
        foreach (var aliveSlot in Actor.formation.Alives)
        {
            aliveSlot.AddExperience(1);
            aliveSlot.PromoteIfCan();
        }
        Actor.AddContribution(Cost / 3f);
    }
}

/// <summary>
/// 陣形をランダムに変更する。
/// </summary>
public class ShuffleFormationAction : PersonalAction
{
    public override int Cost { get; } = 1;

    public override bool CanDo()
    {
        return HasEnoughMoney() && Actor.formation.HasAnySoldier;
    }

    public override void Do()
    {
        var randomSlots = Actor.formation.Slots.OrderBy(x => Random.value).ToList();
        Actor.formation.row1.slots = randomSlots.Take(10).ToList();
        Actor.formation.row2.slots = randomSlots.Skip(10).Take(10).ToList();
        Actor.formation.row3.slots = randomSlots.Skip(20).Take(10).ToList();
    }
}

/// <summary>
/// 城塞を改善する。
/// </summary>
public class ImproveFortressAction : PersonalAction
{
    public override int Cost { get; } = 3;

    public override bool CanDo()
    {
        return HasEnoughMoney();
    }

    public override void Do()
    {
        Location.ImproveFortress(Actor.stewardship);
        Actor.AddContribution(Cost);
    }
}

/// <summary>
/// 商業を改善する。
/// </summary>
public class ImproveCommerceAction : PersonalAction
{
    public override int Cost { get; } = 2;

    public override bool CanDo()
    {
        return HasEnoughMoney();
    }

    public override void Do()
    {
        Location.ImproveCommerce(Actor.stewardship);
        Actor.AddContribution(Cost);
    }
}

/// <summary>
/// 農業を改善する。
/// </summary>
public class ImproveAgricultureAction : PersonalAction
{
    public override int Cost { get; } = 2;

    public override bool CanDo()
    {
        return HasEnoughMoney();
    }

    public override void Do()
    {
        Location.ImproveAgriculture(Actor.stewardship);
        Actor.AddContribution(Cost);
    }
}

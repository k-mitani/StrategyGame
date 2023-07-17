using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 陣形
/// </summary>
public class BattleFormation
{
    public BattleRow row1;
    public BattleRow row2;
    public BattleRow row3;
    public IEnumerable<BattleRow> Rows => new[] { row1, row2, row3 };
    public IEnumerable<SoldierSlot> Slots => Rows.SelectMany(r => r.slots);
    public IEnumerable<SoldierSlot> Alives => Rows.SelectMany(r => r.Alives);

    public bool HasEmptySlot => Rows.Any(r => r.HasEmptySlot);
    public bool HasAnySoldier => Rows.Any(r => r.slots.Count > 0);

    public void AddRecruitSoldier()
    {
        var emptySlot = Slots.FirstOrDefault(r => r.IsEmpty);
        Debug.Assert(emptySlot != null);
        emptySlot.Recruit();
    }
}

/// <summary>
/// 隊列
/// </summary>
public class BattleRow
{
    public List<SoldierSlot> slots;
    public IEnumerable<SoldierSlot> Alives => slots.Where(s => s.IsAlive);

    public float MeanHp => Alives.Select(s => s.hp).DefaultIfEmpty(0).Average();
    public float MinHp => Alives.Select(s => s.hp).DefaultIfEmpty(0).Min();

    public bool HasEmptySlot => slots.Any(s => s.IsEmpty);
}

/// <summary>
/// 兵士
/// </summary>
public class SoldierSlot
{
    public int level;
    public int experience;
    public float hp;
    public float hpMax;

    public bool IsAlive => !IsEmpty && hp > 0;
    public bool IsEmpty => level == 0;

    internal void AddExperience(int v)
    {
        experience += v;
    }

    internal void PromoteIfCan()
    {
        if (experience <= 100) return;
        experience -= 100;
        level++;
    }

    internal void Recruit()
    {
        Debug.Assert(IsEmpty);

        level = 1;
        experience = 0;
        hpMax = 20;
        hp = hpMax;
    }
}


/// <summary>
/// 戦闘用キャラクターデータ
/// </summary>
public class BattleActor
{
    public Character character;
    public BattleFormation formation;
    public float actionPoints;
    public NextAction nextAction;
    public enum NextAction
    {
        None,
        ForwardRow2,
        ForwardRow3,
        Rest,
        Retreat,
    }

    /// <summary>
    /// 全滅していたらtrue
    /// </summary>
    public bool IsEliminated =>
        formation.row1.slots.Count == 0 &&
        formation.row2.slots.Count == 0 &&
        formation.row3.slots.Count == 0;

    public void Swap12()
    {
        (formation.row2, formation.row1) = (formation.row1, formation.row2);
    }

    public void Swap23()
    {
        (formation.row3, formation.row2) = (formation.row2, formation.row3);
    }
}

/// <summary>
/// 戦闘シーケンス
/// </summary>
public class BattleSequence
{
    /// <summary>
    /// 攻撃側
    /// </summary>
    public BattleActor attacker;
    /// <summary>
    /// 防御側
    /// </summary>
    public BattleActor defender;

    public void DoBattle()
    {
        // 戦闘開始
        // 初期状態決定
        InitializeBattle();

        while (true)
        {
            // 1列目が空いていれば、2列目、3列目を前進させる。
            Forward23RowsIf1RowIsEmpty(attacker);
            Forward23RowsIf1RowIsEmpty(defender);

            // 行動決定
            DecideAction(attacker);
            DecideAction(defender);

            // 撤退が選ばれていたら、戦闘終了処理へ
            if (attacker.nextAction == BattleActor.NextAction.Retreat ||
                defender.nextAction == BattleActor.NextAction.Retreat)
            {
                break;
            }

            // 隊列移動・休憩処理
            PlayPreBattleAction(attacker);
            PlayPreBattleAction(defender);

            // 戦闘処理 x 10
            PlayBattle();

            // どちらかが全滅していたら戦闘終了処理へ
            if (attacker.IsEliminated || defender.IsEliminated)
            {
                break;
            }

            // アクションポイントを補充する。
            attacker.actionPoints += 100 * Random.value * attacker.character.intelligence / 100f;
            defender.actionPoints += 100 * Random.value * defender.character.intelligence / 100f;
        }

        // 戦闘終了
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void InitializeBattle()
    {
        attacker.actionPoints = Random.value * 300 * attacker.character.intelligence / 100f;
        defender.actionPoints = Random.value * 300 * attacker.character.intelligence / 100f;
    }

    private void Forward23RowsIf1RowIsEmpty(BattleActor actor)
    {
        actor.Swap12();
        actor.Swap23();
    }

    /// <summary>
    /// 行動を決定します。
    /// </summary>
    private void DecideAction(BattleActor actor)
    {
        if (actor.actionPoints >= 100)
        {
            // 1列列の最小HPが10以下で、
            // 2列列の兵数が十分残っていれば、2列目を前進させる。
            if (actor.formation.row1.MinHp <= 10 &&
                actor.formation.row2.MinHp >= 15)
            {
                actor.nextAction = BattleActor.NextAction.ForwardRow2;
                actor.actionPoints = -100;
                return;
            }
            // 2列目と3列目も同様
            if (
                actor.formation.row2.MinHp <= 10 &&
                actor.formation.row3.MinHp >= 15)
            {
                actor.nextAction = BattleActor.NextAction.ForwardRow3;
                actor.actionPoints = -100;
                return;
            }
            // ポイントが余っていたら休憩する。
            if (actor.actionPoints >= 200)
            {
                actor.nextAction = BattleActor.NextAction.Rest;
                actor.actionPoints = -200;
                return;
            }
        }

        actor.nextAction = BattleActor.NextAction.None;
    }

    /// <summary>
    /// 隊列移動・休憩処理
    /// </summary>
    private void PlayPreBattleAction(BattleActor actor)
    {
        switch (actor.nextAction)
        {
            case BattleActor.NextAction.ForwardRow2:
                actor.Swap12();
                break;
            case BattleActor.NextAction.ForwardRow3:
                actor.Swap23();
                break;
            case BattleActor.NextAction.Rest:
                // 生き残っている兵士のhpをランダムに回復する。
                foreach (var row in new[] { actor.formation.row1, actor.formation.row2, actor.formation.row3 })
                {
                    foreach (var soldier in row.slots)
                    {
                        if (soldier.hp > 0)
                        {
                            soldier.hp = Mathf.Min(soldier.hpMax, soldier.hp + Random.value * 10);
                        }
                    }
                }
                break;
            case BattleActor.NextAction.None:
            case BattleActor.NextAction.Retreat:
                // 何もしない。
                break;
            default:
                throw new System.NotImplementedException(actor.nextAction.ToString());
        }
    }

    private void PlayBattle()
    {
        // ランダムな順番で兵士を攻撃させる。
        var soldiers = new[] { attacker, defender }
            .SelectMany(a => a.formation.row1.slots.Select(s => (a, s)))
            .Where(x => x.s.hp > 0)
            .OrderBy(x => Random.value);
        foreach (var (actor, soldier) in soldiers)
        {
            if (soldier.hp == 0) continue;
            var opponent = actor == attacker ? defender : attacker;

            var attackTarget = opponent.formation.row1.slots
                .Where(s => s.hp > 0)
                .OrderBy(s => Random.value)
                .FirstOrDefault();

            if (attackTarget == null) continue;

            var damage = attacker.character.martial / 100f * (0.5f + Random.value / 2) * soldier.level;
            attackTarget.hp = Mathf.Max(0, attackTarget.hp - damage);
        }
    }
}










using TerminalHyperspace.Models;
using TerminalHyperspace.UI;

namespace TerminalHyperspace.Engine;

public class CombatEngine
{
    private readonly Character _player;
    private readonly Character _enemy;
    private readonly Terminal _term;
    private readonly GameState _state;

    public CombatEngine(Character player, Character enemy, Terminal term, GameState state)
    {
        _player = player;
        _enemy = enemy;
        _term = term;
        _state = state;
        _enemy.InitializeResolve();
    }

    public bool RunCombat()
    {
        _term.Divider();
        _term.Combat($"⚔ COMBAT: {_player.Name} vs {_enemy.Name}");
        _term.Combat($"  Your Resolve: {_player.CurrentResolve}/{_player.Resolve}  |  Enemy Resolve: {_enemy.CurrentResolve}/{_enemy.Resolve}");
        _term.Combat($"  Your Defense: {_player.Defense}  |  Enemy Defense: {_enemy.Defense}");
        _term.Divider();

        // Initiative
        var playerInit = _player.Initiative;
        var enemyInit = _enemy.Initiative;
        _term.DiceRoll($"Initiative — You: {playerInit}  |  {_enemy.Name}: {enemyInit}");

        bool playerFirst = playerInit >= enemyInit;
        _term.Narrative(playerFirst
            ? "You seize the initiative!"
            : $"{_enemy.Name} acts first!");

        while (!_player.IsDefeated && !_enemy.IsDefeated)
        {
            _term.Blank();
            _term.Combat($"[Resolve] You: {_player.CurrentResolve}/{_player.Resolve} | {_enemy.Name}: {_enemy.CurrentResolve}/{_enemy.Resolve}");

            if (playerFirst)
            {
                if (!PlayerTurn()) return true; // player fled
                if (_enemy.IsDefeated) break;
                EnemyTurn();
            }
            else
            {
                EnemyTurn();
                if (_player.IsDefeated) break;
                if (!PlayerTurn()) return true; // player fled
            }
        }

        _term.Blank();
        if (_enemy.IsDefeated)
        {
            _term.Combat($"★ {_enemy.Name} is defeated!");
            LootEnemy();
            return true;
        }
        else
        {
            _term.Combat("You have been defeated...");
            return false;
        }
    }

    private bool PlayerTurn()
    {
        _term.Prompt("Your action: [a]ttack  [d]efend  [u]se item  [f]lee");
        var input = _term.ReadInput().ToLower().Trim();

        switch (input)
        {
            case "a":
            case "attack":
                PlayerAttack();
                return true;
            case "d":
            case "defend":
                _term.Narrative("You brace yourself and focus on defense.");
                _term.Mechanic("+2 to Defense this round.");
                return true;
            case "u":
            case "use":
                UseItem();
                return true;
            case "f":
            case "flee":
                return AttemptFlee();
            default:
                _term.Error("Invalid action. You hesitate!");
                return true;
        }
    }

    private void PlayerAttack()
    {
        var vehicle = _player.ActiveVehicle;

        if (vehicle != null && vehicle.Weapons.Count > 0)
        {
            VehicleAttack(vehicle, _player, _enemy);
            return;
        }

        var weapon = _player.EquippedWeapon;
        if (weapon == null)
        {
            // Unarmed brawl
            var atkCode = _player.GetBestFor(SkillType.Brawl);
            bool fpDouble = ForceRoller.PromptForcePointDouble(_state, _term);
            var atkRoll = DiceRoller.Roll(atkCode);
            _term.DiceRoll($"Brawl attack: {atkRoll}");
            int atkTotal = ForceRoller.ApplyForcePointDouble(atkRoll.Total, fpDouble, _term);

            int defense = _enemy.Defense;
            _term.Mechanic($"vs {_enemy.Name}'s Defense: {defense}");

            if (atkTotal >= defense)
            {
                var dmg = DiceRoller.Roll(new DiceCode(_player.GetAttribute(AttributeType.Strength).Dice));
                _term.DiceRoll($"Unarmed damage: {dmg}");
                int rawDmg = dmg.Total;
                bool crit = atkTotal >= defense * 2;
                if (crit)
                {
                    rawDmg *= 2;
                    _term.Combat($"★ CRITICAL HIT! Damage doubled: {dmg.Total} → {rawDmg}");
                }
                int finalDmg = ApplyArmor(rawDmg, _enemy);
                _enemy.CurrentResolve -= finalDmg;
                _term.Combat($"You land a solid blow! {_enemy.Name} takes {finalDmg} damage.");
            }
            else
            {
                _term.Narrative("Your attack misses!");
            }
            return;
        }

        var skill = weapon.AttackSkill ?? SkillType.Blasters;
        var attackCode = _player.GetBestFor(skill);
        bool fpDoubleWeapon = ForceRoller.PromptForcePointDouble(_state, _term);
        var attackRoll = DiceRoller.Roll(attackCode);
        _term.DiceRoll($"{skill} attack with {weapon.Name}: {attackRoll}");
        int attackTotal = ForceRoller.ApplyForcePointDouble(attackRoll.Total, fpDoubleWeapon, _term);

        int def = _enemy.Defense;
        _term.Mechanic($"vs {_enemy.Name}'s Defense: {def}");

        if (attackTotal >= def)
        {
            var damageRoll = DiceRoller.Roll(weapon.Damage);
            _term.DiceRoll($"Damage ({weapon.Name}): {damageRoll}");
            int rawDmg = damageRoll.Total;
            bool crit = attackTotal >= def * 2;
            if (crit)
            {
                rawDmg *= 2;
                _term.Combat($"★ CRITICAL HIT! Damage doubled: {damageRoll.Total} → {rawDmg}");
            }
            int finalDmg = ApplyArmor(rawDmg, _enemy);
            _enemy.CurrentResolve -= finalDmg;
            _term.Combat($"Hit! {_enemy.Name} takes {finalDmg} damage.");
        }
        else
        {
            _term.Narrative("Your shot goes wide!");
        }

        // Consumable weapons (grenades, detonators) are spent whether they hit or miss.
        if (weapon.IsConsumable)
        {
            _player.Inventory.Remove(weapon);
            if (_player.EquippedWeapon == weapon)
                _player.EquippedWeapon = null;
            _term.Mechanic($"{weapon.Name} consumed. ({_player.Inventory.Count(i => i.Name == weapon.Name)} remaining)");
        }
    }

    private void VehicleAttack(Vehicle vehicle, Character attacker, Character target)
    {
        if (vehicle.Weapons.Count == 0)
        {
            _term.Error("This vehicle has no weapons!");
            return;
        }

        VehicleWeapon selectedWeapon;
        if (vehicle.Weapons.Count == 1)
        {
            selectedWeapon = vehicle.Weapons[0];
        }
        else
        {
            _term.Prompt("Select weapon:");
            for (int i = 0; i < vehicle.Weapons.Count; i++)
                _term.Info($"  [{i + 1}] {vehicle.Weapons[i]}");
            var choice = _term.ReadInput().Trim();
            if (!int.TryParse(choice, out int idx) || idx < 1 || idx > vehicle.Weapons.Count)
            {
                selectedWeapon = vehicle.Weapons[0];
                _term.Info($"Defaulting to {selectedWeapon.Name}.");
            }
            else
            {
                selectedWeapon = vehicle.Weapons[idx - 1];
            }
        }

        var attackCode = attacker.GetBestFor(selectedWeapon.AttackSkill);
        bool fpDouble = attacker == _player && ForceRoller.PromptForcePointDouble(_state, _term);
        var attackRoll = DiceRoller.Roll(attackCode);
        _term.DiceRoll($"{selectedWeapon.AttackSkill} with {selectedWeapon.Name}: {attackRoll}");
        int attackTotal = ForceRoller.ApplyForcePointDouble(attackRoll.Total, fpDouble, _term);

        int def = target.Defense;
        _term.Mechanic($"vs {target.Name}'s Defense: {def}");

        if (attackTotal >= def)
        {
            var damageRoll = DiceRoller.Roll(selectedWeapon.Damage);
            _term.DiceRoll($"Vehicle weapon damage: {damageRoll}");
            int rawDmg = damageRoll.Total;
            bool crit = attacker == _player && attackTotal >= def * 2;
            if (crit)
            {
                rawDmg *= 2;
                _term.Combat($"★ CRITICAL HIT! Damage doubled: {damageRoll.Total} → {rawDmg}");
            }
            int finalDmg = ApplyArmor(rawDmg, target);
            target.CurrentResolve -= finalDmg;
            _term.Combat($"Direct hit! {target.Name} takes {finalDmg} damage.");
        }
        else
        {
            _term.Narrative("Your shot streaks past the target!");
        }
    }

    private void EnemyTurn()
    {
        _term.Blank();
        _term.Combat($"— {_enemy.Name}'s turn —");

        SkillType skill = SkillType.Brawl;
        string weaponName = "fists";
        DiceCode damage = new(_enemy.GetAttribute(AttributeType.Strength).Dice);

        if (_enemy.EquippedWeapon != null)
        {
            skill = _enemy.EquippedWeapon.AttackSkill ?? SkillType.Blasters;
            weaponName = _enemy.EquippedWeapon.Name;
            damage = _enemy.EquippedWeapon.Damage;
        }

        var attackCode = _enemy.GetBestFor(skill);
        var attackRoll = DiceRoller.Roll(attackCode);
        _term.DiceRoll($"{_enemy.Name} attacks with {weaponName}: {attackRoll}");

        int def = _player.Defense;
        _term.Mechanic($"vs your Defense: {def}");

        if (attackRoll.Total >= def)
        {
            var damageRoll = DiceRoller.Roll(damage);
            _term.DiceRoll($"Damage: {damageRoll}");
            int finalDmg = ApplyArmor(damageRoll.Total, _player);
            _player.CurrentResolve -= finalDmg;
            _term.Combat($"You take {finalDmg} damage!");
        }
        else
        {
            _term.Narrative($"{_enemy.Name}'s attack misses you!");
        }
    }

    private bool AttemptFlee()
    {
        bool fpDouble = ForceRoller.PromptForcePointDouble(_state, _term);
        var playerRoll = DiceRoller.Roll(_player.GetBestFor(SkillType.Agility));
        var enemyRoll = DiceRoller.Roll(_enemy.GetBestFor(SkillType.Agility));
        _term.DiceRoll($"Flee attempt — You: {playerRoll} vs {_enemy.Name}: {enemyRoll}");
        int playerTotal = ForceRoller.ApplyForcePointDouble(playerRoll.Total, fpDouble, _term);

        if (playerTotal >= enemyRoll.Total)
        {
            _term.Narrative("You disengage and escape!");
            return false; // false = exited combat loop via flee
        }
        else
        {
            _term.Narrative("You can't get away!");
            EnemyTurn(); // enemy gets a free attack
            return true;
        }
    }

    private void UseItem()
    {
        var medpacks = _player.Inventory.Where(i => i.Name == "Medpack").ToList();
        if (medpacks.Count == 0)
        {
            _term.Error("You have no usable items! You waste your turn fumbling.");
            return;
        }

        var medCheck = DiceRoller.Roll(_player.GetBestFor(SkillType.Medicine));
        _term.DiceRoll($"Medicine check: {medCheck}");

        int healAmount = Math.Max(1, medCheck.Total / 2);
        _player.CurrentResolve = Math.Min(_player.Resolve, _player.CurrentResolve + healAmount);
        _player.Inventory.Remove(medpacks[0]);
        _term.Combat($"You use a Medpack and restore {healAmount} Resolve! (Now: {_player.CurrentResolve}/{_player.Resolve})");
    }

    private int ApplyArmor(int rawDamage, Character target)
    {
        int totalAbsorb = 0;

        // Vehicle shields (if target is in a vehicle)
        var vehicle = target.ActiveVehicle;
        if (vehicle != null && vehicle.Shield.DiceCode.Dice > 0)
        {
            var shieldRoll = DiceRoller.Roll(vehicle.Shield.DiceCode);
            totalAbsorb += shieldRoll.Total;
            _term.DiceRoll($"{vehicle.Shield.Name} absorption: {shieldRoll}");
        }

        // Personal armor
        if (target.EquippedArmor.DiceCode.Dice > 0)
        {
            var armorRoll = DiceRoller.Roll(target.EquippedArmor.DiceCode);
            totalAbsorb += armorRoll.Total;
            _term.DiceRoll($"{target.EquippedArmor.Name} absorption: {armorRoll}");
        }

        int finalDmg = Math.Max(0, rawDamage - totalAbsorb);
        if (totalAbsorb > 0)
            _term.Mechanic($"Damage {rawDamage} - Absorbed {totalAbsorb} = {finalDmg}");
        return finalDmg;
    }

    private void LootEnemy()
    {
        if (_enemy.EquippedWeapon != null)
        {
            _term.Prompt($"The {_enemy.Name} dropped: {_enemy.EquippedWeapon.Name}. Pick it up? [y/n]");
            if (_term.ReadInput().Trim().ToLower() == "y")
            {
                _player.Inventory.Add(_enemy.EquippedWeapon);
                _term.Info($"Added {_enemy.EquippedWeapon.Name} to your inventory.");
            }
        }
    }
}

using TerminalHyperspace.Content;
using TerminalHyperspace.Models;
using TerminalHyperspace.UI;

namespace TerminalHyperspace.Engine;

public class SpaceCombatEngine
{
    private readonly Character _player;
    private readonly Vehicle _playerShip;
    private readonly Character _enemyPilot;
    private readonly Vehicle _enemyShip;
    private readonly Terminal _term;
    private readonly GameState _state;

    public SpaceCombatEngine(Character player, Vehicle playerShip,
        Character enemyPilot, Vehicle enemyShip, Terminal term, GameState state)
    {
        _player = player;
        _playerShip = playerShip;
        _enemyPilot = enemyPilot;
        _enemyShip = enemyShip;
        _term = term;
        _state = state;
        _enemyShip.InitializeResolve();
    }

    public bool RunCombat()
    {
        _term.Divider();
        _term.Combat($"⚔ SPACE COMBAT: {_playerShip.Name} vs {_enemyShip.Name}");
        _term.Combat($"  Your Ship Resolve: {_playerShip.CurrentResolve}/{_playerShip.Resolve}  |  Shields: {_playerShip.Shield}");
        _term.Combat($"  Enemy Ship Resolve: {_enemyShip.CurrentResolve}/{_enemyShip.Resolve}  |  Shields: {_enemyShip.Shield}");
        _term.Mechanic($"  Your Maneuverability: {_playerShip.Maneuverability}  |  Enemy: {_enemyShip.Maneuverability}");
        _term.Divider();

        // Show equipment bonuses
        if (_playerShip.Equipment.Count > 0)
        {
            _term.Mechanic($"  Your ship equipment: {string.Join(", ", _playerShip.Equipment)}");
        }
        if (_enemyShip.Equipment.Count > 0)
        {
            _term.Mechanic($"  Enemy ship equipment: {string.Join(", ", _enemyShip.Equipment)}");
        }

        // Initiative: Pilot skill + ship sensors
        var playerInit = _player.Initiative;
        var enemyInit = _enemyPilot.Initiative;
        _term.DiceRoll($"Initiative — You: {playerInit}  |  {_enemyPilot.Name}: {enemyInit}");

        bool playerFirst = playerInit >= enemyInit;
        _term.Narrative(playerFirst
            ? "You swing into attack position first!"
            : $"The {_enemyShip.Name} gets the drop on you!");

        while (!_playerShip.IsDestroyed && !_enemyShip.IsDestroyed)
        {
            _term.Blank();
            _term.Combat($"[Ship Resolve] {_playerShip.Name}: {_playerShip.CurrentResolve}/{_playerShip.Resolve} | {_enemyShip.Name}: {_enemyShip.CurrentResolve}/{_enemyShip.Resolve}");

            if (playerFirst)
            {
                if (!PlayerTurn()) return true; // fled
                if (_enemyShip.IsDestroyed) break;
                EnemyTurn();
            }
            else
            {
                EnemyTurn();
                if (_playerShip.IsDestroyed) break;
                if (!PlayerTurn()) return true; // fled
            }
        }

        _term.Blank();
        if (_enemyShip.IsDestroyed)
        {
            _term.Combat($"★ The {_enemyShip.Name} breaks apart in a shower of debris!");
            LootWreckage();
            return true;
        }
        else
        {
            _term.Combat($"Your {_playerShip.Name} is destroyed!");
            _term.Narrative("You eject in an escape pod as your ship disintegrates around you...");
            _player.SpaceVehicle = null;
            _player.InVehicle = false;
            _player.InSpaceVehicle = false;
            // Player survives but loses their ship — take some personal damage
            int crashDmg = DiceRoller.RollRaw(2);
            _player.CurrentResolve = Math.Max(1, _player.CurrentResolve - crashDmg);
            _term.Combat($"The ejection impact deals {crashDmg} damage to you. (Resolve: {_player.CurrentResolve}/{_player.Resolve})");
            return true; // survived personally, lost ship
        }
    }

    private bool PlayerTurn()
    {
        _term.Prompt("Your action: [a]ttack  [e]vasive maneuvers  [f]lee");
        var input = _term.ReadInput().ToLower().Trim();

        switch (input)
        {
            case "a":
            case "attack":
                PlayerAttack();
                return true;
            case "e":
            case "evasive":
                _term.Narrative("You throw your ship into a series of evasive maneuvers!");
                _term.Mechanic("+2 effective Maneuverability this round.");
                return true;
            case "f":
            case "flee":
                return AttemptFlee();
            default:
                _term.Error("Invalid action. You hesitate at the controls!");
                return true;
        }
    }

    private void PlayerAttack()
    {
        if (_playerShip.Weapons.Count == 0)
        {
            _term.Error("Your ship has no weapons! You can only flee.");
            return;
        }

        VehicleWeapon selectedWeapon;
        if (_playerShip.Weapons.Count == 1)
        {
            selectedWeapon = _playerShip.Weapons[0];
        }
        else
        {
            _term.Prompt("Select weapon:");
            for (int i = 0; i < _playerShip.Weapons.Count; i++)
                _term.Info($"  [{i + 1}] {_playerShip.Weapons[i]}");
            var choice = _term.ReadInput().Trim();
            if (!int.TryParse(choice, out int idx) || idx < 1 || idx > _playerShip.Weapons.Count)
            {
                selectedWeapon = _playerShip.Weapons[0];
                _term.Info($"Defaulting to {selectedWeapon.Name}.");
            }
            else
            {
                selectedWeapon = _playerShip.Weapons[idx - 1];
            }
        }

        // Attack roll: Gunnery skill + ship equipment Gunnery bonus
        var gunneryCode = _player.GetBestFor(SkillType.Gunnery);
        var equipBonus = _playerShip.GetSkillBonus(SkillType.Gunnery);
        var totalAttack = gunneryCode + equipBonus;

        bool fpDouble = ForceRoller.PromptForcePointDouble(_state, _term);
        var attackRoll = DiceRoller.Roll(totalAttack);
        if (equipBonus.Dice > 0 || equipBonus.Pips > 0)
            _term.DiceRoll($"Gunnery ({gunneryCode} + {equipBonus} equipment) with {selectedWeapon.Name}: {attackRoll}");
        else
            _term.DiceRoll($"Gunnery ({gunneryCode}) with {selectedWeapon.Name}: {attackRoll}");
        int attackTotal = ForceRoller.ApplyForcePointDouble(attackRoll.Total, fpDouble, _term);

        // Defense: enemy Pilot skill + ship Maneuverability
        var enemyPilotCode = _enemyPilot.GetBestFor(SkillType.Pilot);
        var enemyPilotBonus = _enemyShip.GetSkillBonus(SkillType.Pilot);
        var enemyDefenseCode = enemyPilotCode + _enemyShip.Maneuverability + enemyPilotBonus;
        var defenseRoll = DiceRoller.Roll(enemyDefenseCode);
        _term.DiceRoll($"{_enemyPilot.Name} evades (Pilot {enemyPilotCode} + Maneuver {_enemyShip.Maneuverability}): {defenseRoll}");

        if (attackTotal >= defenseRoll.Total)
        {
            var damageRoll = DiceRoller.Roll(selectedWeapon.Damage);
            _term.DiceRoll($"Weapon damage ({selectedWeapon.Name}): {damageRoll}");
            int rawDmg = damageRoll.Total;
            bool crit = attackTotal >= defenseRoll.Total * 2;
            if (crit)
            {
                rawDmg *= 2;
                _term.Combat($"★ CRITICAL HIT! Damage doubled: {damageRoll.Total} → {rawDmg}");
            }
            int finalDmg = ApplyShields(rawDmg, _enemyShip);
            _enemyShip.CurrentResolve -= finalDmg;
            _term.Combat($"Direct hit on {_enemyShip.Name}! {finalDmg} damage dealt.");
        }
        else
        {
            _term.Narrative($"Your shots streak past the {_enemyShip.Name}!");
        }
    }

    private void EnemyTurn()
    {
        _term.Blank();
        _term.Combat($"— {_enemyShip.Name} attacks —");

        if (_enemyShip.Weapons.Count == 0)
        {
            _term.Narrative($"The {_enemyShip.Name} has no weapons and attempts to ram you!");
            return;
        }

        // Enemy picks a random weapon
        var weapon = _enemyShip.Weapons[Random.Shared.Next(_enemyShip.Weapons.Count)];

        // Attack roll: enemy Gunnery + ship equipment Gunnery bonus
        var enemyGunnery = _enemyPilot.GetBestFor(SkillType.Gunnery);
        var enemyEquipBonus = _enemyShip.GetSkillBonus(SkillType.Gunnery);
        var enemyAttackCode = enemyGunnery + enemyEquipBonus;

        var attackRoll = DiceRoller.Roll(enemyAttackCode);
        _term.DiceRoll($"{_enemyPilot.Name} fires {weapon.Name}: {attackRoll}");

        // Player defense: Pilot skill + ship Maneuverability + equipment Pilot bonus
        var playerPilotCode = _player.GetBestFor(SkillType.Pilot);
        var playerPilotBonus = _playerShip.GetSkillBonus(SkillType.Pilot);
        var playerDefenseCode = playerPilotCode + _playerShip.Maneuverability + playerPilotBonus;
        var defenseRoll = DiceRoller.Roll(playerDefenseCode);
        _term.DiceRoll($"You evade (Pilot {playerPilotCode} + Maneuver {_playerShip.Maneuverability}): {defenseRoll}");

        if (attackRoll.Total >= defenseRoll.Total)
        {
            var damageRoll = DiceRoller.Roll(weapon.Damage);
            _term.DiceRoll($"Incoming damage: {damageRoll}");
            int finalDmg = ApplyShields(damageRoll.Total, _playerShip);
            _playerShip.CurrentResolve -= finalDmg;
            _term.Combat($"Your {_playerShip.Name} takes {finalDmg} damage!");
        }
        else
        {
            _term.Narrative($"The {_enemyShip.Name}'s shot misses wide!");
        }
    }

    private int ApplyShields(int rawDamage, Vehicle ship)
    {
        if (ship.Shield.DiceCode.Dice <= 0)
            return rawDamage;

        var shieldRoll = DiceRoller.Roll(ship.Shield.DiceCode);
        _term.DiceRoll($"{ship.Shield.Name} absorption: {shieldRoll}");
        int finalDmg = Math.Max(0, rawDamage - shieldRoll.Total);
        _term.Mechanic($"Damage {rawDamage} - Shields {shieldRoll.Total} = {finalDmg}");
        return finalDmg;
    }

    private bool AttemptFlee()
    {
        // Flee: player Pilot + Maneuverability vs enemy Pilot + Maneuverability
        var playerCode = _player.GetBestFor(SkillType.Pilot) + _playerShip.Maneuverability;
        var enemyCode = _enemyPilot.GetBestFor(SkillType.Pilot) + _enemyShip.Maneuverability;

        var playerRoll = DiceRoller.Roll(playerCode);
        var enemyRoll = DiceRoller.Roll(enemyCode);
        _term.DiceRoll($"Escape attempt — You: {playerRoll} vs {_enemyPilot.Name}: {enemyRoll}");

        if (playerRoll.Total >= enemyRoll.Total)
        {
            _term.Narrative("You punch the throttle and break away into open space!");
            return false; // exited combat via flee
        }
        else
        {
            _term.Narrative("They match your maneuver — you can't shake them!");
            EnemyTurn();
            return true;
        }
    }

    private void LootWreckage()
    {
        int salvage = DiceRoller.RollRaw(2) * 25;
        _term.Info($"You salvage {salvage} credits worth of components from the wreckage.");
        // Credits are added via the CommandParser after combat returns
        _SalvageCredits = salvage;
    }

    private int _SalvageCredits;
    public int SalvageCredits => _SalvageCredits;
}

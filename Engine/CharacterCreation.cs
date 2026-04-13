using TerminalHyperspace.Content;
using TerminalHyperspace.Models;
using TerminalHyperspace.UI;

namespace TerminalHyperspace.Engine;

public class CharacterCreation
{
    private readonly Terminal _term;

    public CharacterCreation(Terminal term)
    {
        _term = term;
    }

    public Character Create()
    {
        _term.Header("CHARACTER CREATION");
        _term.Blank();

        // Name
        _term.Prompt("Enter your character's name:");
        string name = _term.ReadInput().Trim();
        if (string.IsNullOrEmpty(name)) name = "Spacer";

        // Species
        _term.Blank();
        _term.Header("CHOOSE YOUR SPECIES");
        var species = SpeciesData.All;
        for (int i = 0; i < species.Count; i++)
        {
            _term.Blank();
            _term.Info($"  [{i + 1}] {species[i].Name}");
            _term.Narrative($"      {species[i].Description}");
            _term.Mechanic($"      Attributes: {FormatAttributes(species[i].BaseAttributes)}");
            if (species[i].SkillBonuses.Count > 0)
                _term.Mechanic($"      Skill Bonuses: {FormatSkillBonuses(species[i].SkillBonuses)}");
        }

        _term.Blank();
        _term.Prompt("Select species:");
        int speciesChoice = _term.ReadChoice(1, species.Count);
        var selectedSpecies = species[speciesChoice - 1];

        // Role
        _term.Blank();
        _term.Header("CHOOSE YOUR ROLE");
        var roles = RoleData.All;
        for (int i = 0; i < roles.Count; i++)
        {
            _term.Blank();
            _term.Info($"  [{i + 1}] {roles[i].Name}");
            _term.Narrative($"      {roles[i].Description}");
            if (roles[i].AttributeBonuses.Count > 0)
                _term.Mechanic($"      Attribute Bonuses: {FormatAttributes(roles[i].AttributeBonuses)}");
            _term.Mechanic($"      Skill Bonuses: {FormatSkillBonuses(roles[i].SkillBonuses)}");
        }

        _term.Blank();
        _term.Prompt("Select role:");
        int roleChoice = _term.ReadChoice(1, roles.Count);
        var selectedRole = roles[roleChoice - 1];

        // Build character
        var character = new Character
        {
            Name = name,
            SpeciesName = selectedSpecies.Name,
            RoleName = selectedRole.Name,
            IsPlayer = true,
        };

        // Attributes = species base + role bonuses
        foreach (var attr in Enum.GetValues<AttributeType>())
        {
            var baseVal = selectedSpecies.BaseAttributes.TryGetValue(attr, out var b) ? b : new DiceCode(1);
            var roleBonus = selectedRole.AttributeBonuses.TryGetValue(attr, out var rb) ? rb : new DiceCode(0);
            character.Attributes[attr] = baseVal + roleBonus;
        }

        // Skills = species bonuses + role bonuses (stacked)
        var allSkillBonuses = new Dictionary<SkillType, DiceCode>();
        foreach (var kv in selectedSpecies.SkillBonuses)
            allSkillBonuses[kv.Key] = kv.Value;
        foreach (var kv in selectedRole.SkillBonuses)
        {
            if (allSkillBonuses.TryGetValue(kv.Key, out var existing))
                allSkillBonuses[kv.Key] = existing + kv.Value;
            else
                allSkillBonuses[kv.Key] = kv.Value;
        }
        character.SkillBonuses = allSkillBonuses;

        character.InitializeResolve();

        // Starting equipment
        _term.Blank();
        _term.Header("STARTING EQUIPMENT");
        _term.Prompt("Choose your starting weapon:");
        var weapons = ItemData.StarterWeapons;
        for (int i = 0; i < weapons.Count; i++)
            _term.Info($"  [{i + 1}] {weapons[i]}");
        int weaponChoice = _term.ReadChoice(1, weapons.Count);
        var startWeapon = weapons[weaponChoice - 1];

        character.Inventory.Add(startWeapon);
        character.EquippedWeapon = startWeapon;

        // Starting armor
        _term.Blank();
        _term.Prompt("Choose your starting armor:");
        var armors = new[] { ArmorData.Unarmored, ArmorData.PaddedFlightSuit };
        for (int i = 0; i < armors.Length; i++)
            _term.Info($"  [{i + 1}] {armors[i]}");
        int armorChoice = _term.ReadChoice(1, armors.Length);
        character.EquippedArmor = armors[armorChoice - 1];

        // Everyone gets a medpack
        character.Inventory.Add(ItemData.Medpack);

        // Show final character
        _term.Blank();
        _term.CharacterSheet(character);
        _term.Prompt("Accept this character? [y/n]");
        if (_term.ReadInput().Trim().ToLower() == "n")
            return Create(); // recursive re-create

        return character;
    }

    private string FormatAttributes(Dictionary<AttributeType, DiceCode> attrs)
        => string.Join(", ", attrs.Select(kv => $"{kv.Key}: {kv.Value}"));

    private string FormatSkillBonuses(Dictionary<SkillType, DiceCode> skills)
        => string.Join(", ", skills.Select(kv => $"{kv.Key} +{kv.Value}"));
}

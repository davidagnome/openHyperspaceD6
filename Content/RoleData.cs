using TerminalHyperspace.Models;

namespace TerminalHyperspace.Content;

public static partial class RoleData
{
    static partial void RegisterImported(List<Role> list);

    public static List<Role> All
    {
        get
        {
            var list = new List<Role>
            {
        new Role
        {
            Name = "Soldier",
            Description = "A trained combatant, skilled with blasters and battlefield tactics.",
            AttributeBonuses = new()
            {
                [AttributeType.Dexterity] = new DiceCode(2),
                [AttributeType.Strength] = new DiceCode(1),
            },
            SkillBonuses = new()
            {
                [SkillType.Blasters] = new DiceCode(0, 2),
                [SkillType.Tactics] = new DiceCode(0, 1),
                [SkillType.Melee] = new DiceCode(0, 1),
                [SkillType.Stamina] = new DiceCode(0, 1),
            }
        },
        new Role
        {
            Name = "Pilot",
            Description = "A hotshot flyboy who lives in the cockpit. Equally at home dogfighting or running blockades.",
            AttributeBonuses = new()
            {
                [AttributeType.Mechanical] = new DiceCode(0, 2),
                [AttributeType.Dexterity] = new DiceCode(0, 1),
            },
            SkillBonuses = new()
            {
                [SkillType.Pilot] = new DiceCode(2),
                [SkillType.Gunnery] = new DiceCode(0, 1),
                [SkillType.Astrogation] = new DiceCode(0, 1),
                [SkillType.Sensors] = new DiceCode(0, 1),
            }
        },
        
        new Role
        {
            Name = "Doctor",
            Description = "A saw-bones experienced in healing anything not electronic.",
            AttributeBonuses = new()
            {
                [AttributeType.Knowledge] = new DiceCode(1),
                [AttributeType.Technical] = new DiceCode(2),
            },
            SkillBonuses = new()
            {
                [SkillType.Medicine] = new DiceCode(0, 2),
                [SkillType.Search] = new DiceCode(0, 1),
                [SkillType.Xenology] = new DiceCode(0, 1),
                [SkillType.Computers] = new DiceCode(0, 1),
            }
        },
        
        new Role
        {
            Name = "Scoundrel",
            Description = "A smooth-talking rogue who relies on wit, charm, and a fast draw.",
            AttributeBonuses = new()
            {
                [AttributeType.Perception] = new DiceCode(1),
                [AttributeType.Mechanical] = new DiceCode(2),
            },
            SkillBonuses = new()
            {
                [SkillType.Deceive] = new DiceCode(0, 2),
                [SkillType.Persuade] = new DiceCode(0, 2),
                [SkillType.Streetwise] = new DiceCode(0, 2),
                [SkillType.Steal] = new DiceCode(0, 1),
                [SkillType.Blasters] = new DiceCode(0, 1),
            }
        },
        new Role
        {
            Name = "Engineer",
            Description = "A mechanical genius who can fix anything—from hyperdrives to battle droids.",
            AttributeBonuses = new()
            {
                [AttributeType.Technical] = new DiceCode(2),
                [AttributeType.Knowledge] = new DiceCode(1),
            },
            SkillBonuses = new()
            {
                [SkillType.Computers] = new DiceCode(0, 2),
                [SkillType.Droids] = new DiceCode(0, 1),
                [SkillType.Vehicles] = new DiceCode(0, 1),
                [SkillType.Armament] = new DiceCode(0, 1),
            }
        },
        new Role
        {
            Name = "Mystic",
            Description = "A wielder of the Force—sensing the unseen, shaping reality, and seeking balance.",
            AttributeBonuses = new()
            {
                [AttributeType.Force] = new DiceCode(1),
                [AttributeType.Perception] = new DiceCode(2),
            },
            SkillBonuses = new()
            {
                [SkillType.Control] = new DiceCode(0, 1),
                [SkillType.Sense] = new DiceCode(0, 1),
                [SkillType.Alter] = new DiceCode(0, 1),
                [SkillType.Persuade] = new DiceCode(0, 1),
                [SkillType.Medicine] = new DiceCode(0, 2),
            }
        },
        new Role
        {
            Name = "Bounty Hunter",
            Description = "A relentless tracker who brings in targets dead or alive. Resourceful and dangerous.",
            AttributeBonuses = new()
            {
                [AttributeType.Dexterity] = new DiceCode(0, 1),
                [AttributeType.Perception] = new DiceCode(0, 1),
                [AttributeType.Strength] = new DiceCode(0, 1),
            },
            SkillBonuses = new()
            {
                [SkillType.Blasters] = new DiceCode(0, 2),
                [SkillType.Search] = new DiceCode(0, 2),
                [SkillType.Intimidate] = new DiceCode(0, 2),
                [SkillType.Streetwise] = new DiceCode(0, 1),
                [SkillType.Brawl] = new DiceCode(0, 1),
            }
        },
        new Role
        {
            Name = "Politician",
            Description = "Rising politician",
            AttributeBonuses = new()
            {
                [AttributeType.Knowledge] = new DiceCode(1),
                [AttributeType.Perception] = new DiceCode(2),
            },
            SkillBonuses = new()
            {
                [SkillType.Galaxy] = new DiceCode(0,2),
                [SkillType.Persuade] = new DiceCode(0, 2),
                [SkillType.Deceive] = new DiceCode(0, 1),
                [SkillType.Computers] = new DiceCode(0, 1),
            }
        },
        new Role
        {
            Name = "Custom",
            Description = "You'll start with additional upgrade points to upgrade your characters",
            AttributeBonuses = new()
            {
                [AttributeType.Knowledge] = new DiceCode(0),
            },
            SkillBonuses = new()
            {
                [SkillType.Galaxy] = new DiceCode(0,0),
            }
        },
            };
            RegisterImported(list);
            return list;
        }
    }
}

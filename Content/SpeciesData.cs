using TerminalHyperspace.Models;

namespace TerminalHyperspace.Content;

public static partial class SpeciesData
{
    static partial void RegisterImported(List<Species> list);

    public static List<Species> All
    {
        get
        {
            var list = new List<Species>
            {
        new Species
        {
            Name = "Human",
            Description = "Versatile and adaptable, humans are found on nearly every world in the galaxy.",
            BaseAttributes = new()
            {
                [AttributeType.Dexterity] = new DiceCode(2),
                [AttributeType.Knowledge] = new DiceCode(1),
                [AttributeType.Mechanical] = new DiceCode(2),
                [AttributeType.Perception] = new DiceCode(2),
                [AttributeType.Strength] = new DiceCode(1),
                [AttributeType.Technical] = new DiceCode(2),
                [AttributeType.Force] = new DiceCode(1),
            },
            SkillBonuses = new()
            {
                [SkillType.Persuade] = new DiceCode(0, 1),
                [SkillType.Willpower] = new DiceCode(0, 1),
            }
        },
        new Species
        {
            Name = "Bothan",
            Description = "Feline species known for their empathic abilities and striking appearance. Intuitive and fur ripples with mood.",
            BaseAttributes = new()
            {
                [AttributeType.Dexterity] = new DiceCode(1),
                [AttributeType.Knowledge] = new DiceCode(2),
                [AttributeType.Mechanical] = new DiceCode(1),
                [AttributeType.Perception] = new DiceCode(3),
                [AttributeType.Strength] = new DiceCode(1),
                [AttributeType.Technical] = new DiceCode(2),
                [AttributeType.Force] = new DiceCode(0, 1),
            },
            SkillBonuses = new()
            {
                [SkillType.Hide] = new DiceCode(0, 2)
            }
        },
        new Species
        {
            Name = "Mon Calamari",
            Description = "Bipedal cephalapods with blue to brown skin tones and webbed hands.",
            BaseAttributes = new()
            {
                [AttributeType.Dexterity] = new DiceCode(1),
                [AttributeType.Knowledge] = new DiceCode(2),
                [AttributeType.Mechanical] = new DiceCode(2),
                [AttributeType.Perception] = new DiceCode(2),
                [AttributeType.Strength] = new DiceCode(1),
                [AttributeType.Technical] = new DiceCode(2),
                [AttributeType.Force] = new DiceCode(0, 1),
            },
            SkillBonuses = new()
            {
                [SkillType.Swim] = new DiceCode(0, 2),
            }
        },
        new Species
        {
            Name = "Trandoshan",
            Description = "Reptilian warriors from a high-gravity world. Powerful and resilient, but less comfortable with technology.",
            BaseAttributes = new()
            {
                [AttributeType.Dexterity] = new DiceCode(2),
                [AttributeType.Knowledge] = new DiceCode(1),
                [AttributeType.Mechanical] = new DiceCode(1),
                [AttributeType.Perception] = new DiceCode(2),
                [AttributeType.Strength] = new DiceCode(3),
                [AttributeType.Technical] = new DiceCode(1),
                [AttributeType.Force] = new DiceCode(1),
            },
            SkillBonuses = new()
            {
                [SkillType.Stamina] = new DiceCode(0, 2),
            }
        },
        new Species
        {
            Name = "Synthoid",
            Description = "Cybernetic beings—part organic, part machine. Brilliant technicians with questionable speeder-bikes.",
            BaseAttributes = new()
            {
                [AttributeType.Dexterity] = new DiceCode(2),
                [AttributeType.Knowledge] = new DiceCode(2, 1),
                [AttributeType.Mechanical] = new DiceCode(2, 1),
                [AttributeType.Perception] = new DiceCode(2),
                [AttributeType.Strength] = new DiceCode(1, 2),
                [AttributeType.Technical] = new DiceCode(2, 2),
                [AttributeType.Force] = new DiceCode(0, 1),
            },
            SkillBonuses = new()
            {
                [SkillType.Computers] = new DiceCode(0, 2),
                [SkillType.Droids] = new DiceCode(0, 1),
                [SkillType.Sensors] = new DiceCode(0, 1),
            }
        },
        new Species
        {
            Name = "Rodian",
            Description = "Frog-like aliens with spiky, hair-live spines instead of hair.",
            BaseAttributes = new()
            {
                [AttributeType.Dexterity] = new DiceCode(3),
                [AttributeType.Knowledge] = new DiceCode(1),
                [AttributeType.Mechanical] = new DiceCode(1),
                [AttributeType.Perception] = new DiceCode(2),
                [AttributeType.Strength] = new DiceCode(2),
                [AttributeType.Technical] = new DiceCode(1),
                [AttributeType.Force] = new DiceCode(0, 1),
            },
            SkillBonuses = new()
            {
                [SkillType.Persuade] = new DiceCode(0, 2),
            }
        },
        new Species
        {
            Name = "Zabrak",
            Description = "Horned humanoids, many featuring facial tattoos. Tenacious.",
            BaseAttributes = new()
            {
                [AttributeType.Dexterity] = new DiceCode(2),
                [AttributeType.Knowledge] = new DiceCode(2),
                [AttributeType.Mechanical] = new DiceCode(1),
                [AttributeType.Perception] = new DiceCode(2),
                [AttributeType.Strength] = new DiceCode(2),
                [AttributeType.Technical] = new DiceCode(1),
                [AttributeType.Force] = new DiceCode(0, 1),
            },
            SkillBonuses = new()
            {
                [SkillType.Willpower] = new DiceCode(0, 2),
            }
        },
        new Species
        {
            Name = "Wookie",
            Description = "Imposing, bear-like bipeds know for their strength and intimidating size.",
            BaseAttributes = new()
            {
                [AttributeType.Dexterity] = new DiceCode(2, 2),
                [AttributeType.Knowledge] = new DiceCode(1, 2),
                [AttributeType.Mechanical] = new DiceCode(2),
                [AttributeType.Perception] = new DiceCode(2, 1),
                [AttributeType.Strength] = new DiceCode(3),
                [AttributeType.Technical] = new DiceCode(1, 2),
                [AttributeType.Force] = new DiceCode(1),
            },
            SkillBonuses = new()
            {
                [SkillType.Intimidate] = new DiceCode(0, 2),
                [SkillType.Athletics] = new DiceCode(0, 2)
            }
        },
        new Species
        {
            Name = "Green-Ones",
            Description = "Ancient species deeply connected to the cosmic Force. Physically frail but extraordinarily gifted in the unseen arts.",
            BaseAttributes = new()
            {
                [AttributeType.Dexterity] = new DiceCode(1, 2),
                [AttributeType.Knowledge] = new DiceCode(2, 1),
                [AttributeType.Mechanical] = new DiceCode(1, 2),
                [AttributeType.Perception] = new DiceCode(2, 1),
                [AttributeType.Strength] = new DiceCode(1, 1),
                [AttributeType.Technical] = new DiceCode(2),
                [AttributeType.Force] = new DiceCode(2, 1),
            },
            SkillBonuses = new()
            {
                [SkillType.Control] = new DiceCode(0, 2),
                [SkillType.Sense] = new DiceCode(0, 1),
                [SkillType.Alter] = new DiceCode(0, 1),
            }
        },
            };
            RegisterImported(list);
            return list;
        }
    }
}

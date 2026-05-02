using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Parses Content/SpaceEncounterData.cs. Each encounter is a method:
///   public static SpaceEncounter Foo() => new() { Pilot = new Character {...}, Ship = new Vehicle {...} };
public class SpaceEncounterFileParser
{
    public string FilePath { get; }
    public string SourceText { get; private set; } = "";
    public List<SpaceEncounterModel> Encounters { get; } = new();
    public string? Error { get; private set; }

    public SpaceEncounterFileParser(string filePath) { FilePath = filePath; }

    public bool TryLoad()
    {
        try
        {
            SourceText = File.ReadAllText(FilePath);
            var tree = CSharpSyntaxTree.ParseText(SourceText);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var cls = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == "SpaceEncounterData");
            if (cls == null) { Error = "SpaceEncounterData class not found."; return false; }

            foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.ReturnType is not IdentifierNameSyntax id || id.Identifier.Text != "SpaceEncounter") continue;
                if (method.ParameterList.Parameters.Count > 0) continue;
                if (method.ExpressionBody?.Expression is not ImplicitObjectCreationExpressionSyntax create
                    || create.Initializer == null) continue;

                var m = new SpaceEncounterModel
                {
                    MemberName = method.Identifier.Text,
                    OriginalSpan = method.FullSpan,
                };
                Apply(create.Initializer, m);
                Encounters.Add(m);
            }
            return true;
        }
        catch (Exception ex) { Error = ex.Message; return false; }
    }

    private static void Apply(InitializerExpressionSyntax init, SpaceEncounterModel m)
    {
        foreach (var expr in init.Expressions)
        {
            if (expr is not AssignmentExpressionSyntax pa) continue;
            if (pa.Left is not IdentifierNameSyntax name) continue;
            if (name.Identifier.Text == "Pilot") ApplyPilot(pa.Right, m);
            else if (name.Identifier.Text == "Ship") ApplyShip(pa.Right, m);
        }
    }

    private static void ApplyPilot(ExpressionSyntax e, SpaceEncounterModel m)
    {
        var init = GetInit(e);
        if (init == null) return;
        foreach (var expr in init.Expressions)
        {
            if (expr is not AssignmentExpressionSyntax pa) continue;
            if (pa.Left is not IdentifierNameSyntax name) continue;
            switch (name.Identifier.Text)
            {
                case "Name":           m.PilotName = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "IsPlayer":       m.PilotIsPlayer = ItemFileParser.ReadBool(pa.Right); break;
                case "Attributes":     NpcFileParser.ReadDiceMap(pa.Right, m.PilotAttributes,   "AttributeType"); break;
                case "SkillBonuses":   NpcFileParser.ReadDiceMap(pa.Right, m.PilotSkillBonuses, "SkillType"); break;
                case "EquippedArmor":  m.PilotEquippedArmorMember = NpcFileParser.ReadFactoryName(pa.Right, "ArmorData") ?? ""; break;
            }
        }
    }

    private static void ApplyShip(ExpressionSyntax e, SpaceEncounterModel m)
    {
        var init = GetInit(e);
        if (init == null) return;
        foreach (var expr in init.Expressions)
        {
            if (expr is not AssignmentExpressionSyntax pa) continue;
            if (pa.Left is not IdentifierNameSyntax name) continue;
            switch (name.Identifier.Text)
            {
                case "Name":            m.ShipName        = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "Description":     m.ShipDescription = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "IsSpace":         m.ShipIsSpace     = ItemFileParser.ReadBool(pa.Right); break;
                case "Maneuverability": (m.ShipManeuverDice, m.ShipManeuverPips) = ItemFileParser.ReadDiceCode(pa.Right); break;
                case "Resolve":         m.ShipResolve     = ItemFileParser.ReadInt(pa.Right); break;
                case "Shield":          m.ShipShieldMember = NpcFileParser.ReadFactoryName(pa.Right, "ShieldData") ?? ""; break;
                case "Weapons":         m.ShipWeapons     = ReadWeapons(pa.Right); break;
                case "Equipment":       m.ShipEquipment   = ReadEquipment(pa.Right); break;
            }
        }
    }

    private static InitializerExpressionSyntax? GetInit(ExpressionSyntax e)
    {
        if (e is ImplicitObjectCreationExpressionSyntax ioce) return ioce.Initializer;
        if (e is ObjectCreationExpressionSyntax oce) return oce.Initializer;
        return null;
    }

    private static List<VehicleWeaponModel> ReadWeapons(ExpressionSyntax e)
    {
        var result = new List<VehicleWeaponModel>();
        var init = GetInit(e);
        if (init == null) return result;
        foreach (var item in init.Expressions)
        {
            var iinit = GetInit(item);
            if (iinit == null) continue;
            var w = new VehicleWeaponModel();
            foreach (var ex in iinit.Expressions)
            {
                if (ex is not AssignmentExpressionSyntax pa) continue;
                if (pa.Left is not IdentifierNameSyntax name) continue;
                switch (name.Identifier.Text)
                {
                    case "Name":        w.Name = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                    case "Damage":      (w.DamageDice, w.DamagePips) = ItemFileParser.ReadDiceCode(pa.Right); break;
                    case "AttackSkill": w.AttackSkill = ItemFileParser.ReadEnumMember(pa.Right, "SkillType") ?? "Gunnery"; break;
                }
            }
            result.Add(w);
        }
        return result;
    }

    private static List<VehicleEquipmentModel> ReadEquipment(ExpressionSyntax e)
    {
        var result = new List<VehicleEquipmentModel>();
        var init = GetInit(e);
        if (init == null) return result;
        foreach (var item in init.Expressions)
        {
            var iinit = GetInit(item);
            if (iinit == null) continue;
            var eq = new VehicleEquipmentModel();
            foreach (var ex in iinit.Expressions)
            {
                if (ex is not AssignmentExpressionSyntax pa) continue;
                if (pa.Left is not IdentifierNameSyntax name) continue;
                switch (name.Identifier.Text)
                {
                    case "Name":       eq.Name       = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                    case "BonusSkill": eq.BonusSkill = ItemFileParser.ReadEnumMember(pa.Right, "SkillType") ?? ""; break;
                    case "Bonus":      (eq.BonusDice, eq.BonusPips) = ItemFileParser.ReadDiceCode(pa.Right); break;
                }
            }
            result.Add(eq);
        }
        return result;
    }
}

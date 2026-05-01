using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Parses Content/VehicleData.cs. Each vehicle is a property:
///   public static Vehicle LightFreighter => new() { ... };
public class VehicleFileParser
{
    public string FilePath { get; }
    public string SourceText { get; private set; } = "";
    public List<VehicleModel> Vehicles { get; } = new();
    public TextSpan AnchorSpan { get; private set; }
    public string? Error { get; private set; }

    public VehicleFileParser(string filePath) { FilePath = filePath; }

    public bool TryLoad()
    {
        try
        {
            SourceText = File.ReadAllText(FilePath);
            var tree = CSharpSyntaxTree.ParseText(SourceText);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var cls = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == "VehicleData");
            if (cls == null) { Error = "VehicleData class not found."; return false; }

            foreach (var prop in cls.Members.OfType<PropertyDeclarationSyntax>())
            {
                if (IsListVehicle(prop.Type))
                {
                    if (AnchorSpan.IsEmpty) AnchorSpan = prop.FullSpan;
                    continue;
                }
                if (prop.Type is not IdentifierNameSyntax id || id.Identifier.Text != "Vehicle") continue;
                if (prop.ExpressionBody?.Expression is not ImplicitObjectCreationExpressionSyntax create
                    || create.Initializer == null) continue;

                var v = new VehicleModel
                {
                    MemberName = prop.Identifier.Text,
                    OriginalSpan = prop.FullSpan,
                };
                Apply(create.Initializer, v);
                Vehicles.Add(v);
            }
            return true;
        }
        catch (Exception ex) { Error = ex.Message; return false; }
    }

    private static bool IsListVehicle(TypeSyntax t)
        => t is GenericNameSyntax g && g.Identifier.Text == "List"
           && g.TypeArgumentList.Arguments.Count == 1
           && g.TypeArgumentList.Arguments[0] is IdentifierNameSyntax inner
           && inner.Identifier.Text == "Vehicle";

    private static void Apply(InitializerExpressionSyntax init, VehicleModel m)
    {
        foreach (var expr in init.Expressions)
        {
            if (expr is not AssignmentExpressionSyntax pa) continue;
            if (pa.Left is not IdentifierNameSyntax name) continue;
            switch (name.Identifier.Text)
            {
                case "Name":            m.Name        = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "Description":     m.Description = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "IsSpace":         m.IsSpace     = ItemFileParser.ReadBool(pa.Right); break;
                case "Maneuverability": (m.ManeuverDice, m.ManeuverPips) = ItemFileParser.ReadDiceCode(pa.Right); break;
                case "Resolve":         m.Resolve     = ItemFileParser.ReadInt(pa.Right); break;
                case "Shield":          m.ShieldMember = NpcFileParser.ReadFactoryName(pa.Right, "ShieldData") ?? ""; break;
                case "Price":           m.Price       = ItemFileParser.ReadInt(pa.Right); break;
                case "Weapons":         m.Weapons     = ReadWeapons(pa.Right); break;
                case "Equipment":       m.Equipment   = ReadEquipment(pa.Right); break;
            }
        }
    }

    private static List<VehicleWeaponModel> ReadWeapons(ExpressionSyntax e)
    {
        var result = new List<VehicleWeaponModel>();
        InitializerExpressionSyntax? init = null;
        if (e is ImplicitObjectCreationExpressionSyntax ioce) init = ioce.Initializer;
        else if (e is ObjectCreationExpressionSyntax oce) init = oce.Initializer;
        if (init == null) return result;
        foreach (var item in init.Expressions)
        {
            if (item is not ImplicitObjectCreationExpressionSyntax iioce || iioce.Initializer == null) continue;
            var w = new VehicleWeaponModel();
            foreach (var ex in iioce.Initializer.Expressions)
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
        InitializerExpressionSyntax? init = null;
        if (e is ImplicitObjectCreationExpressionSyntax ioce) init = ioce.Initializer;
        else if (e is ObjectCreationExpressionSyntax oce) init = oce.Initializer;
        if (init == null) return result;
        foreach (var item in init.Expressions)
        {
            if (item is not ImplicitObjectCreationExpressionSyntax iioce || iioce.Initializer == null) continue;
            var eq = new VehicleEquipmentModel();
            foreach (var ex in iioce.Initializer.Expressions)
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

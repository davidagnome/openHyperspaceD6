using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Parses Content/SkillCheckData.cs SkillCheckEvent properties. Stops at the
/// LocationChecks / TalkChecks aggregator properties (those are managed
/// separately and need their own editor surface).
public class SkillCheckFileParser
{
    public string FilePath { get; }
    public string SourceText { get; private set; } = "";
    public List<SkillCheckModel> Checks { get; } = new();
    public TextSpan AnchorSpan { get; private set; }
    public string? Error { get; private set; }

    public SkillCheckFileParser(string filePath) { FilePath = filePath; }

    public bool TryLoad()
    {
        try
        {
            SourceText = File.ReadAllText(FilePath);
            var tree = CSharpSyntaxTree.ParseText(SourceText);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var cls = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == "SkillCheckData");
            if (cls == null) { Error = "SkillCheckData class not found."; return false; }

            foreach (var prop in cls.Members.OfType<PropertyDeclarationSyntax>())
            {
                // Skip aggregator dictionaries/lists.
                if (prop.Type is GenericNameSyntax)
                {
                    if (AnchorSpan.IsEmpty) AnchorSpan = prop.FullSpan;
                    continue;
                }
                if (prop.Type is not IdentifierNameSyntax id || id.Identifier.Text != "SkillCheckEvent") continue;
                if (prop.ExpressionBody?.Expression is not ImplicitObjectCreationExpressionSyntax create
                    || create.Initializer == null) continue;

                var sc = new SkillCheckModel
                {
                    MemberName = prop.Identifier.Text,
                    OriginalSpan = prop.FullSpan,
                };
                Apply(create.Initializer, sc);
                Checks.Add(sc);
            }
            return true;
        }
        catch (Exception ex) { Error = ex.Message; return false; }
    }

    private static void Apply(InitializerExpressionSyntax init, SkillCheckModel m)
    {
        foreach (var expr in init.Expressions)
        {
            if (expr is not AssignmentExpressionSyntax pa) continue;
            if (pa.Left is not IdentifierNameSyntax name) continue;
            switch (name.Identifier.Text)
            {
                case "Id":              m.Id              = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "Description":     m.Description     = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "SuccessText":     m.SuccessText     = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "FailText":        m.FailText        = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "FailPenaltyText": m.FailPenaltyText = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "Skill":           m.Skill           = ItemFileParser.ReadEnumMember(pa.Right, "SkillType") ?? ""; break;
                case "Difficulty":      m.Difficulty      = ItemFileParser.ReadEnumMember(pa.Right, "CheckDifficulty") ?? "Moderate"; break;
                case "TargetNumber":       m.TargetNumber       = ItemFileParser.ReadInt(pa.Right); break;
                case "CreditReward":       m.CreditReward       = ItemFileParser.ReadInt(pa.Right); break;
                case "UpgradePointReward": m.UpgradePointReward = ItemFileParser.ReadInt(pa.Right); break;
                case "Repeatable":         m.Repeatable         = ItemFileParser.ReadBool(pa.Right); break;
                case "CreditPenalty":      m.CreditPenalty      = ItemFileParser.ReadInt(pa.Right); break;
                case "CombatNpcOnFail":    m.CombatNpcOnFail    = NpcFileParser.ReadFactoryName(pa.Right, "NPCData") ?? ""; break;
            }
        }
    }
}

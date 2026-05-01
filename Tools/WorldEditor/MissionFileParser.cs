using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Parses Content/MissionData.cs. Each mission is a method:
///   public static Mission EscortDiplomat() => new() { ... };
public class MissionFileParser
{
    public string FilePath { get; }
    public string SourceText { get; private set; } = "";
    public List<MissionModel> Missions { get; } = new();
    public TextSpan AllOffersSpan { get; private set; }
    public string? Error { get; private set; }

    public MissionFileParser(string filePath) { FilePath = filePath; }

    public bool TryLoad()
    {
        try
        {
            SourceText = File.ReadAllText(FilePath);
            var tree = CSharpSyntaxTree.ParseText(SourceText);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var cls = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == "MissionData");
            if (cls == null) { Error = "MissionData class not found."; return false; }

            // AllOffers is the property whose factory list registers each mission.
            var allOffers = cls.Members.OfType<PropertyDeclarationSyntax>()
                .FirstOrDefault(p => p.Identifier.Text == "AllOffers");
            if (allOffers != null) AllOffersSpan = allOffers.FullSpan;

            foreach (var method in cls.Members.OfType<MethodDeclarationSyntax>())
            {
                if (method.ReturnType is not IdentifierNameSyntax id || id.Identifier.Text != "Mission") continue;
                if (method.ParameterList.Parameters.Count > 0) continue;
                if (method.ExpressionBody?.Expression is not ImplicitObjectCreationExpressionSyntax create
                    || create.Initializer == null) continue;

                var m = new MissionModel
                {
                    MemberName = method.Identifier.Text,
                    OriginalSpan = method.FullSpan,
                };
                Apply(create.Initializer, m);
                Missions.Add(m);
            }
            return true;
        }
        catch (Exception ex) { Error = ex.Message; return false; }
    }

    private static void Apply(InitializerExpressionSyntax init, MissionModel m)
    {
        foreach (var expr in init.Expressions)
        {
            if (expr is not AssignmentExpressionSyntax pa) continue;
            if (pa.Left is not IdentifierNameSyntax name) continue;
            switch (name.Identifier.Text)
            {
                case "Id":           m.Id           = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "Title":        m.Title        = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "BriefingText": m.BriefingText = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "Type":         m.Type         = ItemFileParser.ReadEnumMember(pa.Right, "MissionType") ?? "Escort"; break;
                case "DestinationLocationId": m.DestinationLocationId = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "EscortNpcName":         m.EscortNpcName         = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "MissionItem":           ApplyMissionItem(pa.Right, m); break;
                case "CheckSkill":            m.CheckSkill            = ItemFileParser.ReadEnumMember(pa.Right, "SkillType") ?? ""; break;
                case "CheckTargetNumber":     m.CheckTargetNumber     = ItemFileParser.ReadInt(pa.Right); break;
                case "CheckSuccessText":      m.CheckSuccessText      = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "CheckFailText":         m.CheckFailText         = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "CreditReward":          m.CreditReward          = ItemFileParser.ReadInt(pa.Right); break;
                case "UpgradePointReward":    m.UpgradePointReward    = ItemFileParser.ReadInt(pa.Right); break;
            }
        }
    }

    private static void ApplyMissionItem(ExpressionSyntax e, MissionModel m)
    {
        if (e is not ObjectCreationExpressionSyntax oce
            || oce.Type is not IdentifierNameSyntax id || id.Identifier.Text != "Item"
            || oce.Initializer == null) return;
        m.HasMissionItem = true;
        foreach (var ex in oce.Initializer.Expressions)
        {
            if (ex is not AssignmentExpressionSyntax pa) continue;
            if (pa.Left is not IdentifierNameSyntax name) continue;
            switch (name.Identifier.Text)
            {
                case "Name":         m.MissionItemName        = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "Description":  m.MissionItemDescription = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "MissionDestinationLocationId":
                    m.MissionItemDestinationLocationId = ItemFileParser.ReadString(pa.Right) ?? ""; break;
                case "MissionDestinationName":
                    m.MissionItemDestinationName = ItemFileParser.ReadString(pa.Right) ?? ""; break;
            }
        }
    }
}

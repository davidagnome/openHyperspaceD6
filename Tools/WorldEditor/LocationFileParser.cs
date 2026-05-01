using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace TerminalHyperspace.WorldEditor;

/// Parses LocationData.cs via Roslyn, locating each `world["id"] = new Location { ... };`
/// statement inside BuildWorld() and projecting it to a RoomModel. Local string
/// constants declared in BuildWorld() are resolved to their literal values so
/// edits don't accidentally clobber them.
public class LocationFileParser
{
    public string FilePath { get; }
    public string SourceText { get; private set; } = "";
    public List<RoomModel> Rooms { get; } = new();
    public TextSpan RegisterImportedCallSpan { get; private set; }
    public string? Error { get; private set; }

    public LocationFileParser(string filePath) { FilePath = filePath; }

    public bool TryLoad()
    {
        try
        {
            SourceText = File.ReadAllText(FilePath);
            var tree = CSharpSyntaxTree.ParseText(SourceText);
            var root = (CompilationUnitSyntax)tree.GetRoot();

            var buildWorld = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.Text == "BuildWorld");
            if (buildWorld == null || buildWorld.Body == null)
            {
                Error = "BuildWorld() method not found.";
                return false;
            }

            // Build a map of local string constants so e.g. PlanetName = TatooineNormal resolves to "Tatooine".
            var constants = new Dictionary<string, string>();
            foreach (var local in buildWorld.Body.DescendantNodes().OfType<LocalDeclarationStatementSyntax>())
            {
                if (!local.Modifiers.Any(m => m.Text == "const")) continue;
                if (!(local.Declaration.Type is PredefinedTypeSyntax { Keyword.Text: "string" })) continue;
                foreach (var v in local.Declaration.Variables)
                    if (v.Initializer?.Value is LiteralExpressionSyntax lit
                        && lit.IsKind(SyntaxKind.StringLiteralExpression))
                        constants[v.Identifier.Text] = lit.Token.ValueText;
            }

            // Locate the RegisterImported(world); invocation so we know where to insert new rooms.
            var registerCall = buildWorld.Body.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .FirstOrDefault(inv =>
                    inv.Expression is IdentifierNameSyntax { Identifier.Text: "RegisterImported" });
            if (registerCall != null)
            {
                // Walk up to the containing statement to grab the full line span.
                var stmt = registerCall.FirstAncestorOrSelf<StatementSyntax>();
                RegisterImportedCallSpan = stmt?.FullSpan ?? registerCall.FullSpan;
            }

            // Walk every `world["xxx"] = new Location { ... };` statement.
            foreach (var stmt in buildWorld.Body.DescendantNodes().OfType<ExpressionStatementSyntax>())
            {
                if (stmt.Expression is not AssignmentExpressionSyntax asgn) continue;
                if (asgn.Left is not ElementAccessExpressionSyntax elem) continue;
                if (elem.Expression is not IdentifierNameSyntax { Identifier.Text: "world" }) continue;
                if (elem.ArgumentList.Arguments.Count == 0) continue;
                if (elem.ArgumentList.Arguments[0].Expression is not LiteralExpressionSyntax idLit) continue;
                if (asgn.Right is not ObjectCreationExpressionSyntax create) continue;
                if (create.Initializer == null) continue;

                var room = new RoomModel
                {
                    Id = idLit.Token.ValueText,
                    OriginalSpan = stmt.FullSpan,
                };
                ApplyInitializer(create.Initializer, room, constants);
                Rooms.Add(room);
            }

            return true;
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            return false;
        }
    }

    private static void ApplyInitializer(
        InitializerExpressionSyntax init, RoomModel room, Dictionary<string, string> consts)
    {
        foreach (var expr in init.Expressions)
        {
            if (expr is not AssignmentExpressionSyntax pa) continue;
            if (pa.Left is not IdentifierNameSyntax name) continue;
            var prop = name.Identifier.Text;
            var rhs = pa.Right;

            switch (prop)
            {
                case "Id":              room.Id              = ReadString(rhs, consts) ?? room.Id; break;
                case "Name":            room.Name            = ReadString(rhs, consts) ?? ""; break;
                case "Description":     room.Description     = ReadString(rhs, consts) ?? ""; break;
                case "PlanetName":      room.PlanetName      = ReadString(rhs, consts) ?? ""; break;
                case "StarSystemName":  room.StarSystemName  = ReadString(rhs, consts) ?? ""; break;
                case "SectorName":      room.SectorName      = ReadString(rhs, consts) ?? ""; break;
                case "TerritoryName":   room.TerritoryName   = ReadString(rhs, consts) ?? ""; break;

                case "IsSpace":         room.IsSpace         = ReadBool(rhs); break;
                case "IsSystemSpace":   room.IsSystemSpace   = ReadBool(rhs); break;
                case "RequiresVehicle": room.RequiresVehicle = ReadBool(rhs); break;
                case "HasShop":         room.HasShop         = ReadBool(rhs); break;
                case "HasVehicleShop":  room.HasVehicleShop  = ReadBool(rhs); break;

                case "EncounterChance":      room.EncounterChance      = ReadDouble(rhs); break;
                case "SpaceEncounterChance": room.SpaceEncounterChance = ReadDouble(rhs); break;

                case "Climate":
                    if (rhs is MemberAccessExpressionSyntax m
                        && m.Expression is IdentifierNameSyntax { Identifier.Text: "Climate" })
                        room.Climate = m.Name.Identifier.Text;
                    break;

                case "HyperspaceCoordinates":
                    var coords = ReadIntArray(rhs);
                    if (coords.Length >= 2) { room.HyperspaceX = coords[0]; room.HyperspaceY = coords[1]; }
                    break;

                case "Exits":
                    foreach (var (k, v) in ReadDictionary(rhs, consts))
                        room.Exits.Add(new KeyValueEntry(k, v));
                    break;

                case "AmbientMessages":
                    foreach (var s in ReadStringList(rhs, consts)) room.AmbientMessages.Add(s);
                    break;

                case "PossibleEncounters":
                    foreach (var s in ReadFactoryList(rhs, "NPCData")) room.PossibleEncounters.Add(s);
                    break;

                case "FriendlyNPCs":
                    room.FriendlyNPCsPresent = true;
                    foreach (var s in ReadFactoryList(rhs, "NPCData")) room.FriendlyNPCs.Add(s);
                    break;

                case "SpaceEncounters":
                    foreach (var s in ReadFactoryList(rhs, "SpaceEncounterData")) room.SpaceEncounters.Add(s);
                    break;
            }
        }
    }

    private static string? ReadString(ExpressionSyntax expr, Dictionary<string, string> consts)
    {
        if (expr is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression))
            return lit.Token.ValueText;
        if (expr is InterpolatedStringExpressionSyntax isx)
            return isx.ToString().Trim('$', '"'); // best-effort
        if (expr is IdentifierNameSyntax id && consts.TryGetValue(id.Identifier.Text, out var val))
            return val;
        return null;
    }

    private static bool ReadBool(ExpressionSyntax expr)
        => expr.IsKind(SyntaxKind.TrueLiteralExpression);

    private static double ReadDouble(ExpressionSyntax expr)
    {
        if (expr is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.NumericLiteralExpression))
            return Convert.ToDouble(lit.Token.Value, System.Globalization.CultureInfo.InvariantCulture);
        return 0.0;
    }

    private static int[] ReadIntArray(ExpressionSyntax expr)
    {
        // Supports both `new[] { 1, 2 }` and `[1, 2]`.
        IEnumerable<ExpressionSyntax>? items = null;
        if (expr is ImplicitArrayCreationExpressionSyntax iac)
            items = iac.Initializer.Expressions;
        else if (expr is ArrayCreationExpressionSyntax ac && ac.Initializer != null)
            items = ac.Initializer.Expressions;
        else if (expr is CollectionExpressionSyntax ce)
            items = ce.Elements.OfType<ExpressionElementSyntax>().Select(e => e.Expression);

        if (items == null) return Array.Empty<int>();
        return items.OfType<LiteralExpressionSyntax>()
            .Where(l => l.IsKind(SyntaxKind.NumericLiteralExpression))
            .Select(l => Convert.ToInt32(l.Token.Value))
            .ToArray();
    }

    private static IEnumerable<(string Key, string Value)> ReadDictionary(ExpressionSyntax expr, Dictionary<string, string> consts)
    {
        if (expr is not ImplicitObjectCreationExpressionSyntax ioce
            || ioce.Initializer == null) yield break;
        foreach (var e in ioce.Initializer.Expressions)
        {
            if (e is AssignmentExpressionSyntax pa
                && pa.Left is ImplicitElementAccessSyntax iea
                && iea.ArgumentList.Arguments.Count > 0
                && iea.ArgumentList.Arguments[0].Expression is LiteralExpressionSyntax keyLit
                && keyLit.IsKind(SyntaxKind.StringLiteralExpression))
            {
                var k = keyLit.Token.ValueText;
                var v = ReadString(pa.Right, consts);
                if (v != null) yield return (k, v);
            }
        }
    }

    private static IEnumerable<string> ReadStringList(ExpressionSyntax expr, Dictionary<string, string> consts)
    {
        InitializerExpressionSyntax? init = null;
        if (expr is ImplicitObjectCreationExpressionSyntax ioce) init = ioce.Initializer;
        else if (expr is ObjectCreationExpressionSyntax oce) init = oce.Initializer;
        if (init == null) yield break;
        foreach (var item in init.Expressions)
        {
            var s = ReadString(item, consts);
            if (s != null) yield return s;
        }
    }

    /// Reads a list of static-factory references like `NPCData.PirateThugs` and
    /// returns the bare member names (`PirateThugs`).
    private static IEnumerable<string> ReadFactoryList(ExpressionSyntax expr, string expectedClass)
    {
        InitializerExpressionSyntax? init = null;
        if (expr is ImplicitObjectCreationExpressionSyntax ioce) init = ioce.Initializer;
        else if (expr is ObjectCreationExpressionSyntax oce) init = oce.Initializer;
        if (init == null) yield break;
        foreach (var item in init.Expressions)
        {
            if (item is MemberAccessExpressionSyntax m
                && m.Expression is IdentifierNameSyntax cls
                && cls.Identifier.Text == expectedClass)
                yield return m.Name.Identifier.Text;
        }
    }
}

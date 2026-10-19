using HCDE.ConversionInventory;

var root = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
if (!Directory.Exists(Path.Combine(root, "src/playsim")))
{
    Console.Error.WriteLine("Repository root not found. Pass the HCDE repository root as the first argument.");
    return 1;
}

var matrix = SupportMatrixBuilder.Build(root);
var docs = Path.Combine(root, "csharp", "docs");
SupportMatrixBuilder.WriteArtifacts(matrix, docs);
Console.WriteLine($"Wrote matrix with {matrix.Entries.Count} rows to {docs}");
foreach (var summary in matrix.Summaries)
    Console.WriteLine($"{summary.Category}: executed={summary.Executed} partial={summary.Partial} decoded={summary.DecodedOnly} absent={summary.Absent}");
return 0;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace ArchTestSourceGenerator;

[Generator]
public class ArchitectureTestGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Read the modules.mod file from AdditionalFiles
        var modulesFileProvider = context.AdditionalTextsProvider
            .Where(file => file.Path.EndsWith("modules.mod"))
            .Select((file, ct) => file.GetText(ct)!.ToString())
            .Collect();

        context.RegisterSourceOutput(modulesFileProvider, (spc, modules) =>
        {
            if (modules.IsEmpty)
                return;

            var moduleList = modules[0]
                .Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(m => m.Trim())
                .Where(m => !string.IsNullOrEmpty(m))
                .ToList();

            if (!moduleList.Any())
                return;

            // Generate the architecture test class
            var sourceCode = GenerateArchitectureTests(moduleList);
            spc.AddSource("GeneratedArchitectureTests.g.cs", SourceText.From(sourceCode, Encoding.UTF8));
        });
    }

    private string GenerateArchitectureTests(System.Collections.Generic.List<string> moduleProjects)
    {
        var sb = new StringBuilder();

        sb.AppendLine("using ArchUnitNET.Domain;");
        sb.AppendLine("using ArchUnitNET.Loader;");
        sb.AppendLine("using ArchUnitNET.xUnit;");
        sb.AppendLine("using static ArchUnitNET.Fluent.ArchRuleDefinition;");
        sb.AppendLine();
        sb.AppendLine("namespace ArchitectureTests;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Auto-generated architecture tests based on discovered module projects.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public class GeneratedArchitectureTests");
        sb.AppendLine("{");

        // Generate the Architecture field with all assemblies
        sb.AppendLine("    private static readonly Architecture Architecture = new ArchLoader()");
        sb.AppendLine("        .LoadAssemblies(");

        for (int i = 0; i < moduleProjects.Count; i++)
        {
            var project = moduleProjects[i];
            var comma = i < moduleProjects.Count - 1 ? "," : "";
            sb.AppendLine($"            typeof({project}.DoNotDelete).Assembly{comma}");
        }

        sb.AppendLine("        )");
        sb.AppendLine("        .Build();");
        sb.AppendLine();

        // Extract unique module names (top-level prefixes)
        var modules = moduleProjects
            .Select(p => p.Split('.')[0])
            .Distinct()
            .OrderBy(m => m)
            .ToList();

        // Generate modules array
        sb.AppendLine("    private static readonly string[] Modules = {");
        sb.Append("        ");
        sb.Append(string.Join(", ", modules.Select(m => $"\"{m}\"")));
        sb.AppendLine();
        sb.AppendLine("    };");
        sb.AppendLine();

        // Generate test methods
        GenerateTestMethods(sb);

        sb.AppendLine("}");

        return sb.ToString();
    }

    private void GenerateTestMethods(StringBuilder sb)
    {
        // Test 1: DomainModel should not reference other projects within same module
        sb.AppendLine("    [Fact]");
        sb.AppendLine("    public void DomainModelProject_ShouldNotReference_AnyOtherProjectWithinSameModule()");
        sb.AppendLine("    {");
        sb.AppendLine("        foreach (var module in Modules)");
        sb.AppendLine("        {");
        sb.AppendLine("            var domainModelTypes = Types()");
        sb.AppendLine("                .That()");
        sb.AppendLine("                .ResideInNamespaceMatching($\"{module}.DomainModel.*\")");
        sb.AppendLine("                .As($\"{module}.DomainModel types\");");
        sb.AppendLine();
        sb.AppendLine("            var otherModuleProjects = Types()");
        sb.AppendLine("                .That()");
        sb.AppendLine("                .ResideInNamespaceMatching($\"{module}.*\")");
        sb.AppendLine("                .And()");
        sb.AppendLine("                .DoNotResideInNamespaceMatching($\"{module}.DomainModel.*\")");
        sb.AppendLine("                .As($\"Other {module} projects\");");
        sb.AppendLine();
        sb.AppendLine("            var rule = domainModelTypes");
        sb.AppendLine("                .Should()");
        sb.AppendLine("                .NotDependOnAny(otherModuleProjects)");
        sb.AppendLine("                .Because($\"DomainModel project should not reference any other project within {module} module\");");
        sb.AppendLine();
        sb.AppendLine("            rule.Check(Architecture);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Test 2: Inter-module dependencies
        sb.AppendLine("    [Fact]");
        sb.AppendLine("    public void ModuleProjects_MustOnlyReference_PublishedInterfacesOfOtherModules()");
        sb.AppendLine("    {");
        sb.AppendLine("        foreach (var sourceModule in Modules)");
        sb.AppendLine("        {");
        sb.AppendLine("            var sourceModuleTypes = Types()");
        sb.AppendLine("                .That()");
        sb.AppendLine("                .ResideInNamespaceMatching($\"{sourceModule}.*\")");
        sb.AppendLine("                .As($\"{sourceModule} module types\");");
        sb.AppendLine();
        sb.AppendLine("            var disallowedDependencies = new List<IType>();");
        sb.AppendLine();
        sb.AppendLine("            foreach (var targetModule in Modules)");
        sb.AppendLine("            {");
        sb.AppendLine("                if (targetModule != sourceModule)");
        sb.AppendLine("                {");
        sb.AppendLine("                    var moduleTypes = Types()");
        sb.AppendLine("                        .That()");
        sb.AppendLine("                        .ResideInNamespaceMatching($\"{targetModule}.*\")");
        sb.AppendLine("                        .And()");
        sb.AppendLine("                        .DoNotResideInNamespaceMatching($\"{targetModule}.PublishedInterfaces.*\");");
        sb.AppendLine();
        sb.AppendLine("                    var types = moduleTypes.GetObjects(Architecture);");
        sb.AppendLine("                    disallowedDependencies.AddRange(types);");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            var rule = sourceModuleTypes");
        sb.AppendLine("                .Should()");
        sb.AppendLine("                .NotDependOnAny(disallowedDependencies)");
        sb.AppendLine("                .Because($\"{sourceModule} module should only reference PublishedInterfaces of other modules\");");
        sb.AppendLine();
        sb.AppendLine("            rule.Check(Architecture);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Test 3: Each module must have required projects
        sb.AppendLine("    [Fact]");
        sb.AppendLine("    public void EachModule_MustHave_ApplicationAndDomainModelProjects()");
        sb.AppendLine("    {");
        sb.AppendLine("        foreach (var module in Modules)");
        sb.AppendLine("        {");
        sb.AppendLine("            var hasApplication = Architecture.Types");
        sb.AppendLine("                .Any(t => t.Namespace?.FullName?.StartsWith($\"{module}.Application\") ?? false);");
        sb.AppendLine();
        sb.AppendLine("            var hasDomainModel = Architecture.Types");
        sb.AppendLine("                .Any(t => t.Namespace?.FullName?.StartsWith($\"{module}.DomainModel\") ?? false);");
        sb.AppendLine();
        sb.AppendLine("            Assert.True(hasApplication, $\"{module} module must have an Application project\");");
        sb.AppendLine("            Assert.True(hasDomainModel, $\"{module} module must have a DomainModel project\");");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
    }
}

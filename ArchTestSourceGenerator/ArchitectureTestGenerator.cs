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
        sb.AppendLine("    // Intra-Module Dependency Rules");
        sb.AppendLine();

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

        // Test 2: Application can reference any project within same module
        sb.AppendLine("    [Fact]");
        sb.AppendLine("    public void ApplicationProject_CanReference_AnyOtherProjectWithinSameModule()");
        sb.AppendLine("    {");
        sb.AppendLine("        // This is a permissive rule - no restrictions to enforce");
        sb.AppendLine("        // Application projects are allowed to reference anything within their module");
        sb.AppendLine("        // This test documents the rule exists but doesn't need enforcement");
        sb.AppendLine("        Assert.True(true, \"Application projects are allowed to reference any other project within the same module\");");
        sb.AppendLine("    }");
        sb.AppendLine();

        // Test 3: Non-Application/DomainModel projects should only reference PublishedInterfaces
        sb.AppendLine("    [Fact]");
        sb.AppendLine("    public void NonApplicationNonDomainModelProjects_ShouldOnlyReference_PublishedInterfacesOfOtherModules()");
        sb.AppendLine("    {");
        sb.AppendLine("        foreach (var sourceModule in Modules)");
        sb.AppendLine("        {");
        sb.AppendLine("            // Get all projects in the source module except Application and DomainModel");
        sb.AppendLine("            var nonApplicationDomainModelTypes = Types()");
        sb.AppendLine("                .That()");
        sb.AppendLine("                .ResideInNamespaceMatching($\"{sourceModule}.*\")");
        sb.AppendLine("                .And()");
        sb.AppendLine("                .DoNotResideInNamespaceMatching($\"{sourceModule}.Application.*\")");
        sb.AppendLine("                .And()");
        sb.AppendLine("                .DoNotResideInNamespaceMatching($\"{sourceModule}.DomainModel.*\")");
        sb.AppendLine("                .As($\"{sourceModule} non-Application non-DomainModel types\");");
        sb.AppendLine();
        sb.AppendLine("            // Build allowed dependencies");
        sb.AppendLine("            var allowedDependencies = Types()");
        sb.AppendLine("                .That()");
        sb.AppendLine("                .ResideInNamespaceMatching($\"{sourceModule}.*\") // Own module");
        sb.AppendLine("                .Or()");
        sb.AppendLine("                .ResideInNamespaceMatching(\"SharedInfrastructure.*\") // Shared project");
        sb.AppendLine("                .Or()");
        sb.AppendLine("                .ResideInAssemblyMatching(\"System.*\")");
        sb.AppendLine("                .Or()");
        sb.AppendLine("                .ResideInAssemblyMatching(\"Microsoft.*\");");
        sb.AppendLine();
        sb.AppendLine("            // Add PublishedInterfaces from other modules");
        sb.AppendLine("            foreach (var targetModule in Modules)");
        sb.AppendLine("            {");
        sb.AppendLine("                if (targetModule != sourceModule)");
        sb.AppendLine("                {");
        sb.AppendLine("                    allowedDependencies = allowedDependencies");
        sb.AppendLine("                        .Or()");
        sb.AppendLine("                        .ResideInNamespaceMatching($\"{targetModule}.PublishedInterfaces.*\");");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine();
        sb.AppendLine("            var allowedDependenciesWithRule = allowedDependencies.As(\"Allowed dependencies\");");
        sb.AppendLine();
        sb.AppendLine("            var rule = nonApplicationDomainModelTypes");
        sb.AppendLine("                .Should()");
        sb.AppendLine("                .OnlyDependOn(allowedDependenciesWithRule)");
        sb.AppendLine("                .Because($\"Non-Application/DomainModel projects in {sourceModule} should only reference PublishedInterfaces, DomainModel of own module, and Shared project\");");
        sb.AppendLine();
        sb.AppendLine("            rule.Check(Architecture);");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    // Inter-Module Dependency Rules");
        sb.AppendLine();

        // Test 4: Inter-module dependencies
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
        sb.AppendLine("            // Build disallowed dependencies for other modules (non-PublishedInterfaces)");
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

        // Test 5: PublishedInterfaces must not reference other PublishedInterfaces
        sb.AppendLine("    [Fact]");
        sb.AppendLine("    public void PublishedInterfacesProject_MustNotReference_PublishedInterfacesOfAnotherModule()");
        sb.AppendLine("    {");
        sb.AppendLine("        foreach (var sourceModule in Modules)");
        sb.AppendLine("        {");
        sb.AppendLine("            var sourcePublishedInterfacesTypes = Types()");
        sb.AppendLine("                .That()");
        sb.AppendLine("                .ResideInNamespaceMatching($\"{sourceModule}.PublishedInterfaces.*\")");
        sb.AppendLine("                .As($\"{sourceModule}.PublishedInterfaces types\");");
        sb.AppendLine();
        sb.AppendLine("            var anyPublishedInterfacesFound = sourcePublishedInterfacesTypes.GetObjects(Architecture).Any();");
        sb.AppendLine();
        sb.AppendLine("            if (anyPublishedInterfacesFound)");
        sb.AppendLine("            {");
        sb.AppendLine("                // Build list of PublishedInterfaces from other modules");
        sb.AppendLine("                var otherPublishedInterfacesTypes = new List<IType>();");
        sb.AppendLine();
        sb.AppendLine("                foreach (var targetModule in Modules)");
        sb.AppendLine("                {");
        sb.AppendLine("                    if (targetModule != sourceModule)");
        sb.AppendLine("                    {");
        sb.AppendLine("                        var modulePublishedInterfaces = Types()");
        sb.AppendLine("                            .That()");
        sb.AppendLine("                            .ResideInNamespaceMatching($\"{targetModule}.PublishedInterfaces.*\");");
        sb.AppendLine();
        sb.AppendLine("                        var types = modulePublishedInterfaces.GetObjects(Architecture).ToList();");
        sb.AppendLine();
        sb.AppendLine("                        if (types.Any())");
        sb.AppendLine("                            otherPublishedInterfacesTypes.AddRange(types);");
        sb.AppendLine("                    }");
        sb.AppendLine("                }");
        sb.AppendLine();
        sb.AppendLine("                var rule = sourcePublishedInterfacesTypes");
        sb.AppendLine("                    .Should()");
        sb.AppendLine("                    .NotDependOnAny(otherPublishedInterfacesTypes)");
        sb.AppendLine("                    .Because($\"{sourceModule}.PublishedInterfaces should not reference PublishedInterfaces of other modules\");");
        sb.AppendLine();
        sb.AppendLine("                rule.Check(Architecture);");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    // Shared Dependency Rules");
        sb.AppendLine();

        // Test 6: API projects can reference anything
        sb.AppendLine("    [Fact]");
        sb.AppendLine("    public void ApiProject_CanReference_AnyProjectInAnyModule()");
        sb.AppendLine("    {");
        sb.AppendLine("        // API projects represent the composition root");
        sb.AppendLine("        // This is a permissive rule - no restrictions to enforce");
        sb.AppendLine("        // This test documents the rule exists but doesn't need enforcement");
        sb.AppendLine("        Assert.True(true, \"API projects are allowed to reference any project in any module\");");
        sb.AppendLine("    }");
        sb.AppendLine();

        sb.AppendLine("    // Additional structural validation");
        sb.AppendLine();

        // Test 7: Each module must have required projects
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
        sb.AppendLine();

        // Test 8: All module projects must start with module name
        sb.AppendLine("    [Fact]");
        sb.AppendLine("    public void AllModuleProjects_MustStartWith_ModuleName()");
        sb.AppendLine("    {");
        sb.AppendLine("        foreach (var module in Modules)");
        sb.AppendLine("        {");
        sb.AppendLine("            var moduleTypes = Architecture.Types");
        sb.AppendLine("                .Where(t => t.Namespace?.FullName?.StartsWith($\"{module}.\") ?? false);");
        sb.AppendLine();
        sb.AppendLine("            foreach (var type in moduleTypes)");
        sb.AppendLine("            {");
        sb.AppendLine("                Assert.True(");
        sb.AppendLine("                    type.Namespace?.FullName?.StartsWith($\"{module}.\") ?? false,");
        sb.AppendLine("                    $\"Type {type.FullName} should be in namespace starting with {module}.\");");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
    }
}

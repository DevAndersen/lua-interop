global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.CSharp;
global using Microsoft.CodeAnalysis.CSharp.Syntax;
global using System.Collections.Immutable;
global using System.Diagnostics.CodeAnalysis;
global using System.Text;
global using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
global using NullableTypeDictionary = System.Collections.Generic.Dictionary<LuaInterop.SourceGen.TypeDictionaryId, (string FullTypeName, Microsoft.CodeAnalysis.INamedTypeSymbol? NamedTypeSymbol)>;
global using TypeDictionary = System.Collections.Generic.Dictionary<LuaInterop.SourceGen.TypeDictionaryId, Microsoft.CodeAnalysis.INamedTypeSymbol>;

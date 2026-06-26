using System;
using System.Linq;

namespace VYaml.SourceGenerator;

public enum NamingConvention
{
    LowerCamelCase,
    UpperCamelCase,
    SnakeCase,
    KebabCase,
}

// Value-equatable projection of one [YamlObject] type, produced by TypeMetaAnalyzer and consumed by
// Emitter. This is the incremental cache key: it carries only primitives / strings / EquatableArrays,
// so an edit that does not change a type's relevant shape produces an equal model, letting Roslyn skip
// both the emit and the source-output stage downstream.
sealed class TypeMetaModel : IEquatable<TypeMetaModel>
{
    public string HintName { get; }
    public bool IsValid { get; }
    public EquatableArray<DiagnosticInfo> Diagnostics { get; }

    public string TypeName { get; }
    public string SanitizedTypeName { get; }
    public string FullTypeName { get; }
    public string Namespace { get; }
    public bool HasNamespace { get; }
    public bool IsValueType { get; }
    public bool IsInterface { get; }
    public bool HasBaseYamlObject { get; }
    public string TypeDeclarationKeyword { get; }
    public bool IsUnion { get; }
    public NamingConvention NamingConventionByType { get; }
    public bool HasConstructor { get; }

    public EquatableArray<MemberMetaModel> Members { get; }
    public EquatableArray<string> ConstructorParameterNames { get; }
    public EquatableArray<string> SetterMemberNames { get; }
    public EquatableArray<UnionMetaModel> Unions { get; }

    public TypeMetaModel(
        string hintName,
        bool isValid,
        EquatableArray<DiagnosticInfo> diagnostics,
        string typeName,
        string sanitizedTypeName,
        string fullTypeName,
        string @namespace,
        bool hasNamespace,
        bool isValueType,
        bool isInterface,
        bool hasBaseYamlObject,
        string typeDeclarationKeyword,
        bool isUnion,
        NamingConvention namingConventionByType,
        bool hasConstructor,
        EquatableArray<MemberMetaModel> members,
        EquatableArray<string> constructorParameterNames,
        EquatableArray<string> setterMemberNames,
        EquatableArray<UnionMetaModel> unions)
    {
        HintName = hintName;
        IsValid = isValid;
        Diagnostics = diagnostics;
        TypeName = typeName;
        SanitizedTypeName = sanitizedTypeName;
        FullTypeName = fullTypeName;
        Namespace = @namespace;
        HasNamespace = hasNamespace;
        IsValueType = isValueType;
        IsInterface = isInterface;
        HasBaseYamlObject = hasBaseYamlObject;
        TypeDeclarationKeyword = typeDeclarationKeyword;
        IsUnion = isUnion;
        NamingConventionByType = namingConventionByType;
        HasConstructor = hasConstructor;
        Members = members;
        ConstructorParameterNames = constructorParameterNames;
        SetterMemberNames = setterMemberNames;
        Unions = unions;
    }

    public bool Equals(TypeMetaModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return HintName == other.HintName &&
               IsValid == other.IsValid &&
               TypeName == other.TypeName &&
               SanitizedTypeName == other.SanitizedTypeName &&
               FullTypeName == other.FullTypeName &&
               Namespace == other.Namespace &&
               HasNamespace == other.HasNamespace &&
               IsValueType == other.IsValueType &&
               IsInterface == other.IsInterface &&
               HasBaseYamlObject == other.HasBaseYamlObject &&
               TypeDeclarationKeyword == other.TypeDeclarationKeyword &&
               IsUnion == other.IsUnion &&
               NamingConventionByType == other.NamingConventionByType &&
               HasConstructor == other.HasConstructor &&
               Members.Equals(other.Members) &&
               ConstructorParameterNames.Equals(other.ConstructorParameterNames) &&
               SetterMemberNames.Equals(other.SetterMemberNames) &&
               Unions.Equals(other.Unions) &&
               Diagnostics.Equals(other.Diagnostics);
    }

    public override bool Equals(object? obj) => obj is TypeMetaModel other && Equals(other);

    public override int GetHashCode()
    {
        var hash = HintName.GetHashCode();
        hash = unchecked(hash * 397) ^ FullTypeName.GetHashCode();
        hash = unchecked(hash * 397) ^ IsUnion.GetHashCode();
        hash = unchecked(hash * 397) ^ NamingConventionByType.GetHashCode();
        hash = unchecked(hash * 397) ^ Members.GetHashCode();
        hash = unchecked(hash * 397) ^ Unions.GetHashCode();
        hash = unchecked(hash * 397) ^ Diagnostics.GetHashCode();
        return hash;
    }
}

sealed class MemberMetaModel : IEquatable<MemberMetaModel>
{
    public string Name { get; }
    public string KeyName { get; }
    public string FullTypeName { get; }
    public bool HasKeyNameAlias { get; }
    public NamingConvention NamingConventionByType { get; }
    public bool IsReferenceType { get; }
    public bool IsNullableValueType { get; }
    public bool IsValueType { get; }
    public string DefaultValueComparison { get; }
    public string DefaultValueExpression { get; }

    // Derived purely from KeyName (its UTF8 encoding); excluded from equality/hash.
    public byte[] KeyNameUtf8Bytes { get; }

    public MemberMetaModel(
        string name,
        string keyName,
        string fullTypeName,
        bool hasKeyNameAlias,
        NamingConvention namingConventionByType,
        bool isReferenceType,
        bool isNullableValueType,
        bool isValueType,
        string defaultValueComparison,
        string defaultValueExpression,
        byte[] keyNameUtf8Bytes)
    {
        Name = name;
        KeyName = keyName;
        FullTypeName = fullTypeName;
        HasKeyNameAlias = hasKeyNameAlias;
        NamingConventionByType = namingConventionByType;
        IsReferenceType = isReferenceType;
        IsNullableValueType = isNullableValueType;
        IsValueType = isValueType;
        DefaultValueComparison = defaultValueComparison;
        DefaultValueExpression = defaultValueExpression;
        KeyNameUtf8Bytes = keyNameUtf8Bytes;
    }

    public bool Equals(MemberMetaModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Name == other.Name &&
               KeyName == other.KeyName &&
               FullTypeName == other.FullTypeName &&
               HasKeyNameAlias == other.HasKeyNameAlias &&
               NamingConventionByType == other.NamingConventionByType &&
               IsReferenceType == other.IsReferenceType &&
               IsNullableValueType == other.IsNullableValueType &&
               IsValueType == other.IsValueType &&
               DefaultValueComparison == other.DefaultValueComparison &&
               DefaultValueExpression == other.DefaultValueExpression;
    }

    public override bool Equals(object? obj) => obj is MemberMetaModel other && Equals(other);

    public override int GetHashCode()
    {
        var hash = Name.GetHashCode();
        hash = unchecked(hash * 397) ^ KeyName.GetHashCode();
        hash = unchecked(hash * 397) ^ FullTypeName.GetHashCode();
        hash = unchecked(hash * 397) ^ HasKeyNameAlias.GetHashCode();
        hash = unchecked(hash * 397) ^ NamingConventionByType.GetHashCode();
        hash = unchecked(hash * 397) ^ DefaultValueExpression.GetHashCode();
        return hash;
    }
}

sealed class UnionMetaModel : IEquatable<UnionMetaModel>
{
    public string SubTypeTag { get; }
    public string FullTypeName { get; }

    public UnionMetaModel(string subTypeTag, string fullTypeName)
    {
        SubTypeTag = subTypeTag;
        FullTypeName = fullTypeName;
    }

    public bool Equals(UnionMetaModel? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return SubTypeTag == other.SubTypeTag && FullTypeName == other.FullTypeName;
    }

    public override bool Equals(object? obj) => obj is UnionMetaModel other && Equals(other);

    public override int GetHashCode() => unchecked(SubTypeTag.GetHashCode() * 397) ^ FullTypeName.GetHashCode();
}

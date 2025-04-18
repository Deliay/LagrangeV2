﻿using Lagrange.Proto.Serialization;
using Lagrange.Proto.Generator.Utility;
using Lagrange.Proto.Generator.Utility.Extension;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SF = Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using SK = Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace Lagrange.Proto.Generator;

public partial class ProtoSourceGenerator
{
    private partial class Emitter
    {
        private MethodDeclarationSyntax EmitMeasureHandlerMethod()
        {
            ExpressionSyntax syntax;
            if (parser.Fields.Count == 0)
            {
                syntax = SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(0));
            }
            else
            {
                int constant = 0;
                var expressions = new List<ExpressionSyntax>();

                foreach (var kv in parser.Fields)
                {
                    var type = kv.Value.TypeSyntax;
                    string name = kv.Value.Name;
                    
                    var symbol = parser.Model.GetTypeSymbol(type);
                    string identifier = symbol.IsValueType && type.IsNullableType() ? name + ".Value" : name;
                    
                    var expr = kv.Value.WireType switch        
                    {
                        WireType.VarInt when symbol.IsEnumType() => EmitResolvableLengthExpression(kv.Key, kv.Value.WireType, identifier),
                        WireType.VarInt when symbol.SpecialType == SpecialType.System_Boolean => SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(1)),
                        WireType.VarInt => EmitVarIntLengthExpression(identifier),
                        WireType.Fixed32 => SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(4)),
                        WireType.Fixed64 => SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(8)),
                        WireType.LengthDelimited when type.IsStringType() => EmitStringLengthExpression(identifier),
                        WireType.LengthDelimited when type.IsByteArrayType() => EmitBytesLengthExpression(identifier),
                        WireType.LengthDelimited when symbol.IsUserDefinedType() => EmitProtoPackableLengthExpression(identifier),
                        _ => EmitResolvableLengthExpression(kv.Key, kv.Value.WireType, identifier)
                    };
                    
                    var tag = ProtoHelper.EncodeVarInt((kv.Key << 3) | (byte)kv.Value.WireType);
                    if (symbol.IsValueType && !type.IsNullableType())
                    {
                        if (parser.IgnoreDefaultFields)
                        {
                            var left = SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(0));
                            var right = SF.BinaryExpression(SK.AddExpression, SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(tag.Length)), expr);
                            expr = EmitDefaultCheckExpression(name, left, right);
                        }
                        else
                        {
                            constant += tag.Length;
                        }
                    }
                    else // null check with obj.{identifier}
                    {
                        var left = SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(0));
                        var right = SF.BinaryExpression(SK.AddExpression, SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(tag.Length)), expr);
                        expr = EmitNullableCheckExpression(name, left, right);
                    }
                    
                    expressions.Add(expr);
                }
                
                syntax = SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(constant));
                syntax = expressions.Aggregate(syntax, (current, expr) => SF.BinaryExpression(SK.AddExpression, current, expr));
            }

            string classFullName = $"global::{parser.Namespace}.{parser.Identifier}";
            var parameters = SF.ParameterList()
                .AddParameters(SF.Parameter(SF.Identifier("obj")).WithType(SF.ParseTypeName(classFullName)));
            
            return SF.MethodDeclaration(SF.PredefinedType(SF.Token(SK.IntKeyword)), "MeasureHandler")
                .AddModifiers(SF.Token(SK.PublicKeyword), SF.Token(SK.StaticKeyword))
                .WithParameterList(parameters)
                .WithBody(SF.Block(SF.ReturnStatement(syntax)));
        }
        
        private static ExpressionSyntax EmitResolvableLengthExpression(int field, WireType wireType, string identifier)
        {
            var fieldNumber = SF.LiteralExpression(SK.NumericLiteralExpression, SF.Literal(field));
            var wire = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("global::Lagrange.Proto.Serialization.WireType"), SF.IdentifierName(wireType.ToString()));
            var obj = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(identifier));
            var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("global::Lagrange.Proto.Primitives.ProtoResolvableUtility"), SF.IdentifierName("Measure"));
            return SF.InvocationExpression(access).AddArgumentListArguments(SF.Argument(fieldNumber), SF.Argument(wire), SF.Argument(obj));
        }
        
        private static ExpressionSyntax EmitVarIntLengthExpression(string identifier)
        {
            var obj = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(identifier));
            var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("global::Lagrange.Proto.Utility.ProtoHelper"), SF.IdentifierName("GetVarIntLength"));
            return SF.InvocationExpression(access).AddArgumentListArguments(SF.Argument(obj));
        }

        private static ExpressionSyntax EmitStringLengthExpression(string identifier)
        {
            var obj = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(identifier));
            var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("global::Lagrange.Proto.Utility.ProtoHelper"), SF.IdentifierName("CountString"));
            return SF.InvocationExpression(access).AddArgumentListArguments(SF.Argument(obj));
        }
        
        private static ExpressionSyntax EmitBytesLengthExpression(string identifier)
        {
            var obj = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(identifier));
            var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("global::Lagrange.Proto.Utility.ProtoHelper"), SF.IdentifierName("CountBytes"));
            return SF.InvocationExpression(access).AddArgumentListArguments(SF.Argument(obj));
        }
        
        private static ExpressionSyntax EmitProtoPackableLengthExpression(string name)
        {
            var obj = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(name));
            var access = SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("global::Lagrange.Proto.Utility.ProtoHelper"), SF.IdentifierName("CountProtoPackable"));
            return SF.InvocationExpression(access).AddArgumentListArguments(SF.Argument(obj));
        }

        private static ExpressionSyntax EmitNullableCheckExpression(string identifier, ExpressionSyntax left, ExpressionSyntax right) => SF.ParenthesizedExpression(
                SF.ConditionalExpression(SF.BinaryExpression(SK.EqualsExpression, SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(identifier)), SF.LiteralExpression(SK.NullLiteralExpression)), left, right)
            );
        
        private static ExpressionSyntax EmitDefaultCheckExpression(string identifier, ExpressionSyntax left, ExpressionSyntax right) => SF.ParenthesizedExpression(
            SF.ConditionalExpression(SF.BinaryExpression(SK.EqualsExpression, SF.MemberAccessExpression(SK.SimpleMemberAccessExpression, SF.IdentifierName("obj"), SF.IdentifierName(identifier)), SF.LiteralExpression(SK.DefaultLiteralExpression)), left, right)
        );
    }
}
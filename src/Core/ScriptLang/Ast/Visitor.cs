﻿namespace ScTools.ScriptLang.Ast
{
    using ScTools.ScriptLang.Ast.Declarations;
    using ScTools.ScriptLang.Ast.Expressions;
    using ScTools.ScriptLang.Ast.Statements;
    using ScTools.ScriptLang.Ast.Types;

    public interface IVisitor<TReturn, TParam>
    {
        TReturn Visit(Program node, TParam param);

        TReturn Visit(EnumDeclaration node, TParam param);
        TReturn Visit(EnumMemberDeclaration node, TParam param);
        TReturn Visit(FuncDeclaration node, TParam param);
        TReturn Visit(FuncProtoDeclaration node, TParam param);
        TReturn Visit(FuncParameter node, TParam param);
        TReturn Visit(GlobalBlockDeclaration node, TParam param);
        TReturn Visit(LabelDeclaration node, TParam param);
        TReturn Visit(StructDeclaration node, TParam param);
        TReturn Visit(StructField node, TParam param);
        TReturn Visit(VarDeclaration node, TParam param);

        TReturn Visit(BinaryExpression node, TParam param);
        TReturn Visit(BoolLiteralExpression node, TParam param);
        TReturn Visit(FieldAccessExpression node, TParam param);
        TReturn Visit(FloatLiteralExpression node, TParam param);
        TReturn Visit(IndexingExpression node, TParam param);
        TReturn Visit(IntLiteralExpression node, TParam param);
        TReturn Visit(InvocationExpression node, TParam param);
        TReturn Visit(NullExpression node, TParam param);
        TReturn Visit(SizeOfExpression node, TParam param);
        TReturn Visit(StringLiteralExpression node, TParam param);
        TReturn Visit(UnaryExpression node, TParam param);
        TReturn Visit(ValueDeclRefExpression node, TParam param);
        TReturn Visit(VectorExpression node, TParam param);

        TReturn Visit(AssignmentStatement node, TParam param);
        TReturn Visit(BreakStatement node, TParam param);
        TReturn Visit(GotoStatement node, TParam param);
        TReturn Visit(IfStatement node, TParam param);
        TReturn Visit(RepeatStatement node, TParam param);
        TReturn Visit(ReturnStatement node, TParam param);
        TReturn Visit(SwitchStatement node, TParam param);
        TReturn Visit(ValueSwitchCase node, TParam param);
        TReturn Visit(DefaultSwitchCase node, TParam param);
        TReturn Visit(WhileStatement node, TParam param);

        TReturn Visit(ArrayRefType node, TParam param);
        TReturn Visit(ArrayType node, TParam param);
        TReturn Visit(BoolType node, TParam param);
        TReturn Visit(EnumType node, TParam param);
        TReturn Visit(FloatType node, TParam param);
        TReturn Visit(FuncType node, TParam param);
        TReturn Visit(IntType node, TParam param);
        TReturn Visit(RefType node, TParam param);
        TReturn Visit(StringType node, TParam param);
        TReturn Visit(StructType node, TParam param);
        TReturn Visit(UnresolvedNamedType node, TParam param);
    }
}

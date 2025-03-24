﻿namespace ScTools.ScriptLang.CodeGen;

using System;
using System.Diagnostics;
using System.Linq;

using ScTools.ScriptLang.Ast;
using ScTools.ScriptLang.Ast.Declarations;
using ScTools.ScriptLang.Ast.Expressions;
using ScTools.ScriptLang.Ast.Statements;
using ScTools.ScriptLang.Types;

/// <summary>
/// Emits code to execute statements.
/// </summary>
internal sealed class StatementEmitter : AstVisitor
{
    private readonly ICodeEmitter _C;

    public StatementEmitter(ICodeEmitter codeEmitter) => _C = codeEmitter;

    public override void Visit(VarDeclaration node)
    {
        Debug.Assert(node.Kind is VarKind.Local);

        _C.AllocateFrameSpaceForLocal(node);

        if (node.Initializer is null && node.Semantics.ValueType!.IsDefaultInitialized())
        {
            _C.EmitDefaultInit(node);
        }
        else if (node.Initializer is not null)
        {
            _C.EmitAssignmentToVar(node, node.Initializer);
        }
    }

    public override void Visit(AssignmentStatement node)
    {
        if (node.CompoundOperator is not null)
        {
            _C.EmitCompoundAssignment(node.LHS, node.RHS, node.CompoundOperator.Value);
        }
        else
        {
            _C.EmitAssignment(node.LHS, node.RHS);
        }
    }

    public override void Visit(BreakStatement node)
    {
        _C.EmitJump(node.Semantics.EnclosingStatement!.Semantics.ExitLabel!);
    }

    public override void Visit(ContinueStatement node)
    {
        _C.EmitJump(((ISemanticNode<LoopStatementSemantics>)node.Semantics.EnclosingLoop!).Semantics.ContinueLabel!);
    }

    public override void Visit(GotoStatement node)
    {
        _C.EmitJump(node.Semantics.Target!.Name);
    }

    public override void Visit(IfStatement node)
    {
        var sem = node.Semantics;
        // check condition
        _C.EmitValue(node.Condition);
        _C.EmitJumpIfZero(sem.ElseLabel!);

        // then body
        _C.EmitStatementBlock(node.Then);
        if (node.Else.Any())
        {
            // jump over the else body
            _C.EmitJump(sem.EndLabel!);
        }

        // else body
        _C.Label(sem.ElseLabel!);
        _C.EmitStatementBlock(node.Else);

        _C.Label(sem.EndLabel!);
    }

    public override void Visit(WhileStatement node)
    {
        var sem = node.Semantics;
        _C.Label(sem.BeginLabel!);
        _C.Label(sem.ContinueLabel!);

        // check condition
        _C.EmitValue(node.Condition);

        _C.EmitJumpIfZero(sem.ExitLabel!);

        // body
        _C.EmitStatementBlock(node.Body);

        // jump back to condition check
        _C.EmitJump(sem.BeginLabel!);

        _C.Label(sem.ExitLabel!);
    }

    public override void Visit(RepeatStatement node)
    {
        // synthesized expressions and statements
        var constantZero = new IntLiteralExpression(Token.Integer(0)) { Semantics = new(IntType.Instance, ValueKind: ValueKind.RValue | ValueKind.Constant, ArgumentKind: ArgumentKind.None) };
        var constantOne = new IntLiteralExpression(Token.Integer(1)) { Semantics = new(IntType.Instance, ValueKind: ValueKind.RValue | ValueKind.Constant, ArgumentKind: ArgumentKind.None) };
        var counterLessThanLimit = new BinaryExpression(TokenKind.LessThan.Create(), node.Counter, node.Limit) { Semantics = new(BoolType.Instance, ValueKind: ValueKind.RValue, ArgumentKind: ArgumentKind.None) };
        var counterIncrement = new AssignmentStatement(TokenKind.PlusEquals.Create(), node.Counter, constantOne, label: null);

        var sem = node.Semantics;
        
        // set counter to 0
        _C.EmitAssignment(node.Counter, constantZero);

        // check condition counter < limit
        _C.Label(sem.BeginLabel!);
        _C.EmitValue(counterLessThanLimit);
        _C.EmitJumpIfZero(sem.ExitLabel!);

        // body
        _C.EmitStatementBlock(node.Body);

        // increment counter
        _C.Label(sem.ContinueLabel!);
        _C.EmitStatement(counterIncrement);

        // jump back to condition check
        _C.EmitJump(sem.BeginLabel!);

        _C.Label(sem.ExitLabel!);
    }

    public override void Visit(ForStatement node)
    {
        // synthesized expressions and statements
        var constantOne = new IntLiteralExpression(Token.Integer(1)) { Semantics = new(IntType.Instance, ValueKind: ValueKind.RValue | ValueKind.Constant, ArgumentKind: ArgumentKind.None) };
        var counterLessThanOrEqualToLimit = new BinaryExpression(TokenKind.LessThanEquals.Create(), node.Counter, node.Limit) { Semantics = new(BoolType.Instance, ValueKind: ValueKind.RValue, ArgumentKind: ArgumentKind.None) };
        var counterIncrement = new AssignmentStatement(TokenKind.PlusEquals.Create(), node.Counter, constantOne, label: null);

        var sem = node.Semantics;

        // set counter to 0
        _C.EmitAssignment(node.Counter, node.Initializer);

        // check condition counter <= limit
        _C.Label(sem.BeginLabel!);
        _C.EmitValue(counterLessThanOrEqualToLimit);
        _C.EmitJumpIfZero(sem.ExitLabel!);

        // body
        _C.EmitStatementBlock(node.Body);

        // increment counter
        _C.Label(sem.ContinueLabel!);
        _C.EmitStatement(counterIncrement);

        // jump back to condition check
        _C.EmitJump(sem.BeginLabel!);

        _C.Label(sem.ExitLabel!);
    }

    public override void Visit(ReturnStatement node)
    {
        if (node.Expression is not null)
        {
            _C.EmitValue(node.Expression);
        }
        _C.EmitEpilogue();
    }

    public override void Visit(SwitchStatement node)
    {
        _C.EmitValue(node.Expression);

        _C.EmitSwitch(node.Cases.OfType<ValueSwitchCase>());

        var defaultCase = node.Cases.OfType<DefaultSwitchCase>().SingleOrDefault();
        _C.EmitJump(defaultCase?.Semantics.Label ?? node.Semantics.ExitLabel!);
        
        foreach (var @case in node.Cases)
        {
            _C.Label(@case.Semantics.Label!);
            _C.EmitStatementBlock(@case.Body);
        }

        _C.Label(node.Semantics.ExitLabel!);
    }

    public override void Visit(ExpressionStatement node)
    {
        if (node.Expression is PostfixUnaryExpression
            {
                Operator: PostfixUnaryOperator.Increment or PostfixUnaryOperator.Decrement
            })
        {
            // postfix increment/decrement do not actually push any value to the stack when used as statements,
            // so we don't need to drop it
            _C.EmitValue(node.Expression);
        }
        else
        {
            _C.EmitValueAndDrop(node.Expression);
        }
    }
}

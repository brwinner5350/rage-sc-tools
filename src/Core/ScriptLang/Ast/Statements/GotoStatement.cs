﻿namespace ScTools.ScriptLang.Ast.Statements;

using System.Diagnostics;

/// <param name="Target">The statement corresponding to <see cref="GotoStatement.TargetLabel"/>.</param>
public record struct GotoStatementSemantics(IStatement? Target);

public sealed class GotoStatement : BaseStatement, ISemanticNode<GotoStatementSemantics>
{
    public Token TargetLabelToken => Tokens[1];
    public string TargetLabel => TargetLabelToken.Lexeme.ToString();
    public GotoStatementSemantics Semantics { get; set; }

    public GotoStatement(Token gotoToken, Token targetLabelIdentifierToken, Label? label)
        : base(OfTokens(gotoToken, targetLabelIdentifierToken), OfChildren(), label)
    {
        Debug.Assert(gotoToken.Kind is TokenKind.GOTO);
        Debug.Assert(targetLabelIdentifierToken.Kind is TokenKind.Identifier);
    }

    public override TReturn Accept<TReturn, TParam>(IVisitor<TReturn, TParam> visitor, TParam param)
        => visitor.Visit(this, param);
    public override void Accept(IVisitor visitor) => visitor.Visit(this);

    public override string DebuggerDisplay =>
        $@"{nameof(GotoStatement)} {{ {nameof(TargetLabel)} = {TargetLabel} }}";
}

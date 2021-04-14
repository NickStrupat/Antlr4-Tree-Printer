using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Spectre.Console;

var sourceText = "5 + 4a = 9";//"1 + 23 / 3 * 3.14 = 4";
var chars = CharStreams.fromString(sourceText);
var lexer = new CalculatorLexer(chars);
var tokens = new CommonTokenStream(lexer);
var parser = new CalculatorParser(tokens) { BuildParseTree = false };
var listener = new TreePrinterListener(parser);
parser.AddParseListener(listener);
parser.equation();

//ParseTreeWalker.Default.Walk(listener, parser.equation());
AnsiConsole.Render(listener.Tree);

//Console.WriteLine(parser.equation().ToStringTree());
//parser.Reset();

//parser.PrintPrettyTree();
//new PrettyTreePrinterVisitor().Visit(parser.equation());

static class ParseTreeExtensions
{
	public static void PrintPrettyTree(this Parser parser, String ruleColor = "green", String terminalColor = "red")
	{
		var startRuleMethod = parser.GetType().GetMethod(parser.RuleNames[0]);
		var startRule = (ParserRuleContext)startRuleMethod.Invoke(parser, null);
		var tree = new Tree("")
			.Style("grey");
		Visit(startRule, tree);
		AnsiConsole.Render(tree);

		void Visit(IParseTree parseTree, IHasTreeNodes node)
		{
			if (parseTree is ITerminalNode tn)
				node.AddNode($"[{terminalColor}]{tn.Symbol.Text}[/]");
			else if (parseTree is ParserRuleContext prc)
			{
				//if (prc.ChildCount == 1 && prc.children[0] is )
				node = node.AddNode($"[{ruleColor}]{parser.RuleNames[prc.RuleIndex]}[/]");
				foreach (var child in prc.children)
					Visit(child, node);
			}
			else //if (parseTree is ErrorNodeImpl en)
				throw new Exception(parseTree.GetType().Name);
		}
	}
}

class TreePrinterListener : IParseTreeListener
{
	readonly Parser parser;
	readonly String ruleColor;
	readonly String terminalColor;
	public TreePrinterListener(Parser parser, String ruleColor = "green", String terminalColor = "red") =>
		(this.parser, this.ruleColor, this.terminalColor) = (parser, ruleColor, terminalColor);

	public Tree Tree { get; private set; }
	
	readonly Stack<IHasTreeNodes> nodeStack = new();
	readonly Stack<String> ruleNameStack = new();
	readonly List<String> terminals = new();

	public void EnterEveryRule(ParserRuleContext ctx)
	{
		var ruleName = $"[{ruleColor}]{parser.RuleNames[ctx.RuleIndex]}[/]";
		if (Tree == null)
		{
			Tree = new Tree(ruleName);
			nodeStack.Push(Tree);
			return;
		}
		ruleNameStack.Push(parser.RuleNames[ctx.RuleIndex]);
		// nodeStack.Push(nodeStack.Peek().AddNode(ruleName));
		// foreach (var token in ctx.children.OfType<ITerminalNode>())
		// 	nodeStack.Peek().AddNodes($"[red]{token.Symbol.Text}[/]");
	}

	public void ExitEveryRule(ParserRuleContext ctx)
	{
		if (!terminals.Any())
			return;
		nodeStack.Pop().AddNodes(terminals);
		terminals.Clear();
	}

	public void VisitErrorNode(IErrorNode node)
	{
		AddNode();
		terminals.Add($"[black on red]{node.GetText()}[/]");
	}

	public void VisitTerminal(ITerminalNode node)
	{
		AddNode();
		terminals.Add($"[{terminalColor}]{node.GetText()}[/]");
	}

	private void AddNode()
	{
		var text = String.Join(" -> ", ruleNameStack.Reverse().Select(x => $"[{ruleColor}]{x}[/]"));
		ruleNameStack.Clear();
		nodeStack.Push(nodeStack.Peek().AddNode(text));
	}
}

// class PrettyTreePrinterVisitor : IParseTreeVisitor<Object>
// {
// 	public Tree Tree { get; private set; }
// 	private IHasTreeNodes node;
// 	public Object Visit(IParseTree tree)
// 	{
// 		if (Tree == null)
// 		{
// 			Tree = new Tree(tree);
// 		}
// 		Console.WriteLine(tree.GetText());
// 		return null;
// 	}
//
// 	public Object VisitChildren(IRuleNode node)
// 	{
// 		return null;
// 	}
//
// 	public Object VisitErrorNode(IErrorNode node)
// 	{
// 		return null;
// 	}
//
// 	public Object VisitTerminal(ITerminalNode node)
// 	{
// 		return null;
// 	}
// }
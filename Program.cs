using System;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

var sourceText = "1 + 23 / 3 * 3.14 = 4";
var chars = CharStreams.fromString(sourceText);
var lexer = new CalculatorLexer(chars);
var tokens = new CommonTokenStream(lexer);
var parser = new CalculatorParser(tokens) { BuildParseTree = false };
var listener = new TreePrinterListener(parser.RuleNames);
parser.AddParseListener(listener);
parser.equation();

class TreePrinterListener : IParseTreeListener
{
	readonly String[] ruleNames;
	readonly ConsoleColor ruleColor;
	readonly ConsoleColor terminalColor;

	public TreePrinterListener(
		String[] ruleNames,
		ConsoleColor ruleColor = ConsoleColor.Green,
		ConsoleColor terminalColor = ConsoleColor.Red
	)
	{
		this.ruleNames = ruleNames;
		this.ruleColor = ruleColor;
		this.terminalColor = terminalColor;
	}

	Byte depth;

	void WritePadding() => Console.Write(new String(' ', depth * 3));

	public void EnterEveryRule(ParserRuleContext ctx)
	{
		WritePadding();
		using (new ConsoleColorizer(ruleColor))
			Console.WriteLine(ruleNames[ctx.RuleIndex]);
		depth++;
	}

	public void ExitEveryRule(ParserRuleContext ctx) => depth--;

	public void VisitErrorNode(IErrorNode node)
	{
		depth++;
		WritePadding();
		using (new ConsoleColorizer(ConsoleColor.Black, ConsoleColor.Red))
			Console.WriteLine(node.GetText());
		depth--;
	}

	public void VisitTerminal(ITerminalNode node)
	{
		depth++;
		WritePadding();
		using (new ConsoleColorizer(terminalColor))
			Console.WriteLine(node.GetText());
		depth--;
	}

	ref struct ConsoleColorizer
	{
		readonly ConsoleColor background;
		readonly ConsoleColor foreground;

		public ConsoleColorizer(ConsoleColor foreground, ConsoleColor? background = null)
		{
			this.background = Console.BackgroundColor;
			this.foreground = Console.ForegroundColor;
			Console.ForegroundColor = foreground;
			Console.BackgroundColor = background ?? Console.BackgroundColor;
		}

		public void Dispose()
		{
			Console.BackgroundColor = background;
			Console.ForegroundColor = foreground;
		}
	}
}
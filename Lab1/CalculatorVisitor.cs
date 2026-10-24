using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Antlr4.Runtime.Misc;

namespace Lab1
{
    internal class CalculatorVisitor : CalculatorBaseVisitor<BigInteger>
    {
        Dictionary<string, BigInteger> TableId = new Dictionary<string, BigInteger>();

        public override BigInteger VisitAddSub(CalculatorParser.AddSubContext context)
        {
            BigInteger first = Visit(context.expr(0));
            BigInteger second = Visit(context.expr(1));
            if (context.op.Text == "+") return first + second;
            else return first - second;
        }
        public override BigInteger VisitMulDiv(CalculatorParser.MulDivContext context)
        {
            BigInteger first = Visit(context.expr(0));
            BigInteger second = Visit(context.expr(1));
            if (context.op.Text == "*") return first * second;
            else return first / second;
        }
        public override BigInteger VisitPower(CalculatorParser.PowerContext context)
        {
            BigInteger first = Visit(context.expr(0));
            BigInteger second = Visit(context.expr(1));
           return BigInteger.Pow(first, (int)second);
        }
        public override BigInteger VisitCellRef(CalculatorParser.CellRefContext context)
        {
            string cer = context.GetText();
            if (TableId.ContainsKey(cer)) return TableId[cer];
            else throw new ArgumentException("Клітинка не існує");
        }
        public override BigInteger VisitNumber(CalculatorParser.NumberContext context)
        {
            return BigInteger.Parse(context.GetText());
        }
        public override BigInteger VisitMaxFunc(CalculatorParser.MaxFuncContext context)
        {
            return BigInteger.Max(Visit(context.expr(0)), Visit(context.expr(1)));
        }
        public override BigInteger VisitMinFunc(CalculatorParser.MinFuncContext context)
        {
            return BigInteger.Min(Visit(context.expr(0)), Visit(context.expr(1)));
        }
        public override BigInteger VisitIncFunc( CalculatorParser.IncFuncContext context)
        {
            return Visit(context.expr()) + 1;
        }
        public override BigInteger VisitDecFunc(CalculatorParser.DecFuncContext context)
        {
            return Visit(context.expr()) - 1;
        }
        public override BigInteger VisitCompileUnit( CalculatorParser.CompileUnitContext context)
        {
            return Visit(context.expr());
        }
        public override BigInteger VisitParens(CalculatorParser.ParensContext context)
        {
            return Visit(context.expr());
        }
    }
}

using System;
using System.Numerics;

namespace Lab1
{
    public class CalculatorVisitor : CalculatorBaseVisitor<BigInteger>
    {
        private readonly Func<string, BigInteger> resolveCell;

        public CalculatorVisitor(Func<string, BigInteger> resolveCell)
        {
            this.resolveCell = resolveCell;
        }

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
            if (second == 0) throw new DivideByZeroException("Ділення на нуль");
            return first / second;
        }
        public override BigInteger VisitUnaryMinus(CalculatorParser.UnaryMinusContext context)
        {
            return -Visit(context.expr());
        }
        public override BigInteger VisitPower(CalculatorParser.PowerContext context)
        {
            BigInteger first = Visit(context.expr(0));
            BigInteger second = Visit(context.expr(1));

            if (second < 0)
            {
                if (first == 0) throw new DivideByZeroException("Ділення на нуль при від'ємному степені");
                return 1 / BigInteger.Pow(first, (int)(-second));
            }
            if (second > int.MaxValue)
                throw new ArgumentOutOfRangeException(null, "Показник степеня занадто великий");
            return BigInteger.Pow(first, (int)second);
        }

        public override BigInteger VisitCellRef(CalculatorParser.CellRefContext context)
        {
            string cellName = context.GetText();
            return resolveCell(cellName);
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

        public override BigInteger VisitIncFunc(CalculatorParser.IncFuncContext context)
        {
            return Visit(context.expr()) + 1;
        }

        public override BigInteger VisitDecFunc(CalculatorParser.DecFuncContext context)
        {
            return Visit(context.expr()) - 1;
        }

        public override BigInteger VisitCompileUnit(CalculatorParser.CompileUnitContext context)
        {
            return Visit(context.expr());
        }

        public override BigInteger VisitParens(CalculatorParser.ParensContext context)
        {
            return Visit(context.expr());
        }
    }
}
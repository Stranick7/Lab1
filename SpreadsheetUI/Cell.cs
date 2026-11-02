using System.Numerics;

namespace SpreadsheetUI
{
    internal class Cell
    {
        private string cellName;
        private string expression;
        private BigInteger computedValue;
        private bool hasError;

        public Cell(string name)
        {
            cellName = name;
        }

        public void SetExpression(string expr)
        {
            expression = expr;
        }

        public string GetDisplayValue(bool isExprMode)
        {
            if (string.IsNullOrEmpty(expression)) return "";
            if (hasError) return "ERROR";
            if (isExprMode) return expression;
            return computedValue.ToString();
        }

        public void expressionDelete()
        {
            expression = null;
        }

        public void computedValueZero()
        {
            computedValue = 0;
        }

        public string getName()
        {
            return cellName;
        }

        public string getExpression()
        {
            return expression;
        }

        public void setHasError(bool hasError)
        {
            this.hasError = hasError;
        }

        public bool getHasError()
        {
            return hasError;
        }

        public void setComputedValue(BigInteger computedValue)
        {
            this.computedValue = computedValue;
        }

        public BigInteger getComputedValue()
        {
            return computedValue;
        }
    }
}
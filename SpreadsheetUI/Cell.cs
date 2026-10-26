using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Lab1;

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
            else return computedValue.ToString();
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
        public void setComputedValue(BigInteger computedValue)
        {
            this.computedValue = computedValue; 
        }
    }
}

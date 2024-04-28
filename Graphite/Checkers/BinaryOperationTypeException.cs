﻿using Graphite.Lexer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphite.Checkers
{
    internal class BinaryOperationTypeException : CheckException
    {
        public BinaryOperationTypeException(TokenType type1, TokenType @operator, TokenType type2)
        : base(string.Format("Operator {0} cannot be applied to operands of type {1} and {2}", @operator, type1, type2))
        {}
    }
}
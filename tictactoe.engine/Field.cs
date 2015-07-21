using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace tictactoe.engine
{
    [DebuggerDisplay("WinnerX:{WinnerX},WinnerO:{WinnerO},GameOver:{GameOver}")]
    public class Field
    {
        private static int TOTAL = 9;
        private Cell[] _cells;
        private int _move;

        public Field()
        {
            _cells = new Cell[TOTAL];
        }

        public Field Set(int move, Cell cell)
        {
            if (move >= 0 && move < TOTAL)
            {
                if (_cells[move] == Cell.Empty)
                {
                    _cells[move] = cell;
                    return this;
                }
                throw new Exception("cant set occupied cell");
            }
            throw new Exception("cant set cell outside bounds");
        }

        public Cell Get(int position)
        {
            if (position >= 0 && position < TOTAL)
            {
                return _cells[position];
            }
            throw new Exception("cant get cell outside bounds");
        }

        public int Evaluate(int depth)
        {
            if (WinnerO) return +10 - depth;
            if (WinnerX) return depth - 10;
            return 0;
        }

        private bool Winner(Cell cell)
        {
            var combinations = new int[][] {
                new int[] { 0,1,2},
                new int[] { 3,4,5},
                new int[] { 6,7,8},
                new int[] { 0,3,6},
                new int[] { 1,4,7},
                new int[] { 2,5,8},
                new int[] { 0,4,8},
                new int[] { 2,4,6}
            };
            foreach (var combination in combinations)
            {
                var result = combination.All(idx => _cells[idx] == cell);
                if (result) return true;
            }

            return false;
        }

        public bool WinnerX
        {
            get
            {
                return Winner(Cell.X);
            }
        }

        public bool WinnerO
        {
            get
            {
                return Winner(Cell.O);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder(System.Environment.NewLine);
            foreach (var cell in _cells.Select((v, i) => new { Value = v, Index = i }))
            {
                sb.Append(cell.Value.AsString());
                if (cell.Index > 0 && (cell.Index + 1) % 3 == 0)
                {
                    sb.Append(Environment.NewLine);
                }
            }
            sb.Append(Environment.NewLine);
            sb.Append(ImmediateResult());
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        public IEnumerable<int> AvailableMoves
        {
            get
            {
                return _cells
              .Select((val, index) => new { Value = val, Index = index })
              .Where(x => x.Value == Cell.Empty)
              .Select(x => x.Index);
            }
        }

        public bool Full
        {
            get
            {
                return !AvailableMoves.Any();
            }
        }

        public bool GameOver
        {
            get
            {
                return WinnerX || WinnerO || Full;
            }
        }

        public Field Clone()
        {
            var temp = new Field();
            for (var i=0;i<TOTAL;i++)
            {
                temp._cells[i] = _cells[i];
            }
            return temp;
        }

        public void UserMove()
        {
            int move;
            var success = false;
            while (!success)
            {
                var str = Console.ReadLine();
                success = int.TryParse(str, out move);
                if (success)
                {
                    try
                    {
                        Set(move, Cell.X);
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine("Numeric values only");
                }
            }
        }

        public void CPUMove()
        {
            if (!GameOver)
            {
                Minimax(this, Cell.O);
                Set(_move, Cell.O);
            }
        }

        public int Minimax(Field field, Cell cell, int depth = 1)
        {
            if (field.GameOver)
            {
                var eval = field.Evaluate(depth);
                return eval;
            }

            var moves = new List<EvaluatedMove>();

            foreach (var move in field.AvailableMoves)
            {
                var clone = field.Clone().Set(move, cell);

                var evaluation = new EvaluatedMove()
                {
                    Move = move,
                    Score = Minimax(clone, cell.Reverse(), depth + 1)
                };
                moves.Add(evaluation);
            }
            { }
            if (cell == Cell.O)
            {
                var move = moves.Max();
                _move = move.Move;
                return move.Score;
            }
            if (cell == Cell.X)
            {
                var move = moves.Min();
                _move = move.Move;
                return move.Score;
            }
            throw new Exception("cant evaluate empty cell");
        }

        private string ImmediateResult()
        {
            if (GameOver)
            {
                if (WinnerX) return "You win"; //unlikely :(
                if (WinnerO) return "Computer wins";
                return "It's a draw";
            }
            return "Make your move:";
        }
    }
}

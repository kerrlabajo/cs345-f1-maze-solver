using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MazeSolver
{
    internal class ButtonPosition
    {
        private Button _button;
        private int _row;
        private int _column;

        public ButtonPosition(Button button, int row, int column)
        {
            _button = button;
            _row = row;
            _column = column;
        }
        public int GetRow() => _row;
        public int GetColumn() => _column;
        public Button GetButton() => _button;
        public int GetPosition(int size)
        {
            return (_row * size) + _column;
        }

        public string GetButtonPosition()
        {
            return _button.BackColor.ToString() + " at position " + _row + ", " + _column;
        }
    }
}

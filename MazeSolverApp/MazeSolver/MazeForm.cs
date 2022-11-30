using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data.Common;
using System.Collections;
using System.IO;
using System.Xml.Linq;

namespace MazeSolver
{
    public partial class MazeForm : Form
    {
        private Button _button;
        private readonly Queue<ButtonPosition> BFQueue;
        private HashSet<ButtonPosition> visitedPaths;
        private ButtonPosition Start;
        private ButtonPosition Goal;
        private new readonly bool[] WalledPath;
        private readonly bool[] isLocationVisited;
        private IDictionary<Button, List<int>> tree;
        public MazeForm()
        {
            BFQueue = new Queue<ButtonPosition>();
            tree = new Dictionary<Button, List<int>>();
            visitedPaths = new HashSet<ButtonPosition>();
            Goal = null;
            InitializeComponent();
            WalledPath = new bool[mazeLayout.RowCount * mazeLayout.ColumnCount];
            isLocationVisited = new bool[mazeLayout.RowCount * mazeLayout.ColumnCount];
            InitializeMazeLayout();
            AssignClickEvent();
        }

        private void InitializeMazeLayout()
        {
            int row, column;
            for (int i = 0; i < mazeLayout.ColumnCount; i++)
            {
                for (int j = 0; j < mazeLayout.RowCount; j++)
                {
                    if(j == 0 && i == 0)
                    {
                        _button = new Button();
                        _button.BackColor = Color.Green;
                        _button.Visible = true;
                        _button.Dock = DockStyle.Fill;
                        mazeLayout.Controls.Add(_button, i, j);

                        row = mazeLayout.GetPositionFromControl(_button).Row;
                        column = mazeLayout.GetPositionFromControl(_button).Column;
                        BFQueue.Enqueue(new ButtonPosition(_button, row, column));
                        Start = new ButtonPosition(_button, row, column);
                        WalledPath[(mazeLayout.RowCount * row) + column] = true;
                        Debug.WriteLine("Start: " + BFQueue.First().GetButtonPosition());
                    }
                    else
                    {
                        _button = new Button();
                        _button.BackColor = Color.White;
                        _button.Visible = true;
                        _button.Dock = DockStyle.Fill;
                        mazeLayout.Controls.Add(_button, i, j);
                    }
                }
            }
        }

        private void AssignClickEvent()
        {
            foreach (Control c in mazeLayout.Controls.OfType<Button>())
            {
                c.MouseDown += new MouseEventHandler(Onlick);
            }
        }

        private void Onlick(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            int column, row;
            if(e.Button == MouseButtons.Left)
            {
                if(button.BackColor == Color.Gray)
                {
                    row = mazeLayout.GetPositionFromControl(button).Row;
                    column = mazeLayout.GetPositionFromControl(button).Column;
                    WalledPath[(mazeLayout.RowCount * row) + column] = false;
                    button.BackColor = Color.White;
                    Debug.WriteLine("Location: " + ((mazeLayout.RowCount * row) + column) + " = " + WalledPath[(mazeLayout.RowCount * row) + column]);
                }
                else
                {
                    row = mazeLayout.GetPositionFromControl(button).Row;
                    column = mazeLayout.GetPositionFromControl(button).Column;
                    WalledPath[(mazeLayout.RowCount * row) + column] = true;
                    button.BackColor = Color.Gray;
                    Debug.WriteLine("Location: " + ((mazeLayout.RowCount*row)+column) + " = " + WalledPath[(mazeLayout.RowCount*row) + column]);
                }
            }
            else if(e.Button == MouseButtons.Right)
            {
                if (button.BackColor == Color.Green && button == BFQueue.Peek().GetButton())
                {
                    button.BackColor = Color.White;
                    BFQueue.Dequeue();
                    Debug.WriteLine("Start Erased");
                }
                else if (button.BackColor == Color.Green && button == Goal.GetButton())
                {
                    button.BackColor = Color.White;
                    Goal = null;
                    Debug.WriteLine("Goal Erased");
                }
                else if (button.BackColor == Color.White && BFQueue.Count == 0)
                {
                    button.BackColor = Color.Green;
                    row = mazeLayout.GetPositionFromControl(button).Row;
                    column = mazeLayout.GetPositionFromControl(button).Column;
                    BFQueue.Enqueue(new ButtonPosition(button, row, column));
                    Debug.WriteLine("New Start: " + BFQueue.First().GetButtonPosition());
                }
                else if ((button.BackColor == Color.White || button.BackColor == Color.Gray) && Goal == null)
                {
                    button.BackColor = Color.Green;
                    row = mazeLayout.GetPositionFromControl(button).Row;
                    column = mazeLayout.GetPositionFromControl(button).Column;
                    Goal = new ButtonPosition(button, row, column);
                    Debug.WriteLine("Goal: " + Goal.GetButtonPosition());
                    SearchForPath(BFQueue.First().GetButton());
                }
            }
        }

        private void SearchForPath(Button originButton)
        {
            int row = mazeLayout.GetPositionFromControl(originButton).Row;
            int column = mazeLayout.GetPositionFromControl(originButton).Column;

            if (row == Goal.GetRow() && column == Goal.GetColumn())
            {
                Debug.WriteLine("Ended at SearchForPath!");
                return;
            }
            else
            {
                Debug.WriteLine("\nStarting at " + row + ", " + column);

                int searchUp = 0, searchDown = 0, searchLeft = 0, searchRight = 0;
                if (row == 0)
                {
                    if (column == 0)
                    {
                        searchRight += column + 1;
                        searchLeft = column;
                    }
                    else
                    {
                        searchRight += column + 1;
                        searchLeft += column - 1;
                    }
                    searchDown += ((row * 10) + column) + 10;
                }
                else if (row == 9)
                {
                    if (column == 9)
                    {
                        searchLeft += ((row * 10) + column) - 1;
                        searchRight = ((row * 10) + column);
                    }
                    else
                    {
                        searchLeft += ((row * 10) + column) - 1;
                        searchRight += ((row * 10) + column) + 1;
                    }
                    searchUp += ((row * 10) + column)-10;
                }
                else
                {
                    searchUp += (mazeLayout.RowCount * row) - 10 + column;
                    searchDown += (mazeLayout.RowCount * row) + 10 + column;
                    searchLeft += (mazeLayout.RowCount * row) + column - 1;
                    searchRight += (mazeLayout.RowCount * row) + column + 1;
                }

                List<int> neighbours = new List<int> { searchUp, searchDown, searchLeft, searchRight };
                for(int i = 0; i < neighbours.Count; i++)
                {
                    if (WalledPath[neighbours[i]] == true)
                    {
                        Debug.WriteLine("removed: " + neighbours[i] + ", ");
                        neighbours.Remove(neighbours[i]);
                    }
                }
                foreach(var paths in neighbours)
                {
                    Debug.Write(paths + ", ");
                }
                tree.Add(originButton, neighbours);
                CheckOutPaths();
            }
        }

        public void CheckOutPaths()
        {
            int row, col;
            Button buttons;

            while (BFQueue.Count > 0)
            {
                var element = BFQueue.Dequeue();
                if (visitedPaths.Contains(element))
                    continue;
                else
                    visitedPaths.Add(element);
                List<int> neighbours;
                tree.TryGetValue(element.GetButton(), out neighbours);
                if (neighbours == null)
                    continue;
                foreach (var paths in neighbours)
                {
                    row = paths / mazeLayout.RowCount;
                    col = paths % mazeLayout.ColumnCount;
                    buttons = (Button)mazeLayout.GetControlFromPosition(col, row);
                    if (paths == 0 || paths == 9)
                    {
                        Debug.WriteLine("Ignored "+paths);
                        continue;
                    }
                    else if (paths != Goal.GetPosition(mazeLayout.ColumnCount))
                    {
                        if (isLocationVisited[paths] == false && WalledPath[paths] == false)
                        {
                            buttons.BackColor = Color.Blue;
                            BFQueue.Enqueue(new ButtonPosition(buttons, row, col));
                            isLocationVisited[paths] = true;
                        }

                    }
                    else
                    {
                        Debug.WriteLine("Found " + paths +"!");
                        Debug.Write("\n");
                        BFQueue.Enqueue(new ButtonPosition(buttons, row, col));
                        foreach (var line in visitedPaths.Reverse())
                        {
                            line.GetButton().BackColor = Color.Green;
                            Debug.Write(line.GetPosition(mazeLayout.ColumnCount) + ", ");
                        }
                        Debug.Write("\n");
                        foreach(var queue in BFQueue)
                        {
                            Debug.Write(queue.GetPosition(mazeLayout.ColumnCount) + ", ");
                        }
                        return;
                    }
                }
                Debug.Write("\n");
                Debug.WriteLine(BFQueue.First().GetButtonPosition());
                if (!tree.ContainsKey(BFQueue.First().GetButton()))
                {
                    if(BFQueue.Count == 0)
                    {
                        Debug.WriteLine("Ended at CheckOutPaths!");
                        return;
                    }
                    else
                    {
                        SearchForPath(BFQueue.First().GetButton());
                    }

                }
                else
                {
                    continue;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuKata
{
    class Program
    {
        static void Play()
        {
            // Prepare empty board
            string line = "+---+---+---+";
            string middle = "|...|...|...|";
            char[][] board = new char[][]
            {
                line.ToCharArray(),
                middle.ToCharArray(),
                middle.ToCharArray(),
                middle.ToCharArray(),
                line.ToCharArray(),
                middle.ToCharArray(),
                middle.ToCharArray(),
                middle.ToCharArray(),
                line.ToCharArray(),
                middle.ToCharArray(),
                middle.ToCharArray(),
                middle.ToCharArray(),
                line.ToCharArray()
            };

            // Construct board to be solved
            Random rng = new Random();

            // Top element is current state of the board
            Stack<int[]> stateStack = new Stack<int[]>();

            // Top elements are (row, col) of cell which has been modified compared to previous state
            Stack<int> rowIndexStack = new Stack<int>();
            Stack<int> colIndexStack = new Stack<int>();

            // Top element indicates candidate digits (those with False) for (row, col)
            Stack<bool[]> usedDigitsStack = new Stack<bool[]>();

            // Top element is the value that was set on (row, col)
            Stack<int> lastDigitStack = new Stack<int>();

            // Indicates operation to perform next
            // - expand - finds next empty cell and puts new state on stacks
            // - move - finds next candidate number at current pos and applies it to current state
            // - collapse - pops current state from stack as it did not yield a solution
            string command = "expand";
            while (stateStack.Count <= 9 * 9)
            {
                if (command == "expand")
                {
                    int[] currentState = new int[9 * 9];

                    if (stateStack.Count > 0)
                    {
                        Array.Copy(stateStack.Peek(), currentState, currentState.Length);
                    }

                    int bestRow = -1;
                    int bestCol = -1;
                    bool[] bestUsedDigits = null;
                    int bestCandidatesCount = -1;
                    int bestRandomValue = -1;
                    bool containsUnsolvableCells = false;

                    for (int index = 0; index < currentState.Length; index++)
                        if (currentState[index] == 0)
                        {

                            int row = index / 9;
                            int col = index % 9;
                            int blockRow = row / 3;
                            int blockCol = col / 3;

                            bool[] isDigitUsed = new bool[9];

                            for (int i = 0; i < 9; i++)
                            {
                                int rowDigit = currentState[9 * i + col];
                                if (rowDigit > 0)
                                    isDigitUsed[rowDigit - 1] = true;

                                int colDigit = currentState[9 * row + i];
                                if (colDigit > 0)
                                    isDigitUsed[colDigit - 1] = true;

                                int blockDigit = currentState[(blockRow * 3 + i / 3) * 9 + (blockCol * 3 + i % 3)];
                                if (blockDigit > 0)
                                    isDigitUsed[blockDigit - 1] = true;
                            } // for (i = 0..8)

                            int candidatesCount = isDigitUsed.Where(used => !used).Count();

                            if (candidatesCount == 0)
                            {
                                containsUnsolvableCells = true;
                                break;
                            }

                            int randomValue = rng.Next();

                            if (bestCandidatesCount < 0 ||
                                candidatesCount < bestCandidatesCount ||
                                (candidatesCount == bestCandidatesCount && randomValue < bestRandomValue))
                            {
                                bestRow = row;
                                bestCol = col;
                                bestUsedDigits = isDigitUsed;
                                bestCandidatesCount = candidatesCount;
                                bestRandomValue = randomValue;
                            }

                        } // for (index = 0..81)

                    if (!containsUnsolvableCells)
                    {
                        stateStack.Push(currentState);
                        rowIndexStack.Push(bestRow);
                        colIndexStack.Push(bestCol);
                        usedDigitsStack.Push(bestUsedDigits);
                        lastDigitStack.Push(0); // No digit was tried at this position
                    }

                    // Always try to move after expand
                    command = "move";

                } // if (command == "expand")
                else if (command == "collapse")
                {
                    stateStack.Pop();
                    rowIndexStack.Pop();
                    colIndexStack.Pop();
                    usedDigitsStack.Pop();
                    lastDigitStack.Pop();

                    command = "move";   // Always try to move after collapse
                }
                else if (command == "move")
                {

                    int rowToMove = rowIndexStack.Peek();
                    int colToMove = colIndexStack.Peek();
                    int digitToMove = lastDigitStack.Pop();

                    int rowToWrite = rowToMove + rowToMove / 3 + 1;
                    int colToWrite = colToMove + colToMove / 3 + 1;

                    bool[] usedDigits = usedDigitsStack.Peek();
                    int[] currentState = stateStack.Peek();
                    int currentStateIndex = 9 * rowToMove + colToMove;

                    int movedToDigit = digitToMove + 1;
                    while (movedToDigit <= 9 && usedDigits[movedToDigit - 1])
                        movedToDigit += 1;

                    if (digitToMove > 0)
                    {
                        usedDigits[digitToMove - 1] = false;
                        currentState[currentStateIndex] = 0;
                        board[rowToWrite][colToWrite] = '.';
                    }

                    if (movedToDigit <= 9)
                    {
                        lastDigitStack.Push(movedToDigit);
                        usedDigits[movedToDigit - 1] = true;
                        currentState[currentStateIndex] = movedToDigit;
                        board[rowToWrite][colToWrite] = (char)('0' + movedToDigit);

                        // Next possible digit was found at current position
                        // Next step will be to expand the state
                        command = "expand";
                    }
                    else
                    {
                        // No viable candidate was found at current position - pop it in the next iteration
                        lastDigitStack.Push(0);
                        command = "collapse";
                    }
                } // if (command == "move")

            }

            Console.WriteLine();
            Console.WriteLine("Final look of the solved board:");
            Console.WriteLine(string.Join("\n", board.Select(s => new string(s)).ToArray()));

            // Board is solved at this point.
            // Now pick subset of digits as the starting position.
            int remainingDigits = 30;
            int maxRemovedPerBlock = 6;
            int[,] removedPerBlock = new int[3, 3];
            int[] positions = Enumerable.Range(0, 9 * 9).ToArray();
            int[] state = stateStack.Peek();

            int removedPos = 0;
            while (removedPos < 9 * 9 - remainingDigits)
            {
                int curRemainingDigits = positions.Length - removedPos;
                int indexToPick = removedPos + rng.Next(curRemainingDigits);

                int row = positions[indexToPick] / 9;
                int col = positions[indexToPick] % 9;

                int blockRowToRemove = row / 3;
                int blockColToRemove = col / 3;

                if (removedPerBlock[blockRowToRemove, blockColToRemove] >= maxRemovedPerBlock)
                    continue;

                removedPerBlock[blockRowToRemove, blockColToRemove] += 1;

                int temp = positions[removedPos];
                positions[removedPos] = positions[indexToPick];
                positions[indexToPick] = temp;

                int rowToWrite = row + row / 3 + 1;
                int colToWrite = col + col / 3 + 1;

                board[rowToWrite][colToWrite] = '.';

                int stateIndex = 9 * row + col;
                state[stateIndex] = 0;

                removedPos += 1;
            }

            Console.WriteLine();
            Console.WriteLine("Starting look of the board to solve:");
            Console.WriteLine(string.Join("\n", board.Select(s => new string(s)).ToArray()));

            Console.WriteLine();
            Console.WriteLine(new string('=', 80));
            Console.WriteLine();

            Dictionary<int, int> maskToOnesCount = new Dictionary<int, int>();
            maskToOnesCount[0] = 0;
            for (int i = 1; i < (1 << 9); i++)
            {
                int smaller = i >> 1;
                int increment = i & 1;
                maskToOnesCount[i] = maskToOnesCount[smaller] + increment;
            }

            Dictionary<int, int> singleBitToIndex = new Dictionary<int, int>();
            for (int i = 0; i < 9; i++)
                singleBitToIndex[1 << i] = i;

            int allOnes = (1 << 9) - 1;

            bool changeMade = true;

            while (changeMade)
            {
                changeMade = false;

                int[] candidateMasks = new int[state.Length];

                for (int i = 0; i < state.Length; i++)
                    if (state[i] == 0)
                    {

                        int row = i/9;
                        int col = i%9;
                        int blockRow = row/3;
                        int blockCol = col/3;

                        int colidingNumbers = 0;
                        for (int j = 0; j < 9; j++)
                        {
                            int rowSiblingIndex = 9 * row + j;
                            int colSiblingIndex = 9*j + col;
                            int blockSiblingIndex = 9*(blockRow*3 + j/3) + blockCol*3 + j%3;

                            int rowSiblingMask = 1 << (state[rowSiblingIndex] - 1);
                            int colSiblingMask = 1 << (state[colSiblingIndex] - 1);
                            int blockSiblingMask = 1 << (state[blockSiblingIndex] - 1);

                            colidingNumbers = colidingNumbers | rowSiblingMask | colSiblingMask | blockSiblingMask;
                        }

                        candidateMasks[i] = allOnes & ~colidingNumbers;
                    }

                int[] singleCandidateIndices =
                    candidateMasks
                        .Select((mask, index) => new
                        {
                            CandidatesCount = maskToOnesCount[mask],
                            Index = index
                        })
                        .Where(tuple => tuple.CandidatesCount == 1)
                        .Select(tuple => tuple.Index)
                        .ToArray();

                if (singleCandidateIndices.Length > 0)
                {
                    int pickSingleCandidateIndex = rng.Next(singleCandidateIndices.Length);
                    int singleCandidateIndex = singleCandidateIndices[pickSingleCandidateIndex];
                    int candidateMask = candidateMasks[singleCandidateIndex];
                    int candidate = singleBitToIndex[candidateMask];

                    int row = singleCandidateIndex/9;
                    int col = singleCandidateIndex%9;

                    int rowToWrite = row + row/3 + 1;
                    int colToWrite = col + col/3 + 1;

                    board[rowToWrite][colToWrite] = (char) ('1' + candidate);
                    state[singleCandidateIndex] = candidate + 1;
                    changeMade = true;

                    Console.WriteLine("({0}, {1}) can only contain {2}.", row + 1, col + 1, candidate + 1);
                    Console.WriteLine(string.Join("\n", board.Select(s => new string(s)).ToArray()));
                    Console.WriteLine();
                }
                else
                {
                    // Try to find a number which can only appear in one place in a row/column/block

                    List<string> groupDescriptions = new List<string>();
                    List<int> candidateRowIndices = new List<int>();
                    List<int> candidateColIndices = new List<int>();
                    List<int> candidates = new List<int>();

                    for (int digit = 1; digit <= 9; digit++)
                    {
                        int mask = 1 << (digit - 1);
                        for (int cellGroup = 0; cellGroup < 9; cellGroup++)
                        {
                            int rowNumberCount = 0;
                            int indexInRow = 0;

                            int colNumberCount = 0;
                            int indexInCol = 0;

                            int blockNumberCount = 0;
                            int indexInBlock = 0;

                            for (int indexInGroup = 0; indexInGroup < 9; indexInGroup++)
                            {
                                int rowStateIndex = 9*cellGroup + indexInGroup;
                                int colStateIndex = 9*indexInGroup + cellGroup;
                                int blockRowIndex = (cellGroup/3)*3 + indexInGroup/3;
                                int blockColIndex = (cellGroup%3)*3 + indexInGroup%3;
                                int blockStateIndex = blockRowIndex*9 + blockColIndex;

                                if ((candidateMasks[rowStateIndex] & mask) != 0)
                                {
                                    rowNumberCount += 1;
                                    indexInRow = indexInGroup;
                                }

                                if ((candidateMasks[colStateIndex] & mask) != 0)
                                {
                                    colNumberCount += 1;
                                    indexInCol = indexInGroup;
                                }

                                if ((candidateMasks[blockStateIndex] & mask) != 0)
                                {
                                    blockNumberCount += 1;
                                    indexInBlock = indexInGroup;
                                }
                            }

                            if (rowNumberCount == 1)
                            {
                                groupDescriptions.Add($"Row #{cellGroup + 1}");
                                candidateRowIndices.Add(cellGroup);
                                candidateColIndices.Add(indexInRow);
                                candidates.Add(digit);
                            }

                            if (colNumberCount == 1)
                            {
                                groupDescriptions.Add($"Column #{cellGroup + 1}");
                                candidateRowIndices.Add(indexInCol);
                                candidateColIndices.Add(cellGroup);
                                candidates.Add(digit);
                            }

                            if (blockNumberCount == 1)
                            {
                                int blockRow = cellGroup/3;
                                int blockCol = cellGroup%3;

                                groupDescriptions.Add($"Block ({blockRow + 1}, {blockCol + 1})");
                                candidateRowIndices.Add(blockRow*3 + indexInBlock/3);
                                candidateColIndices.Add(blockCol*3 + indexInBlock%3);
                                candidates.Add(digit);
                            }
                        } // for (cellGroup = 0..8)
                    } // for (digit = 1..9)

                    if (candidates.Count > 0)
                    {
                        int index = rng.Next(candidates.Count);
                        string description = groupDescriptions.ElementAt(index);
                        int row = candidateRowIndices.ElementAt(index);
                        int col = candidateColIndices.ElementAt(index);
                        int digit = candidates.ElementAt(index);
                        int rowToWrite = row + row/3 + 1;
                        int colToWrite = col + col/3 + 1;

                        string message = $"{description} can contain {digit} only at ({row + 1}, {col + 1}).";

                        int stateIndex = 9*row + col;
                        state[stateIndex] = digit;
                        board[rowToWrite][colToWrite] = (char) ('0' + digit);

                        changeMade = true;

                        Console.WriteLine(message);
                        Console.WriteLine(string.Join("\n", board.Select(s => new string(s)).ToArray()));
                        Console.WriteLine();
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Play();

            Console.WriteLine();
            Console.Write("Press ENTER to exit... ");
            Console.ReadLine();
        }
    }
}
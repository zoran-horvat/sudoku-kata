# sudoku-kata
Coding practice demonstrating an awfully long method. Try to refactor this method.

This entire exercise consists of one function (static method Play in the Program class). It has around 1,000 lines of code and it implements entire Sudoku game following these steps:

1. Construct a fully solved Sudoku board - this step ensures that there is the solution
2. Remove certain number of digits from the board - this step constructs initial board which is guaranteed to have at least one solution.
3. Try to find a rule which concludes that one digit can be added to the board.
4. Repeat step 3 while new digits are being added to the board.

Final source of the Play function is the starting point for refactoring. Complete exercise consists of refactoring the Play function into a complete object model. There are no correct and incorrect solutions to the exercise - try to go as far as you can with building a better object-oriented solution which is still doing the same thing as the original function.

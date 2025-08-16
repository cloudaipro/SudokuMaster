# CCSUnityToolkit
CCS Unity Toolkit
compiler sudoku generator:
 
mcs Generator.cs Board.cs Cell.cs Solver.cs ../CommonExtensions.cs
# 
mono ./Generator.exe 100 Hard Hard.txt

 { "Easy", Tuple.Create(35, 0) }, <br />
 { "Medium", Tuple.Create(81, 5) },<br />
 { "Hard", Tuple.Create(81, 15) },<br />
 { "Extreme", Tuple.Create(81, 45) }<br />

# cutoff mode
# ex  mono ./Generator.exe 100 72 evit.txt
mono ./Generator.exe 1000 10 medium.txt <br />
mono ./Generator.exe 1000 15 hard.txt <br />
mono ./Generator.exe 500 72 extreme.txt <br />


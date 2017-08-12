# SonarLogApi
Small C# API for working with sonar log files.
Now it supports SL2 and SL3 log files formats from Lowrance(Simrad, B&G) and CSV format.

# ConsileLogConverter 
It's example project for log files conversion.

Usage:
Research example.
"ConsoleLogConverter.exe -i input.sl3 -s 30 -t 10 -c 0:2"
Command takes all frames from input.sl3. At the next step it takes frames from channels 0 and 2 with frame 
index from 0 to 10.
And finally it takes four bytes at 30 offset from each frame start and represent them as differet types 
of value(string, single bytes, short from first two bytes, short from second two bytes, integer, float).

Log file format conversion example.
"ConsoleLogConverter.exe -i input.sl2 -f 10 -t 509 -c 0 -a -o sl2:csv"
Command takes all frames from input.sl2. At the next step it takes frames from channel 0 with frame index 
from 10 to 509 and delete GPS coordinates from it . And finally it save frames to two files with "sl2" and "csv" format.

Manual depth shift example
"ConsoleLogConverter.exe -i input.sl2 -h m1.15 -o csv"
Command takes all frames from input.sl2. At the next step it subtract (use "m"(minus) prefix to substract and "p"(plus) to add value) 1.15 meters from depth value at each frame and save frames to "csv" format.

Depth Ajust(based on second log) example.
There are situations where the depth in one log is necessary to ajust to the depth in another log.
"ConsoleLogConverter.exe -i BaseDepthPoints.sl2 -d pointsForAdjust.sl2 -o csv"
Command takes all frames from BaseDepthPoints.sl2 and pointsForAdjust.sl2 files. At the next step it finds nearest points at two sequences and calculate depth difference between em. After that it add difference to each pointsForAdjust.sl2 frame. Finally it contact two sequences and save frames to file with "csv".

Also you can combine examples.
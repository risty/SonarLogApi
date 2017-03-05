# SonarLogApi
Small C# API for working with sonar log files.
Now it supports SL2 and SL3 log files formats from Lowrance(Simrad, B&G) and CVS format.

# ConsileLogConverter 
It's example project for log files conversion.

Usage:
"ConsoleLogConverter.exe -i input.sl3 -s 30 -t 10 -c 0:2"
Command takes all frames from input.sl3. At the next step it takes frames from channels 0 and 2 with frame 
index from 0 to 10.
And finally it takes four bytes at 30 offset from each frame start and represent them as differet types 
of value(string, single bytes, short from first two bytes, short from second two bytes, integer, float).

"ConsoleLogConverter.exe -i input.sl2 -f 10 -t 509 -c 0 -a -o sl2:cvs"
Command takes all frames from input.sl2. At the next step it takes frames from channel 0 with frame index 
from 10 to 509 and delete GPS coordinates from it . And finally it save frames to two files with "sl2" and "cvs" format.


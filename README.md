# SonarLogApi
C# API for working with sonar log files.
Now it supports SL2 and SL3 log files formats from Lowrance(Simrad, B&G) and CSV format.

# ConsileLogConverter
It's API example project for log files conversion.

### Log file format conversion command example:
"**ConsoleLogConverter.exe -i input.sl2 -f 10 -t 509 -c 0 -a -o sl2:csv**".
Command takes all frames from input.sl2. At the next step it takes frames from 0th(Primary) channel with frame index 
from 10 to 509 and delete GPS coordinates from it . And finally it save frames to two files with "sl2" and "csv" format.

### Manual depth shift command example:
"**ConsoleLogConverter.exe -i input.sl2 -h m1.15 -o csv**"
Command takes all frames from input.sl2. At the next step it subtract (use "m"(minus) prefix to substract and "p"(plus) to add value) 1.15 meters from depth value at each frame and finally saves frames to "csv" format.

### Depth adjust(based on second log) command example:
There are situations where the depth in one log is necessary to adjust to the depth in another log.
"**ConsoleLogConverter.exe -i BaseDepthPoints.sl2 -d pointsForAdjust.sl2 -o csv**"
Command takes all frames from BaseDepthPoints.sl2 and pointsForAdjust.sl2 files. At the next step it finds nearest points at two sequences and calculate depth difference between them. After that it add difference to each pointsForAdjust.sl2 frame. Finally it contact two sequences and save frames to file with "csv".

### Channel generation command example:
There are situations where you have valid data at one channel and corrupt data at another. You can generate frames for specific(corrupted) channel from the frames at other(valid) channel(s).
"**ConsoleLogConverter.exe -i input.sl2 -g 1:2:5:f -o sl2**"
Command takes all frames from 2th(DownScan) and 5th (SidescanComposite) channels from input.sl2 and group them by unique coordinates.
After that it generate 1th(Secondary) channel frames (even if frames with such coordinates from 2th and 5th chanel are in 1th("f" option)).
f - if specified generate frame in destination channel even if frame with such coordinates already exist;
d - if specified generate sounded data from source frame depth value,(otherwise take sounded data from source frame).


### Research command example.
SL binary formats are closed, and there is for all values no public schema . So, you can help to project and make some research by yourself.
"**ConsoleLogConverter.exe -i input.sl3 -s 30 -t 10 -c 0:2**"
Command takes all frames from input.sl3. At the next step it takes frames from channels 0 and 2 with frame 
index from 0 to 10.
And finally it takes four bytes at 30 offset from each frame start and represent them as differet types 
of value(string, single bytes, short from first two bytes, short from second two bytes, integer, float).

Also you can combine command examples.

### Donate:

#### BTC:1NodB8A9fgdLTpx9k6mK1JYscFw5PvHCcR
#### ETH:993C9eC8aF0800a6Be2A0459cABdc19e6B852A49
#### ZEC:t1c1sraePE5PvBGPm8DNmF13cedh3BGsj1C
#### ZEN:znbL6teKJrALBBCmnNKnLS7rFBD527FjjKD

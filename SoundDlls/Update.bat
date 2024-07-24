rem source for this dlls: https://github.com/XMAMan/SoundEngine

set SOURCE_PATH="..\..\..\57 MusicMachine\SoundEngine\Source\SoundEngineTest\bin\Debug\"

copy /y %SOURCE_PATH%DynamicData.dll .
copy /y %SOURCE_PATH%libmp3lame.32.dll .
copy /y %SOURCE_PATH%libmp3lame.64.dll .
copy /y %SOURCE_PATH%MidiParser.dll .
copy /y %SOURCE_PATH%MidiParser.pdb .
copy /y %SOURCE_PATH%NAudio.dll .
copy /y %SOURCE_PATH%NAudio.Lame.dll .
copy /y %SOURCE_PATH%NAudioWaveMaker.dll .
copy /y %SOURCE_PATH%NAudioWaveMaker.pdb .
copy /y %SOURCE_PATH%SoundEngine.dll .
copy /y %SOURCE_PATH%SoundEngine.pdb .
copy /y %SOURCE_PATH%WaveMaker.dll .
copy /y %SOURCE_PATH%WaveMaker.pdb .
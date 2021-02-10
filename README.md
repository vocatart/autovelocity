# AutoVelocity
Automatic consonant velocity calculation for VCCV formatted UST files for UTAU

# Usage
unzip the AutoVelocity vx.x.x folder and drag it into the plugin folder in the UTAU directory.

# Config
AutoVelocity has multiple user-definable variables in the AutoVelocity.exe.config file.
| Configuration | Default Value | Value Type | Description | 
|--|--|--|--|
| baseVelocity | 100 | int | Velocity of a Quarter note if the tempo was 120 (default velocity in UTAU). |
| baseTempo | 120 | float | The tempo in which the current voicebank is recorded, almost always 120.
| velocityAmount | 10 | int | How much velocity is added or subtracted to notes smaller or larger than a quarter note.
| doManualVelocity | false | bool | If true, enables the manual velocity variable. |
| manualVelocity | 100 | int | user-set velocity of a quarter note, replaces automatic calculation. |
| qNoteLength | 480 | int | Length of a quarter note in UTAU-defined ticks. |
| endingVelocity | true | bool | If true, plugin will change the velocity of ending-type notes. |
| beginningVelocity | true | bool | If true, plugin will change the velocity of beginning-type notes. |
| modSmooth | true | bool | If true, plugin will apply a modulation value of 0 during phase 2 velocity setting.
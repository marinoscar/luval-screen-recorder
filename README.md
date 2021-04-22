# Console based Screen Recorder
The application provides a way to record the screen into a mp4 format either in a continuous file or a rolling format, the application consists of two independent console applications, the recorder that will start the screen recorder in thread which only allows a single recording per user session and then a client application to allow to finish the recording session with a command.
---
## Recorder Commands
`/session` the name of the recording session, the default value would be `{MachineName}-{UserName}-RECORDING`
`/outputFile` the name of the mp4 file with the recording the default value would be `{SessionName}-{NewGuid}.mp4`
`/isRolling` indicates if the recording is on a rolling file, the default value is `true`
`/duration` for a rolling file indicates the duration in minutes of the recording, the default value is `3` minutes
`/interval` the number of milliseconds in between frames taken of the desktop to create the video the default value is `250` ms
`/maxRecording` identifies the max amount of minutes a recording session will be active, the default value is `120` minutes
`/windowModel` indicates how the recording screen console application would be shown 0 = hide, 5 = show, 6 = minimized default value is `6` minimized

## Client Commands
`/session` the name of the recording session, the default value would be `{MachineName}-{UserName}-RECORDING`


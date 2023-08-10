# AudioVideo 360° Quality of Experience
 
## Description
This application was created as part of Master thesis: "Investigation into the quality of audio-visual 360° recordings" at Bialystok University of Technology in course of Computer Science.\
It was used for conducting __Quality of Experience__ experiments compliant with __DCR__ methodology.\
Application handles playback of spatial video and orchestrates audio playback, as well as gathers other necessary data (_except participant entry questionnaire_).

## Technology used
Application was developed with __Unity__ and requires use of a __HMD__ and external __DAW__ that supports _VCT_ plugins. \
_VCT_ plugins used in research done with this application are two plugins from [IEM Plug-in Suite](https://plugins.iem.at/): _SceneRotator_ and _BinauralDecoder_.\
Controlling of those plugins is done via _OSC_ built-in plugins. _OSC_ also controls playback via _ReaperDAW_ Default OSC scheme.

## Features
- __DCR__ questionnaires in _Polish_ language
- __VRSQ__ questionnaire after test session
- gathering test setup information
- gathering questionnaire answers
- gathering head tracking data
- audio spatial delay (opt-in and configurable per test video)

## Acknowledgements
I would like to thank:
- Institute of Electronic Music and Acoustics for creating Open-Source plugins to work with Ambisonic audio.
- FORCE Technology for providing HOA-SSR dataset used in Master thesis research.
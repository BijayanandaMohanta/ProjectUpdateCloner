[Version]
Class=IEXPRESS
SEDVersion=3

[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=1
HideExtractAnimation=1
UseLongFileName=1
InsideCompressed=0
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=N
InstallPrompt=%InstallPrompt%
DisplayLicense=%DisplayLicense%
FinishMessage=%FinishMessage%
TargetName=%TargetName%
FriendlyName=%FriendlyName%
AppLaunched=%AppLaunched%
PostInstallCmd=%PostInstallCmd%
AdminQuietInstCmd=%AdminQuietInstCmd%
UserQuietInstCmd=%UserQuietInstCmd%
SourceFiles=SourceFiles

[Strings]
InstallPrompt=
DisplayLicense=
FinishMessage=Project Update Cloner Has Been Installed.
TargetName=C:\xampp\htdocs\practices\clone updates\dist\ProjectUpdateCloner-Setup.exe
FriendlyName=Project Update Cloner Setup
AppLaunched=setup.cmd
PostInstallCmd=<None>
AdminQuietInstCmd=
UserQuietInstCmd=
FILE0="ProjectUpdateCloner.exe"
FILE1="setup.cmd"
FILE2="setup.ps1"
FILE3="uninstall.ps1"

[SourceFiles]
SourceFiles0=C:\xampp\htdocs\practices\clone updates\installer\payload\

[SourceFiles0]
%FILE0%=
%FILE1%=
%FILE2%=
%FILE3%=

@ECHO OFF
echo Do you want to install the auto-update hook in your local git repo? The auto-update is a githook which runs the UpdateBinary script everytime you pull the sources.
echo The hook will be in '.git(a hidden folder)/hooks/post-merge' under the local repo. You can uninstall it by delete the hook file manually.
choice /C YN /M "Press Y for install, N for cancel."
if %errorlevel%==1 goto INSTALL_HOOK
echo Install cancelled
pause
exit

:INSTALL_HOOK

echo #!/bin/sh > "./post-merge"
echo Powershell.exe -executionpolicy unrestricted -File ./Tools/process_downloadartifacts.ps1 NOPAUSE >> "./post-merge"
echo git lfs post-merge "$@" >> "./post-merge"
move /Y "./post-merge" "./../.git/hooks/"

echo #!/bin/sh > "./post-checkout"
echo Powershell.exe -executionpolicy unrestricted -File ./Tools/process_downloadartifacts.ps1 NOPAUSE >> "./post-checkout"
echo command -v git-lfs ^>/dev/null 2^>^&1 ^|^| { echo ^>^&2 "\nThis repository is configured for Git LFS but 'git-lfs' was not found on your path. If you no longer wish to use Git LFS, remove this hook by deleting .git/hooks/post-checkout.\n"; exit 2; } >> "./post-checkout"
echo git lfs post-checkout "$@" >> "./post-checkout"
move /Y "./post-checkout" "./../.git/hooks/"

echo Install done
pause
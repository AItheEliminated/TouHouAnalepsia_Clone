# Touhou Analepsia ~ Illusory Nirvana

This is the repo of Touhou Analepsia. A short tutorial will tell you how to setup the project.

## Checkout

#### Generate the access token

First, you need an access token before checkout. Access tokens are used to access restricted networks without inputting or storing your password. You can generate an access token in Settings - Applications - Manage Access Tokens.

<img src="http://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/branch/master/Tools/Gitea/gat1.png" />

<img src="http://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/branch/master/Tools/Gitea/gat2.png" />

<img src="http://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/branch/master/Tools/Gitea/gat3.png" />

#### Clone the repository

I recommend using [Git Extensions](https://github.com/gitextensions/gitextensions/releases) as the tool for managing git repository.  I will use Git Extensions for the demonstration, although you can use any git tool you like, of course.

Clone the repository with this url "http://\[Your_access_token\]@git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia.git". You can set the destination to either the module folder of Bannerlord, or a singleton folder and then link it to the module folder by the symbolic link.

<img src="http://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/branch/master/Tools/Gitea/clone1.png" />

##### Choice A: Clone to the module folder

Cloning the repo to the module folder is the easiest way. Just set the destination to the module folder and you can move to next section.

<img src="http://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/branch/master/Tools/Gitea/clone2.png" />

*(Notice: If you are using JetBrains Rider, don't do this. I will explain later. Use the method B instead.)*

##### Choice B: Clone to a singleton folder and create a symbolic link

For some reason, you may not want to dump a scrap heap into the game directory. That can be achieved by the symbolic link. Clone the repo to anywhere you like. Then open the cmd as administrator, and run *mklink /D "\[Bannerlord Module Path\]/TouhouAnalepsia" "\[Repo Path\]"*

<img src="http://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/branch/master/Tools/Gitea/clone3.png" />

<img src="http://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/branch/master/Tools/Gitea/clone4.png" />

After that, a symbolic link will be created in the module folder. The game can recognize the mod just as it's in the module folder.

## Setup

#### For artists, mappers, designers...

The repository doesn't contain the compiled binary files (dll files). There is a script to download the latest binary files from the server. In the *Tools* folder, you can run *UpdateBinary.bat* to download the binaries.

<img src="http://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/branch/master/Tools/Gitea/sa1.png" />

However, manually running the script is too bothering. You can install a githook to run the script everytime you pull the repo. Run *InstallAutoUpdateHook.bat* to install the githook.

<img src="http://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/branch/master/Tools/Gitea/sa2.png" />

*Githook is a single file which installed in the .git folder of the repo. It won't affect your computer and can be uninstalled anytime*

#### For programmers

Of course we don't need the pre-compiled binaries - we will build them by ourselves.

The source codes are located in the *Sources* folder. The configuration is almost done. All you need to do is a few settings.

Make a copy of *UserSettings.props.template* in the *Sources*, and then rename the copy to *UserSettings.props*

*UserSettings.props won't be committed because of the gitignore*

<img src="http://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/branch/master/Tools/Gitea/sp1.png" />

Open the file with the text editor. Fill the *BannerlordDirectory* item with the game path. If you have installed the modding tool, you can set *PreferEditor* to true.

<img src="http://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/branch/master/Tools/Gitea/sp2.png" />

That's all. Open *TouhouAnalepsia.sln* with the IDE and the project is ready-to-use. You can launch the game and debug with the IDE. The output files will be copied to bin folder automatically after compile.

*Tips for JetBrains Rider users: (Tested in version 2018.2) Don't clone the project to the module folder of Bannerlord. Due to the ampersand(&) symbol in the path, Rider cannot correctly initialize the project. You will get 'Error executing the AssignProjectConfiguration task' message and the project is corrupt if you do so. Moreover, you have to configure the launching settings by yourself. One more problem is that, the reference libraries don't show up in the left list, though they actully exist. It shouldn't affect the operation, anyway.*

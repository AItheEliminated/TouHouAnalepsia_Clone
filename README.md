# Touhou Analepsia ~ Illusory Nirvana

This is the repo of Touhou Analepsia. A short tutorial will tell you how to setup the project.

[中文版本](https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/src/branch/master/README.zh-CN.md)

## Checkout

#### Generate the access token

First, you need an access token before checkout. Access tokens are used to access restricted networks without inputting or storing your password. You can generate an access token in Settings - Applications - Manage Access Tokens.

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/gat1.png" />

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/gat2.png" />

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/gat3.png" />

#### Clone the repository

I recommend using [Git Extensions](https://github.com/gitextensions/gitextensions/releases) as the tool for managing git repository.  I will use Git Extensions for the demonstration, although you can use any git tool you like, of course.

*If you use other tool, make sure your tool supports [Git-LFS](https://git-lfs.github.com/).*

Clone the repository with this url "https://\[Your_access_token\]@git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia.git". You can set the destination to either the module folder of Bannerlord, or a singleton folder and then link it to the module folder by the symbolic link.

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/clone1.png" />

The codes are now included as a git submodule. To fetch the code module along with the assets, simply run 
`git clone --recursive  git://git.hinageshi.org/TouhouAnalepsia.git` or
`git clone --recurse-submodules  git://git.hinageshi.org/TouhouAnalepsia.git` for Git > 2.13.

##### Choice A: Clone to the module folder

Cloning the repo to the module folder is the easiest way. Just set the destination to the module folder and you can move to next section.

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/clone2.png" />

*(Notice: If you are using JetBrains Rider, don't do this. I will explain later. Use the method B instead.)*

##### Choice B: Clone to a singleton folder and create a symbolic link

*(If you need to edit scenes frequently. It is recommended to use method A. Using method B may encounter bugs when you launch the scene from editor.)*

For some reason, you may not want to dump a scrap heap into the game directory. That can be achieved by the symbolic link. Clone the repo to anywhere you like. Then open the cmd as administrator, and run *mklink /D "\[Bannerlord Module Path\]/TouhouAnalepsia" "\[Repo Path\]"*

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/clone3.png" />

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/clone4.png" />

After that, a symbolic link will be created in the module folder. The game can recognize the mod just as it's in the module folder.

## Setup

#### For artists, mappers, designers...

The repository doesn't contain the compiled binary files (dll files). There is a script to download the latest binary files from the server. In the *Tools* folder, you can run *UpdateBinary.bat* to download the binaries.

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/sa1.png" />

However, manually running the script is too bothering. You can install a githook to run the script everytime you pull the repo. Run *InstallAutoUpdateHook.bat* to install the githook.

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/sa2.png" />

*Githook is a single file which installed in the .git folder of the repo. It won't affect your computer and can be uninstalled anytime*

#### For programmers

Of course we don't need the pre-compiled binaries - we will build them by ourselves.

The source codes are located in the *Sources* folder. The configuration is almost done. All you need to do is a few settings.

Make a copy of *UserSettings.props.template* in the *Sources*, and then rename the copy to *UserSettings.props*

*UserSettings.props won't be committed because of the gitignore. Your settings won't affect the others.*

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/sp1.png" />

Open the file with the text editor (*Windows Notepad* is **NOT recommended**). Fill the *BannerlordDirectory* item with the game path. If you have installed the modding tool, you can set *PreferEditor* to true.

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/sp2.png" />

That's all. Open *TouhouAnalepsia.sln* with your favourite IDE and the project is ready-to-use. You can launch the game and debug with the IDE. The output files will be copied to bin folder automatically after compile.

*Tips for Visual Studio Code users: You need to change VSCode to true in UserSettings.props, then make copies of the two files with the suffix .json.template in Sources/.vscode directory, and remove the .template suffix. tasks.json is an out-of-the-box build configuration. launch.json is a template for the launch configuration, and you need to fill in the location of Bannerlord EXE and the directory where the EXE is located.*

*Tips for JetBrains Rider users: (Tested in version 2018.2) Don't clone the project to the module folder of Bannerlord. Due to the ampersand(&) symbol in the path, Rider cannot correctly initialize the project. You will get 'Error executing the AssignProjectConfiguration task' message and the project is corrupt if you do so. Moreover, you have to configure the launching settings by yourself. One more problem is that, the reference libraries don't show up in the left list, though they actully exist. It shouldn't affect the operation, anyway.*



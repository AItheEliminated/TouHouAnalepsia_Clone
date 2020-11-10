# 东方淬樱骑 ~ Illusory Nirvana

这里是东方淬樱骑的主仓库，这篇教程可以指导你如何从零检出仓库并配置项目。

## 检出

#### 生成访问令牌

首先，你需要有一个访问令牌（Access Token）才能执行Git操作，访问令牌是一段能够验证用户身份的加密文本，用于在不输入密码的情况下访问私有网络，你可以在“设置 - 应用 - 管理Access Tokens”中生成访问令牌。

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/gat1.png" />

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/gat2.png" />

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/gat3.png" />

#### 克隆档案库

我推荐使用[Git Extensions](https://github.com/gitextensions/gitextensions/releases)来管理、操作档案库，在下文中我会使用Git Extensions来演示操作，当然你可以继续使用任何你习惯的Git工具。

*如果你使用别的工具，务必确保你的工具支持[Git-LFS](https://git-lfs.github.com/)。*

使用这个Url来克隆项目"https://\[你的访问令牌\]@git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia.git"。至于克隆的目的地，有两种方案，直接克隆到Module目录内，或者克隆到外部目录然后用符号链接连回Module。

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/clone1.png" />

##### 方案A：直接克隆到砍二的Module目录内

直接把项目克隆到砍二的Module目录内是最简单的方法，把目的地设为Module目录就行了。

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/clone2.png" />

*（注意：如果你使用JetBrains Rider，那就不要这样做，请改用方案 B，我会在后文解释原因）*

##### 方案B：克隆到一个外部目录，然后创建一个符号链接

出于各种原因，你可能不想把一个大垃圾堆直接放到游戏盘内，你可以通过符号链接做到这一点。将项目克隆到一个你想存放它的地方，然后以管理员权限启动CMD窗口，并执行*mklink /D "\[砍二Module目录的路径\]/TouhouAnalepsia" "\[仓库路径\]"*

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/clone3.png" />

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/clone4.png" />

这样就行了，一个符号链接会将项目映射到Module目录下，游戏会像对待正常Mod一样对待它。

## 配置

#### 对于美工、场景、策划...

档案库并不包含编译完的程序文件（即bin目录下的那堆DLL），你需要运行一个脚本来从服务器上下载编译好的程序，运行*Tools*目录中的*UpdateBinary.bat*即可自动下载最新版的程序文件，下载完的文件会被自动部署入bin目录内。

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/sa1.png" />

然而每次都要手动运行脚本也太麻烦了，因此你可以安装一个Githook来在每次拉取项目时自动运行更新脚本，运行*InstallAutoUpdateHook.bat*即可安装Githook。

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/sa2.png" />

*Githook是一个放置于项目的.git目录内的单文件脚本，不会影响你的电脑，不想要时只要删掉它即可完成卸载。*

#### 对于程序员

忘了什么预编译DLL吧，我们当然会在自己的机器上编译。

程序源码位于*Sources*文件夹下，配置已经基本弄好了，你只需要填一个目录参数就行。

将*Sources*内的*UserSettings.props.template*原地复制一份并改名为*UserSettings.props* （即去掉.*template*后缀）

*gitignore已被配置为不会提交UserSettings.props，你的设置不会影响其他人。*

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/sp1.png" />

用文本编辑器（强烈**不建议**使用*记事本*）打开它，在*BannerlordDirectory*一项内填入砍二的路径；如果你安装了Modding Tool的话，可以将*PreferEditor*改为true以在启动时使用砍二开发版。

<img src="https://git.hinageshi.org/TouhouAnalepsia/TouhouAnalepsia/raw/commit/17fd06cf57cb4a3e29541ea8b402dd3e0d5b68f6/Tools/Gitea/sp2.png" />

就这些，用你最爱的IDE打开*TouhouAnalepsia.sln*后就可以搬砖了，你可以在IDE中启动运行砍二，在编译时生成的DLL会被自动复制入Mod的bin目录内。

*给JetBrains Rider用户的提示：（验证于2018.2版）不要直接将项目克隆到砍二Module目录中，由于砍二目录中包含有 &号，Rider在初始化项目时会报错，你会看到类似于'Error executing the AssignProjectConfiguration task'的错误提示。此外，你得自己手动配置启动方案。还有一个问题是项目引用的砍二DLL不会显示在左侧列表中，不过不影响正常使用。*
